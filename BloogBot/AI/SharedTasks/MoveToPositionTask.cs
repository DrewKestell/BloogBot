using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class MoveToPositionTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly Position destination;
        readonly bool use2DPop;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;
        readonly bool forceMove;

        int stuckCount;
        Position lastCheckedPosition;

        public MoveToPositionTask(IClassContainer container, Stack<IBotTask> botTasks, Position destination, bool use2DPop = false, bool force = false)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.destination = destination;
            this.use2DPop = use2DPop;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks); 
            lastCheckedPosition = ObjectManager.Player.Position;
            forceMove = force;
        }

        public void Update()
        {
            if (player.IsInCombat)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(container.CreateMoveToAttackTargetTask(container, botTasks, ObjectManager.Aggressors.First()));
                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (use2DPop)
            {
                if (player.Position.DistanceTo2D(destination) < 3 || stuckCount > 20)
                {
                    player.StopAllMovement();
                    botTasks.Pop();
                    return;
                }
            }
            else
            {
                if (player.Position.DistanceTo(destination) < 3 || stuckCount > 20)
                {
                    player.StopAllMovement();
                    botTasks.Pop();
                    return;
                }
            }

            Logger.Log("forceMove " + forceMove);
            if (!forceMove && lastCheckedPosition.DistanceTo2D(ObjectManager.Player.Position) > 5)
            {
                Logger.Log("CheckForQuestEntitiesState");
                botTasks.Pop();
                botTasks.Push(new ScanForQuestUnitsTask(container, botTasks));
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, destination, false);
            player.MoveToward(nextWaypoint);

            Logger.Log(ObjectManager.Player.Position);
            lastCheckedPosition = ObjectManager.Player.Position;
        }
    }
}
