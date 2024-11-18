using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;

namespace MetaTraderWorkerService.Services.Processors.BaseProcessors;

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

        var metaTraderOrderDto = CreateMetaTraderOpenTradeOrderDto(metaTraderOrder);

        var orderResponseDto = await _metaApiService.PlacePendingOrderAsync(metaTraderOrderDto);

        if (orderResponseDto != null)
        {
            await HandleResponseAsync(metaTraderOrder, orderResponseDto);
        }
        else
        {
            _logger.LogError($"Order failed: no response for Order ID {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }
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
            {
                SetValuesToMetaTraderOrderFromOpenByMarketResponseDto(metaTraderOrder, marketOrderResponse);
            }

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
        metaTraderOrder.Volume = 0.01m; // Example volume, you can customize this
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