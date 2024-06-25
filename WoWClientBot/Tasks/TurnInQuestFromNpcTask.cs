using WoWClientBot.Game.Statics;
using WoWClientBot.Objects;

namespace WoWClientBot.AI.SharedStates
{
    public class TurnInQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, string questName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        readonly LocalPlayer player = ObjectManager.Player;
        readonly int startTime = Environment.TickCount;
        readonly string questName = questName;
        readonly int rewardSelection;

        readonly WoWUnit npc = ObjectManager
                .Units
.First(x => x.Name == npcName);

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat || (Environment.TickCount - startTime > 5000))
            {
                BotTasks.Pop();
                return;
            }

        }
    }
}
