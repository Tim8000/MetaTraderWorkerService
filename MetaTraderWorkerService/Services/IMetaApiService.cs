using MetaTraderWorkerService.Dtos;

namespace MetaTraderWorkerService.Services;

public interface IMetaApiService
{
    Task InitializeAsync();

    Task<MetaTraderOrderResponseDto> PlacePendingOrderAsync(MetaTraderOrderRequestDto requestDto);
}