using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace HunterSurvival.Tasks
{
    internal class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
