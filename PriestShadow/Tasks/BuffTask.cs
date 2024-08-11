using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace PriestShadow.Tasks
{
    public class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                BotTasks.Push(new HealTask(BotContext));
                return;
            }

            if (!ObjectManager.Player.IsSpellReady(PowerWordFortitude) || ObjectManager.PartyMembers.All(x => x.HasBuff(PowerWordFortitude)))
            {
                BotTasks.Pop();
                return;
            }


            IWoWUnit woWUnit = ObjectManager.PartyMembers.First(x => !x.HasBuff(PowerWordFortitude));

            if (woWUnit.Position.DistanceTo(ObjectManager.Player.Position) > 15 || !ObjectManager.Player.InLosWith(woWUnit))
            {
                Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, woWUnit.Position, true);

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
                ObjectManager.Player.CastSpell(PowerWordFortitude);
        }
    }
}
