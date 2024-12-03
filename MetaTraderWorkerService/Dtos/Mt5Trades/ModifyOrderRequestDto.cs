using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class ModifyOrderRequestDto
{
    [JsonProperty("actionType")] public string ActionType { get; set; }

    [JsonProperty("positionId")] public string PositionId { get; set; } // The unique identifier of the position

    [JsonProperty("stopLoss")] public decimal StopLoss { get; set; } // The new Stop Loss price level

    [JsonProperty("takeProfit")] public decimal? TakeProfit { get; set; } // The new Take Profit price level (optional)

    [JsonProperty("comment")] public string Comment { get; set; } // A comment regarding the modification
}