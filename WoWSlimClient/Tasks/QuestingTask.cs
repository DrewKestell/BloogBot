using System.Diagnostics;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedTasks
{
    public class QuestingTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        public static readonly Dictionary<ulong, Stopwatch> TargetGuidBlacklist = [];

        public static Position CurrentHotSpot;

        public void Update()
        {
            BotTasks.Pop();

            if (ObjectManager.Instance.Player.IsInCombat)
            {
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
            }
        }
    }
}
