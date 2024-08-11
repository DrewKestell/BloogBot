using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class TurnInQuestFromNpcTask(IBotContext botContext, string npcName, string questName) : BotTask(botContext), IBotTask
    {
        private readonly int startTime = Environment.TickCount;
        private readonly string questName = questName;
        private readonly int rewardSelection;

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat || Environment.TickCount - startTime > 5000)
            {
                BotTasks.Pop();
                return;
            }

        }
    }
}
