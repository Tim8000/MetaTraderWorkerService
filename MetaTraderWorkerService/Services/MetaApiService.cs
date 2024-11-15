using System.Text;
using System.Threading.Tasks;
using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Http;
using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class MetaApiService : IMetaApiService
{
    private readonly IHttpService _httpService;
    private readonly ILogger<MetaApiService> _logger;
    private readonly string _accountId;

    public MetaApiService(IHttpService httpService, string accountId, ILogger<MetaApiService> logger)
    {
        _httpService = httpService;
        _logger = logger;
        _accountId = accountId;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing MetaApi connection...");

        var url = $"users/current/accounts/{_accountId}";
        _logger.LogInformation("Requesting URL: {0}", url);

        var result = await _httpService.GetAsync(url);
        if (result != null)
            _logger.LogInformation("Account data: {0}", result);
        else
            _logger.LogError("Failed to retrieve account data.");
    }

    public async Task<MetaTraderOrderResponseDto> PlacePendingOrderAsync(MetaTraderOpenTradeOrderRequestDto requestDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";

        var jsonData = JsonConvert.SerializeObject(requestDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var result = await _httpService.PostAsync(url, content, false);

        var orderResponse = JsonConvert.DeserializeObject<MetaTraderOrderResponseDto>(result);

        return orderResponse;
    }

    public async Task<string> PlaceCancelOrderAsync(CancelOrderDto requestDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";

        var jsonData = JsonConvert.SerializeObject(requestDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var result = await _httpService.PostAsync(url, content, false);

        var orderResponse = JsonConvert.DeserializeObject<MetaTraderOrderResponseDto>(result);

        return orderResponse?.Message; // Assuming MetaTraderOrderResponseDto has a Message field
    }

    public Task<string> GetOrderStatusById(string? pendingOrderMetaTraderOrderId)
    {
        var url = $"/users/current/accounts/{_accountId}/orders/{pendingOrderMetaTraderOrderId}";

        var result = _httpService.GetAsync(url, false);

        return result;
    }
}