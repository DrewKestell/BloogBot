using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace ShamanElemental.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private const string LightningBolt = "Lightning Bolt";
        private readonly int stuckCount;
        private Position currentWaypoint;
        private WoWUnit target;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count > 0)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Hostiles.Count() > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != ObjectManager.Player.TargetGuid)
                {
                    target = potentialNewTarget;
                    ObjectManager.Player.SetTarget(potentialNewTarget.Guid);
                }
            }

            if (ObjectManager.Player.Position.DistanceTo(target.Position) < 30 && !ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(LightningBolt) && ObjectManager.Player.InLosWith(target))
            {
                ObjectManager.Player.StopAllMovement();

                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

            if (locations.Length > 1)
            {
                ObjectManager.Player.MoveToward(locations[1]);
            }
        }
    }
}
