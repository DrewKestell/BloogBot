using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RaidMemberBot.AI.SharedTasks
{
    public class QuestingTask : BotTask, IBotTask
    {
        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = new Dictionary<ulong, Stopwatch>();

        public static Location CurrentHotSpot;

        public QuestingTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            Functions.DoString("QUEST_FADING_DISABLE = \"1\"");
            BotTasks.Pop();

            if (ObjectManager.Instance.Player.IsInCombat)
            {
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
            }

            List<QuestObjective> questObjectives = QuestLog.Instance.Quests.SelectMany(x => x.Objectives).ToList();

            
        }
    }
}
