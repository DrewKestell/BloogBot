using Communication;
using Microsoft.Extensions.Options;
using WoWSharpClient;
using WoWSharpClient.Client;
using WoWSharpClient.Manager;
using WoWSharpClient.Models;

namespace ActivityBackgroundMember
{
    public class ActivityBackgroundMemberWorker : BackgroundService
    {
        private readonly WoWClient _woWClient;
        private readonly ILogger<ActivityBackgroundMemberWorker> _logger;
        private readonly ActivityMember _activityMember;
        private readonly ObjectManager _objectManager;
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;

        public ActivityBackgroundMemberWorker(ILogger<ActivityBackgroundMemberWorker> logger, IConfiguration configuration, IOptions<ActivityMember> options)
        {
            _logger = logger;
            _activityMember = options.Value;
            _woWSharpEventEmitter = new();
            _objectManager = new(_woWSharpEventEmitter, new ActivityMemberState());

            _woWSharpEventEmitter.OnHandshakeBegin += Instance_OnHandshakeBegin;
            _woWSharpEventEmitter.OnLoginConnect += Instance_OnLoginConnect;
            _woWSharpEventEmitter.OnLoginSuccess += Instance_OnLoginSuccess;
            _woWSharpEventEmitter.OnLoginFailure += Instance_OnLoginFailure;
            _woWSharpEventEmitter.OnCharacterListLoaded += Instance_OnCharacterListLoaded;

            _woWClient = new WoWClient(configuration["WoWLoginServer:IpAddress"], int.Parse(configuration["WoWLoginServer:Port"]), _woWSharpEventEmitter, _objectManager);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting service");
                _woWClient.ConnectToLogin();

                while (!stoppingToken.IsCancellationRequested)
                {
                    //if (_logger.IsEnabled(LogLevel.Information))
                    //{
                    //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    //}
                    await Task.Delay(1000, stoppingToken);
                }
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ActivityBackgroundMemberWorker");
            }
        }
        private void Instance_OnLoginConnect(object? sender, EventArgs e)
        {
            _logger.LogInformation($"[Main] {_activityMember.AccountName} Connected to WoW Login server");
            _woWClient.Login(_activityMember.AccountName, "password");
        }

        private void Instance_OnHandshakeBegin(object? sender, EventArgs e)
        {
            _logger.LogInformation($"Starting handshake for {_activityMember.AccountName}");
        }

        private void Instance_OnLoginSuccess(object? sender, EventArgs e)
        {
            List<Realm> realms = _woWClient.GetRealmList();

            if (realms.Count > 0)
            {
                _woWClient.SelectRealm(realms[0]);

                if (_objectManager.HasRealmSelected)
                {
                    _woWClient.RefreshCharacterSelects();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Main]No realm selected");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Main]No realms listed");
            }
        }

        private void Instance_OnLoginFailure(object? sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Main]Failed to login to WoW server");
            _woWClient.Dispose();
        }

        private async void Instance_OnCharacterListLoaded(object? sender, EventArgs e)
        {
            await Task.Delay(100);

            if (_objectManager.CharacterSelects.Count > 0)
                _woWClient.EnterWorld(_objectManager.CharacterSelects[0].Guid);
        }
    }
}
