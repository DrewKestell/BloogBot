using WoWActivityMember.Tasks;

namespace PriestDiscipline.Tasks
{
    class RestTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Rest), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
