using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
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
            if (ObjectManager.Player.IsSpellReady(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            try
            {
                if (ObjectManager.Player.IsCasting) return;

                List<WoWPlayer> unhealthyMembers = ObjectManager.PartyMembers.Where(x => x.HealthPercent < 60).OrderBy(x => x.Health).ToList();

                if (unhealthyMembers.Count > 0)
                {
                    Container.FriendlyTarget = unhealthyMembers[0];
                    ObjectManager.Player.SetTarget(Container.FriendlyTarget.Guid);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }

                if (Container.FriendlyTarget.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(healingSpell))
                {
                    if (ObjectManager.Player.IsSpellReady(Renew) && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(Renew) && !Container.FriendlyTarget.HasBuff("Renew"))
                        Functions.LuaCall($"CastSpellByName('{Renew}')");

                    BotTasks.Pop();
                    return;
                }

                if (!ObjectManager.Player.InLosWith(Container.FriendlyTarget.Position))
                {
                    Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.FriendlyTarget.Position, true);

                    if (nextWaypoint.Length > 1)
                    {
                        Container.CurrentWaypoint = nextWaypoint[1];
                    }
                    else
                    {
                        ObjectManager.Player.StopAllMovement();
                        BotTasks.Pop();
                        return;
                    }

                    ObjectManager.Player.MoveToward(Container.CurrentWaypoint);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    Functions.LuaCall($"CastSpellByName('{healingSpell}')");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[HEAL TASK]{e.Message} {e.StackTrace}");
            }
        }
    }
}
