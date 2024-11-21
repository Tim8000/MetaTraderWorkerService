using MetaTraderWorkerService.Dtos.Mt5Trades;

namespace MetaTraderWorkerService.Services.TradeServices;

public interface ITradeProcessor
{
    Task<List<TradeStatusResponseDto>> GetActiveTradesAsync();
    Task ProcessActiveTradesAsync();
    Task ProcessTradeHistoryAsync();
    Task ProcessTryToCloseTradesAsync();
}