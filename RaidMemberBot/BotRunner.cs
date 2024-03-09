using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using RaidMemberBot.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            _asyncBotTaskRunnerTask = Task.Run(StartBotTaskRunnerAsync);
            _asyncServerFeedbackTask = Task.Run(StartServerFeedbackAsync);
        }
        private async void StartServerFeedbackAsync()
        {
            Console.WriteLine($"[BOT RUNNER] Start server feedback task started.");
            while (true)
            {
                try
                {
                    InstanceCommand instanceCommand = CommandClient.Instance.GetCommandBasedOnState(characterState);

                    if (instanceCommand.CommandAction != CommandAction.None && botTasks.Count == 0)
                    {
                        ThreadSynchronizer.RunOnMainThread(() =>
                        {
                            switch (instanceCommand.CommandAction)
                            {
                                case CommandAction.SetRaidLeader:
                                    Console.WriteLine($"[BOT RUNNER] SetRaidLeader {characterState.RaidLeader}");
                                    characterState.RaidLeader = instanceCommand.CommandParam1;

                                    if (!string.IsNullOrEmpty(instanceCommand.CommandParam2))
                                    {
                                        characterState.RaidLeaderGuid = ulong.Parse(instanceCommand.CommandParam2);
                                    }
                                    else
                                    {
                                        characterState.RaidLeaderGuid = ObjectManager.Player.Guid;
                                    }
                                    break;
                                case CommandAction.ResetCharacterState:
                                    Console.WriteLine($"[BOT RUNNER] ResetCharacterState");
                                    botTasks.Push(new ResetCharacterStateTask(classContainer, botTasks));
                                    break;
                                case CommandAction.SetActivity:
                                    Console.WriteLine($"[BOT RUNNER] SetActivity {characterState.CurrentActivity}");
                                    characterState.CurrentActivity = instanceCommand.CommandParam1;
                                    _activityMapId = 389;//int.Parse(instanceCommand.CommandParam2);
                                    break;
                                case CommandAction.SetAccountInfo:
                                    Console.WriteLine($"[BOT RUNNER] SetAccountInfo [{instanceCommand.CommandParam1}] [{instanceCommand.CommandParam2}]");
                                    characterState.AccountName = instanceCommand.CommandParam1;
                                    characterState.BotProfileName = instanceCommand.CommandParam2;

                                    botTasks.Push(new LoginTask(classContainer, botTasks, characterState.AccountName));
                                    AssignClassContainer();
                                    break;
                                case CommandAction.AddTalent:
                                    botTasks.Push(new AddTalentTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                    Console.WriteLine($"[BOT RUNNER] AddTalent {instanceCommand.CommandParam1}");
                                    break;
                                case CommandAction.AddSpell:
                                    botTasks.Push(new AddSpellTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                    Console.WriteLine($"[BOT RUNNER] AddSpell {instanceCommand.CommandParam1}");
                                    break;
                                case CommandAction.AddPartyMember:
                                    Console.WriteLine($"[BOT RUNNER] AddPartyMember {instanceCommand.CommandParam1}");
                                    Functions.LuaCall($"InviteByName(\"{instanceCommand.CommandParam1}\")");
                                    break;
                                case CommandAction.SetFacing:
                                    ObjectManager.Player.SetFacing(float.Parse(instanceCommand.CommandParam1));
                                    break;
                                case CommandAction.SetLevel:
                                    Console.WriteLine($"[BOT RUNNER] SetLevel - {instanceCommand.CommandParam1}");
                                    Functions.LuaCall($"SendChatMessage(\".character level {ObjectManager.Player.Name} {instanceCommand.CommandParam1}\")");
                                    break;
                                case CommandAction.ExecuteLuaCommand:
                                    Console.WriteLine($"[BOT RUNNER] ExecuteChatCommand - {instanceCommand.CommandParam1}");
                                    Functions.LuaCall(instanceCommand.CommandParam1);
                                    break;
                                case CommandAction.SetReadyState:
                                    Console.WriteLine($"[BOT RUNNER] SetReadyState - {instanceCommand.CommandParam1}");
                                    Functions.LuaCall($"ResetInstances()");
                                    Functions.LuaCall($"SendChatMessage(\".maxskill\")");
                                    characterState.IsReadyToStart = bool.Parse(instanceCommand.CommandParam1);
                                    break;
                                case CommandAction.BeginDungeon:
                                    Console.WriteLine($"[BOT RUNNER] Begin dungeon");
                                    botTasks.Push(new DungeoneeringTask(classContainer, botTasks));
                                    break;
                                case CommandAction.AddEquipment:
                                    Console.WriteLine($"[BOT RUNNER] AddEquipment {instanceCommand.CommandParam1} {instanceCommand.CommandParam2}");
                                    botTasks.Push(new AddEquipmentTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1), int.Parse(instanceCommand.CommandParam2)));
                                    break;
                                case CommandAction.AddRole:
                                    Console.WriteLine($"[BOT RUNNER] AddRole {instanceCommand.CommandParam1}");
                                    botTasks.Push(new AddRoleTask(classContainer, botTasks, int.Parse(instanceCommand.CommandParam1)));
                                    break;
                                case CommandAction.TeleTo:
                                    Console.WriteLine($"[BOT RUNNER] TeleTo Map: {instanceCommand.CommandParam4} XYZ: {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3}");
                                    Functions.LuaCall($"SendChatMessage(\".go xyz {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3} {instanceCommand.CommandParam4}\")");
                                    break;
                            }
                        });
                    }
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER]{ex.Message} {ex.StackTrace}");
                }
            }
        }

        private async void StartBotTaskRunnerAsync()
        {
            Console.WriteLine($"[BOT RUNNER] Bot Task Runner started.");
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
                            characterState.Task = botTasks.Peek()?.GetType()?.Name;

                            botTasks.Peek()?.Update();
                        }
                        else
                        {
                            characterState.Task = "Idle";

                            if (ObjectManager.Player != null && classContainer != null)
                            {
                                ObjectManager.Player.StopAllMovement();
                            }
                        }
                    });

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER]{ex.Message} {ex.StackTrace}");
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
                Console.WriteLine($"[BOT RUNNER]ReloadBots {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
