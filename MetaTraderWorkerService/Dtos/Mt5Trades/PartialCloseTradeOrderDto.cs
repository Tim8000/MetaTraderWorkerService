using MetaTraderWorkerService.Enums;
using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos;

using Newtonsoft.Json;
using MetaTraderWorkerService.Enums;

public class PartialCloseTradeOrderDto
{
    [JsonProperty("positionId")]
    public string PositionId { get; set; } // Required field based on the error message

    [JsonProperty("volume")]
    public decimal Volume { get; set; } // Volume to partially close the position

    [JsonProperty("magic")]
    public int Magic { get; set; } // Magic number for trade identification

    [JsonProperty("clientId")]
    public string? ClientId { get; set; } // Optional client ID

    [JsonProperty("comment")]
    public string? Comment { get; set; } // Optional comment for the request

    [JsonProperty("actionType")]
    public string ActionType { get; set; } // Action type, e.g., POSITION_CLOSE_ID
}
