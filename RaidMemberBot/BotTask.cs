using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public abstract class BotTask
    {
        public TaskType TaskType { get; }

        public readonly IClassContainer Container;
        public readonly Stack<IBotTask> BotTasks;
        public BotTask(IClassContainer container, Stack<IBotTask> botTasks, TaskType taskType)
        {
            Container = container;
            BotTasks = botTasks;
            TaskType = taskType;
        }
    }

    public enum TaskType
    {
        Ordinary,
        Rest,
        Buff,
        Pull,
        Heal,
        Combat
    }
}
