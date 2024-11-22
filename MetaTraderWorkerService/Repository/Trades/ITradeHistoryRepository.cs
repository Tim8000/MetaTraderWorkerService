using TradeSignalsDatabaseUpdater.Models;

namespace MetaTraderWorkerService.Repository.Trades;

public interface ITradeHistoryRepository
{
    Task AddAsync(MetaTraderTradeHistory tradeHistory);
    Task<MetaTraderTradeHistory> GetByIdAsync(long id);
    Task<IEnumerable<MetaTraderTradeHistory>> GetAllAsync();
    Task UpdateAsync(MetaTraderTradeHistory tradeHistory);
    Task DeleteAsync(long id);
    Task<IEnumerable<MetaTraderTradeHistory>> GetBySymbolAsync(string symbol);
    Task<IEnumerable<MetaTraderTradeHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalProfitAsync(string symbol);
}