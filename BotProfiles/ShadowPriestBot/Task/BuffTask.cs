using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string PowerWordFortitude = "Power Word: Fortitude";
        const string ShadowProtection = "Shadow Protection";
        const string LesserHeal = "Lesser Heal";
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (!ObjectManager.Player.IsSpellReady(PowerWordFortitude) || ObjectManager.PartyMembers.All(x => x != null && x.HasBuff(PowerWordFortitude)))
            {
                BotTasks.Pop();
                return;
            }


            WoWUnit woWUnit = ObjectManager.PartyMembers.First(x => !x.HasBuff(PowerWordFortitude));

            if (woWUnit.Position.DistanceTo(ObjectManager.Player.Position) > 15 || !ObjectManager.Player.InLosWith(woWUnit.Position))
            {
                Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, woWUnit.Position, true);

                if (locations.Length > 1)
                {
                    ObjectManager.Player.MoveToward(locations[1]);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                }
            }

            ObjectManager.Player.SetTarget(woWUnit.Guid);
            if (!woWUnit.HasBuff(PowerWordFortitude) && ObjectManager.Player.IsSpellReady(PowerWordFortitude))
                Functions.LuaCall($"CastSpellByName('{PowerWordFortitude}')");
        }
    }
}
