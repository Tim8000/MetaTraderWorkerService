using MetaTraderWorkerService.Enums;

namespace MetaTraderWorkerService.Models;

public class MetaTraderOrder
{
    public Guid Id { get; set; }

    // Core fields
    public MetaTraderSignal? TradeSignal { get; set; } // Link to the originating trade signal

    public required MetaTraderInitialTradeSignal
        MetaTraderInitialTradeSignal { get; set; } // Link to the originating initial trade signal

    public OrderStatus Status { get; set; } // Order status, default to "Created"
    public OrderState? OrderState { get; set; }
    public ActionType? ActionType { get; set; } // Action type, e.g., "ORDER_TYPE_SELL_LIMIT"
    public SignalCommandCode? SignalCommandCode { get; set; }

    // MetaTrader-specific fields based on request
    public string? Symbol { get; set; } // Symbol to trade, e.g., "XAUUSD"
    public decimal? Volume { get; set; } // Order volume, e.g., 0.1
    public decimal? OpenPrice { get; set; } // Open price for pending orders
    public decimal? ClosePrice { get; set; }
    public decimal? StopLoss { get; set; } // Stop-loss price
    public decimal? TakeProfit { get; set; } // Take-profit price
    public int? Slippage { get; set; } // Slippage in points
    public string? ClientId { get; set; } // Client-assigned ID, e.g., "123"
    public string? Comment { get; set; } // Order comment
    public string? StopLossUnits { get; set; } // Stop-loss units, e.g., "ABSOLUTE_PRICE"
    public string? TakeProfitUnits { get; set; } // Take-profit units, e.g., "ABSOLUTE_PRICE"
    public string? StopPriceBase { get; set; } // Base price for calculating SL/TP, e.g., "OPEN_PRICE"
    public int? Magic { get; set; } // Magic number for MetaTrader
    public string? ExpirationType { get; set; } // Expiration type, e.g., "ORDER_TIME_SPECIFIED"
    public DateTime? ExpirationTime { get; set; } // Expiration time if applicable

    // Fields to be updated after order placement
    public DateTime CreateDate { get; set; } = DateTime.UtcNow; // Date when the order was created
    public DateTime? MetaTraderTradeStartTime { get; set; } // Start time of the trade
    public DateTime? MetaTraderTradeExecutionTime { get; set; } // Execution time of the trade
    public string? Pair { get; set; }
    public MetaTraderTrade? Trade { get; set; }
    public string? MetaTraderOrderId { get; set; } // MetaTrader order ID
    public string? MetaTraderStringCode { get; set; }
    public string? MetaTraderMessage { get; set; }
    public TradeResultCode? MetaTraderTradeResultCode { get; set; }
}