using System.Net;
using BaseSocketMessanger;
using WoWActivityManager;

namespace WoWActivityManagerService
{
    public class WoWActivityManagerBackgroundService : BackgroundService
    {
        private readonly ILogger<WoWActivityManagerBackgroundService> _logger;
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly IPAddress _stateManagerAddress;
        private readonly int _stateManagerPort;
        private readonly WoWActivityManagerServer _manager;
        private Task _backgroundTask;

        public WoWActivityManagerBackgroundService(ILogger<WoWActivityManagerBackgroundService> logger, IConfiguration configuration)
        {
            _logger = logger;

            _listenAddress = configuration.GetIPAddressFromConfigOrEnv("LISTEN_ADDRESS", "AppSettings:ListenAddress");
            _listenPort = configuration.GetIntFromConfigOrEnv("LISTEN_PORT", "AppSettings:ListenPort");
            _stateManagerAddress = configuration.GetIPAddressFromConfigOrEnv("STATE_MANAGER_ADDRESS", "AppSettings:StateManagerAddress");
            _stateManagerPort = configuration.GetIntFromConfigOrEnv("STATE_MANAGER_PORT", "AppSettings:StateManagerPort");

            _logger?.LogInformation($"Worker created at: {DateTimeOffset.Now}");
            _logger?.LogInformation($"Activity Member Listener {_listenAddress}:{_listenPort}");
            _logger?.LogInformation($"World State Manager {_stateManagerAddress}:{_stateManagerPort}");

            // Use the configuration values in your WoWActivityManager
            _manager = new(_listenAddress, _listenPort, _stateManagerAddress, _stateManagerPort);
        }

        public WoWActivityManagerBackgroundService (IPAddress listenAddress, int listenPort, IPAddress stateManagerAddress, int stateManagerPort)
        {
            _listenAddress = listenAddress;
            _listenPort = listenPort;
            _stateManagerAddress = stateManagerAddress;
            _stateManagerPort = stateManagerPort;

            _manager = new(_listenAddress, _listenPort, _stateManagerAddress, _stateManagerPort);
        }
        public void Execute(CancellationToken cancellationToken)
        {
            _backgroundTask = Task.Run(async () => await ExecuteAsync(cancellationToken), cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now}|[WoWActivityManagerWorker]Executing update loop to WorldStateManager {_stateManagerAddress}:{_stateManagerPort}");

            while (!cancellationToken.IsCancellationRequested)
            {
                _manager.UpdateCurrentState(cancellationToken);

                // Your worker logic here
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}