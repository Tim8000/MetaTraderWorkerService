using MetaTraderWorkerService.Enums.Mt5Trades;
using TradeSignalsDatabaseUpdater.Models;
using TradeStatus = MetaTraderWorkerService.Enums.Mt5Trades.TradeStatus;

namespace MetaTraderWorkerService.Models;

public class MetaTraderTrade
{
    public string Id { get; set; } // Trade ID
    public string Platform { get; set; } // Trading platform (e.g., mt5)
    public string Type { get; set; } // Type of position (e.g., POSITION_TYPE_BUY)
    public string Symbol { get; set; } // Trading symbol (e.g., XAUUSD)
    public int Magic { get; set; } // Magic number for identifying trades
    public DateTime Time { get; set; } // Time of the trade
    public DateTime BrokerTime { get; set; } // Broker-reported time of the trade
    public DateTime UpdateTime { get; set; } // Last update time for the trade
    public decimal OpenPrice { get; set; } // Opening price of the position
    public decimal Volume { get; set; } // Volume of the position
    public double Swap { get; set; } // Swap fees for the position
    public double Commission { get; set; } // Commission fees for the position
    public double RealizedSwap { get; set; } // Realized swap value
    public double RealizedCommission { get; set; } // Realized commission value
    public double UnrealizedSwap { get; set; } // Unrealized swap value
    public double UnrealizedCommission { get; set; } // Unrealized commission value
    public string Reason { get; set; } // Reason for the position (e.g., POSITION_REASON_EXPERT)
    public double CurrentPrice { get; set; } // Current market price
    public double CurrentTickValue { get; set; } // Current tick value
    public double RealizedProfit { get; set; } // Realized profit
    public double UnrealizedProfit { get; set; } // Unrealized profit
    public double Profit { get; set; } // Total profit (realized + unrealized)
    public double AccountCurrencyExchangeRate { get; set; } // Account currency exchange rate
    public decimal StopLoss { get; set; } // Stop loss price
    public decimal TakeProfit { get; set; } // Take profit price
    public long UpdateSequenceNumber { get; set; } // Sequence number for updates

    public TradeState State { get; set; }
    public TradeStatus Status { get; set; }
    public List<MetaTraderOrder>? MetaTraderOrders { get; set; }
    public List<MetaTraderTradeHistory>? MetaTraderTradeHistories = new List<MetaTraderTradeHistory>();
}