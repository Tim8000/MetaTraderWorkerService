using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Processors.ServiceOrderProcessors;

public interface IServiceOrderActionProcessor
{
    string GetSupportedActionType();
    Task ProcessAsync(ServiceOrder order);
}