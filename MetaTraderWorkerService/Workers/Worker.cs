using System.Diagnostics;
using MetaTraderWorkerService.Processors.OrderProcessors;
using MetaTraderWorkerService.Services;
using MetaTraderWorkerService.Services.OrderServices;
using MetaTraderWorkerService.Services.TradeServices;

namespace MetaTraderWorkerService.Workers;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
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
                var stopwatch = Stopwatch.StartNew();
                var metaApiService = scope.ServiceProvider.GetRequiredService<IMetaApiService>();
                var orderStatusService = scope.ServiceProvider.GetRequiredService<IOrderStatusService>();
                var orderProcessor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
                var tradeProcessorService = scope.ServiceProvider.GetRequiredService<ITradeProcessor>();
                await orderProcessor.ProcessCreatedOrdersAsync();
                await orderStatusService.CheckOrderStatus();
                await tradeProcessorService.ProcessActiveTradesAsync();
                await tradeProcessorService.ProcessTradeHistoryAsync();
                await tradeProcessorService.ProcessMovingStopLossAsync();
                await tradeProcessorService.ProcessTryToCloseTradesAsync();
                _logger.LogDebug("All processes running time: {time}", stopwatch.Elapsed);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}