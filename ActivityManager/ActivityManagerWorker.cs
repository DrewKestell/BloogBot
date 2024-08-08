using BotCommLayer;
using Microsoft.Extensions.Options;

namespace ActivityManager
{
    public class ActivityManagerWorker : BackgroundService
    {
        private readonly ILogger<ActivityManagerWorker> _logger;
        private Task _backgroundTask;
        public int Port { get; }

        public ActivityManagerWorker(ILogger<ActivityManagerWorker> logger, IOptions<AppSettings> settings)
        {
            _logger = logger;
            Port = (int)settings.Value.ListenPort;

            _logger?.LogInformation($"Worker created at: {DateTimeOffset.Now}");
        }

        public void Execute(CancellationToken cancellationToken)
        {
            _backgroundTask = Task.Run(async () => await ExecuteAsync(cancellationToken), cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[WoWActivityManagerWorker]Executing update loop to WorldStateManager");

            while (!cancellationToken.IsCancellationRequested)
            {
                // Your worker logic here
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
