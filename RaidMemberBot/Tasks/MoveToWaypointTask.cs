using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class MoveToWaypointTask : BotTask, IBotTask
    {
        readonly bool use2DPop;
        readonly int startingMapId;

        int stuckCount;

        public MoveToWaypointTask(IClassContainer container, Stack<IBotTask> botTasks, bool use2DPop = false) : base(container, botTasks, TaskType.Ordinary)
        {
            this.use2DPop = use2DPop;
            startingMapId = (int)ObjectManager.Instance.Player.MapId;
        }

        public void Update()
        {
            if (use2DPop)
            {
                if (Container.Player.Location.GetDistanceTo2D(Container.CurrentWaypoint) < 3 || stuckCount > 20)
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
            else
            {
                if (Container.Player.Location.GetDistanceTo(Container.CurrentWaypoint) < 1 || stuckCount > 20 || startingMapId != ObjectManager.Instance.Player.MapId)
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }

            if (Container.CurrentWaypoint.GetDistanceTo(Container.Player.Location) < 1 || !Container.Player.IsMoving)
            {
                Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.CurrentWaypoint, true);

                if (locations.Count(loc => Container.Player.InLosWith(loc)) > 1)
                {
                    Container.CurrentWaypoint = locations.Where(loc => Container.Player.InLosWith(loc)).ToArray()[1];
                    Container.Player.MoveToward(Container.CurrentWaypoint);
                }
                else
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
