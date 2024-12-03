using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class OpenTradeByMarketPriceResponseDto
{
    [JsonProperty("orderId")] public string OrderId { get; set; } // Unique identifier for the market order

    [JsonProperty("positionId")] public string PositionId { get; set; } // Position identifier for the trade

    [JsonProperty("numericCode")] public int NumericCode { get; set; } // Numeric result code (e.g., 10009 for success)

    [JsonProperty("stringCode")]
    public string StringCode { get; set; } // String representation of the trade result (e.g., "TRADE_RETCODE_DONE")

    [JsonProperty("message")] public string Message { get; set; } // Optional message from the API

    [JsonProperty("tradeExecutionTime")]
    public DateTime? TradeExecutionTime { get; set; } // Timestamp when the trade was executed

    [JsonProperty("tradeStartTime")] public DateTime? TradeStartTime { get; set; } // Timestamp when the trade started
}