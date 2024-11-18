using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services.Processors;

namespace MetaTraderWorkerService.Services.OrderServices;

public class OrderProcessor : IOrderProcessor
{
    private readonly Dictionary<ActionType, IOrderActionProcessor> _actionProcessors;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(
        IEnumerable<IOrderActionProcessor> processors,
        ILogger<OrderProcessor> logger, IOrderRepository orderRepository)
    {
        _logger = logger;
        _orderRepository = orderRepository;

        // Initialize dictionary from the list of processors
        _actionProcessors = processors.ToDictionary(
            p => p.GetType().Name switch
            {
                nameof(SellLimitProcessor) => ActionType.ORDER_TYPE_SELL_LIMIT,
                nameof(BuyLimitProcessor) => ActionType.ORDER_TYPE_BUY_LIMIT,
                nameof(CancelOrderProcessor) => ActionType.ORDER_CANCEL,
                nameof(PartialPositionCloseProcessor) => ActionType.POSITION_PARTIAL,
                nameof(StopLossToPriceProcessor) => ActionType.POSITION_MODIFY,
                nameof(StopLossToBreakEvenProcessor) => ActionType.POSITION_MODIFY,
                _ => throw new InvalidOperationException("Unknown processor type")
            });
    }

    public async Task ProcessCreatedOrdersAsync()
    {
        var orders = await _orderRepository.GetAllCreatedOrdersAsync();
        if (orders == null) return;

        foreach (var order in orders)
        {
            if (_actionProcessors.TryGetValue(order.ActionType.Value, out var processor))
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