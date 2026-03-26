using System;
using System.Collections.Generic;
using System.Linq;
using BloogBot.Game;
using BloogBot.Game.Objects;

namespace BloogBot.AI.SharedStates
{
    public abstract class MoveToTargetStateBase
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        int stateStartTime = Environment.TickCount;

        public MoveToTargetStateBase(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
        }

        public bool Update()
        {
            // If the target is tapped by others, or we are already in combat with another mob,
            // stop.
            if (target.TappedByOther ||
                (
                    ObjectManager.Aggressors.Count() > 0 &&
                    !ObjectManager.Aggressors.Any(a => a.Guid == target.Guid)
                ))
            {
                player.StopAllMovement();
                botStates.Pop();
                return true;
            }

            // If we've been trying to move to the target for more than 30 seconds, we're probably
            // stuck.
            if (Environment.TickCount - stateStartTime > 30 * 1000)
            {
                // Add the target to the in-memory blacklist and stop trying to fight it.
                container.Probe.BlacklistedMobIds.Add(target.Guid);
                player.StopAllMovement();
                botStates.Pop();
                return true;
            }

            return false;
        }
    }
}