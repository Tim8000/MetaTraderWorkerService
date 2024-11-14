using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository;
using TradeOrderProcessor.Enums;

namespace MetaTraderWorkerService.Services;

public class OrderProcessor : IOrderProcessor
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(IOrderRepository orderRepository, IMetaApiService metaApiService,
        ILogger<OrderProcessor> logger)
    {
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
        _logger = logger;
    }

    public async Task ProcessCreatedOrdersAsync()
    {
        var createdOrders = await _orderRepository.GetAllCreatedOrdersAsync();

        if (createdOrders == null)
            return;


        foreach (var metaTraderOrder in createdOrders)
        {
            switch (metaTraderOrder.ActionType)
            {
                case OrderActionType.ORDER_TYPE_SELL_LIMIT:
                    await ProcessTradeOpening(metaTraderOrder);
                    continue;
                case OrderActionType.ORDER_TYPE_BUY_LIMIT:
                    await ProcessTradeOpening(metaTraderOrder);
                    continue;
                case OrderActionType.ORDER_CANCEL:
                    await ProcessCancelOrder(metaTraderOrder);
                    continue;
                default:
                    continue;
            }
        }
    }

    private async Task ProcessCancelOrder(MetaTraderOrder metaTraderOrder)
    {
        var order = await _orderRepository.GetOrderByInitialTradeSignalId(metaTraderOrder.InitialTradeSignal.Id);

        if (order == null)
            throw new Exception($"Order {metaTraderOrder.MetaTraderOrderId} not found");

        var cancelOrderDto = new CancelOrderDto()
        {
            ActionType = metaTraderOrder.ActionType.ToString(),
            OrderId = order.MetaTraderOrderId,
        };

        var response = await _metaApiService.PlaceCancelOrderAsync(cancelOrderDto);
        order.Status = OrderStatus.Canceled;
        await _orderRepository.UpdateOrderAsync(order);
        metaTraderOrder.Status = OrderStatus.Executed;
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
    }

    private async Task ProcessTradeOpening(MetaTraderOrder metaTraderOrder)
    {
        var expiration = metaTraderOrder.ExpirationType == "ORDER_TIME_GTC"
            ? null
            : new
            {
                type = metaTraderOrder.ExpirationType,
                time = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
                // time = "2024-11-14T23:59:59Z"
            };

        // metaTraderOrder.Volume = CalculateVolumeForDollarAmount(600, metaTraderOrder.OpenPrice!.Value);
        metaTraderOrder.Volume = (decimal)0.01;
        metaTraderOrder.Status = OrderStatus.Pending;

        await _orderRepository.UpdateOrderAsync(metaTraderOrder);

        var metaTraderOrderDto = new MetaTraderOrderRequestDto
        {
            Symbol = metaTraderOrder.Symbol,
            Volume = metaTraderOrder.Volume,
            ActionType = metaTraderOrder.ActionType.Value.ToString(),
            OpenPrice = metaTraderOrder.OpenPrice,
            StopLoss = metaTraderOrder.StopLoss,
            TakeProfit = metaTraderOrder.TakeProfit,
            Slippage = metaTraderOrder.Slippage,
            ClientId = metaTraderOrder.ClientId,
            Comment = metaTraderOrder.Comment,
            StopLossUnits = metaTraderOrder.StopLossUnits,
            TakeProfitUnits = metaTraderOrder.TakeProfitUnits,
            StopPriceBase = metaTraderOrder.StopPriceBase,
            Magic = metaTraderOrder.Magic
            // ExpirationType = "ORDER_TIME_GTC",
            // Expiration = expiration
        };

        var orderResponseDto = await _metaApiService.PlacePendingOrderAsync(metaTraderOrderDto);

        if (orderResponseDto != null)
        {
            if (orderResponseDto.NumericCode == (int)TradeResultCode.Done)
            {
                UpdateMetaTraderOrderFromResponseDto(metaTraderOrder, orderResponseDto);
                metaTraderOrder.Status = OrderStatus.SentToMetaTrader;
                _logger.LogInformation(
                    $"Order was created successfuly, orderId = {metaTraderOrder.MetaTraderOrderId}, metaTraderStringCode = {metaTraderOrder.MetaTraderStringCode}, metatraderOrderId = {metaTraderOrder.MetaTraderOrderId}");
            }
            else
            {
                metaTraderOrder.Status = OrderStatus.Failed;
            }

            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }
    }

    private static void UpdateMetaTraderOrderFromResponseDto(MetaTraderOrder metaTraderOrder,
        MetaTraderOrderResponseDto orderResponseDto)
    {
        metaTraderOrder.MetaTraderTradeStartTime = orderResponseDto.TradeStartTime;
        metaTraderOrder.MetaTraderTradeExecutionTime = orderResponseDto.TradeExecutionTime;
        metaTraderOrder.MetaTraderOrderId = orderResponseDto.OrderId;
        metaTraderOrder.MetaTraderStringCode = orderResponseDto.StringCode;
        metaTraderOrder.MetaTraderMessage = orderResponseDto.Message;

        if (Enum.IsDefined(typeof(TradeResultCode), orderResponseDto.NumericCode))
        {
            metaTraderOrder.MetaTraderTradeResultCode = (TradeResultCode)orderResponseDto.NumericCode;
        }
        else
        {
            metaTraderOrder.MetaTraderTradeResultCode =
                TradeResultCode.Unknown; // Assuming you have an "Unknown" enum value
        }
    }

    public decimal CalculateVolumeForDollarAmount(decimal dollarAmount, decimal currentPrice, decimal minVolume = 0.01m,
        decimal maxVolume = 0.1m)
    {
        // Calculate initial volume
        var volume = dollarAmount / currentPrice;

        // Apply a multiplier if the volume is out of the desired range
        if (volume < minVolume)
            volume = minVolume;
        else if (volume > maxVolume) volume = maxVolume;

        return Math.Round(volume, 2); // Rounds to two decimal places
    }
}