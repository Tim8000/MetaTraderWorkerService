using MetaTraderWorkerService.Enums;
using TradeOrderProcessor.Enums;
using TradeOrderProcessor.Models;

namespace MetaTraderWorkerService.Models;

public class InitialTradeSignal
{
    public Guid Id { get; set; }
    public required OrderActionType Action { get; set; }
    public required string ActionDescription { get; set; } // "BUY", "SELL", etc.
    public required string Pair { get; set; } // E.g., "GOLD"
    public decimal? CommandPrice { get; set; } // Entry price
    public decimal? Sl { get; set; } // Stop-loss
    public decimal? Tp { get; set; } // Take-profit
    public required string MessageText { get; set; } // Original message text
    public long? MessageId { get; set; } // Unique ID for this command
    public string? AdditionalInfo { get; set; } // "FOLLOW MONEY MANAGEMENT" or similar instructions
    public SignalCommandCode? SignalCommand { get; set; }
    public DateTime? MessageDate { get; set; }
    public DateTime? InitialMessageDate { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsInitialSignal { get; set; }
    public long ChatId { get; set; }
    public string GroupName { get; set; }
    public string Status { get; set; }
    public TradeSignalStatusCode StatusCode { get; set; }
    public List<TradeSignal>? TradeSignals { get; set; }
}