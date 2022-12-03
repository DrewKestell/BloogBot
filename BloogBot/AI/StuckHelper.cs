using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public class StuckHelper
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        Position lastPosition;
        int lastTickTime;
        int stuckDuration;

        public StuckHelper(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public bool CheckIfStuck()
        {
            if (lastPosition != null && player.Position.DistanceTo(lastPosition) <= 0.05)
                stuckDuration += Environment.TickCount - lastTickTime;
            if (stuckDuration >= 1000)
            {
                stuckDuration = 0;
                lastPosition = null;
                botStates.Push(new StuckState(botStates, container));
                return true;
            }

            lastPosition = player.Position;
            lastTickTime = Environment.TickCount;

            return false;
        }
    }
}
