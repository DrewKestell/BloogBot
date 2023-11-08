using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
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

        IClassContainer container;
        ObservableCollection<IBot> Bots = new ObservableCollection<IBot>();

        int _activityMapId;

        InstanceCommand _lastCommand;

        Task _asyncBotTaskRunnerTask;
        Task _asyncServerFeedbackTask;

        // General
        IBot currentBot;

        public BotRunner()
        {
            characterState = new CharacterState()
            {
                ProcessId = Process.GetCurrentProcess().Id
            };

            ObjectManager.Instance.StartEnumeration(characterState);

            WoWEventHandler.Instance.OnPartyInvite += (sender, args) =>
            {
                Lua.Instance.Execute($"AcceptGroup()");
            };

            _lastCommand = new InstanceCommand();

            _asyncBotTaskRunnerTask = Task.Run(StartBotTaskRunnerAsync);
            _asyncServerFeedbackTask = Task.Run(StartServerFeedbackAsync);
        }
        private async void StartServerFeedbackAsync()
        {
            Console.WriteLine($"BOTRUNNER: Start server feedback task started.");
            while (true)
            {
                InstanceCommand instanceCommand = CommandClient.Instance.GetCommandBasedOnState(characterState);

                if (_lastCommand.CommandAction != instanceCommand.CommandAction
                    || _lastCommand.CommandParam1 != instanceCommand.CommandParam1
                    || _lastCommand.CommandParam2 != instanceCommand.CommandParam2
                    || _lastCommand.CommandParam3 != instanceCommand.CommandParam3
                    || _lastCommand.CommandParam4 != instanceCommand.CommandParam4)
                {
                    if (instanceCommand.CommandAction == CommandAction.SetAccountInfo)
                    {
                        if (instanceCommand.CommandParam1 != characterState.AccountName)
                        {
                            while (botTasks.Count > 0)
                                botTasks.Pop();

                            botTasks.Push(new LoginTask(container, botTasks, instanceCommand.CommandParam1));
                            botTasks.Push(new LogoutTask(container, botTasks));
                        }

                        ReloadBots();

                        Console.WriteLine($"BOT RUNNER: SetAccountInfo [{instanceCommand.CommandParam1}] [{instanceCommand.CommandParam2}]");
                    }
                    else if (instanceCommand.CommandAction == CommandAction.SetActivity)
                    {
                        characterState.CurrentActivity = instanceCommand.CommandParam1;
                        _activityMapId = int.Parse(instanceCommand.CommandParam2);

                        Console.WriteLine($"BOT RUNNER: SetActivity {characterState.CurrentActivity}");
                    }
                    else if (instanceCommand.CommandAction == CommandAction.SetRaidLeader)
                    {
                        characterState.RaidLeader = instanceCommand.CommandParam1;

                        Console.WriteLine($"BOT RUNNER: SetRaidLeader {characterState.RaidLeader}");
                    }
                    else if (instanceCommand.CommandAction == CommandAction.AddPartyMember)
                    {
                        Lua.Instance.Execute($"InviteByName(\"{instanceCommand.CommandParam1}\")");

                        Console.WriteLine($"BOT RUNNER: AddPartyMember {instanceCommand.CommandParam1}");
                    }
                    else if (instanceCommand.CommandAction == CommandAction.GoTo)
                    {
                        Location destinaton = new Location(float.Parse(instanceCommand.CommandParam1),
                                                            float.Parse(instanceCommand.CommandParam2),
                                                            float.Parse(instanceCommand.CommandParam3));
                        container.CurrentWaypoint = destinaton;
                        botTasks.Push(new MoveToWaypointTask(container, botTasks));

                        Console.WriteLine($"BOT RUNNER: GoTo {destinaton}");
                    }
                    else if (instanceCommand.CommandAction == CommandAction.BeginDungeon)
                    {
                        botTasks.Push(new DungeoneeringTask(container, botTasks));

                        Console.WriteLine($"BOT RUNNER: Begin dungeon");
                    }
                    else if (instanceCommand.CommandAction == CommandAction.SetFacing)
                    {
                        ObjectManager.Instance.Player.Face(float.Parse(instanceCommand.CommandParam1));
                    }
                    else if (instanceCommand.CommandAction == CommandAction.ExecuteLuaCommand)
                    {
                        Console.WriteLine($"BOT RUNNER: ExecuteChatCommand - {instanceCommand.CommandParam1}");
                        Lua.Instance.Execute(instanceCommand.CommandParam1);
                    }

                    _lastCommand = instanceCommand;
                }

                await Task.Delay(100);
            }
        }

        private async void StartBotTaskRunnerAsync()
        {
            Console.WriteLine($"BOTRUNNER: Bot Task Runner started.");
            while (true)
            {
                if (ThreadSynchronizer.Instance.QueueCount == 0)
                {
                    ThreadSynchronizer.Instance.Invoke(() =>
                    {
                        try
                        {
                            if (ObjectManager.Instance.IsInClient)
                            {
                                ObjectManager.Instance.AntiAfk();
                            }

                            if (botTasks.Count > 0)
                            {
                                characterState.Task = botTasks.Peek()?.GetType()?.Name;

                                botTasks.Peek()?.Update();
                            }
                            else
                            {
                                characterState.Task = "Idle"; 
                                
                                if (ObjectManager.Instance.IsIngame && container != null)
                                {
                                    container.Player.StopAllMovement();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Console.WriteLine(ex.StackTrace);
                        }
                    });

                    await Task.Delay(50);
                }
            }
        }

        private void ReloadBots()
        {
            try
            {
                Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

                currentBot = Bots.First(b => b.Name == characterState.BotProfileName);

                container = currentBot.GetClassContainer(characterState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RELOAD BOTS: {ex.Message}");
            }
        }

        void ShapeshiftToHumanForm()
        {
            if (ObjectManager.Instance.Player.Class == ClassId.Druid && ObjectManager.Instance.Player.CurrentShapeshiftForm == "Bear Form")
                Lua.Instance.Execute("CastSpellByName('Bear Form')");
            if (ObjectManager.Instance.Player.Class == ClassId.Druid && ObjectManager.Instance.Player.CurrentShapeshiftForm == "Cat Form")
                Lua.Instance.Execute("CastSpellByName('Cat Form')");
        }

        void PopStackToBaseState()
        {
            while (botTasks.Count > 1)
                botTasks.Pop();
        }
    }
}
