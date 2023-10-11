using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class HealTask : IBotTask
    {
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string Renew = "Renew";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        WoWUnit target;

        readonly string healingSpell;

        public HealTask(Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;

            if (Spellbook.Instance.IsSpellReady(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            List<WoWUnit> unhealthyMembers = ObjectManager.Instance.PartyMembers.Where(x => x.HealthPercent < 60).OrderBy(x => x.Health).ToList();

            if (unhealthyMembers.Count > 0)
            {
                target = unhealthyMembers[0];
                player.SetTarget(target);
            }
            else
            {
                botTasks.Pop();
                return;
            }

            if (target.HealthPercent > 70 || player.Mana < player.GetManaCost(healingSpell))
            {
                if (Spellbook.Instance.IsSpellReady(Renew) && player.Mana > player.GetManaCost(Renew) && !target.HasBuff("Renew"))
                    Lua.Instance.Execute($"CastSpellByName('{Renew}')");

                botTasks.Pop();
                return;
            }

            if (!player.InLosWith(target))
            {
                Location[] waypoints = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);

                if (waypoints.Length > 1)
                {
                    player.MoveToward(waypoints[1]);
                }
            }
            else
            {
                player.StopAllMovement();
                Lua.Instance.Execute($"CastSpellByName('{healingSpell}')");
            }
        }
    }
}
