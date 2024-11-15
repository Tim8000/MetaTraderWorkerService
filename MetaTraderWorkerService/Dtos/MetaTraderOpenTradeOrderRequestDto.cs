using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos;

// public class MetaTraderOpenTradeOrderRequestDto
// {
//     public string? Symbol { get; set; } // Symbol to trade, e.g., "XAUUSD"
//     public decimal? Volume { get; set; } // Order volume, e.g., 0.1
//     public string? ActionType { get; set; } // Action type, e.g., "ORDER_TYPE_SELL_LIMIT"
//     public decimal? OpenPrice { get; set; } // Open price for pending orders
//     public decimal? StopLoss { get; set; } // Stop-loss price
//     public decimal? TakeProfit { get; set; } // Take-profit price
//     public int? Slippage { get; set; } // Slippage in points
//     public string? ClientId { get; set; } // Client-assigned ID, e.g., "123"
//     public string? Comment { get; set; } // Order comment
//     public string? StopLossUnits { get; set; } // Stop-loss units, e.g., "ABSOLUTE_PRICE"
//     public string? TakeProfitUnits { get; set; } // Take-profit units, e.g., "ABSOLUTE_PRICE"
//     public string? StopPriceBase { get; set; } // Base price for calculating SL/TP, e.g., "OPEN_PRICE"
//     public int? Magic { get; set; } // Magic number for MetaTrader
//     public string? ExpirationType { get; set; } // Expiration type, e.g., "ORDER_TIME_SPECIFIED"
//     public object? Expiration { get; set; }
// }

public class MetaTraderOpenTradeOrderRequestDto
{
    [JsonProperty("symbol")] public string? Symbol { get; set; } // Symbol to trade, e.g., "XAUUSD"

    [JsonProperty("volume")] public decimal? Volume { get; set; } // Order volume, e.g., 0.1

    [JsonProperty("actionType")] public string? ActionType { get; set; } // Action type, e.g., "ORDER_TYPE_SELL_LIMIT"

    [JsonProperty("openPrice")] public decimal? OpenPrice { get; set; } // Open price for pending orders

    [JsonProperty("stopLoss")] public decimal? StopLoss { get; set; } // Stop-loss price

    [JsonProperty("takeProfit")] public decimal? TakeProfit { get; set; } // Take-profit price

    [JsonProperty("slippage")] public int? Slippage { get; set; } // Slippage in points

    [JsonProperty("clientId")] public string? ClientId { get; set; } // Client-assigned ID, e.g., "123"

    [JsonProperty("comment")] public string? Comment { get; set; } // Order comment

    [JsonProperty("stopLossUnits")]
    public string? StopLossUnits { get; set; } // Stop-loss units, e.g., "ABSOLUTE_PRICE"

    [JsonProperty("takeProfitUnits")]
    public string? TakeProfitUnits { get; set; } // Take-profit units, e.g., "ABSOLUTE_PRICE"

    [JsonProperty("stopPriceBase")]
    public string? StopPriceBase { get; set; } // Base price for calculating SL/TP, e.g., "OPEN_PRICE"

    [JsonProperty("magic")] public long? Magic { get; set; } // Magic number for MetaTrader

    // [JsonProperty("expirationType")]
    // public string? ExpirationType { get; set; } // Expiration type, e.g., "ORDER_TIME_SPECIFIED"

    // [JsonProperty("expiration")]
    // public object? Expiration { get; set; }
}