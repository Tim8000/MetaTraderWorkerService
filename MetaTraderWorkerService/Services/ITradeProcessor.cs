using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums.Mt5Trades;

namespace MetaTraderWorkerService.Services;

public interface ITradeProcessor
{
    Task<List<TradeStatusResponseDto>> GetActiveTradesAsync();
}