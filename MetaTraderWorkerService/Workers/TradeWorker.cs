using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.Trades;
using MetaTraderWorkerService.Services;
using MetaTraderWorkerService.Services.TradeServices;

namespace MetaTraderWorkerService.Workers;

public class TradeWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TradeWorker> _logger;

    public TradeWorker(IServiceProvider serviceProvider, ILogger<TradeWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var tradeProcessorService = scope.ServiceProvider.GetRequiredService<ITradeProcessor>();
                var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var tradeRepository = scope.ServiceProvider.GetRequiredService<ITradeRepository>();

                try
                {
                    // Retrieve active trades from MetaTrader
                    var activeTrades = await tradeProcessorService.GetActiveTradesAsync();

                    // Process active trades
                    foreach (var trade in activeTrades)
                    {
                        // Find the corresponding order in the database
                        var order = await orderRepository.GetOrderByMetaTraderOrderId(trade.Id, trade.Symbol);

                        if (order != null)
                        {
                            // Check if the trade already exists for this order
                            var existingTrade = order.Trade;

                            if (existingTrade != null && existingTrade.Id == trade.Id)
                            {
                                // Update existing trade
                                existingTrade.Platform = trade.Platform;
                                existingTrade.Symbol = trade.Symbol;
                                existingTrade.Magic = order.Magic.Value;
                                existingTrade.OpenPrice = trade.OpenPrice;
                                existingTrade.Volume = trade.Volume;
                                existingTrade.CurrentPrice = trade.CurrentPrice;
                                existingTrade.StopLoss = trade.StopLoss;
                                existingTrade.TakeProfit = trade.TakeProfit;
                                existingTrade.Time = trade.Time.ToUniversalTime();
                                existingTrade.BrokerTime = trade.BrokerTime.ToUniversalTime();
                                existingTrade.UpdateTime = trade.UpdateTime.ToUniversalTime();
                                existingTrade.Profit = trade.Profit;
                                existingTrade.State = TradeState.Active;
                                existingTrade.Status = TradeStatus.Open;
                                existingTrade.Type = trade.Type;
                                existingTrade.Reason = trade.Reason;
                                existingTrade.Swap = trade.Swap;
                                existingTrade.Commission = trade.Commission;
                                existingTrade.RealizedSwap = trade.RealizedSwap;
                                existingTrade.RealizedCommission = trade.RealizedCommission;
                                existingTrade.UnrealizedSwap = trade.UnrealizedSwap;
                                existingTrade.UnrealizedCommission = trade.UnrealizedCommission;
                                existingTrade.CurrentTickValue = trade.CurrentTickValue;
                                existingTrade.RealizedProfit = trade.RealizedProfit;
                                existingTrade.UnrealizedProfit = trade.UnrealizedProfit;
                                existingTrade.AccountCurrencyExchangeRate = trade.AccountCurrencyExchangeRate;
                                existingTrade.UpdateSequenceNumber = trade.UpdateSequenceNumber;

                                // Save updates to the database
                                order.Status = OrderStatus.Executed;
                                order.OrderState = OrderState.ORDER_STATE_FILLED;
                                await tradeRepository.UpdateTradeAsync(existingTrade);
                            }
                            else
                            {
                                // Create a new trade and associate it with the order
                                var newTrade = new MetaTraderTrade
                                {
                                    Id = trade.Id,
                                    Platform = trade.Platform,
                                    Symbol = trade.Symbol,
                                    Magic = order.Magic.Value,
                                    OpenPrice = trade.OpenPrice,
                                    Volume = trade.Volume,
                                    CurrentPrice = trade.CurrentPrice,
                                    StopLoss = trade.StopLoss,
                                    TakeProfit = trade.TakeProfit,
                                    Time = trade.Time.ToUniversalTime(),
                                    BrokerTime = trade.BrokerTime.ToUniversalTime(),
                                    UpdateTime = trade.UpdateTime.ToUniversalTime(),
                                    Profit = trade.Profit,
                                    State = TradeState.Active,
                                    Status = TradeStatus.Open,
                                    Type = trade.Type,
                                    Reason = trade.Reason,
                                    Swap = trade.Swap,
                                    Commission = trade.Commission,
                                    RealizedSwap = trade.RealizedSwap,
                                    RealizedCommission = trade.RealizedCommission,
                                    UnrealizedSwap = trade.UnrealizedSwap,
                                    UnrealizedCommission = trade.UnrealizedCommission,
                                    CurrentTickValue = trade.CurrentTickValue,
                                    RealizedProfit = trade.RealizedProfit,
                                    UnrealizedProfit = trade.UnrealizedProfit,
                                    AccountCurrencyExchangeRate = trade.AccountCurrencyExchangeRate,
                                    UpdateSequenceNumber = trade.UpdateSequenceNumber
                                };

                                // Associate the new trade with the order
                                order.Trade = newTrade;
                                order.Status = OrderStatus.Executed;
                                order.OrderState = OrderState.ORDER_STATE_FILLED;

                                // Save new trade and update the order in the database
                                await tradeRepository.AddTradeAsync(newTrade);
                                await orderRepository.UpdateOrderAsync(order);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TradeProcessor>>();
                    logger.LogError(ex, "Error while processing active trades.");
                }
            }

            // Wait for a defined interval before the next iteration
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}