using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService;

public class OrderStatusWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public OrderStatusWorker(IServiceProvider serviceProvider, ILogger<Worker> logger)
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
                var orderStatusService = scope.ServiceProvider.GetRequiredService<IOrderStatusService>();
                await orderStatusService.CheckOrderStatus();
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}