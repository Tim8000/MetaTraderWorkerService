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

    public async Task<MetaTraderOrderResponseDto> PlacePendingOrderAsync(MetaTraderOrderRequestDto requestDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";

        var jsonData = JsonConvert.SerializeObject(requestDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var result = await _httpService.PostAsync(url, content, false);

        var orderResponse = JsonConvert.DeserializeObject<MetaTraderOrderResponseDto>(result);

        return orderResponse;
    }
}