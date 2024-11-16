namespace MetaTraderWorkerService.Enums;

public enum SignalCommandCode
{
    Buy = 1,
    Sell = 2,
    CloseTrade = 3,
    PartialProfit = 4,
    MoveStopLoss = 5,
    CloseOrder = 6,
    HoldOrder = 7,
    CloseOrderAtEntryPoint = 8
}