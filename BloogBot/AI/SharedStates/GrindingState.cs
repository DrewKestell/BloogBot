using BloogBot.Game;
using BloogBot.Game.Objects;
using BloogBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    class GrindingState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        static readonly List<QuestTask> tasks = new List<QuestTask>();

        public static List<QuestTask> GetQuestTasks()
        {
                List<int> questIds = QuestHelper.GetQuestsFromQuestLog()
                                                .Select(x => x.ID)
                                                .ToList();

                tasks.RemoveAll(x => !questIds.Contains(x.QuestId));

                tasks.AddRange(questIds.Where(x => tasks.All(y => x != y.QuestId))
                                        .Select(x => QuestHelper.GetQuestTaskById(x))
                                        .ToList());
                return tasks;
        }
        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = new Dictionary<ulong, Stopwatch>();

        public static Position CurrentHotSpot;
        public static string CurrentQuestName;
        public static string CurrentTask;
        public void Update()
        {
            if (ClosestObjective != null)
            {
                CurrentQuestName = GetQuestTasks().Where(x => x.QuestObjectives.Find(y => y.QuestId == ClosestObjective.QuestId && y.Index == ClosestObjective.Index) != null).First().Name;
            }

            if (player.Level < 60)
            {
                if (player.IsInCombat)
                {
                    botStates.Push(container.CreateMoveToTargetState(botStates, container, ObjectManager.Aggressors.First()));
                }

                botStates.Push(new CheckForQuestEntitiesState(botStates, container));

                List<QuestObjective> questObjectives = RemainingQuestObjectives.ToList();

                if (QuestHelper.NearbyQuestGivers.Count == 0 && questObjectives.Count != 0)
                {
                    Position position = QuestHelper.GetNextPositionHotSpot();
                    CurrentTask = string.Format("[Questing: {0}]Moving To X:{1}, Y:{2}, Z:{3}", CurrentQuestName, position.X.ToString(), position.Y.ToString(), position.Z.ToString());
                    botStates.Push(new MoveToPositionState(botStates, container, position));
                }
                else if (GetQuestTasks().Count != 0 && questObjectives.Count == 0)
                {
                    QuestTask questTask = GetQuestTasks().Where(x => x.IsComplete())
                        .OrderBy(x => Navigation.DistanceViaPath(ObjectManager.MapId, x.TurnInNpc.Position, ObjectManager.Player.Position))
                        .ToArray()[0];

                    CurrentQuestName = questTask.Name;
                    CurrentTask = string.Format("[Questing: {0}]Returning to {1} for turn in", CurrentQuestName, CurrentQuestName, questTask.TurnInNpc.NpcName);
                    botStates.Push(new MoveToPositionState(botStates, container, questTask.TurnInNpc.Position));
                }
            }
        }
        public static QuestObjective ClosestObjective
        {
            get
            {
                List<QuestObjective> questObjectives = RemainingQuestObjectives.ToList();
                List<Position> closestPositions = questObjectives
                                                        .SelectMany(x => x.HotSpots)
                                                        .OrderBy(spot => spot.DistanceTo(ObjectManager.Player.Position))
                                                        .ToList();
                return questObjectives.Find(x => x.HotSpots.Select(y => y.DistanceTo(closestPositions[0]) < 5) != null);
            }
        }
        public static List<QuestObjective> RemainingQuestObjectives
        {
            get
            {
                return GetQuestTasks().Where(x => !x.IsComplete())
                                   .SelectMany(x => x.QuestObjectives)
                                       .ToList()
                                       .FindAll(x => !x.IsComplete());
            }
        }
        public static WoWUnit NearestQuestTarget
        {
            get
            {
                List<QuestObjective> questObjectives = RemainingQuestObjectives.ToList();
                List<WoWUnit> woWUnits = ObjectManager.Units.ToList();

                return woWUnits
                    .Where(unit => questObjectives.Where(x => x.TargetCreatureId != 0).Select(x => x.TargetCreatureId).Contains(unit.Id))
                    .Where(unit => unit.Health > 0)
                    .Where(unit => !(TargetGuidBlacklist.ContainsKey(unit.Guid) && unit.Position.DistanceTo(ObjectManager.Player.Position) > 75))
                    .OrderBy(unit => Navigation.DistanceViaPath(ObjectManager.MapId, unit.Position, ObjectManager.Player.Position))
                    .FirstOrDefault();
            }
        }

        public static WoWGameObject NearestQuestObject
        {
            get
            {
                List<QuestObjective> questObjectives = RemainingQuestObjectives.ToList();
                if (questObjectives.Count > 0)
                {

                }
                return ObjectManager.GameObjects
                    .Where(gameObject => questObjectives.Where(x => x.TargetGameObjectId != 0).Select(x => x.TargetGameObjectId).Contains(gameObject.Id))
                    .OrderBy(gameObject => Navigation.DistanceViaPath(ObjectManager.MapId, gameObject.Position, ObjectManager.Player.Position))
                    .FirstOrDefault();
            }
        }

        public GrindingState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }
    }
}
