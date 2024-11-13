namespace MetaTraderWorkerService.Http;

public interface IHttpService
{
    Task<string> GetAsync(string url, bool isAccountRequest = true);
    Task<string> PostAsync(string url, HttpContent content, bool isAccountRequest = true);
}
