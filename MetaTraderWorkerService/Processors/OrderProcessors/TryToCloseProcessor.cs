using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public class TryToCloseProcessor : IOrderActionProcessor
{
    private readonly IOrderRepository _orderRepository;

    public TryToCloseProcessor(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task ProcessAsync(MetaTraderOrder order)
    {
        if (order.ActionType == ActionType.POSITION_CLOSE_ID && order.SignalCommandCode == SignalCommandCode.TryToCloseTrade)
        {
            order.Status = OrderStatus.PendingTryToCLose;
            order.Comment = "Try to Close Trade in progress";
            await _orderRepository.UpdateOrderAsync(order);
        }
    }
}