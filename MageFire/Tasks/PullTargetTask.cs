using WoWActivityMember.Tasks;

namespace MageFire.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
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
