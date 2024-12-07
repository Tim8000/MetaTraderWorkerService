using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.ServiceOrderProcessors;

public class ServiceOrderMoveStopLossProcessor : IServiceOrderActionProcessor
{
    private readonly IMetaApiService _metaApiService;
    private readonly ILogger<ServiceOrderMoveStopLossProcessor> _logger;

    public ServiceOrderMoveStopLossProcessor(IMetaApiService metaApiService,
        ILogger<ServiceOrderMoveStopLossProcessor> logger)
    {
        _metaApiService = metaApiService;
        _logger = logger;
    }

    public string GetSupportedActionType()
    {
        return "MOVE_STOPLOSS";
    }

    public async Task ProcessAsync(ServiceOrder order)
    {
        var modifyOrderDto = new ModifyStopLossRequestDto
        {
            PositionId = order.MetaTraderTrade.Id,
            ActionType = "POSITION_MODIFY",
            StopLoss = decimal.Parse(order.StopLoss!.Value.ToString("G")),
            TakeProfit = decimal.Parse(order.TakeProfit!.Value.ToString("G"))
        };

        try
        {
            var response = await _metaApiService.ModifyStopLossAsync(modifyOrderDto);

            if (response?.NumericCode == TradeResultCode.Done)
            {
                order.Status = ServiceOrderStatus.Executed;
                order.ExecutedAt = DateTime.UtcNow;
            }
            else
            {
                order.Status = ServiceOrderStatus.Failed;
                order.ErrorMessage = response?.Message ?? "Unknown error during stop-loss modification.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing stop-loss for ServiceOrder: {order.Id}");
            order.Status = ServiceOrderStatus.Failed;
            order.ErrorMessage = ex.Message;
        }
    }
}