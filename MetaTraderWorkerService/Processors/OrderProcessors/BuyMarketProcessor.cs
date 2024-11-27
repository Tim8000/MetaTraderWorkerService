using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Processors.BaseProcessors;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public class BuyMarketProcessor : BaseOpenTradeProcessor
{
    public BuyMarketProcessor(
        IOrderRepository orderRepository,
        IMetaApiService metaApiService,
        ILogger<BuyMarketProcessor> logger)
        : base(orderRepository, metaApiService, logger)
    {
    }

    protected override void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder, OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        marketOrderDto.ActionType = ActionType.ORDER_TYPE_BUY.ToString();
        metaTraderOrder.ActionType = ActionType.ORDER_TYPE_BUY;
    }
}