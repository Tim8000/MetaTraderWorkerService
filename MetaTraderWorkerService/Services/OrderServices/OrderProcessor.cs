using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Enums.Mt5Trades;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Repository.Trades;

namespace MetaTraderWorkerService.Services.OrderServices;

public class OrderProcessor : IOrderProcessor
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;
    private readonly ILogger<OrderProcessor> _logger;
    private readonly ITradeRepository _tradeRepository;
    private readonly string _accountId;

    public OrderProcessor(IOrderRepository orderRepository, IMetaApiService metaApiService,
        ILogger<OrderProcessor> logger, IConfiguration configuration, ITradeRepository tradeRepository)
    {
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
        _logger = logger;
        _tradeRepository = tradeRepository;
        _accountId = configuration["MetaApi:ProvisioningProfileId"];
    }

    public async Task ProcessCreatedOrdersAsync()
    {
        var createdOrders = await _orderRepository.GetAllCreatedOrdersAsync();

        if (createdOrders == null)
            return;

        foreach (var metaTraderOrder in createdOrders)
            switch (metaTraderOrder.ActionType)
            {
                case ActionType.ORDER_TYPE_SELL_LIMIT:
                    await ProcessTradeOpening(metaTraderOrder);
                    continue;
                case ActionType.ORDER_TYPE_BUY_LIMIT:
                    await ProcessTradeOpening(metaTraderOrder);
                    continue;
                case ActionType.ORDER_CANCEL:
                    await ProcessCancelOrder(metaTraderOrder);
                    continue;
                case ActionType.POSITION_MODIFY:
                    await ProcessModifyPositionOrder(metaTraderOrder);
                    continue;
                case ActionType.POSITION_PARTIAL:
                    await ProcessPartialPositionCloseOrder(metaTraderOrder);
                    continue;
                default:
                    continue;
            }
    }

    private async Task ProcessModifyPositionOrder(MetaTraderOrder metaTraderOrder)
    {
        switch (metaTraderOrder.SignalCommandCode)
        {
            case SignalCommandCode.MoveStopLossToPrice:
                await ProcessMoveStopLossToPriceOrder(metaTraderOrder);
                return;
            case SignalCommandCode.MoveStopLossToBreakEven:
                await ProcessMoveStopLossToBreakEvenOrder(metaTraderOrder);
                return;
            default:
                return;
        }
    }

    private async Task ProcessPartialPositionCloseOrder(MetaTraderOrder metaTraderOrder)
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


    private async Task ProcessMoveStopLossToBreakEvenOrder(MetaTraderOrder metaTraderOrder)
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


    private async Task ProcessMoveStopLossToPriceOrder(MetaTraderOrder metaTraderOrder)
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


    private async Task ProcessCancelOrder(MetaTraderOrder metaTraderOrder)
    {
        var order = await _orderRepository.GetOrderByInitialTradeSignalId(metaTraderOrder.MetaTraderInitialTradeSignal
            .Id);

        if (order == null)
            throw new Exception($"Order {metaTraderOrder.MetaTraderOrderId} not found");

        if (order.OrderState == OrderState.ORDER_STATE_PLACED)
        {
            var cancelOrderDto = new CancelOrderDto()
            {
                ActionType = metaTraderOrder.ActionType.ToString(),
                OrderId = order.MetaTraderOrderId
            };

            // TODO: Handle response.
            var response = await _metaApiService.PlaceCancelOrderAsync(cancelOrderDto);
            order.Status = OrderStatus.Canceled;
            order.OrderState = OrderState.ORDER_STATE_CANCELED;
            await _orderRepository.UpdateOrderAsync(order);
            metaTraderOrder.Status = OrderStatus.Executed;
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }

        if (order.MetaTraderOrderId == null)
        {
            Console.WriteLine($"Order {metaTraderOrder.MetaTraderOrderId} not found on metatrader platform");
            metaTraderOrder.Status = OrderStatus.Failed;
            metaTraderOrder.Comment = "Initial order does not exist on metatrader platform";
            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }
    }

    private async Task ProcessTradeOpening(MetaTraderOrder metaTraderOrder)
    {
        SetOrderConfigurations(metaTraderOrder);
        await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        var metaTraderOrderDto = CreateMetaTraderOpenTradeOrderDto(metaTraderOrder);

        var orderResponseDto = await _metaApiService.PlacePendingOrderAsync(metaTraderOrderDto);

        if (orderResponseDto != null)
        {
            if (orderResponseDto.NumericCode == TradeResultCode.Done)
            {
                SetValuesToMetaTraderOrderFromResponseDto(metaTraderOrder, orderResponseDto);

                _logger.LogInformation(
                    $"Order was created successfuly, orderId = {metaTraderOrder.MetaTraderOrderId}, metaTraderStringCode = {metaTraderOrder.MetaTraderStringCode}, metatraderOrderId = {metaTraderOrder.MetaTraderOrderId}");
            }
            else
            {
                // if (orderResponseDto.NumericCode == TradeResultCode.InvalidPrice)
                // {
                //     if (metaTraderOrder.ActionType == ActionType.ORDER_TYPE_BUY_LIMIT)
                //     {
                //         metaTraderOrderDto.ActionType = ActionType.ORDER_TYPE_BUY.ToString();
                //     }
                //
                //     if (metaTraderOrder.ActionType == ActionType.ORDER_TYPE_SELL_LIMIT)
                //     {
                //         metaTraderOrderDto.ActionType = ActionType.ORDER_TYPE_SELL.ToString();
                //     }
                //     var response = await _metaApiService.PlacePendingOrderAsync(metaTraderOrderDto);
                // }
                
                metaTraderOrder.Status = OrderStatus.Failed;
                metaTraderOrder.MetaTraderStringCode = orderResponseDto.StringCode;
                metaTraderOrder.MetaTraderTradeResultCode = orderResponseDto.NumericCode;
                metaTraderOrder.MetaTraderMessage = orderResponseDto.Message;
            }

            await _orderRepository.UpdateOrderAsync(metaTraderOrder);
        }
    }

    private void SetOrderConfigurations(MetaTraderOrder metaTraderOrder)
    {
        var expiration = metaTraderOrder.ExpirationType == "ORDER_TIME_GTC"
            ? null
            : new
            {
                type = metaTraderOrder.ExpirationType,
                time = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
                // time = "2024-11-14T23:59:59Z"
            };

        // metaTraderOrder.Volume = CalculateVolumeForDollarAmount(600, metaTraderOrder.OpenPrice!.Value);
        metaTraderOrder.Volume = (decimal)0.01;
        metaTraderOrder.Status = OrderStatus.Pending;
        metaTraderOrder.Slippage = 1;
        metaTraderOrder.Magic = GenerateMagic();
        // metaTraderOrder.ClientId = _accountId; // TODO: Implement, guid does not work.
    }

    private static MetaTraderOpenTradeOrderRequestDto CreateMetaTraderOpenTradeOrderDto(MetaTraderOrder metaTraderOrder)
    {
        var metaTraderOrderDto = new MetaTraderOpenTradeOrderRequestDto
        {
            Symbol = metaTraderOrder.Symbol,
            Volume = metaTraderOrder.Volume,
            ActionType = metaTraderOrder.ActionType.Value.ToString(),
            OpenPrice = metaTraderOrder.OpenPrice,
            StopLoss = metaTraderOrder.StopLoss,
            TakeProfit = metaTraderOrder.TakeProfit,
            Slippage = metaTraderOrder.Slippage,
            ClientId = metaTraderOrder.ClientId,
            Comment = metaTraderOrder.Comment,
            StopLossUnits = metaTraderOrder.StopLossUnits,
            TakeProfitUnits = metaTraderOrder.TakeProfitUnits,
            StopPriceBase = metaTraderOrder.StopPriceBase
        };
        return metaTraderOrderDto;
    }

    private static void SetValuesToMetaTraderOrderFromResponseDto(MetaTraderOrder metaTraderOrder,
        MetaTraderOpenTradeOrderResponseDto openTradeOrderResponseDto)
    {
        metaTraderOrder.MetaTraderTradeStartTime = openTradeOrderResponseDto.TradeStartTime;
        metaTraderOrder.MetaTraderTradeExecutionTime = openTradeOrderResponseDto.TradeExecutionTime;
        metaTraderOrder.MetaTraderOrderId = openTradeOrderResponseDto.OrderId;
        metaTraderOrder.MetaTraderStringCode = openTradeOrderResponseDto.StringCode;
        metaTraderOrder.MetaTraderMessage = openTradeOrderResponseDto.Message;
        metaTraderOrder.Status = OrderStatus.SentToMetaTrader;

        if (Enum.IsDefined(typeof(TradeResultCode), openTradeOrderResponseDto.NumericCode))
            metaTraderOrder.MetaTraderTradeResultCode = (TradeResultCode)openTradeOrderResponseDto.NumericCode;
        else
            metaTraderOrder.MetaTraderTradeResultCode =
                TradeResultCode.Unknown; // Assuming you have an "Unknown" enum value
    }

    public decimal CalculateVolumeForDollarAmount(decimal dollarAmount, decimal currentPrice, decimal minVolume = 0.01m,
        decimal maxVolume = 0.1m)
    {
        // Calculate initial volume
        var volume = dollarAmount / currentPrice;

        // Apply a multiplier if the volume is out of the desired range
        if (volume < minVolume)
            volume = minVolume;
        else if (volume > maxVolume) volume = maxVolume;

        return Math.Round(volume, 2); // Rounds to two decimal places
    }

    private int GenerateMagic()
    {
        return (int)(DateTime.UtcNow.Ticks % 1000000); // Last 6 digits of ticks
    }
}