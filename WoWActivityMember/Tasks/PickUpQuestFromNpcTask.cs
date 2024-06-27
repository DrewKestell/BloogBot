using WoWActivityMember.Game.Frames;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class PickUpQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        readonly string npcName = npcName;
        readonly LocalPlayer player = ObjectManager.Player;
        readonly int startTime = Environment.TickCount;
        readonly int currentQuestLogSize;

        readonly WoWUnit npc = ObjectManager
                .Units
                .First(x => x.Name == npcName);
        readonly DialogFrame dialogFrame;

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
