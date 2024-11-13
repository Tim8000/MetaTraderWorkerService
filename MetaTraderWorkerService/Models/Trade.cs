using TradeOrderProcessor.Enums;

namespace MetaTraderWorkerService.Models;

public class Trade
{
    public Guid Id { get; set; }
    public required List<MetaTraderOrder> Orders { get; set; } // The originating order
    public required string Action { get; set; } // "BUY", "SELL", etc.
    public required string Pair { get; set; } // E.g., "GOLD"
    public decimal EntryPrice { get; set; } // Entry price of the trade
    public decimal? Sl { get; set; } // Stop-loss
    public decimal? Tp { get; set; } // Take-profit
    public decimal? CurrentPrice { get; set; } // Current market price for monitoring
    public DateTime TradeDate { get; set; } // Date the trade was opened
    public DateTime? CloseDate { get; set; } // Date the trade was closed
    public TradeStatus Status { get; set; } // Enum for internal use
    public string StatusDescription => Status.ToString(); // Human-readable status
    public int StatusCode => (int)Status; // Status code as integer for additional referencing
}