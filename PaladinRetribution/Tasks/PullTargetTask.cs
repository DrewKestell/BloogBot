using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace PaladinRetribution.Tasks
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther || (ObjectManager.Aggressors.Count() > 0 && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.Player.TargetGuid)))
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 3 || ObjectManager.Player.IsInCombat)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
