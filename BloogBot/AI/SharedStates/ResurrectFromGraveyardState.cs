using System.Collections.Generic;
using System.Linq;
using BloogBot.Game;
using BloogBot.Game.Objects;

namespace BloogBot.AI.SharedStates
{
    public class ResurrectFromGraveyardState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        State state = State.Uninitialized;
        WoWUnit spiritHealer;

        public ResurrectFromGraveyardState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            // At any point, if we're already alive, just leave the state.
            if (!player.InGhostForm)
            {
                Wait.RemoveAll();
                botStates.Pop();
                return;
            }

            if (state == State.Uninitialized)
            {
                spiritHealer = ObjectManager.Units.First(u => u.Name == "Spirit Healer");

                // Move to the spirit healer.
                botStates.Push(new MoveToPositionState(botStates, container, spiritHealer.Position));
                state = State.Interacting;
            }
            else if (state == State.Interacting)
            {
                // Interact and click resurrect.
                spiritHealer.Interact();
                if (Wait.For("InteactDelay", 1000))
                {
                    player.LuaCall("AcceptXPLoss()");
                    state = State.Resurrecting;
                }
            }
            else if (state == State.Resurrecting)
            {
                if (Wait.For("LeaveDelay", 2000))
                {
                    if (player.InGhostForm)
                    {
                        // Somehow we didn't successfully resurrect. Just try again.
                        state = State.Uninitialized;
                    }
                    else
                    {
                        botStates.Pop();
                    }
                }
            }
        }

        enum State
        {
            Uninitialized,
            Interacting,
            Resurrecting,
        }
    }
}