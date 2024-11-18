using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Services.Processors;

public interface IOrderActionProcessor
{
    Task ProcessAsync(MetaTraderOrder order);
}