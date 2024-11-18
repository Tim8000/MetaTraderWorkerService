using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.Trades;

namespace MetaTraderWorkerService.Services.Processors;

public class StopLossToBreakEvenProcessor : IOrderActionProcessor
{
    private readonly ILogger<CancelOrderProcessor> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;
    private readonly ITradeRepository _tradeRepository;

    public async Task ProcessAsync(MetaTraderOrder metaTraderOrder)
    {
                // Validate the associated trade
        if (metaTraderOrder.Trade == null)
        {
            _logger.LogError($"No active trade found for MetaTraderOrder ID: {metaTraderOrder.Id}");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "No active trade associated with this order.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        // Determine the break-even price
        var breakEvenPrice = metaTraderOrder.Trade.OpenPrice;

        // Check if the stop-loss is already at break-even
        if (metaTraderOrder.Trade.StopLoss == breakEvenPrice)
        {
            _logger.LogInformation($"Stop-loss already at break-even for Trade ID: {metaTraderOrder.Trade.Id}");
            metaTraderOrder.Status = OrderStatus.Executed;
            metaTraderOrder.Comment = "Stop-loss already at break-even.";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
            return;
        }

        // Prepare ModifyOrderRequestDto for modifying stop-loss
        var modifyOrderRequest = new ModifyStopLossRequestDto()
        {
            ActionType = ActionType.POSITION_MODIFY.ToString(),
            TakeProfit = metaTraderOrder.Trade.TakeProfit,
            PositionId = metaTraderOrder.Trade.Id,
            StopLoss = breakEvenPrice,
        };

        // Send the request to MetaTrader
        var response = await _metaApiService.ModifyStopLossAsync(modifyOrderRequest);

        // Handle the response
        if (response != null && response.NumericCode == TradeResultCode.Done)
        {
            _logger.LogInformation($"Stop-loss moved to break-even for Trade ID: {metaTraderOrder.Trade.Id}");

            // Update trade details
            metaTraderOrder.Trade.StopLoss = breakEvenPrice;
            metaTraderOrder.Trade.State = TradeState.Modified;

            // Update order status
            metaTraderOrder.Status = OrderStatus.Executed;
            metaTraderOrder.MetaTraderTradeResultCode = response.NumericCode;
            metaTraderOrder.MetaTraderMessage = response.Message ?? "Stop-loss moved to break-even successfully.";
        }
        else
        {
            _logger.LogError(
                $"Failed to move stop-loss to break-even for Trade ID: {metaTraderOrder.Trade.Id}. Reason: {response?.Message}");

            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.MetaTraderTradeResultCode = response?.NumericCode ?? TradeResultCode.Unknown;
            metaTraderOrder.MetaTraderMessage = response?.Message ?? "Failed to move stop-loss with unknown error.";
        }

        // Save updates to the database
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        if (metaTraderOrder.Trade != null) await _tradeRepository.UpdateTradeAsync(metaTraderOrder.Trade);

    }
}