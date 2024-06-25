using WoWClientBot.AI.SharedStates;
using WoWClientBot.AI.SharedTasks;
using WoWClientBot.Game;
using WoWClientBot.Game.Statics;
using WoWClientBot.Mem;
using WoWClientBot.Tasks;
using System.Collections.ObjectModel;
using WoWClientBot.Models;
using WoWClientBot.Client;
using MaNGOSDBDomain.Client;

namespace WoWClientBot.AI
{
    public class BotRunner
    {
        readonly Stack<IBotTask> botTasks = new();
        readonly BotLoader botLoader = new();
        readonly CharacterState characterState;
        readonly ActivityCommandClient activityCommandClient;
        readonly MaNGOSDBClient maNGOSDBClient;

        IClassContainer classContainer;
        readonly ObservableCollection<IBot> Bots = [];

        int _activityMapId;

        readonly Task _asyncBotTaskRunnerTask;
        readonly Task _asyncServerFeedbackTask;

        // General
        IBot currentBot;

        public BotRunner(
            string accountName, string botProfileName,
            string activityManagerIP, int activityManagerPort,
            string mangosDBIP, int mangosDBPort)
        {
            Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

            characterState = new CharacterState()
            {
                ProcessId = Environment.ProcessId,
                AccountName = accountName,
                BotProfileName = botProfileName,
                ActivityAddress = activityManagerIP,
                ActivityPort = activityManagerPort,
                DatabaseAddress = mangosDBIP,
                DatabasePort = mangosDBPort
            };

            ObjectManager.Initialize(characterState);

            WoWEventHandler.Instance.OnPartyInvite += (sender, args) =>
            {
                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    Functions.LuaCall($"StaticPopup1Button1:Click()");
                    Functions.LuaCall($"AcceptGroup()");
                });
            };

            _asyncBotTaskRunnerTask = StartBotTaskRunnerAsync();
            _asyncServerFeedbackTask = StartServerFeedbackAsync();
        }
        private async Task StartServerFeedbackAsync()
        {
            Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Start server feedback task started.");
            while (true)
            {
                try
                {
                    CharacterCommand instanceCommand = activityCommandClient.GetCommandBasedOnState(characterState);
                    if (botTasks.Count == 0 && instanceCommand.CharacterAction != CharacterAction.None)
                    {
                        ThreadSynchronizer.RunOnMainThread(() =>
                        {
                            switch (instanceCommand.CharacterAction)
                            {
                                case CharacterAction.SetDatabaseConfig:
                                    break;
                                case CharacterAction.SetNavigationConfig:
                                    break;
                                case CharacterAction.SetRaidLeader:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] SetRaidLeader {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                    characterState.RaidLeader = instanceCommand.CommandParam1;
                                    characterState.RaidLeaderGuid = ulong.Parse(instanceCommand.CommandParam2);
                                    break;
                                case CharacterAction.SetActivity:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] SetActivity {instanceCommand.CommandParam1}");
                                    characterState.CurrentActivity = instanceCommand.CommandParam1;
                                    _activityMapId = int.Parse(instanceCommand.CommandParam2);
                                    break;
                                case CharacterAction.SetAccountInfo:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] SetAccountInfo {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                    characterState.AccountName = instanceCommand.CommandParam1;
                                    characterState.BotProfileName = instanceCommand.CommandParam2;

                                    AssignClassContainer();

                                    botTasks.Push(new LoginTask(classContainer, botTasks));
                                    break;
                                case CharacterAction.BeginGathering:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Gathering");
                                    break;
                                case CharacterAction.BeginQuesting:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Questing");
                                    break;
                                case CharacterAction.BeginWorldPvP:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin World PvP");
                                    break;
                                case CharacterAction.AddTalent:
                                    characterState.Action = "Adding talents";
                                    botTasks.Push(new AddTalentTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddTalent {instanceCommand.CommandParam1}");
                                    break;
                                case CharacterAction.AddSpell:
                                    characterState.Action = "Adding spells";
                                    botTasks.Push(new AddSpellTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddSpell {instanceCommand.CommandParam1}");
                                    break;
                                case CharacterAction.AddPartyMember:
                                    characterState.Action = "Inviting members";
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddPartyMember {instanceCommand.CommandParam1}");
                                    botTasks.Push(new AddPartyMemberTask(classContainer, botTasks, instanceCommand.CommandParam1));
                                    break;
                                case CharacterAction.SetLevel:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] SetLevel {instanceCommand.CommandParam1}");
                                    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".character level {ObjectManager.Player.Name} {instanceCommand.CommandParam1}\")"));
                                    break;
                                case CharacterAction.SetReadyState:
                                    Console.WriteLine($"[BOT RUNNER  :{Environment.ProcessId}] SetReadyState {instanceCommand.CommandParam1}");
                                    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, "SendChatMessage(\".maxskill\")"));
                                    characterState.IsReadyToStart = bool.Parse(instanceCommand.CommandParam1);
                                    break;
                                case CharacterAction.BeginDungeon:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Dungeon {ObjectManager.ZoneText}");
                                    botTasks.Push(new DungeoneeringTask(classContainer, botTasks));
                                    break;
                                case CharacterAction.BeginBattleGrounds:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Battleground {instanceCommand.CommandParam1}");
                                    switch (instanceCommand.CommandParam1)
                                    {
                                        case "WSG":
                                            botTasks.Push(new WarsongGultchTask(classContainer, botTasks));
                                            break;
                                        case "AB":
                                            break;
                                        case "AV":
                                            break;
                                    }
                                    break;
                                case CharacterAction.QueuePvP:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin QueuePvP {instanceCommand.CommandParam1}");
                                    botTasks.Push(new QueueForBattlegroundTask(classContainer, botTasks, ObjectManager.Units.First(x => x.Name.StartsWith(instanceCommand.CommandParam2))));
                                    break;
                                case CharacterAction.AddEquipment:
                                    characterState.Action = "Adding equipment";
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddEquipment {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                    botTasks.Push(new AddEquipmentTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1), int.Parse(instanceCommand.CommandParam2)));
                                    break;
                                case CharacterAction.AddRole:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddRole {instanceCommand.CommandParam1}");
                                    botTasks.Push(new AddRoleTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                    break;
                                case CharacterAction.TeleTo:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] TeleTo Map: {instanceCommand.CommandParam4} XYZ: {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3}");
                                    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".go xyz {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3} {instanceCommand.CommandParam4}\")"));
                                    break;
                                case CharacterAction.ResetCharacterState:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] ResetCharacterState");
                                    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, "ResetInstances()"));
                                    botTasks.Push(new ResetCharacterStateTask(classContainer, botTasks));
                                    break;
                                case CharacterAction.ExecuteLuaCommand:
                                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] ExecuteLuaCommand {instanceCommand.CommandParam1}");
                                    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, instanceCommand.CommandParam1));
                                    break;
                                case CharacterAction.SetFacing:
                                    ObjectManager.Player.SetFacing(float.Parse(instanceCommand.CommandParam1));
                                    break;
                                case CharacterAction.SetTankInPosition:
                                    characterState.TankInPosition = bool.Parse(instanceCommand.CommandParam1);
                                    break;
                                case CharacterAction.SetCombatLocation:
                                    characterState.TankPosition = new System.Numerics.Vector3(
                                        float.Parse(instanceCommand.CommandParam1),
                                        float.Parse(instanceCommand.CommandParam2),
                                        float.Parse(instanceCommand.CommandParam3));
                                    characterState.TankFacing = float.Parse(instanceCommand.CommandParam4);
                                    break;
                                case CharacterAction.FullStop:
                                    botTasks.Clear();
                                    if (ObjectManager.Player != null && classContainer != null)
                                    {
                                        ObjectManager.Player.StopAllMovement();
                                    }
                                    break;
                            }
                        });
                    }
                }
                catch (Exception e)
                {

                }

                await Task.Delay(500);
            }
        }

        private async Task StartBotTaskRunnerAsync()
        {
            Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Bot Task Runner started.");
            while (true)
            {
                try
                {
                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        if (Wait.For("AntiAFK", 5000, true))
                        {
                            ObjectManager.AntiAfk();
                        }
                        if (botTasks.Count > 0)
                        {
                            botTasks.Peek()?.Update();
                        }
                    });

                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}]{ex.Message} {ex.StackTrace}");
                }
            }
        }

        private void AssignClassContainer()
        {
            try
            {
                currentBot = Bots.First(b => b.Name == characterState.BotProfileName);

                classContainer = currentBot.GetClassContainer(characterState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BOT RUNNER {Environment.ProcessId}]ReloadBots {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
