using MetaTraderWorkerService.Models;
using Microsoft.EntityFrameworkCore;
using TradeOrderProcessor.Data;
using TradeOrderProcessor.Enums;

namespace MetaTraderWorkerService.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MetaTraderOrder>?> GetAllCreatedOrdersAsync()
    {
        return await _dbContext.MetaTraderOrders.Where(mo => mo.Status == OrderStatus.Created)
            .Include(o => o.InitialTradeSignal)
            .ToListAsync();
    }

    public async Task UpdateOrderAsync(MetaTraderOrder order)
    {
        _dbContext.Entry(order).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<MetaTraderOrder?> GetOrderByMetaTraderId(Guid initialTradeSignalId)
    {
        return await _dbContext.MetaTraderOrders
            .Include(o => o.InitialTradeSignal) // Eagerly load the InitialTradeSignal
            .FirstOrDefaultAsync(o => o.InitialTradeSignal.Id == initialTradeSignalId);
    }
}