using WoWActivityMember.Tasks;

namespace WarriorProtection.Tasks
{
    class BuffTask : BotTask, IBotTask
    {
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }

        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
