using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class LogoutTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
