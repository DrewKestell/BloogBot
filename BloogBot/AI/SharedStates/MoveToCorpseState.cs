using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class MoveToCorpseState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        bool walkingOnWater;
        int stuckCount;

        bool initialized;
        
        public MoveToCorpseState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);
        }

        public void Update()
        {
            if (!initialized)
            {
                container.DisableTeleportChecker = false;
                initialized = true;
            }

            if (stuckCount == 10)
            {
                DiscordClientWrapper.SendMessage($"{player.Name} is stuck in the MoveToCorpseState. Stopping.");

                while (botStates.Count > 0)
                    botStates.Pop();

                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (player.Position.DistanceTo2D(player.CorpsePosition) < 3)
            {
                player.StopAllMovement();
                botStates.Pop();
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, player.CorpsePosition, false);

            if (player.Position.Z - nextWaypoint.Z > 5)
                walkingOnWater = true;

            if (walkingOnWater)
            {
                if (!player.IsMoving)
                    player.StartMovement(ControlBits.Front);

                if (player.Position.Z - nextWaypoint.Z < .05)
                {
                    walkingOnWater = false;
                    player.StopMovement(ControlBits.Front);
                }
            }

            else
                player.MoveToward(nextWaypoint);
        }
    }
}
