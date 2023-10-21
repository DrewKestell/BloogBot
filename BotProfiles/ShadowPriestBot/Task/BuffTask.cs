using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
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
            if (ObjectManager.Instance.PartyMembers.Any(x => x.HealthPercent < 70) && Container.Player.Mana >= Container.Player.GetManaCost(LesserHeal))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (!Spellbook.Instance.IsSpellReady(PowerWordFortitude) || ObjectManager.Instance.PartyMembers.All(x => x != null && x.GotAura(PowerWordFortitude)))
            {
                BotTasks.Pop();
                return;
            }


            WoWUnit woWUnit = ObjectManager.Instance.PartyMembers.First(x => !x.GotAura(PowerWordFortitude));

            if (woWUnit.DistanceToPlayer > 15 || !Container.Player.InLosWith(woWUnit))
            {
                Location[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, ObjectManager.Instance.Player.Location, woWUnit.Location, true);

                if (locations.Length > 1)
                {
                    ObjectManager.Instance.Player.MoveToward(locations[1]);
                }
                else
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                }
            }

            Container.Player.SetTarget(woWUnit);
            if (!woWUnit.GotAura(PowerWordFortitude) && Spellbook.Instance.IsSpellReady(PowerWordFortitude))
                Lua.Instance.Execute($"CastSpellByName('{PowerWordFortitude}')");
        }
    }
}
