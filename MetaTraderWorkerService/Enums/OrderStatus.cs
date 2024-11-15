namespace MetaTraderWorkerService.Enums;

public enum OrderStatus
{
    Pending = 0,
    SentToMetaTrader = 1,
    Failed = 2,
    Canceled = 3,
    Created = 4,
    Executed = 5,
}