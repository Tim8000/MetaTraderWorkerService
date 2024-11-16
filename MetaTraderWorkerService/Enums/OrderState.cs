namespace MetaTraderWorkerService.Enums;

public enum OrderState
{
    ORDER_STATE_PLACED = 5, // MetaTrader-specific
    ORDER_STATE_STARTED = 6, // MetaTrader-specific
    ORDER_STATE_CANCELED = 7, // MetaTrader-specific
    ORDER_STATE_PARTIAL = 8, // MetaTrader-specific
    ORDER_STATE_FILLED = 9, // MetaTrader-specific
    ORDER_STATE_REJECTED = 10 // MetaTrader-specific
}