namespace MetaTraderWorkerService.Services;

public interface IOrderProcessor
{
    Task ProcessCreatedOrdersAsync();
}