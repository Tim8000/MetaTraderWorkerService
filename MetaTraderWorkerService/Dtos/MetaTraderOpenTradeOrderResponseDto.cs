using MetaTraderWorkerService.Enums;

namespace MetaTraderWorkerService.Dtos;

public class MetaTraderOpenTradeOrderResponseDto
{
    public TradeResultCode NumericCode { get; set; }
    public string StringCode { get; set; }
    public string Message { get; set; }
    public string OrderId { get; set; }
    public DateTime TradeExecutionTime { get; set; }
    public DateTime TradeStartTime { get; set; }
}