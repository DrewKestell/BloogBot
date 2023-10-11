using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ElementalShamanBot
{
    class MoveToTargetTask : IBotTask
    {
        const string LightningBolt = "Lightning Bolt";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        int stuckCount;
        Location currentWaypoint;
        WoWUnit target;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            currentWaypoint = player.Location;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (ObjectManager.Instance.Hostiles.Count > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Instance.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != target.Guid)
                {
                    target = potentialNewTarget;
                    player.SetTarget(potentialNewTarget);
                }
            }

            if (player.Location.GetDistanceTo(target.Location) < 30 && !player.IsCasting && Spellbook.Instance.IsSpellReady(LightningBolt) && player.InLosWith(target.Location))
            {
                player.StopAllMovement();

                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, false);
            if (nextWaypoint.Length > 1)
            {
                currentWaypoint = nextWaypoint[1];
            }
            else
            {
                botTasks.Pop();
                return;
            }

            player.MoveToward(currentWaypoint);
        }
    }
}
