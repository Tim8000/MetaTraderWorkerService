namespace MetaTraderWorkerService.Dtos.Mt5Trades;

public class ModifyOrderRequestDto
{
    public string Symbol { get; set; } // Trading symbol
    public string TradeId { get; set; } // Trade ID to modify
    public decimal StopLoss { get; set; } // New stop-loss price
    public int Magic { get; set; } // Magic number
    public string? Comment { get; set; } // Optional comment
}