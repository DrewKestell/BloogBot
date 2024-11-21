namespace DecisionEngine
{
    public class DecisionEngineWorker : BackgroundService
    {
        private readonly ILogger<DecisionEngineWorker> _logger;

        public DecisionEngineWorker(ILogger<DecisionEngineWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
