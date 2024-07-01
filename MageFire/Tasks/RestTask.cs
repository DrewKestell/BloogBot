using WoWActivityMember.Tasks;

namespace MageFire.Tasks
{
    internal class RestTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Rest), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
