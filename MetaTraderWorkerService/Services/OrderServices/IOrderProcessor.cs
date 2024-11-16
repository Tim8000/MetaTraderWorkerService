namespace MetaTraderWorkerService.Services.OrderServices;

public interface IOrderProcessor
{
    Task ProcessCreatedOrdersAsync();
}