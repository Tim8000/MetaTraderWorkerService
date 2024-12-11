using System.Diagnostics;
using MetaTraderWorkerService.Services.OrderServices;
using MetaTraderWorkerService.Services.TradeServices;

namespace MetaTraderWorkerService.BackgroundServices;

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
            var today = DateTime.UtcNow.DayOfWeek;
            if (today == DayOfWeek.Saturday || today == DayOfWeek.Sunday)
            {
                _logger.LogInformation("Today is a weekend. Skipping processing.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Delay for an hour or any suitable duration
                continue;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var stopwatch = Stopwatch.StartNew();
                var orderStatusService = scope.ServiceProvider.GetRequiredService<IOrderStatusService>();
                var tradeProcessorService = scope.ServiceProvider.GetRequiredService<ITradeProcessor>();
                await orderStatusService.CheckOrderStatus();
                await tradeProcessorService.ProcessActiveTradesAsync();
                await tradeProcessorService.ProcessTradeHistoryAsync();
                await tradeProcessorService.ProcessMovingStopLossAsync();
                await tradeProcessorService.ProcessCancelOrderIfOneTradeInProfit();
                // await tradeProcessorService.ProcessTryToCloseTradesAsync();
                _logger.LogDebug("All processes running time: {time}", stopwatch.Elapsed);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}