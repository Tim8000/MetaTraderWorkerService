namespace MetaTraderWorkerService.Processors.OrderProcessors;

public interface IOrderProcessor
{
    Task ProcessCreatedOrdersAsync();
}