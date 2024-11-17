using MetaTraderWorkerService.Enums;

namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class MetaTradePartialCloseResponseDto
{
    public TradeResultCode NumericCode { get; set; }
    public string? Message { get; set; }
}