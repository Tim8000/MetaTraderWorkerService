using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Helpers;
using MetaTraderWorkerService.Http;
using MetaTraderWorkerService.Mappers;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.ServiceOrders;
using MetaTraderWorkerService.Repository.Trades;
using MetaTraderWorkerService.Services.MarketServices;
using Newtonsoft.Json;

namespace MetaTraderWorkerService.Services.TradeServices;

public class TradeProcessor : ITradeProcessor
{
    private readonly IHttpService _httpService;
    private readonly IOrderRepository _orderRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly ILogger<TradeProcessor> _logger;
    private readonly IMetaApiService _metaApiService;
    private readonly IMarketService _marketService;
    private readonly ITradeHistoryRepository _tradeHistoryRepository;
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly string _accountId;
    private readonly ITradeProcessingService _tradeProcessingService;

    private const string GetPositionsEndpoint = "/users/current/accounts/{0}/positions";
    private const string CreateTradeEndpoint = "/users/current/accounts/{0}/trade";

    public TradeProcessor(IHttpService httpService, IConfiguration configuration, IOrderRepository orderRepository,
        ITradeRepository tradeRepository, ILogger<TradeProcessor> logger, IMetaApiService metaApiService,
        IMarketService marketService, ITradeHistoryRepository tradeHistoryRepository,
        IServiceOrderRepository serviceOrderRepository, ITradeProcessingService tradeProcessingService)
    {
        _httpService = httpService;
        _orderRepository = orderRepository;
        _tradeRepository = tradeRepository;
        _logger = logger;
        _metaApiService = metaApiService;
        _marketService = marketService;
        _tradeHistoryRepository = tradeHistoryRepository;
        _serviceOrderRepository = serviceOrderRepository;
        _tradeProcessingService = tradeProcessingService;

        // Retrieve accountId from the configuration
        _accountId = configuration["MetaApi:AccountId"];
        if (string.IsNullOrEmpty(_accountId))
            throw new ArgumentException("Account ID is not configured in appsettings.");
    }

    public async Task<List<TradeStatusResponseDto>> GetActiveTradesAsync()
    {
        var url = string.Format(GetPositionsEndpoint, _accountId);
        var response = await _httpService.GetAsync(url, false);


        var trades = JsonConvert.DeserializeObject<List<TradeStatusResponseDto>>(response);
        return trades;
    }

    /// <summary>
    /// Processes active trades by retrieving the active trades and updating the corresponding orders and trades.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ProcessActiveTradesAsync()
    {
        try
        {
            var activeTrades = await GetActiveTradesAsync();

            foreach (var trade in activeTrades)
            {
                var order = await _orderRepository.GetOrderByMetaTraderOrderId(trade.Id, trade.Symbol);

                if (order != null)
                {
                    var existingTrade = order.Trade;

                    if (existingTrade != null && existingTrade.Id == trade.Id)
                    {
                        existingTrade.Platform = trade.Platform;
                        existingTrade.Symbol = trade.Symbol;
                        existingTrade.Magic = order.Magic.Value;
                        existingTrade.OpenPrice = trade.OpenPrice;
                        existingTrade.Volume = trade.Volume;
                        existingTrade.CurrentPrice = trade.CurrentPrice;
                        existingTrade.StopLoss = trade.StopLoss;
                        existingTrade.TakeProfit = trade.TakeProfit;
                        existingTrade.Time = trade.Time.ToUniversalTime();
                        existingTrade.BrokerTime = trade.BrokerTime.ToUniversalTime();
                        existingTrade.UpdateTime = trade.UpdateTime.ToUniversalTime();
                        existingTrade.Profit = trade.Profit;
                        existingTrade.State = TradeState.Active;
                        existingTrade.Status = TradeStatus.Open;
                        existingTrade.Type = trade.Type;
                        existingTrade.Reason = trade.Reason;
                        existingTrade.Swap = trade.Swap;
                        existingTrade.Commission = trade.Commission;
                        existingTrade.RealizedSwap = trade.RealizedSwap;
                        existingTrade.RealizedCommission = trade.RealizedCommission;
                        existingTrade.UnrealizedSwap = trade.UnrealizedSwap;
                        existingTrade.UnrealizedCommission = trade.UnrealizedCommission;
                        existingTrade.CurrentTickValue = trade.CurrentTickValue;
                        existingTrade.RealizedProfit = trade.RealizedProfit;
                        existingTrade.UnrealizedProfit = trade.UnrealizedProfit;
                        existingTrade.AccountCurrencyExchangeRate = trade.AccountCurrencyExchangeRate;
                        existingTrade.UpdateSequenceNumber = trade.UpdateSequenceNumber;

                        order.Status = OrderStatus.Executed;
                        order.OrderState = OrderState.ORDER_STATE_FILLED;
                        await _tradeRepository.UpdateTradeAsync(existingTrade);
                    }
                    else
                    {
                        var newTrade = new MetaTraderTrade
                        {
                            Id = trade.Id,
                            Platform = trade.Platform,
                            Symbol = trade.Symbol,
                            Magic = order.Magic.Value,
                            OpenPrice = trade.OpenPrice,
                            Volume = trade.Volume,
                            CurrentPrice = trade.CurrentPrice,
                            StopLoss = trade.StopLoss,
                            TakeProfit = trade.TakeProfit,
                            Time = trade.Time.ToUniversalTime(),
                            BrokerTime = trade.BrokerTime.ToUniversalTime(),
                            UpdateTime = trade.UpdateTime.ToUniversalTime(),
                            Profit = trade.Profit,
                            State = TradeState.Active,
                            Status = TradeStatus.Open,
                            Type = trade.Type,
                            Reason = trade.Reason,
                            Swap = trade.Swap,
                            Commission = trade.Commission,
                            RealizedSwap = trade.RealizedSwap,
                            RealizedCommission = trade.RealizedCommission,
                            UnrealizedSwap = trade.UnrealizedSwap,
                            UnrealizedCommission = trade.UnrealizedCommission,
                            CurrentTickValue = trade.CurrentTickValue,
                            RealizedProfit = trade.RealizedProfit,
                            UnrealizedProfit = trade.UnrealizedProfit,
                            AccountCurrencyExchangeRate = trade.AccountCurrencyExchangeRate,
                            UpdateSequenceNumber = trade.UpdateSequenceNumber
                        };

                        order.Trade = newTrade;
                        order.Status = OrderStatus.Executed;
                        order.OrderState = OrderState.ORDER_STATE_FILLED;

                        await _tradeRepository.AddTradeAsync(newTrade);
                        await _orderRepository.UpdateOrderAsync(order);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing active trades.");
        }

        _logger.LogInformation("Active trades processed.");
    }

    public async Task ProcessTradeHistoryAsync()
    {
        var openedTrades = await _tradeRepository.GetAllOpenedTradesAsync();

        var tradeHistories = await Task.WhenAll(
            openedTrades.Select(async trade => new
            {
                Trade = trade,
                History = await _metaApiService.GetTradeHistoryByPositionIdAsync(trade.Id)
            })
        );

        foreach (var tradeHistory in tradeHistories)
        {
            var trade = tradeHistory.Trade;
            var trades = tradeHistory.History;

            if (trades.Count > 0)
            {
                var metaTraderTradeHistories = trades
                    .Select(dto => dto.ToMetaTraderTradeHistory())
                    .ToList();

                foreach (var history in metaTraderTradeHistories)
                {
                    history.Time = DateTime.SpecifyKind(history.Time, DateTimeKind.Utc);
                    history.BrokerTime = DateTime.SpecifyKind(history.BrokerTime, DateTimeKind.Utc);

                    var existingEntity = await _tradeHistoryRepository.GetByTradeHistoryIdAsync(history.TradeHistoryId);

                    if (existingEntity == null)
                    {
                        history.MetaTraderTrade = trade;
                        await _tradeHistoryRepository.AddAsync(history);
                    }

                    var brokerComment = history.BrokerComment;
                    var containsSlOrTp = brokerComment?.ToLowerInvariant().Contains("sl") == true ||
                                         brokerComment?.ToLowerInvariant().Contains("tp") == true;

                    if (containsSlOrTp && trade.State != TradeState.Closed)
                    {
                        trade.State = TradeState.Closed;
                        trade.Status = TradeStatus.Closed;

                        await _tradeRepository.UpdateTradeAsync(trade);
                        _logger.LogInformation(
                            $"Trade {trade.Id} marked as closed due to BrokerComment: {brokerComment}");
                    }
                }
            }
        }

        _logger.LogInformation("Trade history processed.");
    }

    public async Task ProcessTryToCloseTradesAsync()
    {
        // var openedTrades = await _tradeRepository.GetAllOpenedTradesAsync();
        //
        // var tradesFromOneInitialSignal = openedTrades
        //     .SelectMany(trade => trade.MetaTraderOrders
        //         .Where(order =>
        //             order.MetaTraderInitialTradeSignal
        //                 .IsInitialSignal) // Only include orders where IsInitialSignal is true
        //         .Select(order => new
        //         {
        //             Trade = trade,
        //             MessageId = order.MetaTraderInitialTradeSignal.MessageId
        //         }))
        //     .GroupBy(x => x.MessageId)
        //     .Where(group => group.Count() > 1) // Only include groups with the same MessageId
        //     .ToDictionary(
        //         group => group.Key,
        //         group => group.Select(x => x.Trade).Distinct().ToList()
        //     );
    }

    public async Task ProcessMovingStopLossAsync()
    {
        var openedTrades = await _tradeRepository.GetAllOpenedTradesAsync();

        foreach (var openedTrade in openedTrades)
        {
            if (openedTrade.ServiceOrders != null &&
                openedTrade.ServiceOrders.Any(s => s.ActionType == "MOVE_STOPLOSS"))
                return;

            await _tradeProcessingService.ProcessMoveStopLossToOpenPrice(openedTrade);
        }

        _logger.LogInformation("Moving stop-loss processed.");
    }

    public async Task ProcessCancelOrderIfOneTradeInProfit()
    {
        var openedTrades = await _tradeRepository.GetAllOpenedTradesAsync();

        foreach (var trade in openedTrades)
        {
            // TODO: If opened trade is in profit for 30 pips -> then do below.

            var pipsDifference = PipCalculator.CalculatePipDifference(trade.OpenPrice, trade.CurrentPrice);

            if (trade.Type == "POSITION_TYPE_SELL" && trade.StopLoss > trade.OpenPrice &&
                trade.OpenPrice > trade.CurrentPrice && pipsDifference >= 30)
            {
            }

            if (trade.Type == "POSITION_TYPE_BUY" && trade.StopLoss < trade.OpenPrice &&
                trade.OpenPrice < trade.CurrentPrice && pipsDifference >= 30)
            {
            }

            var initialTradeSignalMessageId =
                trade?.MetaTraderOrders?.FirstOrDefault()?.MetaTraderInitialTradeSignal.MessageId;

            // var sameMessageIdTrades =
            //     await _tradeRepository.GetTradesByInitialSignalMessageId(initialTradeSignalMessageId!.Value);
            var ordersFromSameMessageId =
                await _orderRepository.GetOpenedOrdersFromOneInitialSignal(initialTradeSignalMessageId.Value);

            foreach (var order in ordersFromSameMessageId)
            {
                if ((order.ActionType == ActionType.ORDER_TYPE_BUY_LIMIT ||
                     order.ActionType == ActionType.ORDER_TYPE_SELL_LIMIT ||
                     order.ActionType == ActionType.ORDER_TYPE_BUY || order.ActionType == ActionType.ORDER_TYPE_SELL) &&
                    order.OrderState == OrderState.ORDER_STATE_PLACED)
                {
                    var existingOrder =
                        await _serviceOrderRepository.GetServiceOrderByMetaTraderOrderId(order.MetaTraderOrderId);

                    if (existingOrder != null)
                        return;

                    var serviceOrder = new ServiceOrder
                    {
                        Id = Guid.NewGuid(),
                        ActionType = "ORDER_CANCEL",
                        MetaTraderTrade = trade,
                        Status = ServiceOrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        PositionId = trade.Id,
                        MetaTraderOrder = order
                    };

                    // Save the ServiceOrder
                    _logger.LogInformation(
                        $"Cancelling second order for trade {trade.Id}: Order to be cancelled = {order.MetaTraderOrderId}");
                    await _serviceOrderRepository.AddAsync(serviceOrder);
                }
            }
        }
    }
}