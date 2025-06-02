using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PriestHoly.Tasks
{
    internal class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
