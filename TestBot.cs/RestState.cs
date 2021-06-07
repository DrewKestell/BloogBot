using BloogBot.AI;
using System.Collections.Generic;

namespace TestBot
{
    class RestState : IBotState
    {
        readonly Stack<IBotState> botStates;

        public RestState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
        }

        public void Update()
        {
            botStates.Pop();
        }
    }
}
