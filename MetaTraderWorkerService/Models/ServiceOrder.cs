using MetaTraderWorkerService.Enums;

namespace MetaTraderWorkerService.Models;

public class ServiceOrder
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } // "MOVE_STOPLOSS", "CLOSE_TRADE"
    public MetaTraderTrade MetaTraderTrade { get; set; } // MetaTrader trade ID
    public decimal? StopLoss { get; set; } // New StopLoss price
    public decimal? TakeProfit { get; set; } // Optional
    public ServiceOrderStatus Status { get; set; } // Pending, Executing, Executed, Failed
    public string? ErrorMessage { get; set; } // Error message on failure
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Creation timestamp
    public DateTime? ExecutedAt { get; set; } // Execution timestamp
    public string PositionId { get; set; } // MetaTrader position ID
    public MetaTraderOrder? MetaTraderOrder { get; set; }
}