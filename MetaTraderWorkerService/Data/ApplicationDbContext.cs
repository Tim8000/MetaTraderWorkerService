using MetaTraderWorkerService.Models;
using Microsoft.EntityFrameworkCore;
using TradeOrderProcessor.Models;

namespace TradeOrderProcessor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<InitialTradeSignal> InitialTradeSignals { get; set; }
    public DbSet<TradeSignal> TradeSignals { get; set; }
    public DbSet<MetaTraderOrder> MetaTraderOrders { get; set; }
}