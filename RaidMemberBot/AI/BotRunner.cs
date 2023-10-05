using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.AI.SharedTasks;
using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Models;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using RaidMemberBot.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using static RaidMemberBot.Constants.Enums;
using System.Threading.Tasks;

namespace RaidMemberBot.AI
{
    public class BotRunner
    {
        readonly Stack<IBotTask> botTasks = new Stack<IBotTask>();
        readonly BotLoader botLoader = new BotLoader();

        IClassContainer container;
        ObservableCollection<IBot> Bots = new ObservableCollection<IBot>();

        bool retrievingCorpse;
        bool _shouldRun;
        bool _runningErrands;

        string currentState;
        int currentLevel;

        string _accountName;
        string _botProfileName;
        int _characterSlot;

        Task _asyncTask;

        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = new Dictionary<ulong, Stopwatch>();

        readonly CharacterState characterState;
        public BotRunner(CharacterState characterState)
        {
            this.characterState = characterState;

            ObjectManager.Instance.StartEnumeration(this.characterState);
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

        public bool Running() => _shouldRun;

        public void Start()
        {
            try
            {
                _shouldRun = true;
                _asyncTask = Task.Run(StartAsync);
            }
            catch (Exception e)
            {
                Console.WriteLine("BotRunner: " + e);
            }
        }

        public void Stop()
        {
            _shouldRun = false;
            ThreadSynchronizer.Instance.ClearQueue();

            while (botTasks.Count > 0)
            {
                botTasks.Pop();
            }
        }
        private async void StartAsync()
        {
            while (_shouldRun)
            {
                try
                {
                    if (ThreadSynchronizer.Instance.QueueCount() == 0)
                    {
                        ThreadSynchronizer.Instance.RunOnMainThread(() =>
                        {
                            if (ObjectManager.Instance.IsIngame
                                && characterState.AccountName == _accountName
                                && characterState.CharacterSlot == _characterSlot
                                && characterState.BotProfileName == _botProfileName)
                            {
                                var player = ObjectManager.Instance.Player;

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
                                else
                                {
                                    // if equipment needs to be repaired
                                    int mainhandDurability = Inventory.Instance.GetEquippedItem(EquipSlot.MainHand)?.DurabilityPercent ?? 100;
                                    int offhandDurability = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged)?.DurabilityPercent ?? 100;

                                    // offhand throwns don't have durability, but instead register `-2147483648`.
                                    // This is a workaround to prevent that from causing us to get caught in a loop.
                                    // We default to a durability value of 100 for items that are null because 100 will register them as not needing repaired.
                                    if ((mainhandDurability <= 20 && mainhandDurability > -1) || (offhandDurability <= 20 && offhandDurability > -1))
                                    {
                                        ShapeshiftToHumanForm();
                                        PopStackToBaseState();
                                    }

                                    // if inventory is full
                                    if (!_runningErrands && !player.IsInCombat && Inventory.Instance.CountFreeSlots(false) < 3)
                                    {
                                        ShapeshiftToHumanForm();
                                        PopStackToBaseState();

                                        Creature vendor = SqliteRepository.GetAllVendors()
                                                                            .Where(x => !string.IsNullOrEmpty(x.Name))
                                                                            .OrderBy(x => new Location(x.LocationX, x.LocationY, x.LocationZ).GetDistanceTo(ObjectManager.Instance.Player.Location))
                                                                            .First();

                                        botTasks.Push(new SellItemsTask(container, botTasks, vendor));
                                        _runningErrands = true;
                                    }
                                }

                                currentLevel = ObjectManager.Instance.Player.Level;
                            }

                            if (characterState.AccountName != _accountName
                                    || characterState.CharacterSlot != _characterSlot
                                    || characterState.BotProfileName != _botProfileName)
                            {
                                while (botTasks.Count > 0)
                                    botTasks.Pop();

                                characterState.AccountName = _accountName;
                                characterState.CharacterSlot = _characterSlot;
                                characterState.BotProfileName = _botProfileName;

                                botTasks.Push(new LoginTask(container, botTasks, _accountName, _characterSlot));
                                botTasks.Push(new LogoutTask(container, botTasks));
                            }

                            if (botTasks.Count > 0)
                            {
                                characterState.CurrentTask = string.Format("[{0}] {1}", botTasks.Peek()?.GetType()?.Name, botTasks.Count);
                                botTasks.Peek()?.Update();
                            }
                            else if (ObjectManager.Instance.IsIngame)
                            {
                                //WoWGameObject questObject = NearestQuestObject;
                                //WoWUnit questTarget = NearestQuestTarget;
                            }
                        });
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex + "\n");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public void SetAccountInfo(string accountName, int characterSlot, string botName)
        {
            try
            {
                if (accountName != _accountName
                    || characterSlot != _characterSlot
                    || botName != _botProfileName)
                {
                    bool wasRunning = _shouldRun;
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
            catch (Exception ex)
            {
                Console.WriteLine(ex + "\n");
            }
        }

        private void ReloadBots()
        {
            try
            {
                Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

                CurrentBot = Bots.First(b => b.Name == _botProfileName);

                container = CurrentBot.GetClassContainer(characterState);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + "\n");
            }
        }

        void LogToFile(string text)
        {
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RaidMemberViewModel)).CodeBase);
            var path = new UriBuilder(dir).Path;
            var file = Path.Combine(path, "StuckLog.txt");

            using (var sw = File.AppendText(file))
            {
                sw.WriteLine(text);
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

        private void HandleAttackableTarget(WoWUnit questTarget)
        {
            //List<int> itemToUseOnMob = QuestLog.Instance.Quests.SelectMany(x => x.Objectives
            //                                                                            .Where(y => y.UsableItemId != 0 && y.TargetCreatureId == questTarget.Id)
            //                                                                            .Select(z => z.UsableItemId))
            //                                                                            .ToList();
            List<int> itemToUseOnMob = new List<int>();

            if (itemToUseOnMob.Count > 0)
            {
                Console.WriteLine(Inventory.Instance.GetItem(0, 0).Name);

                foreach (WoWItem wowItem in Inventory.Instance.GetAllItems())
                {
                    Console.WriteLine(wowItem.Name);
                }
                WoWItem questItem = Inventory.Instance.GetAllItems().First(x => x.Id == itemToUseOnMob[0]);

                botTasks.Push(new UseItemOnUnitTask(container, botTasks, questTarget, questItem));
                if (questTarget.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) > 5)
                {
                    botTasks.Push(new MoveToLocationTask(container, botTasks, questTarget.Location));
                }

                TargetGuidBlacklist.Add(questTarget.Guid, Stopwatch.StartNew());
            }
            else
            {
                botTasks.Push(container.CreateMoveToAttackTargetTask(container, botTasks, questTarget));
            }
        }
        public WoWUnit NearestQuestTarget
        {
            get
            {
                List<QuestObjective> questObjectives = QuestLog.Instance.Quests.SelectMany(x => x.Objectives).ToList();
                List<WoWUnit> woWUnits = ObjectManager.Instance.Units.ToList();

                return woWUnits
                    .Where(unit => questObjectives.Where(x => x.ObjectId != 0).Select(x => x.ObjectId).Contains(unit.Id))
                    .Where(unit => unit.Health > 0)
                    .Where(unit => !(TargetGuidBlacklist.ContainsKey(unit.Guid) && unit.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) > 75))
                    .OrderBy(unit => Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, unit.Location, ObjectManager.Instance.Player.Location, true))
                    .FirstOrDefault();
            }
        }

        public WoWGameObject NearestQuestObject
        {
            get
            {
                List<QuestObjective> questObjectives = QuestLog.Instance.Quests.SelectMany(x => x.Objectives).ToList();

                Console.WriteLine($"[ScanForQuestUnitsTask] questObjectives {JsonConvert.SerializeObject(questObjectives)}");
                Console.WriteLine($"[ScanForQuestUnitsTask] GameObjects {JsonConvert.SerializeObject(ObjectManager.Instance.GameObjects)}");

                return ObjectManager.Instance.GameObjects
                    .Where(gameObject => questObjectives.Where(x => x.ObjectId != 0).Select(x => x.ObjectId).Contains(gameObject.Id))
                    .OrderBy(gameObject => Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, gameObject.Location, ObjectManager.Instance.Player.Location, true))
                    .FirstOrDefault();
            }
        }
    }
}
