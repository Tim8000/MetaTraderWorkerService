using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Repository.ServiceOrders;

namespace MetaTraderWorkerService.Processors.ServiceOrderProcessors;

public class ServiceOrderProcessor : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceOrderProcessor> _logger;

    public ServiceOrderProcessor(
        IServiceProvider serviceProvider,
        ILogger<ServiceOrderProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServiceOrderProcessor started.");
        _ = ProcessPendingServiceOrdersAsync(cancellationToken); // Fire-and-forget
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServiceOrderProcessor stopped.");
        return Task.CompletedTask;
    }

    private async Task ProcessPendingServiceOrdersAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var serviceOrderRepository = scope.ServiceProvider.GetRequiredService<IServiceOrderRepository>();
                var processors = scope.ServiceProvider.GetServices<IServiceOrderActionProcessor>();
                var actionProcessors = processors.ToDictionary(p => p.GetSupportedActionType(), p => p);

                var pendingOrders = await serviceOrderRepository.GetPendingServiceOrdersAsync();

                foreach (var order in pendingOrders)
                {
                    if (actionProcessors.TryGetValue(order.ActionType, out var processor))
                    {
                        await processor.ProcessAsync(order);
                        await serviceOrderRepository.UpdateAsync(order);
                    }
                    else
                    {
                        _logger.LogWarning($"No processor found for ActionType: {order.ActionType}");
                        order.Status = ServiceOrderStatus.Failed;
                        order.ErrorMessage = $"Unsupported ActionType: {order.ActionType}";
                        await serviceOrderRepository.UpdateAsync(order);
                    }
                }
            }

            await Task.Delay(1000, stoppingToken); // Adjust interval as needed
        }
    }
}