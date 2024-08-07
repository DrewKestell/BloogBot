using BotRunner.Interfaces;
using System.Diagnostics;

namespace BotRunner.Tasks
{
    public class QuestingTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = [];

        public void Update()
        {
            BotTasks.Pop();

            if (ObjectManager.Player.IsInCombat)
            {
                BotTasks.Push(Container.CreatePvERotationTask(BotContext));
            }
        }
    }
}
