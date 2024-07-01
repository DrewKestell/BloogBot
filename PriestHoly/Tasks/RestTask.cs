using WoWActivityMember.Tasks;

namespace PriestHoly.Tasks
{
    internal class RestTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Rest), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
