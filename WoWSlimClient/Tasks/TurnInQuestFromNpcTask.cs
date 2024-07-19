using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class TurnInQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, string questName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly WoWLocalPlayer player = ObjectManager.Instance.Player;
        private readonly int startTime = Environment.TickCount;
        private readonly string questName = questName;
        private readonly int rewardSelection;
        private readonly WoWUnit npc = ObjectManager.Instance.Units.First(x => x.Name == npcName);

        public void Update()
        {
            if (ObjectManager.Instance.Player.IsInCombat || (Environment.TickCount - startTime > 5000))
            {
                BotTasks.Pop();
                return;
            }

        }
    }
}
