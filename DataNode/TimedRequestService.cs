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

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _httpClient.GetAsync($"http://localhost:5000/ping/{_sharedState.Port}", stoppingToken);
        }
    }
}
