using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace ShamanRestoration.Tasks
{
    public class PullTargetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
