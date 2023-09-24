using BloogBot.Game;
using BloogBot.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    class GrindingState : BotState, IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        static readonly List<QuestTask> tasks = new List<QuestTask>();

        public GrindingState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }

        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = new Dictionary<ulong, Stopwatch>();
        
        public static Position CurrentHotSpot;
        public static string CurrentQuestName;
        public static string CurrentTask;
        public void Update()
        {
            Functions.LuaCall("QUEST_FADING_DISABLE = \"1\"");

            if (ClosestObjective != null)
            {
                QuestObjective questObjective = (QuestObjective)ClosestObjective.Clone();
                CurrentQuestName = GetQuestTasks().Where(x => x.QuestObjectives.Find(y => y.QuestId == questObjective.QuestId && y.Index == questObjective.Index) != null).First().Name;
            }

            if (ObjectManager.Player.Level < 60)
            {
                if (ObjectManager.Player.IsInCombat)
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
        public static List<QuestTask> GetQuestTasks()
        {
            List<int> questIds = ObjectManager.Player.GetPlayerQuests()
                                            .Select(x => x.ID)
                                            .ToList();

            tasks.RemoveAll(x => !questIds.Contains(x.QuestId));

            tasks.AddRange(questIds.Where(x => tasks.All(y => x != y.QuestId))
                                    .Select(x => QuestHelper.GetQuestTaskById(x))
                                    .Where(x => x != null)
                                    .ToList());
            return tasks;
        }
    }
}
