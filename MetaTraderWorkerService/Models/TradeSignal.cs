using MetaTraderWorkerService.Models;
using TradeOrderProcessor.Enums;

namespace TradeOrderProcessor.Models;

public class TradeSignal
{
    public Guid Id { get; set; }
    public required InitialTradeSignal InitialTradeSignal { get; set; }
    public required string Action { get; set; } // "BUY", "SELL", etc.
    public required string Pair { get; set; } // E.g., "GOLD"
    public decimal? CommandPrice { get; set; } // Entry price
    public decimal? Sl { get; set; } // Stop-loss
    public decimal? Tp { get; set; } // Take-profit
    public required string MessageText { get; set; } // Original message text
    public long? MessageId { get; set; } // Unique ID for this command
    public long? ParentMessageId { get; set; } // ID of the initial command if this is a reply
    public string? AdditionalInfo { get; set; } // "FOLLOW MONEY MANAGEMENT" or similar instructions
    public SignalCommandCode? SignalCommand { get; set; }
    public DateTime? MessageDate { get; set; }
    public DateTime? InitialMessageDate { get; set; }
    public DateTime? CreateDate { get; set; }
    public long ChatId { get; set; }
    public required string GroupName { get; set; }
    public required string Status { get; set; }
    public int StatusCode { get; set; }
}