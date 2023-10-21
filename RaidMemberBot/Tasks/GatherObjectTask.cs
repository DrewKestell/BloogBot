using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    class GatherObjectTask : BotTask, IBotTask
    {
        readonly WoWGameObject target;
        readonly LocalPlayer player;
        readonly int initialCount = 0;

        readonly int startTime = Environment.TickCount;

        internal GatherObjectTask(IClassContainer container, Stack<IBotTask> botTasks, WoWGameObject target) : base(container, botTasks, TaskType.Ordinary)
        {
            this.target = target;
            player = ObjectManager.Instance.Player;
            initialCount = Inventory.Instance.GetItemCount(target.Name);
        }

        public void Update()
        {
            if (Container.Player.IsInCombat || (Environment.TickCount - startTime > 15000))
            {
                Console.WriteLine("GatherObjectTask Container.Player.IsInCombat || (Environment.TickCount - startTime > 15000)");
                BotTasks.Pop();
                return;
            }

            if (Wait.For("InteractWithObjectDelay", 15000, true))
            {
                Console.WriteLine("GatherObjectTask Wait.For(\"InteractWithObjectDelay\", 15000, true)");
                Container.HostileTarget.Interact(false);
            }

            if (Inventory.Instance.GetItemCount(target.Name) > initialCount)
            {
                Console.WriteLine("GatherObjectTask Inventory.Instance.GetItemCount(target.Name) > initialCount");
                if (Wait.For("PopGatherObjectStateDelay", 2000))
                {
                    Console.WriteLine("GatherObjectTask Wait.For(\"PopGatherObjectStateDelay\", 2000)");
                    Wait.RemoveAll();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
