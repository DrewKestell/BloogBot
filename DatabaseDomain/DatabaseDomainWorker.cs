namespace DatabaseDomain
{
    public class DatabaseDomainWorker(ILogger<DatabaseDomainWorker> logger, IConfiguration configuration) : BackgroundService
    {
        private readonly ILogger<DatabaseDomainWorker> _logger = logger;
        private readonly DatabaseSocketServer _databaseDomainSocketServer = new(configuration["DatabaseDomain:IpAddress"], int.Parse(configuration["DatabaseDomain:Port"]));

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
