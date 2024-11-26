using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Mappers;

public static class TradeHistoryMapper
{
    public static MetaTraderTradeHistory ToMetaTraderTradeHistory(this TradeHistoryResponseDto dto)
    {
        return new MetaTraderTradeHistory
        {
            TradeHistoryId = dto.Id,
            Platform = dto.Platform,
            Type = dto.Type,
            Time = dto.Time,
            BrokerTime = dto.BrokerTime,
            Commission = dto.Commission,
            Swap = dto.Swap,
            Profit = dto.Profit,
            Symbol = dto.Symbol,
            Magic = dto.Magic,
            OrderId = dto.OrderId,
            PositionId = dto.PositionId,
            Volume = dto.Volume,
            Price = dto.Price,
            EntryType = dto.EntryType,
            Reason = dto.Reason,
            StopLoss = dto.StopLoss,
            TakeProfit = dto.TakeProfit,
            BrokerComment = dto.BrokerComment,
            AccountCurrencyExchangeRate = dto.AccountCurrencyExchangeRate
        };
    }
}