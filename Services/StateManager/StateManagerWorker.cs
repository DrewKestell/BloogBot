using BackgroundBotRunner;
using Communication;
using StateManager.Clients;
using StateManager.Listeners;
using StateManager.Repository;
using StateManager.Settings;

namespace StateManager
{
    public class StateManagerWorker : BackgroundService
    {
        private readonly ILogger<StateManagerWorker> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private readonly CharacterStateSocketListener _activityMemberSocketListener;
        private readonly StateManagerSocketListener _worldStateManagerSocketListener;

        private readonly MangosSOAPClient _mangosSOAPClient;

        private readonly Dictionary<string, (IHostedService Service, CancellationTokenSource TokenSource, Task asyncTask)> _managedServices = [];

        public StateManagerWorker(
            ILogger<StateManagerWorker> logger,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _mangosSOAPClient = new MangosSOAPClient(configuration["MangosSOAP:IpAddress"]);

            _activityMemberSocketListener = new CharacterStateSocketListener(
                StateManagerSettings.Instance.CharacterDefinitions,
                configuration["CharacterStateListener:IpAddress"],
                int.Parse(configuration["CharacterStateListener:Port"]),
                _loggerFactory.CreateLogger<CharacterStateSocketListener>()
            );

            _logger.LogInformation($"Started ActivityMemberListener| {configuration["CharacterStateListener:IpAddress"]}:{configuration["CharacterStateListener:Port"]}");

            _worldStateManagerSocketListener = new StateManagerSocketListener(
                configuration["StateManagerListener:IpAddress"],
                int.Parse(configuration["StateManagerListener:Port"]),
                _loggerFactory.CreateLogger<StateManagerSocketListener>()
            );

            _logger.LogInformation($"Started StateManagerListener| {configuration["StateManagerListener:IpAddress"]}:{configuration["StateManagerListener:Port"]}");

            _worldStateManagerSocketListener.DataMessageSubject.Subscribe(OnWorldStateUpdate);
        }

        public void StartBackgroundBotWorker(string accountName)
        {
            var scope = _serviceProvider.CreateScope();
            var tokenSource = new CancellationTokenSource();
            var service = ActivatorUtilities.CreateInstance<BackgroundBotWorker>(
                scope.ServiceProvider,
                _loggerFactory,
                _configuration
            );

            _managedServices.Add(accountName, (service, tokenSource, Task.Run(async () => await service.StartAsync(tokenSource.Token))));
            _logger.LogInformation($"Started ActivityManagerService for account {accountName}");
        }

        public void StopManagedService(string accountName)
        {
            if (_managedServices.TryGetValue(accountName, out var serviceTuple))
            {
                serviceTuple.TokenSource.Cancel();
                Task.Factory.StartNew(async () => await serviceTuple.Service.StopAsync(CancellationToken.None));
                _managedServices.Remove(accountName);
                _logger.LogInformation($"Stopped ActivityManagerService for account {accountName}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"StateManagerServiceWorker is running.");

            stoppingToken.Register(() => _logger.LogInformation($"StateManagerServiceWorker is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                // Here you can add logic to start/stop services based on certain conditions.
                await ApplyDesiredWorkerState();
                await Task.Delay(100, stoppingToken);
            }

            foreach (var (Service, TokenSource, Task) in _managedServices.Values)
                await Service.StopAsync(stoppingToken);

            _logger.LogInformation($"StateManagerServiceWorker has stopped.");
        }

        private void OnWorldStateUpdate(AsyncRequest dataMessage)
        {
            StateChangeRequest stateChange = dataMessage.StateChange;

            if (stateChange != null)
            {

            }

            StateChangeResponse stateChangeResponse = new();
            _worldStateManagerSocketListener.SendMessageToClient(dataMessage.Id, stateChangeResponse);
        }

        private async Task<bool> ApplyDesiredWorkerState()
        {
            for (int i = 0; i < StateManagerSettings.Instance.CharacterDefinitions.Count; i++)
                if (!_managedServices.ContainsKey(StateManagerSettings.Instance.CharacterDefinitions[i].AccountName))
                {
                    if (!ReamldRepository.CheckIfAccountExists(StateManagerSettings.Instance.CharacterDefinitions[i].AccountName))
                    {
                        await _mangosSOAPClient.CreateAccountAsync(StateManagerSettings.Instance.CharacterDefinitions[i].AccountName);

                        await Task.Delay(100);

                        await _mangosSOAPClient.SetGMLevelAsync(StateManagerSettings.Instance.CharacterDefinitions[i].AccountName, 3);
                    }
                    StartBackgroundBotWorker(StateManagerSettings.Instance.CharacterDefinitions[i].AccountName);

                    await Task.Delay(500);
                }

            return true;
        }
    }
}
