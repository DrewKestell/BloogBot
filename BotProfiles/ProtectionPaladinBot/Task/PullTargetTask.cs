using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionPaladinBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == Container.HostileTarget.Guid))
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (Container.Player.Location.GetDistanceTo(Container.Player.Location) < 3 || Container.Player.IsInCombat)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
