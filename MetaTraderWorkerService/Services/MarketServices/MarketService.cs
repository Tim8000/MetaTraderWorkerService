using MetaTraderWorkerService.Dtos.Market;
using MetaTraderWorkerService.Http;
using MetaTraderWorkerService.Settings;
using Microsoft.Extensions.Options;

namespace MetaTraderWorkerService.Services.MarketServices;

using System.Text.Json;
using System.Threading.Tasks;

public class MarketService : IMarketService
{
    private readonly IHttpService _httpService;
    private readonly string _accountId;


    public MarketService(IOptions<MetaApiSettings> metaApiSettings, IHttpService httpService)
    {
        _httpService = httpService;
        var settings = metaApiSettings.Value;
        _accountId = settings.AccountId;
    }

    public async Task<SymbolPriceResponseDto> GetCurrentPriceAsync(string symbol)
    {
        var url = $"/users/current/accounts/{_accountId}/symbols/{symbol}/current-price";
        var response = await _httpService.GetAsync(url, false);

       var responseDto = JsonSerializer.Deserialize<SymbolPriceResponseDto>(response);

       return responseDto;
    }
}
