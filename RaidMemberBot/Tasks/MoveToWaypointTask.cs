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
            startingMapId = (int)ObjectManager.MapId;
        }

        public void Update()
        {
            if (use2DPop)
            {
                if (ObjectManager.Player.Position.DistanceTo2D(Container.CurrentWaypoint) < 3 || stuckCount > 20)
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
            else
            {
                if (ObjectManager.Player.Position.DistanceTo(Container.CurrentWaypoint) < 1 || stuckCount > 20 || startingMapId != ObjectManager.MapId)
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }

            if (Container.CurrentWaypoint.DistanceTo(ObjectManager.Player.Position) < 1 || !ObjectManager.Player.IsMoving)
            {
                Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.CurrentWaypoint, true);

                if (locations.Count(loc => ObjectManager.Player.InLosWith(loc)) > 1)
                {
                    Container.CurrentWaypoint = locations.Where(loc => ObjectManager.Player.InLosWith(loc)).ToArray()[1];
                    ObjectManager.Player.MoveToward(Container.CurrentWaypoint);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
