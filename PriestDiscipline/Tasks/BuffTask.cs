using WoWActivityMember.Tasks;

namespace PriestDiscipline.Tasks
{
    internal class BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
