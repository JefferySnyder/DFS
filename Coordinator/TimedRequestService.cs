using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TimedRequestService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly SharedState _sharedState;

    public TimedRequestService(IHttpClientFactory httpClientFactory, SharedState sharedState)
    {
        _httpClient = httpClientFactory.CreateClient();
        _sharedState = sharedState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the server to fully start up
        //await Task.Delay(5000, stoppingToken);

        for (int i = 1; i <= _sharedState.StorageNodes.Capacity; i++) 
        {
            string node = "http://localhost:500" + i;
            try
            {
                var response = await _httpClient.GetAsync(node + "/ping");
            }
            catch (HttpRequestException)
            {
                _sharedState.StorageNodes.Remove(node);
                return;
            }
            if (!_sharedState.StorageNodes.Contains(node))
            {
                _sharedState.StorageNodes.Add(node);
            }
        }
    }
}
