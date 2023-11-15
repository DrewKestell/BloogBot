using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaidMemberBot.AI.SharedTasks
{
    public class QuestingTask : BotTask, IBotTask
    {
        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = new Dictionary<ulong, Stopwatch>();

        public static Position CurrentHotSpot;

        public QuestingTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            BotTasks.Pop();

            if (ObjectManager.Player.IsInCombat)
            {
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
            }
        }
    }
}
