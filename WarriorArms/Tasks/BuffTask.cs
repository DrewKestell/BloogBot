using WoWActivityMember.Tasks;

namespace WarriorArms.Tasks
{
    internal class BuffTask : BotTask, IBotTask
    {
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
