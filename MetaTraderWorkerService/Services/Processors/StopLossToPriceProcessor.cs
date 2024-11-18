using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.Trades;

namespace MetaTraderWorkerService.Services.Processors;

public class StopLossToPriceProcessor : IOrderActionProcessor
{
    private readonly ILogger<CancelOrderProcessor> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;
    private readonly ITradeRepository _tradeRepository;

    public StopLossToPriceProcessor(ILogger<CancelOrderProcessor> logger, IOrderRepository orderRepository, IMetaApiService metaApiService, ITradeRepository tradeRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
        _tradeRepository = tradeRepository;
    }

    public async Task ProcessAsync(MetaTraderOrder metaTraderOrder)
    {
                // Validate the trade associated with the order
        if (metaTraderOrder.Trade == null)
        {
            _logger.LogError($"No active trade found for MetaTraderOrder with ID: {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "No active trade associated with this order.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        // Prepare DTO for modifying stop-loss
        var modifyOrderDto = new ModifyStopLossRequestDto()
        {
            ActionType = ActionType.POSITION_MODIFY.ToString(),
            PositionId = metaTraderOrder.Trade.Id,
            StopLoss = metaTraderOrder.StopLoss.Value,
            TakeProfit = metaTraderOrder.Trade.TakeProfit,
            // Magic = metaTraderOrder.Magic.Value,
            // Comment = "Updating stop-loss to new value."
        };

        // Send request to MetaTrader
        var response = await _metaApiService.ModifyStopLossAsync(modifyOrderDto);

        // Handle the response
        if (response != null && response.NumericCode == TradeResultCode.Done)
        {
            _logger.LogInformation($"Stop-loss updated successfully for Trade ID: {metaTraderOrder.Trade.Id}");

            // Update trade details
            metaTraderOrder.Trade.StopLoss = metaTraderOrder.StopLoss.Value;
            metaTraderOrder.Trade.State = TradeState.Modified;

            // Update order status
            metaTraderOrder.Status = OrderStatus.Executed;
            metaTraderOrder.MetaTraderTradeResultCode = response.NumericCode;
            metaTraderOrder.MetaTraderMessage = "Stop-loss updated successfully.";
        }
        else
        {
            _logger.LogError(
                $"Failed to update stop-loss for Trade ID: {metaTraderOrder.Trade.Id}. Reason: {response?.Message}");

            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.MetaTraderTradeResultCode = response?.NumericCode ?? TradeResultCode.Unknown;
            metaTraderOrder.MetaTraderMessage = response?.Message ?? "Failed to update stop-loss with unknown error.";
        }

        // Save updates to the database
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        if (metaTraderOrder.Trade != null) await _tradeRepository.UpdateTradeAsync(metaTraderOrder.Trade);

    }
}