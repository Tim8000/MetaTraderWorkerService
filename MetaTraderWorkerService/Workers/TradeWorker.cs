using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
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

                try
                {
                    // Retrieve active trades from MetaTrader
                    var activeTrades = await tradeProcessorService.GetActiveTradesAsync();

                    // Process active trades
                    foreach (var trade in activeTrades)
                    {
                        // Find the corresponding order in the database
                        var order = await orderRepository.GetOrderByMagicAndSymbolAsync(trade.Magic, trade.Symbol);

                        if (order != null)
                        {
                            // Associate trade with the order
                            order.Trade = new MetaTraderTrade
                            {
                                Id = trade.Id,
                                Platform = trade.Platform,
                                Symbol = trade.Symbol,
                                Magic = trade.Magic,
                                OpenPrice = trade.OpenPrice,
                                Volume = trade.Volume,
                                CurrentPrice = trade.CurrentPrice,
                                StopLoss = trade.StopLoss,
                                TakeProfit = trade.TakeProfit,
                                Time = trade.Time,
                                BrokerTime = trade.BrokerTime,
                                UpdateTime = trade.UpdateTime,
                                Profit = trade.Profit,
                                State = TradeState.Active,
                                Status = TradeStatus.Open
                            };

                            // Update the order in the database
                            await orderRepository.UpdateOrderAsync(order);
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
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}