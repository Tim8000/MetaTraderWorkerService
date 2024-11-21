using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Processors;

public interface IOrderActionProcessor
{
    Task ProcessAsync(MetaTraderOrder order);
}