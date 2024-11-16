using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaTraderWorkerService;
using MetaTraderWorkerService.Data;
using MetaTraderWorkerService.Http;
using MetaTraderWorkerService.Repository;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;
using MetaTraderWorkerService.Services.OrderServices;
using MetaTraderWorkerService.Services.TradeServices;
using MetaTraderWorkerService.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", true, true);

// Get MetaApi configuration values
var accountBaseUrl = builder.Configuration["MetaApi:AccountBaseUrl"];
var tradeBaseUrl = builder.Configuration["MetaApi:TradeBaseUrl"];
var authToken = builder.Configuration["MetaApi:AuthToken"]; // auth-token from config
var provisioningProfileId = builder.Configuration["MetaApi:ProvisioningProfileId"]; // provisioningProfileId from config

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // Adjust for your database

// Register HttpService and MetaApiService
builder.Services.AddScoped<IHttpService>(sp => new HttpService(accountBaseUrl, tradeBaseUrl, authToken));
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<ITradeProcessor, TradeProcessor>();
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

var host = builder.Build();
host.Run();