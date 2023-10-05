using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class MoveToLocationTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly Location destination;
        readonly bool use2DPop;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        int stuckCount;
        Location lastCheckedLocation;

        public MoveToLocationTask(IClassContainer container, Stack<IBotTask> botTasks, Location destination, bool use2DPop = false)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.destination = destination;
            this.use2DPop = use2DPop;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks); 
            lastCheckedLocation = ObjectManager.Instance.Player.Location;
        }

        public void Update()
        {
            if (player.IsInCombat)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Push(container.CreateOffensiveRotationTask(container, botTasks, ObjectManager.Instance.Units
                    .Where(x => x.Reaction == UnitReaction.Hostile && x.IsInCombat && x.TargetGuid == player.Guid).ToList()));
                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (use2DPop)
            {
                if (player.Location.GetDistanceTo2D(destination) < 3 || stuckCount > 20)
                {
                    player.StopMovement(ControlBits.Nothing);
                    botTasks.Pop();
                    return;
                }
            }
            else
            {
                if (player.Location.GetDistanceTo(destination) < 3 || stuckCount > 20)
                {
                    Console.WriteLine("[MoveToLocationTask] player.Location.GetDistanceTo(destination) < 3 || stuckCount > 20\n");
                    player.StopMovement(ControlBits.Nothing);
                    botTasks.Pop();
                    return;
                }
            }

            var nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, destination, false);

            if (nextWaypoint.Length > 0)
            {
                player.MoveToward(nextWaypoint[0]);
            } else
            {
                botTasks.Pop();
            }
        }
    }
}
