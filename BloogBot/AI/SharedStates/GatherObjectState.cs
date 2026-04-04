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
        readonly Action onDeadline;
        readonly int initialItemCount = Inventory.GetAllItems().Sum(i => i.StackCount);

        int startTime = -1;

        internal GatherObjectState(
            Stack<IBotState> botStates,
            IDependencyContainer container,
            WoWGameObject target,
            Action onDeadline = null)
        {
            this.botStates = botStates;
            this.target = target;
            this.onDeadline = onDeadline;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (startTime == -1)
            {
                startTime = Environment.TickCount;
            }

            if (player.IsInCombat)
            {
                botStates.Pop();
                return;
            }

            if (Environment.TickCount - startTime > 15 * 1000)
            {
                onDeadline?.Invoke();
                botStates.Pop();
                return;
            }

            if (Wait.For("InteractWithObjectDelay", 10 * 1000, true))
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
