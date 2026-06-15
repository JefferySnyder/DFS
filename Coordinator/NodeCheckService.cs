using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class NodeCheckService : BackgroundService
{
    private readonly CoorShared _shared;

    public NodeCheckService(CoorShared shared)
    {
        _shared = shared;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the server to fully start up
        //await Task.Delay(5000, stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var ageLimit = DateTime.Now.AddSeconds(-10);
            var expiredNodes = _shared.StorageNodes.FindAll(x => x.Age < ageLimit);
            foreach (var node in expiredNodes)
            {
                _shared.StorageNodes.Remove(node);
                Console.WriteLine($"Removed {node} at {DateTime.Now}");
            }
        }
    }
}
