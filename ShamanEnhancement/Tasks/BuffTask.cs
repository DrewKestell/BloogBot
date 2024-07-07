using WoWActivityMember.Tasks;

namespace ShamanEnhancement.Tasks
{
    internal class BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
