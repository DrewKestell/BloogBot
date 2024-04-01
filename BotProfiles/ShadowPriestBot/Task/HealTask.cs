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
            List<WoWPlayer> unhealthyMembers = ObjectManager.PartyMembers.Where(x => x.HealthPercent < 70).OrderBy(x => x.Health).ToList();

            if (unhealthyMembers.Count > 0 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(healingSpell))
            {
                ObjectManager.Player.SetTarget(unhealthyMembers[0].Guid);
                ObjectManager.Player.StopAllMovement();
            }
            else
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsCasting || ObjectManager.Player.Target == null) return;

            if (ObjectManager.Player.InLosWith(ObjectManager.Player.Target.Position) && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 20)
            {
                Container.State.Action = $"Healing {ObjectManager.Player.Target.Name}";

                if (!ObjectManager.Player.Target.HasBuff(Renew))
                {
                    Functions.LuaCall($"CastSpellByName('{Renew}')");
                }
                else if (ObjectManager.Player.IsSpellReady(healingSpell))
                {
                    Functions.LuaCall($"CastSpellByName('{healingSpell}')");
                }
            }
            else
            {
                Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

                Container.State.Action = $"Moving to heal {ObjectManager.Player.Target.Name}";

                if (nextWaypoint.Length > 1)
                {
                    ObjectManager.Player.MoveToward(nextWaypoint[1]);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
