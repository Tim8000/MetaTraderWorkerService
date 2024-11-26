using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public interface IOrderActionProcessor
{
    Task ProcessAsync(MetaTraderOrder order);
}