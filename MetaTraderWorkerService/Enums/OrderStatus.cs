namespace MetaTraderWorkerService.Enums;

public enum OrderStatus
{
    Pending = 0,
    Executed = 1,
    Failed = 2,
    Canceled = 3,
    Created = 4,
    ORDER_STATE_PLACED = 5, // MetaTrader-specific
    ORDER_STATE_STARTED = 6, // MetaTrader-specific
    ORDER_STATE_CANCELED = 7, // MetaTrader-specific
    ORDER_STATE_PARTIAL = 8, // MetaTrader-specific
    ORDER_STATE_FILLED = 9, // MetaTrader-specific
    ORDER_STATE_REJECTED = 10, // MetaTrader-specific

    SentToMetaTrader = 11
}