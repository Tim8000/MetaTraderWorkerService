using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.Trades;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public class PartialPositionCloseProcessor : IOrderActionProcessor
{
    private readonly ILogger<CancelOrderProcessor> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;
    private readonly ITradeRepository _tradeRepository;

    public PartialPositionCloseProcessor(ILogger<CancelOrderProcessor> logger, IOrderRepository orderRepository,
        IMetaApiService metaApiService, ITradeRepository tradeRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
        _tradeRepository = tradeRepository;
    }

    public async Task ProcessAsync(MetaTraderOrder metaTraderOrder)
    {
        if (metaTraderOrder.Trade == null)
        {
            _logger.LogError($"No active trade found for MetaTraderOrder with ID: {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "No active trade associated with this order.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        // Calculate the new partial volume
        var partialVolume = metaTraderOrder.Trade.Volume * 0.5m; // Example: Closing 50% of the volume

        if (partialVolume < 0.01m) partialVolume = 0.01m;

        if (partialVolume <= 0 || metaTraderOrder.Trade.Volume < partialVolume)
        {
            _logger.LogError($"Invalid partial volume for MetaTraderOrder with ID: {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "Invalid partial volume.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        // Prepare DTO for partial position close
        var partialCloseDto = new PartialCloseTradeOrderDto
        {
            Volume = partialVolume,
            ActionType = ActionType.POSITION_PARTIAL.ToString(),
            PositionId = metaTraderOrder.Trade.Id,
            // Magic = metaTraderOrder.Magic.Value,
            ClientId = metaTraderOrder.ClientId,
            Comment = $"Partial close:{metaTraderOrder.Trade.Id}"
        };

        // Send request to MetaTrader
        var response = await _metaApiService.ClosePartialPositionAsync(partialCloseDto);

        // Handle the response
        if (response != null && response.NumericCode == TradeResultCode.Done)
        {
            _logger.LogInformation($"Partial position close successful for Trade ID: {metaTraderOrder.Trade.Id}");

            // Update trade details
            metaTraderOrder.Trade.Volume -= partialVolume;
            if (metaTraderOrder.Trade.Volume <= 0)
                metaTraderOrder.Trade.Status = TradeStatus.Closed;
            else
                metaTraderOrder.Trade.Status = TradeStatus.PartiallyClosed;

            metaTraderOrder.Status = OrderStatus.Executed;
            metaTraderOrder.MetaTraderTradeResultCode = response.NumericCode;
            metaTraderOrder.MetaTraderMessage = "Partial close completed successfully.";
        }
        else
        {
            _logger.LogError($"Partial close failed for Trade ID: {metaTraderOrder.Trade.Id}.");

            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.MetaTraderTradeResultCode = response?.NumericCode ?? TradeResultCode.Unknown;
            metaTraderOrder.MetaTraderMessage = response?.Message ?? "Partial close failed with unknown error.";
        }

        // Save updates to the database
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
    }
}