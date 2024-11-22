using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Repository.Trades;

public interface ITradeRepository
{
    Task<MetaTraderTrade?> GetTradeByIdAsync(string tradeId);
    Task<IEnumerable<MetaTraderTrade>> GetAllTradesAsync();
    Task AddTradeAsync(MetaTraderTrade trade);
    Task UpdateTradeAsync(MetaTraderTrade trade);
    Task DeleteTradeAsync(string tradeId);
    Task<IEnumerable<MetaTraderTrade>> GetTradesByMagicAsync(int magic);
    Task<IEnumerable<MetaTraderTrade>> GetTradesByStatusAsync(TradeStatus status);
    Task<List<MetaTraderTrade>> GetAllOpenedTradesAsync();
}