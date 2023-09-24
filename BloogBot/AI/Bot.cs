using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Models;
using BloogBot.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace BloogBot.AI
{
    public abstract class Bot
    {
        readonly Stack<IBotState> botStates = new Stack<IBotState>();
        Timer timer;

        bool running;
        bool retrievingCorpse;

        Type currentState;
        int currentLevel;

        Action stopCallback;

        IDependencyContainer container;

        public bool Running() => running;

        public void Stop()
        {
            running = false;
            timer.Stop();

            while (botStates.Count > 0)
            {
                botStates.Pop();
            }
            ThreadSynchronizer.ClearQueue();

            stopCallback?.Invoke();
        }

        public async void Logout()
        {
            running = true;
            botStates.Push(new LogoutState(botStates, container));

            while (botStates.Count > 0)
            {
                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    botStates.Peek()?.Update();
                });
                await Task.Delay(50);
            }
            running = false;
        }
        public async void Login()
        {
            running = true;
            botStates.Push(new LoginState(botStates, container));

            while (botStates.Count > 0)
            {
                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    botStates.Peek()?.Update();
                });
                await Task.Delay(50);
            }
            running = false;
        }

        public void AddState(IBotState state)
        {
            botStates.Push(state);
        }

        public void ClearStack()
        {
            while(botStates.Count > 0)
            {
                botStates.Clear();
            }
        }

        public void Start(IDependencyContainer container, Action stopCallback)
        {
            this.container = container;
            this.stopCallback = stopCallback;

            try
            {
                StartInternal();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (ThreadSynchronizer.QueueCount() == 0)
                {
                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        if (ObjectManager.IsLoggedIn)
                        {
                            var player = ObjectManager.Player;

                            if (player == null)
                            {
                                return;
                            }

                            if (player.Level > currentLevel)
                            {
                                currentLevel = player.Level;
                                DiscordClientWrapper.SendMessage($"Ding! {player.Name} is now level {player.Level}!");
                            }

                            if (botStates.Count > 0 && botStates.Peek()?.GetType() == typeof(GrindingState))
                            {
                                container.RunningErrands = false;
                                retrievingCorpse = false;
                            }

                            // if the player has been stuck in the same state for more than 5 minutes
                            if (Environment.TickCount < 0 && currentState != typeof(TravelState))
                            {
                                var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the {currentState.Name} for over 5 minutes. I'm stopping for now.";
                                LogToFile(msg);
                                DiscordClientWrapper.SendMessage(msg);
                                Stop();
                                return;
                            }
                            if (botStates.Count > 0 && botStates.Peek().GetType() != currentState)
                            {
                                currentState = botStates.Peek().GetType();
                            }

                            // if the player has been stuck in the same position for more than 5 minutes
                            if (Environment.TickCount < 0)
                            {
                                var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the same position for over 5 minutes. I'm stopping for now.";
                                LogToFile(msg);
                                DiscordClientWrapper.SendMessage(msg);
                                Stop();
                                return;
                            }

                            // if the player dies
                            if ((player.Health <= 0 || player.InGhostForm) && !retrievingCorpse)
                            {
                                PopStackToBaseState();

                                retrievingCorpse = true;
                                container.RunningErrands = true;

                                botStates.Push(container.CreateRestState(botStates, container));
                                botStates.Push(new RetrieveCorpseState(botStates, container));
                                botStates.Push(new MoveToCorpseState(botStates, container));
                                botStates.Push(new ReleaseCorpseState(botStates, container));
                            }

                            //var currentHotspot = container.GetCurrentHotspot();

                            // if equipment needs to be repaired
                            int mainhandDurability = Inventory.GetEquippedItem(EquipSlot.MainHand)?.DurabilityPercentage ?? 100;
                            int offhandDurability = Inventory.GetEquippedItem(EquipSlot.Ranged)?.DurabilityPercentage ?? 100;

                            // offhand throwns don't have durability, but instead register `-2147483648`.
                            // This is a workaround to prevent that from causing us to get caught in a loop.
                            // We default to a durability value of 100 for items that are null because 100 will register them as not needing repaired.
                            if ((mainhandDurability <= 20 && mainhandDurability > -1 || (offhandDurability <= 20 && offhandDurability > -1)) != null && !container.RunningErrands)
                            {
                                ShapeshiftToHumanForm(container);
                                PopStackToBaseState();

                                container.RunningErrands = true;

                                //if (currentHotspot.TravelPath != null)
                                //{
                                //    botStates.Push(new TravelState(botStates, container, currentHotspot.TravelPath.Waypoints, 0));
                                //    botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.TravelPath.Waypoints[0]));
                                //}

                                //botStates.Push(new RepairEquipmentState(botStates, container, currentHotspot.RepairVendor.Name));
                                //botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.RepairVendor.Position));
                                //container.CheckForTravelPath(botStates, true);
                            }

                            // if inventory is full
                            if (Inventory.CountFreeSlots(false) < 3 && !container.RunningErrands)
                            {
                                ShapeshiftToHumanForm(container);
                                PopStackToBaseState();

                                container.RunningErrands = true;

                                //if (currentHotspot.TravelPath != null)
                                //{
                                //    botStates.Push(new TravelState(botStates, container, currentHotspot.TravelPath.Waypoints, 0));
                                //    botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.TravelPath.Waypoints[0]));
                                //}
                                Creature vendor = SqliteRepository.GetAllVendors()
                                                                    .OrderBy(x => new Position(x.PositionX, x.PositionY, x.PositionZ).DistanceTo(ObjectManager.Player.Position))
                                                                    .First();

                                botStates.Push(new SellItemsState(botStates, container, vendor.Name));
                                botStates.Push(new MoveToPositionState(botStates, container, new Position(vendor.PositionX, vendor.PositionY, vendor.PositionZ)));
                                //container.CheckForTravelPath(botStates, true);
                            }

                            currentLevel = ObjectManager.Player.Level;
                        }

                        if (botStates.Count > 0)
                        {
                            if (!FrameHelper.IsElementVisibile("CinematicFrame"))
                            {
                                container.Probe.CurrentTask = botStates.Peek()?.GetType().Name;
                                running = true;
                                botStates.Peek()?.Update();
                            }
                        }
                    });
                }

                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Logger.Log(ex + "\n");
            }
        }

        void StartInternal()
        {
            timer = new Timer(50)
            {
                AutoReset = true
            };
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Start();
        }

        void LogToFile(string text)
        {
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(MainViewModel)).CodeBase);
            var path = new UriBuilder(dir).Path;
            var file = Path.Combine(path, "StuckLog.txt");

            using (var sw = File.AppendText(file))
            {
                sw.WriteLine(text);
            }
        }

        void ShapeshiftToHumanForm(IDependencyContainer container)
        {
            if (ObjectManager.Player.Class == Class.Druid && ObjectManager.Player.CurrentShapeshiftForm == "Bear Form")
                ObjectManager.Player.LuaCall("CastSpellByName('Bear Form')");
            if (ObjectManager.Player.Class == Class.Druid && ObjectManager.Player.CurrentShapeshiftForm == "Cat Form")
                ObjectManager.Player.LuaCall("CastSpellByName('Cat Form')");
        }

        void PopStackToBaseState()
        {
            while (botStates.Count > 1)
                botStates.Pop();
        }
    }
}
