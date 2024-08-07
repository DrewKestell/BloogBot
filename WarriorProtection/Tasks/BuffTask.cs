using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace WarriorProtection.Tasks
{
    internal class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
