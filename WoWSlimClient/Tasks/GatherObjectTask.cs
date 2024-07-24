using WoWSlimClient.Frames;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    internal class GatherObjectTask : BotTask, IBotTask
    {
        private readonly WoWGameObject target;
        private readonly int initialCount = 0;
        private readonly int startTime = Environment.TickCount;
        private readonly LootFrame lootFrame;
        internal GatherObjectTask(IClassContainer container, Stack<IBotTask> botTasks, WoWGameObject target) : base(container, botTasks, TaskType.Ordinary)
        {
            this.target = target;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Player.IsInCombat || (Environment.TickCount - startTime > 15000))
            {
                BotTasks.Pop();
                return;
            }

            if (Wait.For("InteractWithObjectDelay", 15000, true))
            {
                ObjectManager.Instance.Player.Target.Interact();
            }

            if (lootFrame.LootItems.Count(x => x.Info.Name == target.Name) > initialCount)
            {
                if (Wait.For("PopGatherObjectStateDelay", 2000))
                {
                    Wait.RemoveAll();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
