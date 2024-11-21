using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Processors.BaseProcessors;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors;

public class BuyLimitProcessor : BaseOpenTradeProcessor
{
    public BuyLimitProcessor(
        IOrderRepository orderRepository,
        IMetaApiService metaApiService,
        ILogger<BuyLimitProcessor> logger) // Use ILogger<T>
        : base(orderRepository, metaApiService, logger) // Pass logger to base class
    {
    }


    protected override void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder, OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        marketOrderDto.ActionType = ActionType.ORDER_TYPE_BUY.ToString();
        metaTraderOrder.ActionType = ActionType.ORDER_TYPE_BUY;
    }
}