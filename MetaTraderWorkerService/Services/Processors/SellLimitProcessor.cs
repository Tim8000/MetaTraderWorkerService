using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services.OrderServices;
using MetaTraderWorkerService.Services.Processors.BaseProcessors;

namespace MetaTraderWorkerService.Services.Processors;

public class SellLimitProcessor : BaseOpenTradeProcessor
{
    public SellLimitProcessor(IOrderRepository orderRepository, ILogger<SellLimitProcessor> logger, IMetaApiService metaApiService)
        : base(orderRepository, logger, metaApiService)
    {
    }

    protected override void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder, OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        marketOrderDto.ActionType = ActionType.ORDER_TYPE_SELL.ToString();
        metaTraderOrder.ActionType = ActionType.ORDER_TYPE_SELL;
    }
}
