using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace ArmsWarriorBot
{
    class MoveToTargetTask : IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

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
            if (target.TappedByOther)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            }

            if (player.IsInCombat)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 25 && distanceToTarget > 8 && player.Casting == 0 && Spellbook.Instance.IsSpellReady("Charge") && player.InLosWith(target.Location))
            {
                if (player.Casting == 0)
                {
                        Lua.Instance.Execute($"CastSpellByName('Charge')");
                }
            }

            if (distanceToTarget < 3)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                return;
            }

            var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}
