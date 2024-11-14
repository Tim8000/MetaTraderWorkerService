namespace MetaTraderWorkerService.Enums;

public enum TradeResultCode
{
    Done = 10009,             // Request completed
    Placed = 10008,           // Order placed
    DonePartial = 10010,      // Only part of the request was completed
    Rejected = 10006,         // Request rejected
    Canceled = 10007,         // Request canceled by trader
    Error = 10011,            // Request processing error
    Timeout = 10012,          // Request canceled by timeout
    Invalid = 10013,          // Invalid request
    InvalidVolume = 10014,    // Invalid volume in the request
    InvalidPrice = 10015,     // Invalid price in the request
    InvalidStops = 10016,     // Invalid stops in the request
    TradeDisabled = 10017,    // Trade is disabled
    MarketClosed = 10018,     // Market is closed
    NoMoney = 10019,          // Not enough money to complete the request
    PriceChanged = 10020,     // Prices changed
    PriceOff = 10021,         // No quotes to process the request
    InvalidExpiration = 10022,// Invalid order expiration date in the request
    OrderChanged = 10023,     // Order state changed
    TooManyRequests = 10024,  // Too frequent requests
    NoChanges = 10025,        // No changes in request
    Locked = 10028,           // Request locked for processing
    Frozen = 10029,           // Order or position frozen
    InvalidFill = 10030,      // Invalid order filling type
    Connection = 10031,       // No connection with the trade server
    OnlyReal = 10032,         // Operation is allowed only for live accounts
    LimitOrders = 10033,      // Number of pending orders has reached the limit
    LimitVolume = 10034,      // Volume of orders and positions for the symbol has reached the limit
    InvalidOrder = 10035,     // Incorrect or prohibited order type
    PositionClosed = 10036,   // Position has already been closed
    InvalidCloseVolume = 10038,// Close volume exceeds the current position volume
    CloseOrderExist = 10039,  // A close order already exists for the specified position
    RejectCancel = 10041,     // Pending order activation request is rejected, the order is canceled
    LongOnly = 10042,         // Only long positions are allowed
    ShortOnly = 10043,        // Only short positions are allowed
    CloseOnly = 10044,        // Only position closing is allowed
    FifoClose = 10045         // Position closing is allowed only by FIFO rule
    ,
    Unknown = 0000
}
