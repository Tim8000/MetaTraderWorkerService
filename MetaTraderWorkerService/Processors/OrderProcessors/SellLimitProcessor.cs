using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Processors.BaseProcessors;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public class SellLimitProcessor : BaseOpenTradeProcessor
{
    public SellLimitProcessor(
        IOrderRepository orderRepository,
        IMetaApiService metaApiService,
        ILogger<SellLimitProcessor> logger) // Use ILogger<T>
        : base(orderRepository, metaApiService, logger) // Pass logger to base class
    {
    }

    protected override void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder,
        OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        marketOrderDto.ActionType = ActionType.ORDER_TYPE_SELL.ToString();
        metaTraderOrder.ActionType = ActionType.ORDER_TYPE_SELL;
    }
}