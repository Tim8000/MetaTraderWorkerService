namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class ModifyOrderRequestDto
{
    public string Symbol { get; set; }
    public string TradeId { get; set; }
    public decimal NewStopLoss { get; set; }
    public int Magic { get; set; }
    public string? Comment { get; set; }
}