namespace MetaTraderWorkerService.Dtos;

public class PartialCloseTradeOrderDto
{
    public string Symbol { get; set; }
    public decimal Volume { get; set; }
    public string TradeId { get; set; }
    public int Magic { get; set; }
    public string? ClientId { get; set; }
    public string? Comment { get; set; }
}
