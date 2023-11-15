using RaidMemberBot.Game;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    class GatherObjectTask : BotTask, IBotTask
    {
        readonly WoWGameObject target;
        readonly int initialCount = 0;

        readonly int startTime = Environment.TickCount;
        LootFrame lootFrame;
        internal GatherObjectTask(IClassContainer container, Stack<IBotTask> botTasks, WoWGameObject target) : base(container, botTasks, TaskType.Ordinary)
        {
            this.target = target;
        }

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat || (Environment.TickCount - startTime > 15000))
            {
                BotTasks.Pop();
                return;
            }

            if (Wait.For("InteractWithObjectDelay", 15000, true))
            {
                Container.HostileTarget.Interact();
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
