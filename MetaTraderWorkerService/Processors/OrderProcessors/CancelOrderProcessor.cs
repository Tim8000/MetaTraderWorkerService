using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public class CancelOrderProcessor : IOrderActionProcessor
{
    private readonly ILogger<CancelOrderProcessor> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;

    public CancelOrderProcessor(ILogger<CancelOrderProcessor> logger, IOrderRepository orderRepository, IMetaApiService metaApiService)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
    }

    public async Task ProcessAsync(MetaTraderOrder metaTraderOrder)
    {
        var orders = await _orderRepository.GetPlacedOrdersAsync();

        var order = orders.FirstOrDefault(o => o.OpenPrice == metaTraderOrder.OpenPrice && o.Symbol == metaTraderOrder.Symbol && o.MetaTraderInitialTradeSignal.Id == metaTraderOrder.MetaTraderInitialTradeSignal.Id);



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
}