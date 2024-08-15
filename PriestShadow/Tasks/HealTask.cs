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
            List<IWoWPlayer> unhealthyMembers = [.. ObjectManager.PartyMembers.Where(x => x.HealthPercent < 70).OrderBy(x => x.Health)];

            if (unhealthyMembers.Count > 0 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(healingSpell))
            {
                ObjectManager.Player.SetTarget(unhealthyMembers[0].Guid);

                if (ObjectManager.GetTarget(ObjectManager.Player) == null || ObjectManager.GetTarget(ObjectManager.Player).Guid != unhealthyMembers[0].Guid)
                    return;
            }
            else
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsCasting || ObjectManager.GetTarget(ObjectManager.Player) == null)
                return;

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) < 40 && ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                ObjectManager.Player.StopAllMovement();

                if (!ObjectManager.GetTarget(ObjectManager.Player).HasBuff(Renew))
                    ObjectManager.Player.CastSpell(Renew);
                if (ObjectManager.Player.IsSpellReady(healingSpell))
                    ObjectManager.Player.CastSpell(healingSpell);
            }
            else
            {
                Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);

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
