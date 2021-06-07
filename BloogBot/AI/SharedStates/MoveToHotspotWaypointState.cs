using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class MoveToHotspotWaypointState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly Position destination;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        public MoveToHotspotWaypointState(Stack<IBotState> botStates, IDependencyContainer container, Position destination)
        {
            this.botStates = botStates;
            this.container = container;
            this.destination = destination;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);
        }

        public void Update()
        {
            stuckHelper.CheckIfStuck();
            
            if (container.FindClosestTarget() != null || player.Position.DistanceTo(destination) < 3)
            {
                player.StopAllMovement();
                botStates.Pop();
                return;
            }
            
            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, destination, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
