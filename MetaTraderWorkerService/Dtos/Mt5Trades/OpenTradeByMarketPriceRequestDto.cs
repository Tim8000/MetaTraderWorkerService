using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class OpenTradeByMarketPriceRequestDto
{
    [JsonProperty("symbol")] public string Symbol { get; set; } // Trading symbol (e.g., "EURUSD")

    [JsonProperty("volume")] public decimal Volume { get; set; } // Trade volume

    [JsonProperty("actionType")]
    public string ActionType { get; set; } // Action type (e.g., "ORDER_TYPE_BUY" or "ORDER_TYPE_SELL")

    [JsonProperty("stopLoss")] public decimal? StopLoss { get; set; } // Stop-loss price (optional)

    [JsonProperty("takeProfit")] public decimal? TakeProfit { get; set; } // Take-profit price (optional)

    [JsonProperty("clientId")] public string ClientId { get; set; } // Client ID for tracking

    [JsonProperty("comment")] public string Comment { get; set; } // Optional comment
}