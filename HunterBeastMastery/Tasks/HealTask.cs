

using WoWActivityMember.Tasks;

namespace HunterBeastMastery.Tasks
{
    internal class HealTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Heal), IBotTask
    {
        public void Update()
        {

        }
    }
}
