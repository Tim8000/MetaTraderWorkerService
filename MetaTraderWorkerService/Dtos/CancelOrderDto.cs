using Newtonsoft.Json;

namespace MetaTraderWorkerService.Dtos;

public class CancelOrderDto
{
    [JsonProperty("actionType")]
    public string ActionType { get; set; }

    [JsonProperty("orderId")]
    public string OrderId { get; set; }
}