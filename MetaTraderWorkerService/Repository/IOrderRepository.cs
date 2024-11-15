using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Repository;

public interface IOrderRepository
{
    Task<List<MetaTraderOrder>?> GetAllCreatedOrdersAsync();
    Task UpdateOrderAsync(MetaTraderOrder order);
    Task<MetaTraderOrder?> GetOrderByInitialTradeSignalId(Guid metaTraderOrderId);
    Task<List<MetaTraderOrder>> GetSentToMetaTraderOrdersAsync();
    Task<List<MetaTraderOrder>> GetPlacedOrdersAsync();
}