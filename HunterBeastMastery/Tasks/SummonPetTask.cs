using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace HunterBeastMastery.Tasks
{
    internal class SummonPetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
