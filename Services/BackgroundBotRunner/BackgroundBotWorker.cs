using BotRunner;
using BotRunner.Clients;
using PromptHandlingService;
using WoWSharpClient;
using WoWSharpClient.Client;

namespace BackgroundBotRunner
{
    public class BackgroundBotWorker : BackgroundService
    {
        private readonly ILogger<BackgroundBotWorker> _logger;

        private readonly IPromptRunner _promptRunner;

        private readonly PathfindingClient _pathfindingClient;
        private readonly WoWClient _wowClient;
        private readonly CharacterStateUpdateClient _characterStateUpdateClient;

        private readonly BotRunnerService _botRunner;

        private CancellationToken _stoppingToken;

        public BackgroundBotWorker(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<BackgroundBotWorker>();

            _promptRunner = PromptRunnerFactory.GetOllamaPromptRunner(new Uri(configuration["Ollama:BaseUri"]), configuration["Ollama:Model"]);

            _pathfindingClient = new PathfindingClient(configuration["PathfindingService:IpAddress"], int.Parse(configuration["PathfindingService:Port"]), loggerFactory.CreateLogger<PathfindingClient>());
            _characterStateUpdateClient = new CharacterStateUpdateClient(configuration["CharacterStateListener:IpAddress"], int.Parse(configuration["CharacterStateListener:Port"]), loggerFactory.CreateLogger<CharacterStateUpdateClient>());
            _wowClient = new();
            _wowClient.SetIpAddress(configuration["RealmEndpoint:IpAddress"]);
            WoWSharpObjectManager.Instance.Initialize(_wowClient, _pathfindingClient, loggerFactory.CreateLogger<WoWSharpObjectManager>());
            _botRunner = new BotRunnerService(WoWSharpObjectManager.Instance, _characterStateUpdateClient, _pathfindingClient);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            try
            {
                _botRunner.Start();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(100, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ActivityBackgroundMemberWorker");
            }
        }
    }
}
