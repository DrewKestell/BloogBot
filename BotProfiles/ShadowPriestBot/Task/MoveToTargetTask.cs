using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
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
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;
        Location currentWaypoint;
        WoWUnit target;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
            currentWaypoint = player.Location;
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
            if (ObjectManager.Instance.Hostiles.Count > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Instance.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != target.Guid)
                {
                    target = potentialNewTarget;
                    player.SetTarget(potentialNewTarget);
                }
            }

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 27)
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell))
                {
                    if (!Spellbook.Instance.IsSpellReady(PowerWordShield) || player.HasBuff(PowerWordShield) || player.IsInCombat)
                    {
                        if (Wait.For("ShadowPriestPullDelay", 250))
                        {
                            player.SetTarget(target.Guid);
                            Wait.Remove("ShadowPriestPullDelay");

                            if (!player.IsInCombat)
                                Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

                            player.StopAllMovement();
                            botTasks.Pop();
                            botTasks.Push(new PvERotationTask(container, botTasks));
                        }
                    }

                    if (Spellbook.Instance.IsSpellReady(PowerWordShield) && !player.HasDebuff(WeakenedSoul) && !player.HasBuff(PowerWordShield))
                        Lua.Instance.Execute($"CastSpellByName('{PowerWordShield}',1)");

                    return;
                }
            }
            else
            {
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
}
