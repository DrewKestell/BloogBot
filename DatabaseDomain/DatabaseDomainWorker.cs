namespace DatabaseDomain
{
    public class DatabaseDomainWorker : BackgroundService
    {
        private readonly ILogger<DatabaseDomainWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        private readonly DatabaseSocketServer _databaseDomainSocketServer;

        public DatabaseDomainWorker(ILogger<DatabaseDomainWorker> logger, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _loggerFactory = loggerFactory;

            // Use the logger factory to create a logger for DatabaseSocketServer if needed
            var databaseSocketServerLogger = _loggerFactory.CreateLogger<DatabaseSocketServer>();
            _databaseDomainSocketServer = new DatabaseSocketServer(configuration["DatabaseDomain:IpAddress"], int.Parse(configuration["DatabaseDomain:Port"]), databaseSocketServerLogger);

            _logger.LogInformation($"Started DatabaseDomain| {configuration["DatabaseDomain:IpAddress"]}:{configuration["DatabaseDomain:Port"]}");
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
