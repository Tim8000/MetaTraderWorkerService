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
                // var trades = await tradeProcessorService.GetActiveTradesAsync(); // TODO: turn on
            }

            // Wait for a defined interval before placing the next order
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}