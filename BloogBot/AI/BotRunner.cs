using BloogBot.AI.SharedStates;
using BloogBot.AI.SharedTasks;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Models;
using BloogBot.Models.Dto;
using BloogBot.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace BloogBot.AI
{
    public class BotRunner
    {
        readonly Stack<IBotTask> botTasks = new Stack<IBotTask>();
        readonly BotLoader botLoader = new BotLoader();
        readonly Timer timer;

        IClassContainer container;
        ObservableCollection<IBot> Bots = new ObservableCollection<IBot>();

        bool retrievingCorpse;

        string currentState;
        int currentLevel;

        string _accountName;
        string _botProfileName;
        int _characterSlot;

        readonly CharacterState characterState;
        public BotRunner(CharacterState characterState)
        {
            this.characterState = characterState;
            timer = new Timer(50)
            {
                AutoReset = true
            };
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        }

        // General
        IBot currentBot;
        public IBot CurrentBot
        {
            get => currentBot;
            set
            {
                currentBot = value;
            }
        }

        public bool Running() => timer.Enabled;

        public void Start()
        {
            try
            {
                timer.Start();
            }
            catch (Exception e)
            {
                Logger.Log("BotRunner: " + e);
            }
        }

        public void Stop()
        {
            timer.Stop();

            while (botTasks.Count > 0)
            {
                botTasks.Pop();
            }
            ThreadSynchronizer.ClearQueue();
        }
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (ThreadSynchronizer.QueueCount() == 0)
                {
                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        if (ObjectManager.IsLoggedIn 
                            && characterState.AccountName == _accountName
                            && characterState.CharacterSlot == _characterSlot
                            && characterState.BotProfileName == _botProfileName)
                        {
                            var player = ObjectManager.Player;

                            if (player == null)
                            {
                                return;
                            }

                            characterState.SetAccountInfoRequested = false;
                            characterState.StartRequested = false;
                            characterState.StopRequested = false;

                            if (player.Level > currentLevel)
                            {
                                currentLevel = player.Level;
                                DiscordClientWrapper.SendMessage($"Ding! {player.Name} is now level {player.Level}!");
                            }

                            if (botTasks.Count > 0 && botTasks.Peek()?.GetType() == typeof(QuestingTask))
                            {
                                retrievingCorpse = false;
                            }

                            // if the player has been stuck in the same state for more than 5 minutes
                            if (Environment.TickCount < 0)
                            {
                                var msg = $"Hey, it's {player.Name}, and I need help! I've been stuck in the {currentState} for over 5 minutes. I'm stopping for now.";
                                LogToFile(msg);
                                DiscordClientWrapper.SendMessage(msg);
                                Stop();
                                return;
                            }
                            if (botTasks.Count > 0 && botTasks.Peek()?.GetType().Name != currentState)
                            {
                                currentState = botTasks.Peek()?.GetType().Name;
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

                                botTasks.Push(container.CreateRestTask(container, botTasks));
                                botTasks.Push(new RetrieveCorpseTask(container, botTasks));
                                botTasks.Push(new MoveToCorpseTask(container, botTasks));
                                botTasks.Push(new ReleaseCorpseTask(container, botTasks));
                            }

                            // if equipment needs to be repaired
                            int mainhandDurability = Inventory.GetEquippedItem(EquipSlot.MainHand)?.DurabilityPercentage ?? 100;
                            int offhandDurability = Inventory.GetEquippedItem(EquipSlot.Ranged)?.DurabilityPercentage ?? 100;

                            // offhand throwns don't have durability, but instead register `-2147483648`.
                            // This is a workaround to prevent that from causing us to get caught in a loop.
                            // We default to a durability value of 100 for items that are null because 100 will register them as not needing repaired.
                            if ((mainhandDurability <= 20 && mainhandDurability > -1) || (offhandDurability <= 20 && offhandDurability > -1))
                            {
                                ShapeshiftToHumanForm();
                                PopStackToBaseState();
                            }

                            // if inventory is full
                            if (Inventory.CountFreeSlots(false) < 3)
                            {
                                ShapeshiftToHumanForm();
                                PopStackToBaseState();

                                Creature vendor = SqliteRepository.GetAllVendors()
                                                                    .OrderBy(x => new Position(x.PositionX, x.PositionY, x.PositionZ).DistanceTo(ObjectManager.Player.Position))
                                                                    .First();

                                botTasks.Push(new SellItemsTask(container, botTasks, vendor.Name));
                                botTasks.Push(new MoveToPositionTask(container, botTasks, new Position(vendor.PositionX, vendor.PositionY, vendor.PositionZ)));
                            }

                            currentLevel = ObjectManager.Player.Level;
                        }

                        if (characterState.AccountName != _accountName
                                || characterState.CharacterSlot != _characterSlot
                                || characterState.BotProfileName != _botProfileName)
                        {
                            while (botTasks.Count > 0)
                                botTasks.Pop();

                            botTasks.Push(new LoginTask(container, botTasks, _accountName, _characterSlot));
                            botTasks.Push(new LogoutTask(container, botTasks));

                            characterState.AccountName = _accountName;
                            characterState.CharacterSlot = _characterSlot;
                            characterState.BotProfileName = _botProfileName;

                            characterState.SetAccountInfoRequested = true;
                        }

                        if (botTasks.Count > 0)
                        {
                            if (!FrameHelper.IsElementVisibile("CinematicFrame"))
                            {
                                botTasks.Peek()?.Update();
                            }
                        } else if (ObjectManager.IsLoggedIn)
                        {
                            botTasks.Push(new QuestingTask(container, botTasks));
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

        public void SetAccountInfo(string accountName, int characterSlot, string botName)
        {
            if (accountName != _accountName
                || characterSlot != _characterSlot
                || botName != _botProfileName)
            {
                bool wasRunning = timer.Enabled;
                if (wasRunning)
                {
                    Stop();
                }

                _accountName = accountName;
                _characterSlot = characterSlot;
                _botProfileName = botName;

                ReloadBots();

                if (wasRunning)
                {
                    Start();
                }
            }
        }

        private void ReloadBots()
        {
            try
            {
                Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

                CurrentBot = Bots.First(b => b.Name == _botProfileName);

                container = CurrentBot.GetClassContainer(characterState);
            } catch (Exception ex)
            {
                Logger.Log(ex + "\n");
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

        void ShapeshiftToHumanForm()
        {
            if (ObjectManager.Player.Class == Class.Druid && ObjectManager.Player.CurrentShapeshiftForm == "Bear Form")
                ObjectManager.Player.LuaCall("CastSpellByName('Bear Form')");
            if (ObjectManager.Player.Class == Class.Druid && ObjectManager.Player.CurrentShapeshiftForm == "Cat Form")
                ObjectManager.Player.LuaCall("CastSpellByName('Cat Form')");
        }

        void PopStackToBaseState()
        {
            while (botTasks.Count > 1)
                botTasks.Pop();
        }
    }
}
