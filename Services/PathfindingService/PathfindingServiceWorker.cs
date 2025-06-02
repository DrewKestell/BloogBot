namespace PathfindingService
{
    public class PathfindingServiceWorker : BackgroundService
    {
        private readonly ILogger<PathfindingServiceWorker> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        private readonly PathfindingSocketServer _pathfindingSocketServer;
        public PathfindingServiceWorker(
            ILogger<PathfindingServiceWorker> logger,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _configuration = configuration;

            _pathfindingSocketServer = new PathfindingSocketServer(
                configuration["PathfindingService:IpAddress"],
                int.Parse(configuration["PathfindingService:Port"]),
                _loggerFactory.CreateLogger<PathfindingSocketServer>()
            );

            _logger.LogInformation($"Started PathfindingService| {_configuration["PathfindingService:IpAddress"]}:{_configuration["PathfindingService:Port"]}");
        }

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
