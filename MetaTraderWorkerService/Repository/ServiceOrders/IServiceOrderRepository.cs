using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Repository.ServiceOrders;

public interface IServiceOrderRepository
{
    Task<List<ServiceOrder>> GetPendingServiceOrdersAsync();
    Task AddAsync(ServiceOrder serviceOrder);
    Task UpdateAsync(ServiceOrder serviceOrder);
}