using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ElementalShamanBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string LightningBolt = "Lightning Bolt";

        int stuckCount;
        Location currentWaypoint;
        WoWUnit target;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count > 0)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Instance.Hostiles.Count > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Instance.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != Container.HostileTarget.Guid)
                {
                    target = potentialNewTarget;
                    Container.Player.SetTarget(potentialNewTarget);
                }
            }

            if (Container.Player.Location.GetDistanceTo(target.Location) < 30 && !Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(LightningBolt) && Container.Player.InLosWith(target.Location))
            {
                Container.Player.StopAllMovement();

                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Location[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, ObjectManager.Instance.Player.Location, Container.HostileTarget.Location, true);

            if (locations.Length > 1)
            {
                ObjectManager.Instance.Player.MoveToward(locations[1]);
            }
        }
    }
}
