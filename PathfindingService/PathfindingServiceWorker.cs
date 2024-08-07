namespace PathfindingService
{
    public class PathfindingServiceWorker(ILogger<PathfindingServiceWorker> logger, IConfiguration configuration) : BackgroundService
    {
        private readonly ILogger<PathfindingServiceWorker> _logger = logger;
        private readonly PathfindingSocketServer _pathfindingSocketServer = new(configuration["PathfindingService:IpAddress"], int.Parse(configuration["PathfindingService:Port"]));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_logger.IsEnabled(LogLevel.Information))
                //{
                //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //}
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
