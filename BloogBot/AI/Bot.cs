using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BloogBot.AI
{
    public abstract class Bot
    {
        readonly Stack<IBotState> botStates = new Stack<IBotState>();
        readonly Stopwatch stopwatch = new Stopwatch();

        bool running;
        bool retrievingCorpse;

        Type currentState;
        int currentStateStartTime;
        Position currentPosition;
        int currentPositionStartTime;
        Position teleportCheckPosition;
        bool isFalling;
        int currentLevel;

        Action stopCallback;

        public bool Running() => running;

        public void Stop()
        {
            running = false;
            currentLevel = 0;

            while (botStates.Count > 0)
                botStates.Pop();

            stopCallback?.Invoke();
        }

        public void Start(IDependencyContainer container, Action stopCallback)
        {
            this.stopCallback = stopCallback;

            try
            {
                running = true;

                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    currentLevel = ObjectManager.Player.Level;

                    botStates.Push(new GrindState(botStates, container));

                    currentState = botStates.Peek().GetType();
                    currentStateStartTime = Environment.TickCount;
                    currentPosition = ObjectManager.Player.Position;
                    currentPositionStartTime = Environment.TickCount;
                    teleportCheckPosition = ObjectManager.Player.Position;
                });

                container.CheckForTravelPath(botStates, false);
                StartInternal(container);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }
        
        public void Travel(IDependencyContainer container, bool reverseTravelPath, Action callback)
        {
            try
            {
                running = true;

                var waypoints = container
                    .BotSettings
                    .CurrentTravelPath
                    .Waypoints;

                if (reverseTravelPath)
                    waypoints = waypoints.Reverse().ToArray();

                var closestWaypoint = waypoints
                    .OrderBy(w => w.DistanceTo(ObjectManager.Player.Position))
                    .First();

                var startingIndex = waypoints
                    .ToList()
                    .IndexOf(closestWaypoint);

                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    currentLevel = ObjectManager.Player.Level;

                    void callbackInternal()
                    {
                        running = false;
                        currentState = null;
                        currentPosition = null;
                        callback();
                    }

                    botStates.Push(new TravelState(botStates, container, waypoints, startingIndex, callbackInternal));
                    botStates.Push(new MoveToPositionState(botStates, container, closestWaypoint));

                    currentState = botStates.Peek().GetType();
                    currentStateStartTime = Environment.TickCount;
                    currentPosition = ObjectManager.Player.Position;
                    currentPositionStartTime = Environment.TickCount;
                    teleportCheckPosition = ObjectManager.Player.Position;
                });

                StartInternal(container);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public void StartPowerlevel(IDependencyContainer container,Action stopCallback)
        {
            this.stopCallback = stopCallback;

            try
            {
                running = true;

                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    botStates.Push(new PowerlevelState(botStates, container));

                    currentState = botStates.Peek().GetType();
                    currentStateStartTime = Environment.TickCount;
                    currentPosition = ObjectManager.Player.Position;
                    currentPositionStartTime = Environment.TickCount;
                    teleportCheckPosition = ObjectManager.Player.Position;
                });

                StartPowerlevelInternal(container);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        async void StartPowerlevelInternal(IDependencyContainer container)
        {
            while (running)
            {
                try
                {
                    stopwatch.Restart();

                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        if (botStates.Count() == 0)
                        {
                            Stop();
                            return;
                        }

                        var player = ObjectManager.Player;
                        player.AntiAfk();

                        if (player.IsFalling)
                        {
                            container.DisableTeleportChecker = true;
                            isFalling = true;
                        }

                        if (player.Position.DistanceTo(teleportCheckPosition) > 10 && !container.DisableTeleportChecker && container.BotSettings.UseTeleportKillswitch)
                        {
                            DiscordClientWrapper.TeleportAlert(player.Name);
                            Stop();
                            return;
                        }
                        teleportCheckPosition = player.Position;

                        if (isFalling && !player.IsFalling)
                        {
                            container.DisableTeleportChecker = false;
                            isFalling = false;
                        }

                        if (botStates.Count > 0 && botStates.Peek()?.GetType() == typeof(GrindState))
                        {
                            container.RunningErrands = false;
                            retrievingCorpse = false;
                        }

                        // if the player has been stuck in the same state for more than 5 minutes
                        if (Environment.TickCount - currentStateStartTime > 300000 && currentState != typeof(TravelState) && container.BotSettings.UseStuckInStateKillswitch)
                        {
                            var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the {currentState.Name} for over 5 minutes. I'm stopping for now.";
                            LogToFile(msg);
                            DiscordClientWrapper.SendMessage(msg);
                            Stop();
                            return;
                        }
                        if (botStates.Peek().GetType() != currentState)
                        {
                            currentState = botStates.Peek().GetType();
                            currentStateStartTime = Environment.TickCount;
                        }

                        // if the player has been stuck in the same position for more than 5 minutes
                        if (Environment.TickCount - currentPositionStartTime > 300000 && container.BotSettings.UseStuckInPositionKillswitch)
                        {
                            var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the same position for over 5 minutes. I'm stopping for now.";
                            LogToFile(msg);
                            DiscordClientWrapper.SendMessage(msg);
                            Stop();
                            return;
                        }
                        if (player.Position.DistanceTo(currentPosition) > 10)
                        {
                            currentPosition = player.Position;
                            currentPositionStartTime = Environment.TickCount;
                        }

                        // if the player dies
                        if ((player.Health <= 0 || player.InGhostForm) && !retrievingCorpse)
                        {
                            PopStackToBaseState();

                            retrievingCorpse = true;
                            container.RunningErrands = true;

                            container.DisableTeleportChecker = true;

                            botStates.Push(container.CreateRestState(botStates, container));
                            botStates.Push(new RetrieveCorpseState(botStates, container));
                            botStates.Push(new MoveToCorpseState(botStates, container));
                            botStates.Push(new ReleaseCorpseState(botStates, container));
                        }

                        var currentHotspot = container.GetCurrentHotspot();

                        // if equipment needs to be repaired
                        int mainhandDurability = Inventory.GetEquippedItem(EquipSlot.MainHand)?.DurabilityPercentage ?? 100;
                        int offhandDurability = Inventory.GetEquippedItem(EquipSlot.Ranged)?.DurabilityPercentage ?? 100;

                        // offhand throwns don't have durability, but instead register `-2147483648`.
                        // This is a workaround to prevent that from causing us to get caught in a loop.
                        // We default to a durability value of 100 for items that are null because 100 will register them as not needing repaired.
                        if ((mainhandDurability <= 20 && mainhandDurability > -1 || (offhandDurability <= 20 && offhandDurability > -1)) && currentHotspot.RepairVendor != null && !container.RunningErrands)
                        {
                            ShapeshiftToHumanForm(container);
                            PopStackToBaseState();

                            container.RunningErrands = true;

                            if (currentHotspot.TravelPath != null)
                            {
                                botStates.Push(new TravelState(botStates, container, currentHotspot.TravelPath.Waypoints, 0));
                                botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.TravelPath.Waypoints[0]));
                            }

                            botStates.Push(new RepairEquipmentState(botStates, container, currentHotspot.RepairVendor.Name));
                            botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.RepairVendor.Position));
                            container.CheckForTravelPath(botStates, true);
                        }

                        if (botStates.Count > 0)
                        {
                            container.Probe.CurrentState = botStates.Peek()?.GetType().Name;
                            botStates.Peek()?.Update();
                        }
                    });

                    await Task.Delay(25);

                    container.Probe.UpdateLatency = $"{stopwatch.ElapsedMilliseconds}ms";
                }
                catch (Exception e)
                {
                    Logger.Log(e + "\n");
                }
            }
        }

        async void StartInternal(IDependencyContainer container)
        {
            while (running)
            {
                try
                {
                    stopwatch.Restart();

                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        if (botStates.Count() == 0)
                        {
                            Stop();
                            return;
                        }

                        var player = ObjectManager.Player;

                        if (player.Level > currentLevel)
                        {
                            currentLevel = player.Level;
                            DiscordClientWrapper.SendMessage($"Ding! {player.Name} is now level {player.Level}!");
                        }

                        player.AntiAfk();

                        if (container.UpdatePlayerTrackers())
                        {
                            Stop();
                            return;
                        }

                        if (player.IsFalling)
                        {
                            container.DisableTeleportChecker = true;
                            isFalling = true;
                        }

                        if (player.Position.DistanceTo(teleportCheckPosition) > 5 && !container.DisableTeleportChecker && container.BotSettings.UseTeleportKillswitch)
                        {
                            DiscordClientWrapper.TeleportAlert(player.Name);
                            Stop();
                            return;
                        }
                        teleportCheckPosition = player.Position;

                        if (isFalling && !player.IsFalling)
                        {
                            container.DisableTeleportChecker = false;
                            isFalling = false;
                        }

                        if (botStates.Count > 0 && (botStates.Peek()?.GetType() == typeof(GrindState) || botStates.Peek()?.GetType() == typeof(PowerlevelState)))
                        {
                            container.RunningErrands = false;
                            retrievingCorpse = false;
                        }

                        // if the player has been stuck in the same state for more than 5 minutes
                        if (Environment.TickCount - currentStateStartTime > 300000 && currentState != typeof(TravelState) && container.BotSettings.UseStuckInStateKillswitch)
                        {
                            var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the {currentState.Name} for over 5 minutes. I'm stopping for now.";
                            LogToFile(msg);
                            DiscordClientWrapper.SendMessage(msg);
                            Stop();
                            return;
                        }
                        if (botStates.Peek().GetType() != currentState)
                        {
                            currentState = botStates.Peek().GetType();
                            currentStateStartTime = Environment.TickCount;
                        }

                        // if the player has been stuck in the same position for more than 5 minutes
                        if (Environment.TickCount - currentPositionStartTime > 300000 && container.BotSettings.UseStuckInPositionKillswitch)
                        {
                            var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the same position for over 5 minutes. I'm stopping for now.";
                            LogToFile(msg);
                            DiscordClientWrapper.SendMessage(msg);
                            Stop();
                            return;
                        }
                        if (player.Position.DistanceTo(currentPosition) > 10)
                        {
                            currentPosition = player.Position;
                            currentPositionStartTime = Environment.TickCount;
                        }

                        // if the player dies
                        if ((player.Health <= 0 || player.InGhostForm) && !retrievingCorpse)
                        {
                            PopStackToBaseState();

                            retrievingCorpse = true;
                            container.RunningErrands = true;

                            container.DisableTeleportChecker = true;

                            botStates.Push(container.CreateRestState(botStates, container));
                            botStates.Push(new RetrieveCorpseState(botStates, container));
                            botStates.Push(new MoveToCorpseState(botStates, container));
                            botStates.Push(new ReleaseCorpseState(botStates, container));
                        }

                        var currentHotspot = container.GetCurrentHotspot();

                        // if equipment needs to be repaired
                        int mainhandDurability = Inventory.GetEquippedItem(EquipSlot.MainHand)?.DurabilityPercentage ?? 100;
                        int offhandDurability = Inventory.GetEquippedItem(EquipSlot.Ranged)?.DurabilityPercentage ?? 100;

                        // offhand throwns don't have durability, but instead register `-2147483648`.
                        // This is a workaround to prevent that from causing us to get caught in a loop.
                        // We default to a durability value of 100 for items that are null because 100 will register them as not needing repaired.
                        if ((mainhandDurability <= 20 && mainhandDurability > -1 || (offhandDurability <= 20 && offhandDurability > -1)) && currentHotspot.RepairVendor != null && !container.RunningErrands)
                        {
                            ShapeshiftToHumanForm(container);
                            PopStackToBaseState();

                            container.RunningErrands = true;

                            if (currentHotspot.TravelPath != null)
                            {
                                botStates.Push(new TravelState(botStates, container, currentHotspot.TravelPath.Waypoints, 0));
                                botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.TravelPath.Waypoints[0]));
                            }

                            botStates.Push(new RepairEquipmentState(botStates, container, currentHotspot.RepairVendor.Name));
                            botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.RepairVendor.Position));
                            container.CheckForTravelPath(botStates, true);
                        }

                        // if inventory is full
                        if (Inventory.CountFreeSlots(false) == 0 && currentHotspot.Innkeeper != null && !container.RunningErrands)
                        {
                            ShapeshiftToHumanForm(container);
                            PopStackToBaseState();

                            container.RunningErrands = true;

                            if (currentHotspot.TravelPath != null)
                            {
                                botStates.Push(new TravelState(botStates, container, currentHotspot.TravelPath.Waypoints, 0));
                                botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.TravelPath.Waypoints[0]));
                            }
                            
                            botStates.Push(new SellItemsState(botStates, container, currentHotspot.Innkeeper.Name));
                            botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.Innkeeper.Position));
                            container.CheckForTravelPath(botStates, true);
                        }

                        if (botStates.Count > 0)
                        {
                            container.Probe.CurrentState = botStates.Peek()?.GetType().Name;
                            botStates.Peek()?.Update();
                        }
                    });
                    
                    await Task.Delay(50);

                    container.Probe.UpdateLatency = $"{stopwatch.ElapsedMilliseconds}ms";
                }
                catch (Exception e)
                {
                    Logger.Log(e + "\n");
                }
            }
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
