using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    class GatherObjectState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly WoWGameObject target;
        readonly LocalPlayer player;
        readonly int initialItemCount = Inventory.GetAllItems().Sum(i => i.StackCount);

        readonly int startTime = Environment.TickCount;

        internal GatherObjectState(Stack<IBotState> botStates, IDependencyContainer container, WoWGameObject target)
        {
            this.botStates = botStates;
            this.target = target;
            player = ObjectManager.Player;
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

            if (Inventory.GetAllItems().Sum(i => i.StackCount) != initialItemCount)
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
