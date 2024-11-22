using MetaTraderWorkerService.Data;
using Microsoft.EntityFrameworkCore;
using TradeSignalsDatabaseUpdater.Models;

namespace MetaTraderWorkerService.Repository.Trades;

public class TradeHistoryRepository : ITradeHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public TradeHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MetaTraderTradeHistory tradeHistory)
    {
        if (tradeHistory == null) throw new ArgumentNullException(nameof(tradeHistory));
        await _context.MetaTraderTradeHistories.AddAsync(tradeHistory);
        await _context.SaveChangesAsync();
    }

    public async Task<MetaTraderTradeHistory> GetByIdAsync(long id)
    {
        return await _context.MetaTraderTradeHistories.FindAsync(id);
    }

    public async Task<IEnumerable<MetaTraderTradeHistory>> GetAllAsync()
    {
        return await _context.MetaTraderTradeHistories.ToListAsync();
    }

    public async Task UpdateAsync(MetaTraderTradeHistory tradeHistory)
    {
        if (tradeHistory == null) throw new ArgumentNullException(nameof(tradeHistory));
        _context.MetaTraderTradeHistories.Update(tradeHistory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var tradeHistory = await GetByIdAsync(id);
        if (tradeHistory == null) throw new KeyNotFoundException($"TradeHistory with ID {id} not found.");
        _context.MetaTraderTradeHistories.Remove(tradeHistory);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MetaTraderTradeHistory>> GetBySymbolAsync(string symbol)
    {
        return await _context.MetaTraderTradeHistories
            .Where(t => t.Symbol == symbol)
            .ToListAsync();
    }

    public async Task<IEnumerable<MetaTraderTradeHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.MetaTraderTradeHistories
            .Where(t => t.Time >= startDate && t.Time <= endDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalProfitAsync(string symbol)
    {
        return await _context.MetaTraderTradeHistories
            .Where(t => t.Symbol == symbol)
            .SumAsync(t => t.Profit);
    }
}