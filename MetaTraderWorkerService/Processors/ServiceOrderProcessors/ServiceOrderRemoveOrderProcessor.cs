using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Services;

namespace MetaTraderWorkerService.Processors.ServiceOrderProcessors;

public class ServiceOrderRemoveOrderProcessor : IServiceOrderActionProcessor
{
    private readonly IMetaApiService _metaApiService;

    public ServiceOrderRemoveOrderProcessor(IMetaApiService metaApiService)
    {
        _metaApiService = metaApiService;
    }

    public string GetSupportedActionType()
    {
        return "ORDER_CANCEL";
    }

    public async Task ProcessAsync(ServiceOrder order)
    {
        var cancelOrderDto = new CancelOrderDto()
        {
            ActionType = order.ActionType,
            OrderId = order.Id.ToString()
        };

        var response = await _metaApiService.PlaceCancelOrderAsync(cancelOrderDto);
    }
}