using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class TurnInQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, string questName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly LocalPlayer player = ObjectManager.Player;
        private readonly int startTime = Environment.TickCount;
        private readonly string questName = questName;
        private readonly int rewardSelection;
        private readonly WoWUnit npc = ObjectManager
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
