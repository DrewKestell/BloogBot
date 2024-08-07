using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class PickUpQuestFromNpcTask(IBotContext botContext, string npcName) : BotTask(botContext), IBotTask
    {
        private readonly string npcName = npcName;
        private readonly ILocalPlayer player = botContext.ObjectManager.Player;
        private readonly int startTime = Environment.TickCount;
        private readonly int currentQuestLogSize;
        private readonly IWoWUnit npc = botContext.ObjectManager
                .Units
                .First(x => x.Name == npcName);
        private readonly IDialogFrame dialogFrame;

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
