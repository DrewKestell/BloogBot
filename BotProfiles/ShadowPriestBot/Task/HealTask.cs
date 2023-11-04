using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class HealTask : BotTask, IBotTask
    {
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string Renew = "Renew";

        readonly string healingSpell;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal)
        {
            if (Spellbook.Instance.IsSpellReady(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            List<WoWUnit> unhealthyMembers = ObjectManager.Instance.PartyMembers.Where(x => x.HealthPercent < 60).OrderBy(x => x.Health).ToList();

            if (unhealthyMembers.Count > 0)
            {
                Container.FriendlyTarget = unhealthyMembers[0];
                Container.Player.SetTarget(Container.FriendlyTarget);
            }
            else
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (Container.FriendlyTarget.HealthPercent > 70 || Container.Player.Mana < Container.Player.GetManaCost(healingSpell))
            {
                if (Spellbook.Instance.IsSpellReady(Renew) && Container.Player.Mana > Container.Player.GetManaCost(Renew) && !Container.FriendlyTarget.HasBuff("Renew"))
                    Lua.Instance.Execute($"CastSpellByName('{Renew}')");

                BotTasks.Pop();
                return;
            }

            if (!Container.Player.InLosWith(Container.FriendlyTarget))
            {
                Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);

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
            else
            {
                Container.Player.StopAllMovement();
                Lua.Instance.Execute($"CastSpellByName('{healingSpell}')");
            }
        }
    }
}
