using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
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
            player = ObjectManager.Instance.Player;
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

            if (player.Location.GetDistanceTo2D(player.CorpseLocation) < 3)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            }

            var nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, player.CorpseLocation, false);

            if (player.Location.Z - nextWaypoint[0].Z > 5)
                walkingOnWater = true;

            if (walkingOnWater)
            {
                if (player.MovementState != MovementFlags.None)
                    player.StartMovement(ControlBits.Front);

                if (player.Location.Z - nextWaypoint[0].Z < .05)
                {
                    walkingOnWater = false;
                    player.StopMovement(ControlBits.Front);
                }
            }

            else
                player.MoveToward(nextWaypoint[0]);
        }
    }
}
