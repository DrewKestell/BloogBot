using BloogBot.AI.SharedTasks;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using BloogBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    class ScanForQuestUnitsTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = new Dictionary<ulong, Stopwatch>();

        readonly int startTime = Environment.TickCount;
        public ScanForQuestUnitsTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
        }
        public void Update()
        {
            if (ObjectManager.Player.IsInCombat)
            {
                ObjectManager.Player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(container.CreateMoveToAttackTargetTask(container, botTasks, ObjectManager.Aggressors.First()));
                return;
            }

            botTasks.Pop();

            QuestingTask.CurrentTask = string.Format("[Questing: {0}]Scanning the area", QuestingTask.CurrentQuestName);

            WoWGameObject questObject = NearestQuestObject;
            WoWUnit questTarget = NearestQuestTarget;
            List<WoWUnit> questGivers = QuestHelper.NearbyQuestGivers;
            List<WoWUnit> questTurnIns = QuestHelper.NearbyQuestTurnIns;

            if (questGivers.Count > 0 || questTurnIns.Count > 0 || questObject != null || questTarget != null)
            {
                List<WoWUnit> nearbyMobs = ObjectManager.Units.ToList()
                                .FindAll(x => x.Health > 0 && !x.NotAttackable && x.UnitReaction == UnitReaction.Hostile)
                                .OrderBy(x => Navigation.DistanceViaPath(ObjectManager.MapId, x.Position, ObjectManager.Player.Position))
                                .ToList();

                if (questTurnIns.Count > 0)
                {
                    QuestingTask.CurrentTask = string.Format("[Questing: {0}]Turning in quest to {1}", QuestingTask.CurrentQuestName, questTurnIns[0].Name);
                    botTasks.Push(new TurnInQuestFromNpcTask(container, botTasks, questTurnIns[0].Name, QuestingTask.GetQuestTasks().Where(x => x.IsComplete() && x.TurnInNpc.NpcName == questTurnIns[0].Name).First()));

                    if (Navigation.DistanceViaPath(ObjectManager.MapId, ObjectManager.Player.Position, questTurnIns[0].Position) > 3)
                    {
                        botTasks.Push(new MoveToPositionTask(container, botTasks, questTurnIns[0].Position, false, true));
                    }
                }
                else if (questGivers.Count > 0 && ObjectManager.Player.GetPlayerQuests().Count < 20)
                {
                    QuestingTask.CurrentTask = string.Format("[Questing: {0}]Picking up quest from {1}", QuestingTask.CurrentQuestName, questGivers[0].Name);
                    botTasks.Push(new PickUpQuestFromNpcTask(container, botTasks, questGivers[0].Name));

                    if (Navigation.DistanceViaPath(ObjectManager.MapId, ObjectManager.Player.Position, questGivers[0].Position) > 3)
                    {
                        botTasks.Push(new MoveToPositionTask(container, botTasks, questGivers[0].Position, false, true));
                    }
                }
                else if (questObject != null)
                {
                    QuestingTask.CurrentTask = string.Format("[Questing: {0}]Looting quest object {1}", QuestingTask.CurrentQuestName, questObject.Name);
                    // If the object is closer to the character than the mob
                    // or the aggro distance (with padding) is still longer than the distance from the mob to the game object
                    if (nearbyMobs.Count > 0)
                    {
                        if (Navigation.DistanceViaPath(ObjectManager.MapId, questObject.Position, ObjectManager.Player.Position) < Navigation.DistanceViaPath(ObjectManager.MapId, nearbyMobs[0].Position, ObjectManager.Player.Position)
                            || nearbyMobs[0].AggroDistance + 5 > Navigation.DistanceViaPath(ObjectManager.MapId, nearbyMobs[0].Position, questObject.Position))
                        {
                            botTasks.Push(new GatherObjectTask(container, botTasks, questObject));
                            // Go ahead and get the game object
                            if (Navigation.DistanceViaPath(ObjectManager.MapId, ObjectManager.Player.Position, questObject.Position) > 3)
                            {
                                botTasks.Push(new MoveToPositionTask(container, botTasks, questObject.Position, false, true));
                            }
                        }
                        else
                        {
                            QuestingTask.CurrentTask = string.Format("[Questing: {0}]Clearing {1}", QuestingTask.CurrentQuestName, nearbyMobs[0].Name);
                            // Handle the mob since we might aggro it while interacting with the game object
                            botTasks.Push(container.CreateMoveToAttackTargetTask(container, botTasks, nearbyMobs[0]));
                        }
                    }
                    else
                    {
                        botTasks.Push(new GatherObjectTask(container, botTasks, questObject));
                        if (Navigation.DistanceViaPath(ObjectManager.MapId, ObjectManager.Player.Position, questObject.Position) > 3)
                        {
                            botTasks.Push(new MoveToPositionTask(container, botTasks, questObject.Position, false, true));
                        }
                    }
                }
                else if (questTarget != null)
                {
                    nearbyMobs.RemoveAll(x => x.Guid == questTarget.Guid);

                    if (nearbyMobs.Count > 0)
                    {
                        if (Navigation.DistanceViaPath(ObjectManager.MapId, questTarget.Position, ObjectManager.Player.Position) < Navigation.DistanceViaPath(ObjectManager.MapId, nearbyMobs[0].Position, ObjectManager.Player.Position)
                            || nearbyMobs[0].AggroDistance + 5 > Navigation.DistanceViaPath(ObjectManager.MapId, nearbyMobs[0].Position, questTarget.Position))
                        {
                            HandleAttackableTarget(questTarget);
                        }
                        else
                        {
                            QuestingTask.CurrentTask = string.Format("[Questing: {0}]Clearing {1}", QuestingTask.CurrentQuestName, nearbyMobs[0].Name);
                            // Handle the mob since we might aggro it while attacking the quest target
                            botTasks.Push(container.CreateMoveToAttackTargetTask(container, botTasks, nearbyMobs[0]));
                        }
                    }
                    else
                    {
                        HandleAttackableTarget(questTarget);
                    }
                }

                if (TargetGuidBlacklist.Keys.Count > 0)
                {
                    List<ulong> keys = TargetGuidBlacklist.Keys.ToList();
                    foreach (var key in keys)
                    {
                        if (TargetGuidBlacklist[key] != null && TargetGuidBlacklist[key].Elapsed > TimeSpan.FromMinutes(2))
                        {
                            TargetGuidBlacklist.Remove(key);
                        }
                    }
                }
            }
        }

        private void HandleAttackableTarget(WoWUnit questTarget)
        {
            List<int> itemToUseOnMob = QuestingTask.GetQuestTasks().SelectMany(x => x.QuestObjectives
                                                                                        .Where(y => y.UsableItemId != 0 && y.TargetCreatureId == questTarget.Id)
                                                                                        .Select(z => z.UsableItemId))
                                                                                        .ToList();

            if (itemToUseOnMob.Count > 0)
            {
                Logger.Log(Inventory.GetItem(0, 0).Name);

                foreach(WoWItem wowItem in Inventory.GetAllItems())
                {
                    Logger.Log(wowItem.Name);
                }
                WoWItem questItem = Inventory.GetAllItems().First(x => x.ItemId == itemToUseOnMob[0]);

                QuestingTask.CurrentTask = string.Format("[Questing: {0}]Marking {1} with {2}", QuestingTask.CurrentQuestName, questTarget.Name, questItem.Name);

                botTasks.Push(new UseItemOnUnitTask(container, botTasks, questTarget, questItem));
                if (questTarget.Position.DistanceTo(ObjectManager.Player.Position) > 5)
                {
                    botTasks.Push(new MoveToPositionTask(container, botTasks, questTarget.Position, false, true));
                }
            }
            else
            {
                QuestingTask.CurrentTask = string.Format("[Questing: {0}]Attacking {1}", QuestingTask.CurrentQuestName, questTarget.Name);
                botTasks.Push(container.CreateMoveToAttackTargetTask(container, botTasks, questTarget));
            }
        }
        public WoWUnit NearestQuestTarget
        {
            get
            {
                List<QuestObjective> questObjectives = QuestingTask.RemainingQuestObjectives.ToList();
                List<WoWUnit> woWUnits = ObjectManager.Units.ToList();

                return woWUnits
                    .Where(unit => questObjectives.Where(x => x.TargetCreatureId != 0).Select(x => x.TargetCreatureId).Contains(unit.Id))
                    .Where(unit => unit.Health > 0)
                    .Where(unit => !(TargetGuidBlacklist.ContainsKey(unit.Guid) && unit.Position.DistanceTo(ObjectManager.Player.Position) > 75))
                    .OrderBy(unit => Navigation.DistanceViaPath(ObjectManager.MapId, unit.Position, ObjectManager.Player.Position))
                    .FirstOrDefault();
            }
        }

        public WoWGameObject NearestQuestObject
        {
            get
            {
                List<QuestObjective> questObjectives = QuestingTask.RemainingQuestObjectives.ToList();
                if (questObjectives.Count > 0)
                {

                }
                return ObjectManager.GameObjects
                    .Where(gameObject => questObjectives.Where(x => x.TargetGameObjectId != 0).Select(x => x.TargetGameObjectId).Contains(gameObject.Id))
                    .OrderBy(gameObject => Navigation.DistanceViaPath(ObjectManager.MapId, gameObject.Position, ObjectManager.Player.Position))
                    .FirstOrDefault();
            }
        }
    }
}
