using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Models;

namespace MetaTraderWorkerService.Services;

public interface IMetaApiService
{
    Task InitializeAsync();

    Task<MetaTraderOpenTradeOrderResponseDto> PlacePendingOrderAsync(MetaTraderOpenTradeOrderRequestDto requestDto);
    Task<string> PlaceCancelOrderAsync(CancelOrderDto requestDto);
    Task<string> GetOrderStatusById(string? pendingOrderMetaTraderOrderId);
    Task<MetaTradePartialCloseResponseDto> ClosePartialPositionAsync(PartialCloseTradeOrderDto partialCloseDto);
    Task<ModifyOrderResponseDto> ModifyStopLossAsync(ModifyStopLossRequestDto modifyOrderDto);
    Task<List<TradeHistoryResponseDto>> GetTradeHistoryByPositionIdAsync(string positionId);
}