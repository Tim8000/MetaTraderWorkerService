using MetaTraderWorkerService.Data;
using MetaTraderWorkerService.Http;
using MetaTraderWorkerService.Processors.OrderProcessors;
using MetaTraderWorkerService.Processors.ServiceOrderProcessors;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.ServiceOrders;
using MetaTraderWorkerService.Repository.Trades;
using MetaTraderWorkerService.Services;
using MetaTraderWorkerService.Services.MarketServices;
using MetaTraderWorkerService.Services.OrderServices;
using MetaTraderWorkerService.Services.TradeServices;
using MetaTraderWorkerService.Settings;
using MetaTraderWorkerService.Workers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", true, true);

// Get MetaApi configuration values
var accountBaseUrl = builder.Configuration["MetaApi:AccountBaseUrl"];
var tradeBaseUrl = builder.Configuration["MetaApi:TradeBaseUrl"];
var authToken = builder.Configuration["MetaApi:AuthToken"]; // auth-token from config
var provisioningProfileId = builder.Configuration["MetaApi:AccountId"]; // provisioningProfileId from config

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // Adjust for your database
builder.Services.Configure<MetaApiSettings>(builder.Configuration.GetSection("MetaApi"));
// Register HttpService and MetaApiService
// Hosted services
builder.Services.AddHostedService<ServiceOrderProcessor>();

builder.Services.AddScoped<IHttpService>(sp => new HttpService(accountBaseUrl, tradeBaseUrl, authToken));
builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<ITradeHistoryRepository, TradeHistoryRepository>();
builder.Services.AddScoped<ITradeProcessor, TradeProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, CancelOrderProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, PartialPositionCloseProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, BuyLimitProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, BuyMarketProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, SellLimitProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, SellMarketProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, StopLossProcessor>();
builder.Services.AddScoped<IOrderActionProcessor, TryToCloseProcessor>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServiceOrderActionProcessor, ServiceOrderMoveStopLossProcessor>();
// builder.Services.AddScoped<IServiceOrderActionProcessor, CloseTradeProcessor>();
builder.Services.AddScoped<IMetaApiService, MetaApiService>(sp =>
    new MetaApiService(
        sp.GetRequiredService<IHttpService>(),
        provisioningProfileId,
        sp.GetRequiredService<ILogger<MetaApiService>>()
    )
);

// Register the Worker as a hosted service
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OrderStatusWorker>();
builder.Services.AddHostedService<TradeWorker>();
builder.Services.AddHostedService<TryToCloseTradeWorker>();

var host = builder.Build();
host.Run();