using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string HolyFire = "Holy Fire";
        const string MindBlast = "Mind Blast";
        const string PowerWordShield = "Power Word: Shield";
        const string ShadowForm = "Shadowform";
        const string Smite = "Smite";
        const string WeakenedSoul = "WeakenedSoul";

        readonly string pullingSpell;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (Container.Player.HasBuff(ShadowForm))
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

                if (potentialNewTarget != null && potentialNewTarget.Guid != Container.HostileTarget.Guid)
                {
                    Container.HostileTarget = potentialNewTarget;
                    Container.Player.SetTarget(potentialNewTarget);
                }
            }

            var distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);
            if (distanceToTarget < 27)
            {
                if (Container.Player.IsMoving)
                    Container.Player.StopAllMovement();

                if (Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell))
                {
                    if (!Spellbook.Instance.IsSpellReady(PowerWordShield) || Container.Player.HasBuff(PowerWordShield) || Container.Player.IsInCombat)
                    {
                        if (Wait.For("ShadowPriestPullDelay", 250))
                        {
                            Container.Player.SetTarget(Container.HostileTarget.Guid);
                            Wait.Remove("ShadowPriestPullDelay");

                            if (!Container.Player.IsInCombat)
                                Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

                            Container.Player.StopAllMovement();
                            BotTasks.Pop();
                            BotTasks.Push(new PvERotationTask(Container, BotTasks));
                        }
                    }

                    if (Spellbook.Instance.IsSpellReady(PowerWordShield) && !Container.Player.HasDebuff(WeakenedSoul) && !Container.Player.HasBuff(PowerWordShield))
                        Lua.Instance.Execute($"CastSpellByName('{PowerWordShield}',1)");

                    return;
                }
            }
            else
            {
                var nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
                if (nextWaypoint.Length > 1)
                {
                    Container.CurrentWaypoint = nextWaypoint[1];
                }
                else
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }

                Container.Player.MoveToward(Container.CurrentWaypoint);
            }
        }
    }
}
