using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services.Processors.BaseProcessors;

namespace MetaTraderWorkerService.Services.Processors;

public class BuyLimitProcessor : BaseOpenTradeProcessor
{
    public BuyLimitProcessor(IOrderRepository orderRepository, ILogger logger, IMetaApiService metaApiService) : base(orderRepository, logger, metaApiService)
    {
    }


    protected override void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder, OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        marketOrderDto.ActionType = ActionType.ORDER_TYPE_BUY.ToString();
        metaTraderOrder.ActionType = ActionType.ORDER_TYPE_BUY;
    }
}