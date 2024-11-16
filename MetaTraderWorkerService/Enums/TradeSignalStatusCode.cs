namespace MetaTraderWorkerService.Enums;

public enum TradeSignalStatusCode
{
    Created = 0, // Signal created and saved in the database
    Processing = 1, // Signal picked up by the worker for order creation
    Sent = 2, // Order sent to trade platform API
    AwaitingConfirmation = 3, // Awaiting confirmation from trade platform
    TradeOpen = 4, // Trade successfully opened on platform
    TradePending = 5, // Trade is pending (if applicable)
    TradeFailed = 6, // Trade opening failed
    Completed = 7, // Order process completed, trade data finalized
    TradeModified = 8, // Trade modified (e.g., SL or TP updated)
    TradeClosed = 9, // Trade has been closed
    OrderCreated = 10
}