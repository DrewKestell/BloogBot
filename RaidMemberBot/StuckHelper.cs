using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public class StuckHelper
    {
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        Location lastLocation;
        int lastTickTime;
        int stuckDuration;

        public StuckHelper(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public bool CheckIfStuck()
        {
            if (lastLocation != null && player.Location.GetDistanceTo(lastLocation) <= 0.05)
                stuckDuration += Environment.TickCount - lastTickTime;
            if (stuckDuration >= 1000)
            {
                stuckDuration = 0;
                lastLocation = null;
                botTasks.Push(new StuckTask(container, botTasks));
                return true;
            }

            lastLocation = player.Location;
            lastTickTime = Environment.TickCount;

            return false;
        }
    }
}
