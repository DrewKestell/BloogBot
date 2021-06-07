using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class GrindState : IBotState
    {
        static readonly Random random = new Random();

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        public GrindState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            var enemyTarget = container.FindClosestTarget();

            if (enemyTarget != null)
            {
                player.SetTarget(enemyTarget.Guid);
                botStates.Push(container.CreateMoveToTargetState(botStates, container, enemyTarget));
            }
            else
            {
                var hotspot = container.GetCurrentHotspot();
                var waypointCount = hotspot.Waypoints.Length;
                var waypoint = hotspot.Waypoints[random.Next(0, waypointCount)];
                botStates.Push(new MoveToHotspotWaypointState(botStates, container, waypoint));
            }
        }
    }
}
