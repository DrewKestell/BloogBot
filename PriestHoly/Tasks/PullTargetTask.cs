using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PriestHoly.Tasks
{
    public class PullTargetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
