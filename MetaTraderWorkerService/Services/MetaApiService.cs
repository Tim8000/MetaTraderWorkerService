using System.Text;
using MetaTraderWorkerService.Dtos;
using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Http;
using MetaTraderWorkerService.Models;
using Newtonsoft.Json;

namespace MetaTraderWorkerService.Services;

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

    public async Task<MetaTraderOpenTradeOrderResponseDto> PlacePendingOrderAsync(
        MetaTraderOpenTradeOrderRequestDto requestDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";

        var jsonData = JsonConvert.SerializeObject(requestDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        _logger.LogInformation($"Requesting URL:{url} for trade {requestDto.ActionType}");
        var result = await _httpService.PostAsync(url, content, false);

        var orderResponse = JsonConvert.DeserializeObject<MetaTraderOpenTradeOrderResponseDto>(result);

        return orderResponse;
    }

    public async Task<string> PlaceCancelOrderAsync(CancelOrderDto requestDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";

        var jsonData = JsonConvert.SerializeObject(requestDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var result = await _httpService.PostAsync(url, content, false);

        var orderResponse = JsonConvert.DeserializeObject<MetaTraderOpenTradeOrderResponseDto>(result);

        return orderResponse?.Message; // Assuming MetaTraderOpenTradeOrderResponseDto has a Message field
    }

    public Task<string> GetOrderStatusById(string? pendingOrderMetaTraderOrderId)
    {
        var url = $"/users/current/accounts/{_accountId}/orders/{pendingOrderMetaTraderOrderId}";

        var result = _httpService.GetAsync(url, false);

        return result;
    }

    public async Task<MetaTradePartialCloseResponseDto> ClosePartialPositionAsync(
        PartialCloseTradeOrderDto partialCloseDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";
        var jsonData = JsonConvert.SerializeObject(partialCloseDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var result = await _httpService.PostAsync(url, content, false);
        return JsonConvert.DeserializeObject<MetaTradePartialCloseResponseDto>(result);
    }

    public async Task<ModifyOrderResponseDto> ModifyStopLossAsync(ModifyStopLossRequestDto modifyOrderDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";
        var jsonData = JsonConvert.SerializeObject(modifyOrderDto);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var result = await _httpService.PostAsync(url, content, false);
        return JsonConvert.DeserializeObject<ModifyOrderResponseDto>(result);
    }

    public async Task<List<TradeHistoryResponseDto>> GetTradeHistoryByPositionIdAsync(string positionId)
    {
        var url = @$"/users/current/accounts/{_accountId}/history-deals/position/{positionId}";

        // Send the request
        var response = await _httpService.GetAsync(url, false);

        // Handle null or error response
        if (string.IsNullOrEmpty(response))
        {
            _logger.LogError($"No response or null response received for position ID: {positionId}");
            return new List<TradeHistoryResponseDto>();
        }

        try
        {
            // Deserialize the response into a list of TradeHistoryResponseDto
            var trades = JsonConvert.DeserializeObject<List<TradeHistoryResponseDto>>(response);
            return trades ?? new List<TradeHistoryResponseDto>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Failed to deserialize response for position ID: {positionId}");
            return new List<TradeHistoryResponseDto>();
        }
    }

    public async Task<OpenTradeByMarketPriceResponseDto> OpenTradeByMarketPriceAsync(
        OpenTradeByMarketPriceRequestDto marketOrderDto)
    {
        var url = $"users/current/accounts/{_accountId}/trade";

        // Serialize the DTO to JSON
        var jsonData = JsonConvert.SerializeObject(marketOrderDto);

        // Create HTTP content for the POST request
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        _logger.LogInformation(
            $"Sending request to URL: {url} to open trade by market price for symbol {marketOrderDto.Symbol}");

        try
        {
            // Send the HTTP POST request
            var result = await _httpService.PostAsync(url, content, false);

            // Deserialize the response to OpenTradeByMarketPriceResponseDto
            var response = JsonConvert.DeserializeObject<OpenTradeByMarketPriceResponseDto>(result);

            if (response == null)
            {
                _logger.LogError("Failed to deserialize response from MetaApi for market order.");
                return null;
            }

            // Log the successful response
            _logger.LogInformation(
                $"Market order created successfully. Symbol: {marketOrderDto.Symbol}, Volume: {marketOrderDto.Volume}, ActionType: {marketOrderDto.ActionType}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while placing market order via MetaApi.");
            throw;
        }
    }
}