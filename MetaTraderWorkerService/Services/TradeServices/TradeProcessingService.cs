using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Helpers;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.ServiceOrders;
using MetaTraderWorkerService.Repository.Trades;

namespace MetaTraderWorkerService.Services.TradeServices;

public class TradeProcessingService : ITradeProcessingService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ILogger<TradeProcessingService> _logger;

    public TradeProcessingService(
        ITradeRepository tradeRepository,
        IServiceOrderRepository serviceOrderRepository,
        ILogger<TradeProcessingService> logger)
    {
        _tradeRepository = tradeRepository;
        _serviceOrderRepository = serviceOrderRepository;
        _logger = logger;
    }

    public async Task ProcessMovingStopLossAsync(MetaTraderTrade trade)
    {
        var nextThreshold = CalculateNextThreshold(trade);
        if (nextThreshold == null) return;

        await ProcessStopLossAdjustmentAsync(trade, nextThreshold.Value);
    }

    public async Task ProcessMoveStopLossToOpenPrice(MetaTraderTrade trade)
    {
        var pipDifference = PipCalculator.CalculatePipDifference(trade.OpenPrice, trade.CurrentPrice);

        if (pipDifference >= 20 && trade.StopLoss != trade.OpenPrice && trade.StopLoss < trade.OpenPrice )
        {
            await ProcessStopLossAdjustmentAsync(trade, trade.OpenPrice);
        }
    }

    public async Task RemovePendingLimitOrderIfProfitableAsync()
    {
        // var openedTrades = await _tradeRepository.GetAllOpenedTradesAsync();
        //
        // foreach (var trade in openedTrades)
        // {
        //     if (trade.ServiceOrders.Count(o => o.ActionType == "PENDING_LIMIT") > 1)
        //     {
        //         var mainOrder = trade.ServiceOrders
        //             .Where(o => o.ActionType == "PENDING_LIMIT")
        //             .OrderBy(o => o.CreatedAt)
        //             .First();
        //
        //         if (mainOrder.Status == ServiceOrderStatus.Completed &&
        //             mainOrder.ProfitPips >= 30)
        //         {
        //             var secondOrder = trade.ServiceOrders
        //                 .Where(o => o.ActionType == "PENDING_LIMIT" && o != mainOrder)
        //                 .FirstOrDefault();
        //
        //             if (secondOrder != null)
        //             {
        //                 _logger.LogInformation($"Removing pending limit order {secondOrder.Id}");
        //                 secondOrder.Status = ServiceOrderStatus.Cancelled;
        //                 await _serviceOrderRepository.UpdateAsync(secondOrder);
        //             }
        //         }
        //     }
        // }
    }

    private decimal? CalculateNextThreshold(MetaTraderTrade trade)
    {
        var openPrice = trade.OpenPrice;
        var currentPrice = (decimal)trade.CurrentPrice;

        var latestStopLoss = trade.ServiceOrders?
            .Where(o => o.ActionType == "MOVE_STOPLOSS")
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault()?.StopLoss ?? trade.StopLoss;

        var latestPipDifference =
            PipCalculator.CalculatePipDifference(openPrice, latestStopLoss);

        return latestPipDifference >= 20
            ? latestPipDifference + 10
            : 20;
    }

    private async Task ProcessStopLossAdjustmentAsync(MetaTraderTrade trade, decimal targetStopLoss)
    {

        // Update the stop-loss
        trade.StopLoss = targetStopLoss;

        // Create a new ServiceOrder for the adjustment
        var serviceOrder = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            ActionType = "MOVE_STOPLOSS",
            MetaTraderTrade = trade,
            StopLoss = targetStopLoss,
            Status = ServiceOrderStatus.Pending,
            TakeProfit = trade.TakeProfit,
            CreatedAt = DateTime.UtcNow,
            PositionId = trade.Id
        };

        // Save the ServiceOrder
        _logger.LogInformation($"Adjusting Stop-loss for trade {trade.Id}: New Stop-loss = {targetStopLoss}");
        await _serviceOrderRepository.AddAsync(serviceOrder);
    }

    private decimal CalculateTargetStopLoss(MetaTraderTrade trade, decimal openPrice, decimal nextThreshold)
    {
        return trade.Type switch
        {
            "POSITION_TYPE_BUY" => nextThreshold switch
            {
                20 => openPrice,
                _ => openPrice + (nextThreshold - 10) * 0.01m
            },
            "POSITION_TYPE_SELL" => nextThreshold switch
            {
                20 => openPrice,
                _ => openPrice - (nextThreshold - 10) * 0.01m
            },
            _ => throw new InvalidOperationException("Unsupported trade type")
        };
    }
}