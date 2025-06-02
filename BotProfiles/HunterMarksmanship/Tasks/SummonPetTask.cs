using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace HunterMarksmanship.Tasks
{
    internal class SummonPetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
