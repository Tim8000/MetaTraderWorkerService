using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MetaTraderWorkerService.Data;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Repository.Trades;

public class TradeRepository : ITradeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TradeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MetaTraderTrade?> GetTradeByIdAsync(string tradeId)
    {
        return await _dbContext.MetaTraderTrades
            .Include(t => t.MetaTraderOrders)
            .FirstOrDefaultAsync(t => t.Id == tradeId);
    }

    public async Task<IEnumerable<MetaTraderTrade>> GetAllTradesAsync()
    {
        return await _dbContext.MetaTraderTrades
            .Include(t => t.MetaTraderOrders)
            .ToListAsync();
    }

    public async Task AddTradeAsync(MetaTraderTrade trade)
    {
        await _dbContext.MetaTraderTrades.AddAsync(trade);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTradeAsync(MetaTraderTrade trade)
    {
        _dbContext.MetaTraderTrades.Update(trade);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTradeAsync(string tradeId)
    {
        var trade = await GetTradeByIdAsync(tradeId);
        if (trade != null)
        {
            _dbContext.MetaTraderTrades.Remove(trade);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MetaTraderTrade>> GetTradesByMagicAsync(int magic)
    {
        return await _dbContext.MetaTraderTrades
            .Where(t => t.Magic == magic)
            .ToListAsync();
    }

    public async Task<IEnumerable<MetaTraderTrade>> GetTradesByStatusAsync(TradeStatus status)
    {
        return await _dbContext.MetaTraderTrades
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<List<MetaTraderTrade>> GetAllOpenedTradesAsync()
    {
        return await _dbContext.MetaTraderTrades
            .Where(t => t.Status == TradeStatus.Open && t.State == TradeState.Active)
            .Include(t => t.ServiceOrders)
            .Include(t => t.MetaTraderOrders)
            .ThenInclude(o => o.MetaTraderInitialTradeSignal)
            .ToListAsync();
    }
}