using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Processors.OrderProcessors;
using MetaTraderWorkerService.Repository.Orders;

public class OrderProcessor : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessor> _logger;
    private readonly int _pollingIntervalMs = 1000; // Configurable interval

    private CancellationTokenSource? _cancellationTokenSource;

    public OrderProcessor(IServiceProvider serviceProvider, ILogger<OrderProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderProcessor hosted service starting.");
        _cancellationTokenSource = new CancellationTokenSource();

        _ = ProcessOrdersInBackground(_cancellationTokenSource.Token); // Fire-and-forget
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderProcessor hosted service stopping.");
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    private async Task ProcessOrdersInBackground(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    var processors = scope.ServiceProvider.GetServices<IOrderActionProcessor>();
                    var actionProcessors = processors.ToDictionary(
                        p => p.GetType().Name switch
                        {
                            nameof(SellLimitProcessor) => ActionType.ORDER_TYPE_SELL_LIMIT,
                            nameof(BuyLimitProcessor) => ActionType.ORDER_TYPE_BUY_LIMIT,
                            nameof(SellMarketProcessor) => ActionType.ORDER_TYPE_SELL,
                            nameof(BuyMarketProcessor) => ActionType.ORDER_TYPE_BUY,
                            nameof(CancelOrderProcessor) => ActionType.ORDER_CANCEL,
                            nameof(PartialPositionCloseProcessor) => ActionType.POSITION_PARTIAL,
                            nameof(StopLossProcessor) => ActionType.POSITION_MODIFY,
                            nameof(TryToCloseProcessor) => ActionType.POSITION_CLOSE_ID,
                            _ => throw new InvalidOperationException("Unknown processor type")
                        });

                    var orders = await orderRepository.GetAllCreatedOrdersAsync();

                    foreach (var order in orders)
                    {
                        if (actionProcessors.TryGetValue(order.ActionType.Value, out var processor))
                        {
                            await processor.ProcessAsync(order);
                        }
                        else
                        {
                            _logger.LogWarning($"No processor found for ActionType: {order.ActionType}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing orders.");
            }

            await Task.Delay(_pollingIntervalMs, stoppingToken);
        }
    }
}