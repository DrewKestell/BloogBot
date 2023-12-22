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

        Position currentWaypoint;

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

                List<WoWUnit> unhealthyMembers = ObjectManager.PartyMembers.Where(x => x.HealthPercent < 60).OrderBy(x => x.Health).ToList();

                if (unhealthyMembers.Count > 0)
                {
                    ObjectManager.Player.SetTarget(unhealthyMembers[0].Guid);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }

                if (ObjectManager.Player.Target == null) return;

                if (ObjectManager.Player.InLosWith(ObjectManager.Player.Target.Position))
                {
                    if (ObjectManager.Player.IsSpellReady(healingSpell) && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(healingSpell))
                    {
                        ObjectManager.Player.StopAllMovement();
                        Functions.LuaCall($"CastSpellByName('{healingSpell}')");
                    }
                    else
                    {
                        if (ObjectManager.Player.IsSpellReady(Renew) && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(Renew) && !ObjectManager.Player.Target.HasBuff("Renew"))
                        {
                            Functions.LuaCall($"CastSpellByName('{Renew}')");
                        }
                        BotTasks.Pop();
                        return;
                    }
                }
                else
                {
                    Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

                    if (nextWaypoint.Length > 1)
                    {
                        currentWaypoint = nextWaypoint[1];
                    }
                    else
                    {
                        ObjectManager.Player.StopAllMovement();
                        BotTasks.Pop();
                        return;
                    }

                    ObjectManager.Player.MoveToward(currentWaypoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[HEAL TASK]{e.Message} {e.StackTrace}");
            }
        }
    }
}
