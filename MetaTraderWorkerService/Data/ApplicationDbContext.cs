using MetaTraderWorkerService.Models;
using Microsoft.EntityFrameworkCore;
using TradeSignalsDatabaseUpdater.Models;

namespace MetaTraderWorkerService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<MetaTraderInitialTradeSignal> MetaTraderInitialTradeSignals { get; set; }
    public DbSet<MetaTraderSignal> TradeSignals { get; set; }
    public DbSet<MetaTraderOrder> MetaTraderOrders { get; set; }
    public DbSet<MetaTraderTrade> MetaTraderTrades { get; set; }
    public DbSet<MetaTraderTradeHistory> MetaTraderTradeHistories { get; set; }
}