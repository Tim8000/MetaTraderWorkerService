using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors;

public class StopLossProcessor : IOrderActionProcessor
{
    private readonly IMetaApiService _metaApiService;
    private readonly ILogger<StopLossProcessor> _logger;
    private readonly IOrderRepository _orderRepository;

    public StopLossProcessor(IMetaApiService metaApiService, ILogger<StopLossProcessor> logger, IOrderRepository orderRepository)
    {
        _metaApiService = metaApiService;
        _logger = logger;
        _orderRepository = orderRepository;
    }

    public async Task ProcessAsync(MetaTraderOrder metaTraderOrder)
    {
        switch (metaTraderOrder.SignalCommandCode)
        {
            case SignalCommandCode.MoveStopLossToPrice:
                await ProcessMoveStopLossToPrice(metaTraderOrder);
                break;
            case SignalCommandCode.MoveStopLossToBreakEven:
                await ProcessMoveStopLossToBreakEven(metaTraderOrder);
                break;
            default:
                _logger.LogWarning($"Unsupported SignalCommandCode: {metaTraderOrder.SignalCommandCode}");
                break;
        }
    }

    private async Task ProcessMoveStopLossToPrice(MetaTraderOrder metaTraderOrder)
    {
        if (metaTraderOrder.Trade == null)
        {
            _logger.LogError($"No active trade found for Order ID: {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "No active trade associated with this order.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        var modifyOrderDto = new ModifyStopLossRequestDto
        {
            ActionType = ActionType.POSITION_MODIFY.ToString(),
            PositionId = metaTraderOrder.Trade.Id,
            StopLoss = metaTraderOrder.StopLoss.Value,
            TakeProfit = metaTraderOrder.Trade.TakeProfit
        };

        var response = await _metaApiService.ModifyStopLossAsync(modifyOrderDto);
        if (response?.NumericCode == TradeResultCode.Done)
        {
            metaTraderOrder.Trade.StopLoss = metaTraderOrder.StopLoss.Value;
            metaTraderOrder.Status = OrderStatus.Executed;
        }
        else
        {
            metaTraderOrder.Status = OrderStatus.Failed;
        }

        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
    }

    private async Task ProcessMoveStopLossToBreakEven(MetaTraderOrder metaTraderOrder)
    {
        if (metaTraderOrder.Trade == null)
        {
            _logger.LogError($"No active trade found for Order ID: {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "No active trade associated with this order.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        var breakEvenPrice = metaTraderOrder.Trade.OpenPrice;
        if (metaTraderOrder.Trade.StopLoss == breakEvenPrice)
        {
            metaTraderOrder.Status = OrderStatus.Executed;
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        var modifyOrderDto = new ModifyStopLossRequestDto
        {
            ActionType = ActionType.POSITION_MODIFY.ToString(),
            PositionId = metaTraderOrder.Trade.Id,
            StopLoss = breakEvenPrice,
            TakeProfit = metaTraderOrder.Trade.TakeProfit
        };

        var response = await _metaApiService.ModifyStopLossAsync(modifyOrderDto);
        if (response?.NumericCode == TradeResultCode.Done)
        {
            metaTraderOrder.Trade.StopLoss = breakEvenPrice;
            metaTraderOrder.Status = OrderStatus.Executed;
        }
        else
        {
            metaTraderOrder.Status = OrderStatus.Failed;
        }

        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
    }
}
