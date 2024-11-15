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
    private readonly string _accountId;

    public OrderProcessor(IOrderRepository orderRepository, IMetaApiService metaApiService,
        ILogger<OrderProcessor> logger, IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
        _logger = logger;
        _accountId = configuration["MetaApi:ProvisioningProfileId"];
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
                case ActionType.ORDER_TYPE_SELL_LIMIT:
                    await ProcessTradeOpening(metaTraderOrder);
                    continue;
                case ActionType.ORDER_TYPE_BUY_LIMIT:
                    await ProcessTradeOpening(metaTraderOrder);
                    continue;
                case ActionType.ORDER_CANCEL:
                    await ProcessCancelOrder(metaTraderOrder);
                    continue;
                default:
                    continue;
            }
        }
    }

    private async Task ProcessCancelOrder(MetaTraderOrder metaTraderOrder)
    {
        var order = await _orderRepository.GetOrderByInitialTradeSignalId(metaTraderOrder.MetaTraderInitialTradeSignal.Id);

        if (order == null)
            throw new Exception($"Order {metaTraderOrder.MetaTraderOrderId} not found");

        var cancelOrderDto = new CancelOrderDto()
        {
            ActionType = metaTraderOrder.ActionType.ToString(),
            OrderId = order.MetaTraderOrderId,
        };

        // TODO: Handle response.
        var response = await _metaApiService.PlaceCancelOrderAsync(cancelOrderDto);
        order.Status = OrderStatus.Canceled;
        order.OrderState = OrderState.ORDER_STATE_CANCELED;
        await _orderRepository.UpdateOrderAsync(order);
        metaTraderOrder.Status = OrderStatus.Executed;
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
    }

    private async Task ProcessTradeOpening(MetaTraderOrder metaTraderOrder)
    {
        SetOrderConfigurations(metaTraderOrder);
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        var metaTraderOrderDto = CreateMetaTraderOpenTradeOrderDto(metaTraderOrder);

        var orderResponseDto = await _metaApiService.PlacePendingOrderAsync(metaTraderOrderDto);

        if (orderResponseDto != null)
        {
            if (orderResponseDto.NumericCode == TradeResultCode.Done)
            {
                SetValuesToMetaTraderOrderFromResponseDto(metaTraderOrder, orderResponseDto);
                metaTraderOrder.Status = OrderStatus.SentToMetaTrader;

                _logger.LogInformation(
                    $"Order was created successfuly, orderId = {metaTraderOrder.MetaTraderOrderId}, metaTraderStringCode = {metaTraderOrder.MetaTraderStringCode}, metatraderOrderId = {metaTraderOrder.MetaTraderOrderId}");
            }
            else
            {
                metaTraderOrder.Status = OrderStatus.Failed;
                metaTraderOrder.MetaTraderStringCode = orderResponseDto.StringCode;
                metaTraderOrder.MetaTraderTradeResultCode = orderResponseDto.NumericCode;
                metaTraderOrder.MetaTraderMessage = orderResponseDto.Message;
            }

            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }
    }

    private void SetOrderConfigurations(MetaTraderOrder metaTraderOrder)
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
        metaTraderOrder.Slippage = 1;
        // metaTraderOrder.ClientId = _accountId; // TODO: Implement, guid does not work.
    }

    private static MetaTraderOpenTradeOrderRequestDto CreateMetaTraderOpenTradeOrderDto(MetaTraderOrder metaTraderOrder)
    {
        var metaTraderOrderDto = new MetaTraderOpenTradeOrderRequestDto
        {
            Symbol = metaTraderOrder.Symbol,
            Volume = metaTraderOrder.Volume,
            ActionType = metaTraderOrder.ActionType.Value.ToString(),
            OpenPrice = metaTraderOrder.OpenPrice,
            StopLoss = metaTraderOrder.StopLoss,
            TakeProfit = metaTraderOrder.TakeProfit,
            Slippage = metaTraderOrder.Slippage,
            Magic = metaTraderOrder.Magic,
            ClientId = metaTraderOrder.ClientId,
            Comment = metaTraderOrder.Comment,
            StopLossUnits = metaTraderOrder.StopLossUnits,
            TakeProfitUnits = metaTraderOrder.TakeProfitUnits,
            StopPriceBase = metaTraderOrder.StopPriceBase,
        };
        return metaTraderOrderDto;
    }

    private static void SetValuesToMetaTraderOrderFromResponseDto(MetaTraderOrder metaTraderOrder,
        MetaTraderOpenTradeOrderResponseDto openTradeOrderResponseDto)
    {
        metaTraderOrder.MetaTraderTradeStartTime = openTradeOrderResponseDto.TradeStartTime;
        metaTraderOrder.MetaTraderTradeExecutionTime = openTradeOrderResponseDto.TradeExecutionTime;
        metaTraderOrder.MetaTraderOrderId = openTradeOrderResponseDto.OrderId;
        metaTraderOrder.MetaTraderStringCode = openTradeOrderResponseDto.StringCode;
        metaTraderOrder.MetaTraderMessage = openTradeOrderResponseDto.Message;

        if (Enum.IsDefined(typeof(TradeResultCode), openTradeOrderResponseDto.NumericCode))
        {
            metaTraderOrder.MetaTraderTradeResultCode = (TradeResultCode)openTradeOrderResponseDto.NumericCode;
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