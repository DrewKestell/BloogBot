using WoWClientBot.AI;
using WoWClientBot.Objects;

namespace DruidRestoration.Tasks
{
    class PullTargetTask : BotTask, IBotTask
    {
        readonly string pullingSpell;
        Position currentWaypoint;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {

        }

        public void Update()
        {

        }
    }
}
