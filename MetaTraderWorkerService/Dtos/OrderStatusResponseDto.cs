using MetaTraderWorkerService.Enums;

namespace MetaTraderWorkerService.Dtos;

public class OrderStatusResponseDto
{
    public string Id { get; set; } // Order ID
    public string Platform { get; set; } // Platform type, e.g., "mt5"
    public string Type { get; set; } // Order type, e.g., "ORDER_TYPE_BUY_LIMIT"
    public OrderStatus State { get; set; } // Order state, e.g., "ORDER_STATE_PLACED"
    public string Symbol { get; set; } // Trading symbol, e.g., "XAUUSD"
    public int Magic { get; set; } // Magic number
    public DateTime Time { get; set; } // Order time in UTC
    public DateTime BrokerTime { get; set; } // Broker time
    public decimal OpenPrice { get; set; } // Open price
    public decimal Volume { get; set; } // Order volume
    public decimal CurrentVolume { get; set; } // Current order volume
    public string PositionId { get; set; } // Position ID
    public string Reason { get; set; } // Reason for the order, e.g., "ORDER_REASON_EXPERT"
    public string FillingMode { get; set; } // Filling mode, e.g., "ORDER_FILLING_RETURN"
    public string ExpirationType { get; set; } // Expiration type, e.g., "ORDER_TIME_GTC"
    public decimal? StopLoss { get; set; } // Stop-loss price
    public decimal? TakeProfit { get; set; } // Take-profit price
    public long UpdateSequenceNumber { get; set; } // Update sequence number
    public decimal CurrentPrice { get; set; } // Current price
}