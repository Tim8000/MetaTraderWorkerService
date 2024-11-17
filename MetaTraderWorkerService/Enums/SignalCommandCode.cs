namespace MetaTraderWorkerService.Enums;

public enum SignalCommandCode
{
    Buy = 1,
    Sell = 2,
    CloseTrade = 3,
    PartialProfit = 4,
    MoveStopLossToPrice = 5,
    TryToCloseTrade = 6,
    HoldOrder = 7,
    CloseOrderAtEntryPoint = 8,
    CancelOrder = 9,
    MoveStopLossToBreakEven = 10
}