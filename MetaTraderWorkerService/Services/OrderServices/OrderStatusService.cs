using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Repository.Orders;
using Newtonsoft.Json;

namespace MetaTraderWorkerService.Services.OrderServices;

public class OrderStatusService : IOrderStatusService
{
    private readonly ILogger<OrderStatusService> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IMetaApiService _metaApiService;

    public OrderStatusService(ILogger<OrderStatusService> logger, IOrderRepository orderRepository,
        IMetaApiService metaApiService)
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

                pendingOrder.OrderState = orderStatus.State;
                await _orderRepository.UpdateOrderAsync(pendingOrder);
            }
        }
    }
}