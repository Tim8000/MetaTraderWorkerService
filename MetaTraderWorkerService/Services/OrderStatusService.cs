using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Repository;
using Newtonsoft.Json;
using TradeOrderProcessor.Enums;

namespace MetaTraderWorkerService.Services;

public class OrderStatusService : IOrderStatusService
{
    private readonly ILogger<OrderStatusService> _logger;
    private readonly IOrderRepository _orderRepository;
private readonly IMetaApiService _metaApiService;

    public OrderStatusService(ILogger<OrderStatusService> logger, IOrderRepository orderRepository, IMetaApiService metaApiService)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _metaApiService = metaApiService;
    }

    public async Task CheckOrderStatus()
    {
        var pendingOrders = await _orderRepository.GetSentToMetaTraderOrdersAsync();

        foreach (var pendingOrder in pendingOrders)
        {
            var response = await _metaApiService.GetOrderStatusById(pendingOrder.MetaTraderOrderId);

            if (!string.IsNullOrWhiteSpace(response))
            {
                var orderStatus = JsonConvert.DeserializeObject<OrderStatusResponseDto>(response);

                pendingOrder.Status = orderStatus.State;
                await _orderRepository.UpdateOrderAsync(pendingOrder);
            }

            if (pendingOrder.Status != OrderStatus.ORDER_STATE_PLACED)
            {
                pendingOrder.Status = OrderStatus.ORDER_STATE_PLACED;
                await _orderRepository.UpdateOrderAsync(pendingOrder);
            }

            var placedOrders = await _orderRepository.GetPlacedOrdersAsync();

            foreach (var placedOrder in placedOrders)
            {
                var placedOrderStatus = await _metaApiService.GetOrderStatusById(placedOrder.MetaTraderOrderId);
            }

        }
    }
}