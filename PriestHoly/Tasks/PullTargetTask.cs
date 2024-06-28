using WoWActivityMember.Tasks;

namespace PriestHoly.Tasks
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {

        }

        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
