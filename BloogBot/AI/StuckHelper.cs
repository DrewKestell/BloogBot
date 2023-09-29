using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public class StuckHelper
    {
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        Position lastPosition;
        int lastTickTime;
        int stuckDuration;

        public StuckHelper(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
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
                botTasks.Push(new StuckTask(container, botTasks));
                return true;
            }

            lastPosition = player.Position;
            lastTickTime = Environment.TickCount;

            return false;
        }
    }
}
