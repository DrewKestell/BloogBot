using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class MoveToCorpseTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        bool walkingOnWater;
        int stuckCount;

        bool initialized;
        
        public MoveToCorpseTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (!initialized)
            {
                initialized = true;
            }

            if (stuckCount == 10)
            {
                DiscordClientWrapper.SendMessage($"{player.Name} is stuck in the MoveToCorpseState. Stopping.");

                while (botTasks.Count > 0)
                    botTasks.Pop();

                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (player.Position.DistanceTo2D(player.CorpsePosition) < 3)
            {
                player.StopAllMovement();
                botTasks.Pop();
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
