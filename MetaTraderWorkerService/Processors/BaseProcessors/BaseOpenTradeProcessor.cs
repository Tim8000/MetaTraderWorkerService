using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Processors.OrderProcessors;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.BaseProcessors;

public abstract class BaseOpenTradeProcessor : IOrderActionProcessor
{
    protected readonly IOrderRepository _orderRepository;
    protected readonly IMetaApiService _metaApiService;
    protected readonly ILogger<BaseOpenTradeProcessor> _logger;

    protected BaseOpenTradeProcessor(
        IOrderRepository orderRepository,
        IMetaApiService metaApiService,
        ILogger<BaseOpenTradeProcessor> logger) // Use ILogger<T>
    {
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
        _logger = logger;
    }

    public async Task ProcessAsync(MetaTraderOrder metaTraderOrder)
    {
        SetOrderConfigurations(metaTraderOrder);
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);

        OpenTradeByMarketPriceResponseDto marketResponseDto = null;
        MetaTraderOpenTradeOrderResponseDto limitResponseDto = null;

        // Handle market orders
        if (metaTraderOrder.ActionType == ActionType.ORDER_TYPE_BUY ||
            metaTraderOrder.ActionType == ActionType.ORDER_TYPE_SELL)
        {
            var marketOrderDto = CreateOpenTradeByMarketPriceRequestDto(metaTraderOrder);
            marketResponseDto = await _metaApiService.OpenTradeByMarketPriceAsync(marketOrderDto);

            if (marketResponseDto != null)
            {
                await HandleMarketResponseAsync(metaTraderOrder, marketResponseDto);
            }
            else
            {
                _logger.LogError($"Market order failed: no response for Order ID {metaTraderOrder.Id}");
                metaTraderOrder.Status = OrderStatus.Failed;
                await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            }
        }

        // Handle limit orders
        if (metaTraderOrder.ActionType == ActionType.ORDER_TYPE_BUY_LIMIT ||
            metaTraderOrder.ActionType == ActionType.ORDER_TYPE_SELL_LIMIT)
        {
            var limitOrderDto = CreateMetaTraderOpenTradeOrderDto(metaTraderOrder);
            limitResponseDto = await _metaApiService.PlacePendingOrderAsync(limitOrderDto);

            if (limitResponseDto != null)
            {
                await HandleLimitResponseAsync(metaTraderOrder, limitResponseDto);
            }
            else
            {
                _logger.LogError($"Limit order failed: no response for Order ID {metaTraderOrder.Id}");
                metaTraderOrder.Status = OrderStatus.Failed;
                await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            }
        }


        async Task HandleMarketResponseAsync(MetaTraderOrder metaTraderOrder,
            OpenTradeByMarketPriceResponseDto responseDto)
        {
            if (responseDto.PositionId != null)
            {
                SetValuesToMetaTraderOrderFromOpenByMarketResponseDto(metaTraderOrder, responseDto);
                _logger.LogInformation($"Market order successfully created for ID: {metaTraderOrder.Id}");
            }
            else
            {
                metaTraderOrder.Status = OrderStatus.Failed;
                metaTraderOrder.MetaTraderMessage = "Market order failed due to invalid response.";
                _logger.LogError($"Market order creation failed for ID {metaTraderOrder.Id}");
            }

            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }

        async Task HandleLimitResponseAsync(MetaTraderOrder metaTraderOrder,
            MetaTraderOpenTradeOrderResponseDto responseDto)
        {
            if (responseDto.NumericCode == TradeResultCode.Done)
            {
                SetValuesToMetaTraderOrderFromResponseDto(metaTraderOrder, responseDto);
                _logger.LogInformation($"Limit order successfully created for ID: {metaTraderOrder.Id}");
            }
            else
            {
                metaTraderOrder.Status = OrderStatus.Failed;
                metaTraderOrder.MetaTraderStringCode = responseDto.StringCode;
                metaTraderOrder.MetaTraderTradeResultCode = responseDto.NumericCode;
                metaTraderOrder.MetaTraderMessage = responseDto.Message;
                _logger.LogError($"Limit order creation failed for ID {metaTraderOrder.Id}: {responseDto.Message}");
            }

            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }

        // else
        // {
        //     _logger.LogError($"Order failed: no response for Order ID {metaTraderOrder.Id}");
        //     metaTraderOrder.Status = OrderStatus.Failed;
        //     await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        // }
    }

    protected abstract void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder,
        OpenTradeByMarketPriceRequestDto marketOrderDto);

    private async Task HandleResponseAsync(MetaTraderOrder metaTraderOrder,
        MetaTraderOpenTradeOrderResponseDto orderResponseDto)
    {
        if (orderResponseDto.NumericCode == TradeResultCode.Done)
        {
            SetValuesToMetaTraderOrderFromResponseDto(metaTraderOrder, orderResponseDto);
            _logger.LogInformation($"Order successfully created for ID: {metaTraderOrder.Id}");
        }
        else if (orderResponseDto.NumericCode == TradeResultCode.InvalidPrice)
        {
            var marketOrderDto = CreateOpenTradeByMarketPriceRequestDto(metaTraderOrder);
            SetActionTypeForMarketOrder(metaTraderOrder, marketOrderDto);

            var marketOrderResponse = await _metaApiService.OpenTradeByMarketPriceAsync(marketOrderDto);
            if (marketOrderResponse.PositionId != null)
                SetValuesToMetaTraderOrderFromOpenByMarketResponseDto(metaTraderOrder, marketOrderResponse);

            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }
        else
        {
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.MetaTraderStringCode = orderResponseDto.StringCode;
            metaTraderOrder.MetaTraderTradeResultCode = orderResponseDto.NumericCode;
            metaTraderOrder.MetaTraderMessage = orderResponseDto.Message;
            _logger.LogError($"Order creation failed for ID {metaTraderOrder.Id}: {orderResponseDto.Message}");
        }

        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
    }

    private void SetOrderConfigurations(MetaTraderOrder metaTraderOrder)
    {
        metaTraderOrder.Volume = 0.02m; // Example volume, you can customize this
        metaTraderOrder.Status = OrderStatus.Pending;
        metaTraderOrder.Slippage = 1;
        metaTraderOrder.Magic = GenerateMagic();
    }

    private static MetaTraderOpenTradeOrderRequestDto CreateMetaTraderOpenTradeOrderDto(MetaTraderOrder metaTraderOrder)
    {
        return new MetaTraderOpenTradeOrderRequestDto
        {
            Symbol = metaTraderOrder.Symbol,
            Volume = metaTraderOrder.Volume,
            ActionType = metaTraderOrder.ActionType.Value.ToString(),
            OpenPrice = metaTraderOrder.OpenPrice,
            StopLoss = metaTraderOrder.StopLoss,
            TakeProfit = metaTraderOrder.TakeProfit,
            Slippage = metaTraderOrder.Slippage,
            ClientId = metaTraderOrder.ClientId,
            Comment = metaTraderOrder.Comment
        };
    }

    private static OpenTradeByMarketPriceRequestDto CreateOpenTradeByMarketPriceRequestDto(
        MetaTraderOrder metaTraderOrder)
    {
        return new OpenTradeByMarketPriceRequestDto
        {
            Symbol = metaTraderOrder.Symbol,
            Volume = metaTraderOrder.Volume.Value,
            ActionType = metaTraderOrder.ActionType.Value.ToString(),
            StopLoss = metaTraderOrder.StopLoss,
            TakeProfit = metaTraderOrder.TakeProfit,
            ClientId = metaTraderOrder.ClientId,
            Comment = $"Market order for {metaTraderOrder.Symbol}"
        };
    }

    private static void SetValuesToMetaTraderOrderFromResponseDto(MetaTraderOrder metaTraderOrder,
        MetaTraderOpenTradeOrderResponseDto responseDto)
    {
        metaTraderOrder.MetaTraderTradeStartTime = responseDto.TradeStartTime;
        metaTraderOrder.MetaTraderTradeExecutionTime = responseDto.TradeExecutionTime;
        metaTraderOrder.MetaTraderOrderId = responseDto.OrderId;
        metaTraderOrder.MetaTraderStringCode = responseDto.StringCode;
        metaTraderOrder.MetaTraderMessage = responseDto.Message;
        metaTraderOrder.Status = OrderStatus.SentToMetaTrader;
        metaTraderOrder.MetaTraderTradeResultCode = (TradeResultCode)responseDto.NumericCode;
    }

    private static void SetValuesToMetaTraderOrderFromOpenByMarketResponseDto(MetaTraderOrder metaTraderOrder,
        OpenTradeByMarketPriceResponseDto responseDto)
    {
        metaTraderOrder.MetaTraderTradeStartTime = responseDto.TradeStartTime;
        metaTraderOrder.MetaTraderTradeExecutionTime = responseDto.TradeExecutionTime;
        metaTraderOrder.MetaTraderOrderId = responseDto.OrderId;
        metaTraderOrder.MetaTraderStringCode = responseDto.StringCode;
        metaTraderOrder.MetaTraderMessage = responseDto.Message;
        metaTraderOrder.Status = OrderStatus.SentToMetaTrader;
        metaTraderOrder.MetaTraderTradeResultCode = (TradeResultCode)responseDto.NumericCode;
    }

    private int GenerateMagic()
    {
        return (int)(DateTime.UtcNow.Ticks % 1000000); // Example magic number generation
    }
}