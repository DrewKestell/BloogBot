namespace WoWSlimClient.Tasks
{
    public abstract class BotTask(IClassContainer container, Stack<IBotTask> botTasks, TaskType taskType)
    {
        public TaskType TaskType { get; } = taskType;

        public readonly IClassContainer Container = container;
        public readonly Stack<IBotTask> BotTasks = botTasks;
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
