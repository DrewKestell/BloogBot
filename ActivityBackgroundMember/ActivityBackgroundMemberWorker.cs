using Communication;
using PathfindingService.Client;
using PromptHandlingService;
using WoWSharpClient;
using WoWSharpClient.Client;
using WoWSharpClient.Manager;

namespace ActivityBackgroundMember
{
    public class ActivityBackgroundMemberWorker : BackgroundService
    {
        private readonly WoWClient _woWClient;
        private readonly ILogger<ActivityBackgroundMemberWorker> _logger;

        private readonly IPromptRunner _promptRunner;
        private readonly ActivitySnapshot _activitySnapshot;
        private readonly ObjectManager _objectManager;
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;

        private readonly PathfindingClient _pathfindingClient;

        private BotRunner.BotRunner _botRunner;

        private CancellationToken _stoppingToken;

        public ActivityBackgroundMemberWorker(ILoggerFactory loggerFactory, ILogger<ActivityBackgroundMemberWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _activitySnapshot = new();
            _woWSharpEventEmitter = new();
            _objectManager = new(_woWSharpEventEmitter, _activitySnapshot);
            _botRunner = new BotRunner.BotRunner(_objectManager, _woWSharpEventEmitter);

            _promptRunner = PromptRunnerFactory.GetOllamaPromptRunner(new Uri(configuration["Ollama:BaseUri"]), configuration["Ollama:Model"]);

            _pathfindingClient = new PathfindingClient(configuration["PathfindingService:IpAddress"], int.Parse(configuration["PathfindingService:Port"]), loggerFactory.CreateLogger<PathfindingClient>());

            _woWClient = new WoWClient(configuration["WoWLoginServer:IpAddress"], int.Parse(configuration["WoWLoginServer:Port"]), _woWSharpEventEmitter, _objectManager);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            try
            {
                _logger.LogInformation("Starting service");
                _woWClient.ConnectToLogin();

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        if (_objectManager.Objects.Any(x => x.Position.X != 0 || x.Position.Y != 0 || x.Position.Z != 0))
                        {
                            PathfindingService.Models.Position position1 = _objectManager.Units.OrderBy(x => x.Guid).First(x => x.Position.X != 0 || x.Position.Y != 0 || x.Position.Z != 0).Position;
                            PathfindingService.Models.Position position2 = _objectManager.Units.Last(x => x.Position.X != 0 || x.Position.Y != 0 || x.Position.Z != 0).Position;
                            PathfindingService.Models.Position[] positions = _pathfindingClient.GetPath(1, position1, position2, true);
                            Console.WriteLine($"Pathfinding result: {positions.Length} positions");
                        }
                    }
                    await Task.Delay(10000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ActivityBackgroundMemberWorker");
            }
        }        
    }
}
