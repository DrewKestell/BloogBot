using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    class GatherObjectState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly WoWGameObject target;
        readonly LocalPlayer player;
        readonly int initialCount = 0;

        readonly int startTime = Environment.TickCount;

        internal GatherObjectState(Stack<IBotState> botStates, IDependencyContainer container, WoWGameObject target)
        {
            this.botStates = botStates;
            this.target = target;
            player = ObjectManager.Player;
            initialCount = Inventory.GetItemCount(target.Name);
        }

        public void Update()
        {
            if (player.IsInCombat || (Environment.TickCount - startTime > 15000))
            {
                botStates.Pop();
                return;
            }

            if (Wait.For("InteractWithObjectDelay", 15000, true))
                target.Interact();

            if (Inventory.GetItemCount(target.Name) > initialCount)
            {
                if (Wait.For("PopGatherObjectStateDelay", 2000))
                {
                    Wait.RemoveAll();
                    botStates.Pop();
                    return;
                }
            }
        }
    }
}
