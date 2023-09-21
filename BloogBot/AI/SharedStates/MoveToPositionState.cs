using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class MoveToPositionState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly Position destination;
        readonly bool use2DPop;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;
        readonly bool forceMove;

        int stuckCount;
        Position lastCheckedPosition;

        public MoveToPositionState(Stack<IBotState> botStates, IDependencyContainer container, Position destination, bool use2DPop = false, bool force = false)
        {
            this.botStates = botStates;
            this.container = container;
            this.destination = destination;
            this.use2DPop = use2DPop;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container); 
            lastCheckedPosition = ObjectManager.Player.Position;
            forceMove = force;
        }

        public void Update()
        {
            if (player.IsInCombat)
            {
                player.StopAllMovement();
                botStates.Pop();
                botStates.Push(container.CreateMoveToTargetState(botStates, container, container.FindThreat()));
                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (use2DPop)
            {
                if (player.Position.DistanceTo2D(destination) < 3 || stuckCount > 20)
                {
                    player.StopAllMovement();
                    botStates.Pop();
                    return;
                }
            }
            else
            {
                if (player.Position.DistanceTo(destination) < 3 || stuckCount > 20)
                {
                    player.StopAllMovement();
                    botStates.Pop();
                    return;
                }
            }
            
            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, destination, false);
            player.MoveToward(nextWaypoint);

            lastCheckedPosition = ObjectManager.Player.Position;

            if (!forceMove && lastCheckedPosition.DistanceTo2D(ObjectManager.Player.Position) > 15)
            {
                botStates.Pop();
                botStates.Push(new CheckForQuestEntitiesState(botStates, container));
            }
        }
    }
}
