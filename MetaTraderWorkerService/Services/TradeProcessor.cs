using MetaTraderWorkerService.Dtos.Mt5Trades;
using MetaTraderWorkerService.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace MetaTraderWorkerService.Services
{
    public class TradeProcessor : ITradeProcessor
    {
        private readonly IHttpService _httpService;
        private readonly string _accountId;

        private const string GetPositionsEndpoint = "/users/current/accounts/{0}/positions";
        private const string CreateTradeEndpoint = "/users/current/accounts/{0}/trade";

        public TradeProcessor(IHttpService httpService, IConfiguration configuration)
        {
            _httpService = httpService;

            // Retrieve accountId from the configuration
            _accountId = configuration["MetaApi:ProvisioningProfileId"];
            if (string.IsNullOrEmpty(_accountId))
            {
                throw new ArgumentException("Account ID is not configured in appsettings.");
            }
        }

        public async Task<List<TradeStatusResponseDto>> GetActiveTradesAsync()
        {
            var url = string.Format(GetPositionsEndpoint, _accountId);
            var response = await _httpService.GetAsync(url, isAccountRequest: false);



            var trades = JsonConvert.DeserializeObject<List<TradeStatusResponseDto>>(response);
            return trades;
        }
    }
}