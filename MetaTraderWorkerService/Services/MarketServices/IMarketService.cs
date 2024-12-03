using MetaTraderWorkerService.Dtos.Market;

namespace MetaTraderWorkerService.Services.MarketServices;

public interface IMarketService
{
    /// <summary>
    /// Retrieves the current price of the specified trading symbol.
    /// </summary>
    /// <param name="accountId">The MetaApi account ID.</param>
    /// <param name="symbol">The trading symbol (e.g., "EURUSD").</param>
    /// <returns>The current price of the trading symbol.</returns>
    Task<SymbolPriceResponseDto> GetCurrentPriceAsync(string symbol);
}