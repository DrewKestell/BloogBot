using WoWActivityMember.Tasks;

namespace HunterSurvival.Tasks
{
    class SummonPetTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
