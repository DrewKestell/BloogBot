using Newtonsoft.Json;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.AI.SharedTasks;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RaidMemberBot.AI
{
    public class BotRunner
    {
        readonly Stack<IBotTask> botTasks = new Stack<IBotTask>();
        readonly BotLoader botLoader = new BotLoader();
        readonly CharacterState characterState;

        IClassContainer classContainer;
        ObservableCollection<IBot> Bots = new ObservableCollection<IBot>();

        int _activityMapId;

        Task _asyncBotTaskRunnerTask;
        Task _asyncServerFeedbackTask;

        // General
        IBot currentBot;

        public BotRunner()
        {
            Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

            characterState = new CharacterState()
            {
                ProcessId = Process.GetCurrentProcess().Id
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
            Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Start server feedback task started.");
            while (true)
            {
                InstanceCommand instanceCommand = CommandClient.Instance.GetCommandBasedOnState(characterState);
                if (botTasks.Count == 0 && instanceCommand.CommandAction != CommandAction.None)
                {
                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        switch (instanceCommand.CommandAction)
                        {
                            case CommandAction.SetRaidLeader:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] SetRaidLeader {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                characterState.RaidLeader = instanceCommand.CommandParam1;
                                characterState.RaidLeaderGuid = ulong.Parse(instanceCommand.CommandParam2);
                                break;
                            case CommandAction.SetActivity:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] SetActivity {instanceCommand.CommandParam1}");
                                characterState.CurrentActivity = instanceCommand.CommandParam1;
                                _activityMapId = int.Parse(instanceCommand.CommandParam2);
                                break;
                            case CommandAction.SetAccountInfo:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] SetAccountInfo {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                characterState.AccountName = instanceCommand.CommandParam1;
                                characterState.BotProfileName = instanceCommand.CommandParam2;

                                AssignClassContainer();

                                botTasks.Push(new LoginTask(classContainer, botTasks, characterState.AccountName));
                                break;
                            case CommandAction.BeginGathering:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Begin Gathering");
                                break;
                            case CommandAction.BeginQuesting:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Begin Questing");
                                break;
                            case CommandAction.BeginWorldPvP:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Begin World PvP");
                                break;
                            case CommandAction.AddTalent:
                                characterState.Action = "Adding talents";
                                botTasks.Push(new AddTalentTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] AddTalent {instanceCommand.CommandParam1}");
                                break;
                            case CommandAction.AddSpell:
                                characterState.Action = "Adding spells";
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] AddSpell {instanceCommand.CommandParam1}");
                                break;
                            case CommandAction.AddPartyMember:
                                characterState.Action = "Inviting members";
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] AddPartyMember {instanceCommand.CommandParam1}");
                                botTasks.Push(new AddPartyMemberTask(classContainer, botTasks, instanceCommand.CommandParam1));
                                break;
                            case CommandAction.SetLevel:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] SetLevel {instanceCommand.CommandParam1}");
                                botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".character level {ObjectManager.Player.Name} {instanceCommand.CommandParam1}\")"));
                                break;
                            case CommandAction.SetReadyState:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] SetReadyState {instanceCommand.CommandParam1}");
                                botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, "SendChatMessage(\".maxskill\")"));
                                characterState.IsReadyToStart = bool.Parse(instanceCommand.CommandParam1);
                                break;
                            case CommandAction.BeginDungeon:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Begin Dungeon {ObjectManager.ZoneText}");
                                botTasks.Push(new DungeoneeringTask(classContainer, botTasks));
                                break;
                            case CommandAction.BeginBattleGrounds:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Begin Battleground {instanceCommand.CommandParam1}");
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
                            case CommandAction.QueuePvP:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Begin QueuePvP {instanceCommand.CommandParam1}");
                                botTasks.Push(new QueueForBattlegroundTask(classContainer, botTasks, ObjectManager.Units.First(x => x.Name.StartsWith(instanceCommand.CommandParam2))));
                                break;
                            case CommandAction.AddEquipment:
                                characterState.Action = "Adding equipment";
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] AddEquipment {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                botTasks.Push(new AddEquipmentTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1), int.Parse(instanceCommand.CommandParam2)));
                                break;
                            case CommandAction.AddRole:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] AddRole {instanceCommand.CommandParam1}");
                                botTasks.Push(new AddRoleTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                break;
                            case CommandAction.TeleTo:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] TeleTo Map: {instanceCommand.CommandParam4} XYZ: {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3}");
                                botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".go xyz {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3} {instanceCommand.CommandParam4}\")"));
                                break;
                            case CommandAction.ResetCharacterState:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] ResetCharacterState");
                                botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, "ResetInstances()"));
                                botTasks.Push(new ResetCharacterStateTask(classContainer, botTasks));
                                break;
                            case CommandAction.ExecuteLuaCommand:
                                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] ExecuteLuaCommand {instanceCommand.CommandParam1}");
                                botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, instanceCommand.CommandParam1));
                                break;
                            case CommandAction.SetFacing:
                                ObjectManager.Player.SetFacing(float.Parse(instanceCommand.CommandParam1));
                                break;
                            case CommandAction.SetTankInPosition:
                                characterState.TankInPosition = bool.Parse(instanceCommand.CommandParam1);
                                break;
                            case CommandAction.SetCombatLocation:
                                characterState.TankPosition = new System.Numerics.Vector3(
                                    float.Parse(instanceCommand.CommandParam1),
                                    float.Parse(instanceCommand.CommandParam2),
                                    float.Parse(instanceCommand.CommandParam3));
                                characterState.TankFacing = float.Parse(instanceCommand.CommandParam4);
                                break;
                            case CommandAction.FullStop:
                                botTasks.Clear();
                                if (ObjectManager.Player != null && classContainer != null)
                                {
                                    ObjectManager.Player.StopAllMovement();
                                }
                                break;
                        }
                    });
                }
                await Task.Delay(500);
            }
        }

        private async Task StartBotTaskRunnerAsync()
        {
            Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}] Bot Task Runner started.");
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
                    Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}]{ex.Message} {ex.StackTrace}");
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
                Console.WriteLine($"[BOT RUNNER {Process.GetCurrentProcess().Id}]ReloadBots {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
