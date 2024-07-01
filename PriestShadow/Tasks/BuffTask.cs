using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace PriestShadow.Tasks
{
    internal class BuffTask : BotTask, IBotTask
    {
        private const string PowerWordFortitude = "Power Word: Fortitude";
        private const string ShadowProtection = "Shadow Protection";
        private const string LesserHeal = "Lesser Heal";
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (!ObjectManager.Player.IsSpellReady(PowerWordFortitude) || ObjectManager.PartyMembers.All(x => x.HasBuff(PowerWordFortitude)))
            {
                BotTasks.Pop();
                return;
            }


            WoWUnit woWUnit = ObjectManager.PartyMembers.First(x => !x.HasBuff(PowerWordFortitude));

            if (woWUnit.Position.DistanceTo(ObjectManager.Player.Position) > 15 || !ObjectManager.Player.InLosWith(woWUnit))
            {
                Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, woWUnit.Position, true);

                if (locations.Length > 1)
                {
                    ObjectManager.Player.MoveToward(locations[1]);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }

            ObjectManager.Player.SetTarget(woWUnit.Guid);
            if (!woWUnit.HasBuff(PowerWordFortitude) && ObjectManager.Player.IsSpellReady(PowerWordFortitude))
                Functions.LuaCall($"CastSpellByName('{PowerWordFortitude}')");
        }
    }
}
