using MetaTraderWorkerService.Services;

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
                var metaApiService = scope.ServiceProvider.GetRequiredService<IMetaApiService>();

                var orderProcessor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
                await orderProcessor.ProcessCreatedOrdersAsync();
                // await metaApiService.InitializeAsync();


                // Use metaApiService as needed, e.g., metaApiService.PlaceMarketOrderAsync("XAUUSD", 0.1, "BUY");
                // Remember to handle exceptions as needed
            }

            // await _metaApiService.PlacePendingOrderAsync(
            //     symbol: "XAUUSD",
            //     volume: 0.1,
            //     actionType: "ORDER_TYPE_SELL_LIMIT",
            //     openPrice: 2600.00,
            //     stopLoss: 2620.00,
            //     takeProfit: 2580.00,
            //     slippage: 2,
            //     clientId: "123",
            //     comment: "Pending order for XAUUSD",
            //     stopLossUnits: "ABSOLUTE_PRICE",
            //     takeProfitUnits: "ABSOLUTE_PRICE",
            //     stopPriceBase: "OPEN_PRICE",
            //     expirationType: "ORDER_TIME_SPECIFIED",
            //     expirationTime: DateTime.Parse("2024-12-01T12:00:00Z")
            // );

            // Place a market order as an example
            // await _metaApiService.PlaceMarketOrderAsync("XAUUSD", 0.1, "BUY");

            // Wait for a defined interval before placing the next order
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}