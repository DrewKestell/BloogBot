using RaidMemberBot.AI;
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
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        int stuckCount;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (target.TappedByOther || (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == target.Guid)))
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            }
            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (stuckCount > 20)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            } 

            if (player.Location.GetDistanceTo(target.Location) < 30 && player.Casting == 0 && Spellbook.Instance.IsSpellReady(LightningBolt) && player.InLosWith(target.Location))
            {
                player.StopMovement(ControlBits.Nothing);

                botTasks.Pop();
                botTasks.Push(new OffensiveRotationTask(container, botTasks, new List<WoWUnit>() { target }));
                return;
            }

            var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}
