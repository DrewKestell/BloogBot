using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    internal class GatherObjectTask : BotTask, IBotTask
    {
        private readonly IWoWGameObject target;
        private readonly int initialCount = 0;
        private readonly int startTime = Environment.TickCount;
        private readonly ILootFrame lootFrame;
        internal GatherObjectTask(IBotContext botContext, IWoWGameObject target) : base(botContext) => this.target = target;

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat || Environment.TickCount - startTime > 15000)
            {
                BotTasks.Pop();
                return;
            }

            if (Wait.For("InteractWithObjectDelay", 15000, true))
            {
                ObjectManager.GetTarget(ObjectManager.Player).Interact();
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
