using MetaTraderWorkerService.Data;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using Microsoft.EntityFrameworkCore;

namespace MetaTraderWorkerService.Repository.Orders;

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
            .Include(o => o.MetaTraderInitialTradeSignal)
            .Include(o => o.Trade)
            .ToListAsync();
    }

    public async Task UpdateOrderAsync(MetaTraderOrder order)
    {
        _dbContext.Entry(order).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<MetaTraderOrder?> GetOrderByInitialTradeSignalId(Guid initialTradeSignalId)
    {
        return await _dbContext.MetaTraderOrders
            .Include(o => o.MetaTraderInitialTradeSignal) // Eagerly load the InitialTradeSignal
            .FirstOrDefaultAsync(o =>
                o.MetaTraderInitialTradeSignal.Id == initialTradeSignalId && o.Status != OrderStatus.Canceled);
    }

    public async Task<List<MetaTraderOrder>> GetSentToMetaTraderOrdersAsync()
    {
        return await _dbContext.MetaTraderOrders.Where(mo => mo.Status == OrderStatus.SentToMetaTrader).ToListAsync();
        // return await _dbContext.MetaTraderOrders.Where(mo => mo.MetaTraderStringCode == "TRADE_RETCODE_DONE").ToListAsync();
    }

    public async Task<List<MetaTraderOrder>> GetPlacedOrdersAsync()
    {
        return await _dbContext.MetaTraderOrders.Where(mo => mo.OrderState == OrderState.ORDER_STATE_PLACED)
            .ToListAsync();
    }

    public async Task<MetaTraderOrder?> GetOrderByMagicAndSymbolAsync(int magic, string symbol)
    {
        return await _dbContext.MetaTraderOrders
            .Include(o => o.Trade) // Include associated trade
            .FirstOrDefaultAsync(o => o.Magic == magic && o.Symbol == symbol);
    }

    public async Task<MetaTraderOrder> GetOrderByMetaTraderOrderId(string metaTraderOrderId, string tradeSymbol)
    {
        return await _dbContext.MetaTraderOrders
            .Include(o => o.Trade) // Include associated trade
            .FirstOrDefaultAsync(o => o.MetaTraderOrderId == metaTraderOrderId.ToString() && o.Symbol == tradeSymbol);
    }

    public async Task<List<MetaTraderOrder>> GetOpenedOrdersFromOneInitialSignal(long messageId)
    {
        return await _dbContext.MetaTraderOrders
            .Include(o => o.Trade) // Ensure Trade navigation property is included
            .Where(o => o.MetaTraderInitialTradeSignal.MessageId == messageId &&
                        o.Status == OrderStatus.SentToMetaTrader)
            .ToListAsync();
    }
}