using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Repository.Orders;

public interface IOrderRepository
{
    Task<List<MetaTraderOrder>?> GetAllCreatedOrdersAsync();
    Task UpdateOrderAsync(MetaTraderOrder order);
    Task<MetaTraderOrder?> GetOrderByInitialTradeSignalId(Guid metaTraderOrderId);
    Task<List<MetaTraderOrder>> GetSentToMetaTraderOrdersAsync();
    Task<List<MetaTraderOrder>> GetPlacedOrdersAsync();
    Task<MetaTraderOrder> GetOrderByMagicAndSymbolAsync(int tradeMagic, string tradeSymbol);
    Task<MetaTraderOrder> GetOrderByMetaTraderOrderId(string metaTraderOrderId, string tradeSymbol);
}