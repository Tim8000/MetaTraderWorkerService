using MetaTraderWorkerService.Models;
using Microsoft.EntityFrameworkCore;
using TradeOrderProcessor.Models;

namespace TradeOrderProcessor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<MetaTraderInitialTradeSignal> MetaTraderInitialTradeSignals { get; set; }
    public DbSet<MetaTraderSignal> TradeSignals { get; set; }
    public DbSet<MetaTraderOrder> MetaTraderOrders { get; set; }
}