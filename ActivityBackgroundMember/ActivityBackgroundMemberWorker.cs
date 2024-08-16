using BotRunner.Constants;
using BotRunner.Interfaces;
using Communication;
using Microsoft.Extensions.Options;
using PromptHandling;
using PromptHandling.Predefined.CharacterSkills;
using System.Text;
using WoWSharpClient;
using WoWSharpClient.Client;
using WoWSharpClient.Manager;
using WoWSharpClient.Models;
using static System.Net.Mime.MediaTypeNames;

namespace ActivityBackgroundMember
{
    public class ActivityBackgroundMemberWorker : BackgroundService
    {
        private readonly WoWClient _woWClient;
        private readonly IPromptRunner _promptRunner;
        private readonly ILogger<ActivityBackgroundMemberWorker> _logger;
        private readonly ActivityMember _activityMember;
        private readonly ObjectManager _objectManager;
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;
        private CancellationToken _stoppingToken;

        public ActivityBackgroundMemberWorker(ILogger<ActivityBackgroundMemberWorker> logger, IConfiguration configuration, IOptions<ActivityMember> options)
        {
            _logger = logger;
            _activityMember = options.Value;
            _woWSharpEventEmitter = new();
            _objectManager = new(_woWSharpEventEmitter, new ActivityMemberState());

            _promptRunner = PromptRunnerFactory.GetOllamaPromptRunner(new Uri(configuration["Ollama:BaseUri"]), configuration["Ollama:Model"]);

            _woWSharpEventEmitter.OnHandshakeBegin += Instance_OnHandshakeBegin;
            _woWSharpEventEmitter.OnLoginConnect += Instance_OnLoginConnect;
            _woWSharpEventEmitter.OnLoginSuccess += Instance_OnLoginSuccess;
            _woWSharpEventEmitter.OnLoginFailure += Instance_OnLoginFailure;
            _woWSharpEventEmitter.OnCharacterListLoaded += Instance_OnCharacterListLoaded;
            _woWSharpEventEmitter.OnChatMessage += Instance_OnChatMessage;
            _woWSharpEventEmitter.OnGameObjectCreated += Instance_OnGameObjectCreated;

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
                    //if (_logger.IsEnabled(LogLevel.Information))
                    //{
                    //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    //}
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ActivityBackgroundMemberWorker");
            }
        }

        private void Instance_OnGameObjectCreated(object? sender, GameObjectCreatedArgs e)
        {
            _woWClient.SendNameQuery(e.Guid);
        }

        private async void Instance_OnChatMessage(object? sender, ChatMessageArgs e)
        {
            Console.ResetColor();
            StringBuilder sb = new();
            switch (e.MsgType)
            {
                case ChatMsg.CHAT_MSG_SAY:
                case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    sb.Append($"[{e.SenderGuid}]");

                    if (e.SenderGuid != _objectManager.PlayerGuid.FullGuid)
                    {
                        _woWClient.SendChatMessage(ChatMsg.CHAT_MSG_SAY, Language.Orcish, "Dallawha", e.Text);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case ChatMsg.CHAT_MSG_YELL:
                case ChatMsg.CHAT_MSG_MONSTER_YELL:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case ChatMsg.CHAT_MSG_WHISPER:
                case ChatMsg.CHAT_MSG_MONSTER_WHISPER:
                    string result = await CharacterSkillPrioritizationFunction.GetPrioritizedCharacterSkill(
                        _promptRunner,
                        new CharacterSkillPrioritizationFunction.CharacterDescription()
                        {
                            Skills = ["128", "2446"],
                            ClassName = "Shaman",
                            Race = "Orc",
                            Level = 60
                        },
                        _stoppingToken);
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    if (e.SenderGuid != _objectManager.PlayerGuid.FullGuid)
                    {
                        _woWClient.SendChatMessage(ChatMsg.CHAT_MSG_WHISPER, Language.Orcish, "Dallawha", result);
                    }
                    break;

                case ChatMsg.CHAT_MSG_WHISPER_INFORM:
                    sb.Append($"To[{e.SenderGuid}]");
                    break;
                case ChatMsg.CHAT_MSG_EMOTE:
                case ChatMsg.CHAT_MSG_TEXT_EMOTE:
                case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                case ChatMsg.CHAT_MSG_RAID_BOSS_EMOTE:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case ChatMsg.CHAT_MSG_SYSTEM:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    sb.Append($"[System]");
                    break;

                case ChatMsg.CHAT_MSG_PARTY:
                case ChatMsg.CHAT_MSG_RAID:
                case ChatMsg.CHAT_MSG_GUILD:
                case ChatMsg.CHAT_MSG_OFFICER:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;

                case ChatMsg.CHAT_MSG_CHANNEL:
                case ChatMsg.CHAT_MSG_CHANNEL_NOTICE:
                    sb.Append($"[Channel]");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case ChatMsg.CHAT_MSG_RAID_WARNING:
                    sb.Append($"[Raid Warning]");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;

                case ChatMsg.CHAT_MSG_LOOT:
                    sb.Append($"[Loot]");
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                default:
                    sb.Append($"[{e.SenderGuid}][{e.MsgType}]");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            sb.Append(e.Text);

            Console.WriteLine(sb.ToString());
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
