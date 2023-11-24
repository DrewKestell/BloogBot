using Newtonsoft.Json;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static RaidMemberBot.Constants.Enums;

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

        InstanceCommand _lastCommand;

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
                Functions.LuaCall($"AcceptGroup()");
            };

            _lastCommand = new InstanceCommand();

            _asyncBotTaskRunnerTask = Task.Run(StartBotTaskRunnerAsync);
            _asyncServerFeedbackTask = Task.Run(StartServerFeedbackAsync);
        }
        private async void StartServerFeedbackAsync()
        {
            Console.WriteLine($"[BOT RUNNER] Start server feedback task started.");
            while (true)
            {
                InstanceCommand instanceCommand = CommandClient.Instance.GetCommandBasedOnState(characterState);

                if (_lastCommand.CommandAction != instanceCommand.CommandAction
                    || _lastCommand.CommandParam1 != instanceCommand.CommandParam1
                    || _lastCommand.CommandParam2 != instanceCommand.CommandParam2
                    || _lastCommand.CommandParam3 != instanceCommand.CommandParam3
                    || _lastCommand.CommandParam4 != instanceCommand.CommandParam4)
                {
                    switch (instanceCommand.CommandAction)
                    {
                        case CommandAction.SetAccountInfo:
                            if (instanceCommand.CommandParam1 != characterState.AccountName)
                            {
                                characterState.AccountName = instanceCommand.CommandParam1;
                                characterState.BotProfileName = instanceCommand.CommandParam2;

                                while (botTasks.Count > 0)
                                    botTasks.Pop();

                                botTasks.Push(new LoginTask(classContainer, botTasks, characterState.AccountName));
                                botTasks.Push(new LogoutTask(classContainer, botTasks));
                            }
                            Console.WriteLine($"[BOT RUNNER] SetAccountInfo [{instanceCommand.CommandParam1}] [{instanceCommand.CommandParam2}]");

                            ReloadBots();
                            break;
                        case CommandAction.SetActivity:
                            characterState.CurrentActivity = instanceCommand.CommandParam1;
                            _activityMapId = 389;//int.Parse(instanceCommand.CommandParam2);

                            Console.WriteLine($"[BOT RUNNER] SetActivity {characterState.CurrentActivity}");
                            break;
                        case CommandAction.SetRaidLeader:
                            characterState.RaidLeader = instanceCommand.CommandParam1;

                            Console.WriteLine($"[BOT RUNNER] SetRaidLeader {characterState.RaidLeader}");
                            break;
                        case CommandAction.AddPartyMember:
                            Functions.LuaCall($"InviteByName('{instanceCommand.CommandParam1}')");

                            Console.WriteLine($"[BOT RUNNER] AddPartyMember {instanceCommand.CommandParam1}");
                            break;
                        case CommandAction.TeleTo:
                            Functions.LuaCall($"SendChatMessage('.go xyz {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3} {instanceCommand.CommandParam4}')");

                            Console.WriteLine($"[BOT RUNNER] TeleTo Map: {instanceCommand.CommandParam4} XYZ: {instanceCommand.CommandParam1} {instanceCommand.CommandParam2} {instanceCommand.CommandParam3}");
                            break;
                        case CommandAction.BeginDungeon:
                            botTasks.Push(new DungeoneeringTask(classContainer, botTasks));

                            Console.WriteLine($"[BOT RUNNER] Begin dungeon");
                            break;
                        case CommandAction.SetFacing:
                            ObjectManager.Player.SetFacing(float.Parse(instanceCommand.CommandParam1));
                            break;
                        case CommandAction.SetLevel:
                            Console.WriteLine($"[BOT RUNNER] SetLevel - {instanceCommand.CommandParam1}");
                            Functions.LuaCall($"SendChatMessage('.character level {ObjectManager.Player.Name} {instanceCommand.CommandParam1}')");
                            break;
                        case CommandAction.ExecuteLuaCommand:
                            Console.WriteLine($"[BOT RUNNER] ExecuteChatCommand - {instanceCommand.CommandParam1}");
                            Functions.LuaCall(instanceCommand.CommandParam1);
                            break;
                    }

                    _lastCommand = instanceCommand;
                }

                await Task.Delay(100);
            }
        }

        private async void StartBotTaskRunnerAsync()
        {
            Console.WriteLine($"[BOT RUNNER] Bot Task Runner started.");
            while (true)
            {
                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    try
                    {
                        if (ObjectManager.Player != null)
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BOT RUNNER]BotTaskRunner {ex.Message} {ex.StackTrace}");
                    }
                });

                await Task.Delay(50);
            }
        }

        private void ReloadBots()
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
