using MetaTraderWorkerService.Data;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using Microsoft.EntityFrameworkCore;

namespace MetaTraderWorkerService.Repository.ServiceOrders;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ServiceOrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ServiceOrder>> GetPendingServiceOrdersAsync()
    {
        return await _dbContext.Set<ServiceOrder>()
            .Where(order => order.Status == ServiceOrderStatus.Pending)
            .Include(o => o.MetaTraderTrade)
            .ToListAsync();
    }

    public async Task AddAsync(ServiceOrder serviceOrder)
    {
        await _dbContext.Set<ServiceOrder>().AddAsync(serviceOrder);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(ServiceOrder serviceOrder)
    {
        _dbContext.Set<ServiceOrder>().Update(serviceOrder);
        await _dbContext.SaveChangesAsync();
    }
}