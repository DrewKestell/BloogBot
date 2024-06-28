

using WoWActivityMember.Tasks;

namespace HunterBeastMastery.Tasks
{
    class HealTask : BotTask, IBotTask
    {
        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {

        }
    }
}
