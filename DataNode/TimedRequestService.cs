using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TimedRequestService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly TimedRequestSettings _settings;

    public TimedRequestService(IHttpClientFactory httpClientFactory, TimedRequestSettings settings)
    {
        _httpClient = httpClientFactory.CreateClient();
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the server to fully start up
        //await Task.Delay(5000, stoppingToken); 

        // Add try-catch?
        var response = await _httpClient.GetAsync($"http://localhost:5000/ping/{_settings.Args.First()}", stoppingToken);
    }
}
