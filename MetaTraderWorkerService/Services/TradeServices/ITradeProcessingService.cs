using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Services.TradeServices;

public interface ITradeProcessingService
{
    Task ProcessMovingStopLossAsync(MetaTraderTrade trade);
    Task RemovePendingLimitOrderIfProfitableAsync();
    Task ProcessMoveStopLossToOpenPrice(MetaTraderTrade trade);
}