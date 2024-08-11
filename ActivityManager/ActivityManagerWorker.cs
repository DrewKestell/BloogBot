using ActivityManager.Clients;
using BotCommLayer;
using Communication;
using Microsoft.Extensions.Options;

namespace ActivityManager
{
    public class ActivityManagerWorker : BackgroundService
    {
        private readonly ILogger<ActivityManagerWorker> _logger;
        private readonly StateManagerActivityClient _stateManagerActivityClient;
        private readonly Activity _activity;
        public int Port { get; }

        public ActivityManagerWorker(ILogger<ActivityManagerWorker> logger, ILoggerFactory loggerFactory, IOptions<AppSettings> settings, IConfiguration configuration)
        {
            _logger = logger;
            Port = (int)settings.Value.ListenPort;

            var stateManagerLogger = loggerFactory.CreateLogger<StateManagerActivityClient>();
            _stateManagerActivityClient = new StateManagerActivityClient(
                settings.Value.StateManagerAddress,
                (int)settings.Value.StateManagerPort,
                stateManagerLogger
            );

            _activity = new Activity()
            {
                Port = (uint)Port,
            };

            _logger?.LogInformation($"Worker {Port} created at: {DateTimeOffset.Now}");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Activity activity = _stateManagerActivityClient.UpdateCurrentActivityState(_activity);
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
