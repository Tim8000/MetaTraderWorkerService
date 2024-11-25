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

                await tradeProcessorService.ProcessActiveTradesAsync();
                await tradeProcessorService.ProcessTradeHistoryAsync();
            }

            // Wait for a defined interval before the next iteration
            await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);
        }
    }
}