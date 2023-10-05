using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace BalanceDruidBot
{
    class MoveToTargetTask : IBotTask
    {
        const string Wrath = "Wrath";
        const string Starfire = "Starfire";
        const string MoonkinForm = "Moonkin Form";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly int range;
        readonly string pullingSpell;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (player.Level <= 19)
                range = 28;
            else if (player.Level == 20)
                range = 31;
            else
                range = 34;

            if (Spellbook.Instance.IsSpellReady(Starfire))
                pullingSpell = Starfire;
            else
                pullingSpell = Wrath;
        }

        public void Update()
        {
            if (target.TappedByOther || (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == target.Guid)))
            {
                Wait.RemoveAll();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            if (player.IsCasting)
                return;

            if (Spellbook.Instance.IsSpellReady(MoonkinForm) && !player.HasBuff(MoonkinForm))
            {
                Lua.Instance.Execute($"CastSpellByName('{MoonkinForm}')");
            }

            if (player.Location.GetDistanceTo(target.Location) < range && player.Casting == 0 && Spellbook.Instance.IsSpellReady(pullingSpell) && player.InLosWith(target.Location))
            {
                if (player.IsMoving)
                    player.StopMovement(ControlBits.Nothing);

                if (Wait.For("BalanceDruidPullDelay", 100))
                {
                    if (!player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

                    if (player.IsCasting || player.IsInCombat)
                    {
                        player.StopMovement(ControlBits.Nothing);
                        Wait.RemoveAll();
                        botTasks.Pop();
                        botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                    }
                }
                return;
            }

            var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}
