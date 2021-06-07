using BloogBot.AI;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace TestBot
{
    class MoveToTargetState : IBotState
    {
        readonly Stack<IBotState> botStates;

        internal MoveToTargetState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target)
        {
            this.botStates = botStates;
        }

        public void Update()
        {
            botStates.Pop();
        }
    }
}
