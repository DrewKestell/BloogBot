using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BaseSocketMessanger;

namespace WoWActivityManagerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly IPAddress _stateManagerAddress;
        private readonly int _stateManagerPort;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;

            _listenAddress = configuration.GetIPAddressFromConfigOrEnv("LISTEN_ADDRESS", "AppSettings:ListenAddress");
            _listenPort = configuration.GetIntFromConfigOrEnv("LISTEN_PORT", "AppSettings:ListenPort");
            _stateManagerAddress = configuration.GetIPAddressFromConfigOrEnv("STATE_MANAGER_ADDRESS", "AppSettings:StateManagerAddress");
            _stateManagerPort = configuration.GetIntFromConfigOrEnv("STATE_MANAGER_PORT", "AppSettings:StateManagerPort");
        }

        public Worker (IPAddress listenAddress, int listenPort, IPAddress stateManagerAddress, int stateManagerPort)
        {
            _listenAddress = listenAddress;
            _listenPort = listenPort;
            _stateManagerAddress = stateManagerAddress;
            _stateManagerPort = stateManagerPort;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Execute(cancellationToken);
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            // Use the configuration values in your WoWActivityManager
            WoWActivityManager.WoWActivityManager manager = new WoWActivityManager.WoWActivityManager(
                _listenAddress, _listenPort, _stateManagerAddress, _stateManagerPort);

            while (!cancellationToken.IsCancellationRequested)
            {
                await manager.UpdateCurrentState(cancellationToken);

                // Your worker logic here
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}