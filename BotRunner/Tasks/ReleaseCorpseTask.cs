using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class ReleaseCorpseTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
        }
    }
}
