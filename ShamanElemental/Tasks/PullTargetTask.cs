using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;

namespace ShamanElemental.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private const string LightningBolt = "Lightning Bolt";
        private readonly int stuckCount;
        private Position currentWaypoint;
        private IWoWUnit target;

        internal PullTargetTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (ObjectManager.Aggressors.Any())
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(BotContext));
                return;
            }

            if (ObjectManager.Hostiles.Any())
            {
                IWoWUnit potentialNewTarget = ObjectManager.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid)
                {
                    target = potentialNewTarget;
                    ObjectManager.Player.SetTarget(potentialNewTarget.Guid);
                }
            }

            if (ObjectManager.Player.Position.DistanceTo(target.Position) < 30 && !ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(LightningBolt) && ObjectManager.Player.InLosWith(target))
            {
                ObjectManager.Player.StopAllMovement();

                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(BotContext));
                return;
            }

            Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);

            if (locations.Length > 1)
            {
                ObjectManager.Player.MoveToward(locations[1]);
            }
        }
    }
}
