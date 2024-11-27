namespace MetaTraderWorkerService.Models;

public class MetaTraderTradeHistory
{
    public long Id { get; set; } // Deal ID
    public string? TradeHistoryId { get; set; }
    public string? Platform { get; set; } // Trading platform (e.g., "mt5")
    public string? Type { get; set; } // Deal type (e.g., "DEAL_TYPE_BUY", "DEAL_TYPE_SELL")
    public DateTime Time { get; set; } // Time of the deal
    public DateTime BrokerTime { get; set; } // Broker-reported time of the deal
    public decimal Commission { get; set; } // Commission for the deal
    public decimal Swap { get; set; } // Swap value for the deal
    public decimal Profit { get; set; } // Profit from the deal
    public string? Symbol { get; set; } // Trading symbol (e.g., "XAUUSD")
    public int Magic { get; set; } // Magic number for identifying trades
    public string? OrderId { get; set; } // Associated order ID
    public string? PositionId { get; set; } // Associated position ID
    public decimal Volume { get; set; } // Volume of the deal
    public decimal Price { get; set; } // Price of the deal
    public string? EntryType { get; set; } // Entry type (e.g., "DEAL_ENTRY_IN", "DEAL_ENTRY_OUT")
    public string? Reason { get; set; } // Reason for the deal (e.g., "DEAL_REASON_EXPERT")
    public decimal StopLoss { get; set; } // Stop-loss value
    public decimal TakeProfit { get; set; } // Take-profit value
    public string? BrokerComment { get; set; } // Optional comment from the broker
    public decimal AccountCurrencyExchangeRate { get; set; } // Account currency exchange rate
    public MetaTraderTrade? MetaTraderTrade { get; set; }
}