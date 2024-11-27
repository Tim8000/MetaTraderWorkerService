using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Enums;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Processors.BaseProcessors;
using MetaTraderWorkerService.Repository.Orders;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.OrderProcessors;

public class SellMarketProcessor : BaseOpenTradeProcessor
{
    public SellMarketProcessor(
        IOrderRepository orderRepository,
        IMetaApiService metaApiService,
        ILogger<SellMarketProcessor> logger)
        : base(orderRepository, metaApiService, logger)
    {
    }

    protected override void SetActionTypeForMarketOrder(MetaTraderOrder metaTraderOrder, OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        marketOrderDto.ActionType = ActionType.ORDER_TYPE_SELL.ToString();
        metaTraderOrder.ActionType = ActionType.ORDER_TYPE_SELL;
    }
}