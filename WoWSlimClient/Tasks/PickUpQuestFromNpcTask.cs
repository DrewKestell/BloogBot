
using WoWSlimClient.Frames;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class PickUpQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly string npcName = npcName;
        private readonly WoWLocalPlayer player = ObjectManager.Instance.Player;
        private readonly int startTime = Environment.TickCount;
        private readonly int currentQuestLogSize;
        private readonly WoWUnit npc = ObjectManager.Instance
                .Units
                .First(x => x.Name == npcName);
        private readonly DialogFrame dialogFrame;

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
