using WoWActivityMember.Game.Frames;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class PickUpQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly string npcName = npcName;
        private readonly LocalPlayer player = ObjectManager.Player;
        private readonly int startTime = Environment.TickCount;
        private readonly int currentQuestLogSize;
        private readonly WoWUnit npc = ObjectManager
                .Units
                .First(x => x.Name == npcName);
        private readonly DialogFrame dialogFrame;

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
