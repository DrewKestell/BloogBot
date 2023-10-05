using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace ShadowPriestBot
{
    class MoveToTargetTask : IBotTask
    {
        const string HolyFire = "Holy Fire";
        const string MindBlast = "Mind Blast";
        const string PowerWordShield = "Power Word: Shield";
        const string ShadowForm = "Shadowform";
        const string Smite = "Smite";
        const string WeakenedSoul = "WeakenedSoul";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (player.HasBuff(ShadowForm))
                pullingSpell = MindBlast;
            else if (Spellbook.Instance.IsSpellReady(HolyFire))
                pullingSpell = HolyFire;
            else
                pullingSpell = Smite;
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            }

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 27)
            {
                if (player.IsMoving)
                    player.StopMovement(ControlBits.Nothing);

                if (player.Casting == 0 && Spellbook.Instance.IsSpellReady(pullingSpell))
                {
                    if (!Spellbook.Instance.IsSpellReady(PowerWordShield) || player.HasBuff(PowerWordShield) || player.IsInCombat)
                    {
                        if (Wait.For("ShadowPriestPullDelay", 250))
                        {
                            player.SetTarget(target.Guid);
                            Wait.Remove("ShadowPriestPullDelay");

                            if (!player.IsInCombat)
                                Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

                            player.StopMovement(ControlBits.Nothing);
                            botTasks.Pop();
                            botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                        }
                    }

                    if (Spellbook.Instance.IsSpellReady(PowerWordShield) && !player.HasDebuff(WeakenedSoul) && !player.HasBuff(PowerWordShield))
                        Lua.Instance.Execute($"CastSpellByName('{PowerWordShield}',1)");

                    return;
                }
            }
            else
            {
                stuckHelper.CheckIfStuck();

                var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
                player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
