using MetaTraderWorkerService.Dtos;

namespace MetaTraderWorkerService.Services;

public interface IMetaApiService
{
    Task InitializeAsync();

    Task<MetaTraderOrderResponseDto> PlacePendingOrderAsync(MetaTraderOpenTradeOrderRequestDto requestDto);
    Task<string> PlaceCancelOrderAsync(CancelOrderDto requestDto);
    Task<string> GetOrderStatusById(string? pendingOrderMetaTraderOrderId);
}