using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace MageFire.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {

        internal PullTargetTask(IBotContext botContext) : base(botContext)
        {
            
        }

        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
