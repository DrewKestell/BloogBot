using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace PriestShadow.Tasks
{
    internal class HealTask : BotTask, IBotTask
    {
        private readonly string healingSpell;

        public HealTask(IBotContext botContext) : base(botContext)
        {
            if (ObjectManager.Player.IsSpellReady(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            List<IWoWPlayer> unhealthyMembers = ObjectManager.PartyMembers.Where(x => x.HealthPercent < 70).OrderBy(x => x.Health).ToList();

            if (unhealthyMembers.Count > 0 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(healingSpell))
            {
                ObjectManager.Player.SetTarget(unhealthyMembers[0].Guid);

                if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.Guid != unhealthyMembers[0].Guid)
                    return;
            }
            else
            {
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
                    ObjectManager.Player.CastSpell(Renew);
                if (ObjectManager.Player.IsSpellReady(healingSpell))
                    ObjectManager.Player.CastSpell(healingSpell);
            }
            else
            {
                Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

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
