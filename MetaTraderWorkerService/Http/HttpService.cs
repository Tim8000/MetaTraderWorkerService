using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MetaTraderWorkerService.Http
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _accountClient;
        private readonly HttpClient _tradeClient;

        public HttpService(string accountBaseUrl, string tradeBaseUrl, string authToken)
        {
            var handler = new HttpClientHandler
            {
                // Bypass SSL certificate validation for development purposes (not for production use)
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _accountClient = new HttpClient(handler) { BaseAddress = new Uri(accountBaseUrl) };
            _tradeClient = new HttpClient(handler) { BaseAddress = new Uri(tradeBaseUrl) };

            // Add authentication and accept headers to both clients
            ConfigureClient(_accountClient, authToken);
            ConfigureClient(_tradeClient, authToken);
        }

        private void ConfigureClient(HttpClient client, string authToken)
        {
            client.DefaultRequestHeaders.Add("auth-token", authToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetAsync(string url, bool isAccountRequest = true)
        {
            var client = isAccountRequest ? _accountClient : _tradeClient;
            Console.WriteLine($"Requesting URL: {client.BaseAddress}{url}");

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to get data. Status Code: {response.StatusCode}, Error: {errorContent}");
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string url, HttpContent content, bool isAccountRequest = true)
        {
            var client = isAccountRequest ? _accountClient : _tradeClient;
            Console.WriteLine($"Requesting URL: {client.BaseAddress}{url}");

            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to post data. Status Code: {response.StatusCode}, Error: {errorContent}");
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
