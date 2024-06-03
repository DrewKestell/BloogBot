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

                if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.Guid != unhealthyMembers[0].Guid)
                    return;
            }
            else
            {
                Container.State.Action = $"Exiting healing";
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsCasting || ObjectManager.Player.Target == null)
                return;

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 40 && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                ObjectManager.Player.StopAllMovement();

                if (!ObjectManager.Player.Target.HasBuff(Renew))
                    Functions.LuaCall($"CastSpellByName('{Renew}')");
                if (ObjectManager.Player.IsSpellReady(healingSpell))
                    Functions.LuaCall($"CastSpellByName('{healingSpell}')");
            }
            else
            {
                Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

                if (nextWaypoint.Length > 1)
                {
                    Container.State.Action = $"Moving to heal {ObjectManager.Player.Target.Name}";
                    ObjectManager.Player.MoveToward(nextWaypoint[1]);
                }
                else
                {
                    Container.State.Action = $"Can't move to heal {ObjectManager.Player.Target.Name}";
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
