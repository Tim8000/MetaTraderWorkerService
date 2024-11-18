using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class TradeHistoryResponseDto
{
    [JsonProperty("id")]
    public string Id { get; set; } // Deal ID

    [JsonProperty("platform")]
    public string Platform { get; set; } // Trading platform (e.g., "mt5")

    [JsonProperty("type")]
    public string Type { get; set; } // Deal type (e.g., "DEAL_TYPE_BUY", "DEAL_TYPE_SELL")

    [JsonProperty("time")]
    public DateTime Time { get; set; } // Time of the deal

    [JsonProperty("brokerTime")]
    public DateTime BrokerTime { get; set; } // Broker-reported time of the deal

    [JsonProperty("commission")]
    public decimal Commission { get; set; } // Commission for the deal

    [JsonProperty("swap")]
    public decimal Swap { get; set; } // Swap value for the deal

    [JsonProperty("profit")]
    public decimal Profit { get; set; } // Profit from the deal

    [JsonProperty("symbol")]
    public string Symbol { get; set; } // Trading symbol (e.g., "XAUUSD")

    [JsonProperty("magic")]
    public int Magic { get; set; } // Magic number for identifying trades

    [JsonProperty("orderId")]
    public string OrderId { get; set; } // Associated order ID

    [JsonProperty("positionId")]
    public string PositionId { get; set; } // Associated position ID

    [JsonProperty("volume")]
    public decimal Volume { get; set; } // Volume of the deal

    [JsonProperty("price")]
    public decimal Price { get; set; } // Price of the deal

    [JsonProperty("entryType")]
    public string EntryType { get; set; } // Entry type (e.g., "DEAL_ENTRY_IN", "DEAL_ENTRY_OUT")

    [JsonProperty("reason")]
    public string Reason { get; set; } // Reason for the deal (e.g., "DEAL_REASON_EXPERT")

    [JsonProperty("stopLoss")]
    public decimal StopLoss { get; set; } // Stop-loss value

    [JsonProperty("takeProfit")]
    public decimal TakeProfit { get; set; } // Take-profit value

    [JsonProperty("brokerComment")]
    public string BrokerComment { get; set; } // Optional comment from the broker

    [JsonProperty("accountCurrencyExchangeRate")]
    public decimal AccountCurrencyExchangeRate { get; set; } // Account currency exchange rate
}