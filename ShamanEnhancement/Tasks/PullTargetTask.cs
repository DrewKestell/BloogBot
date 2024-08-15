using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace ShamanEnhancement.Tasks
{
    public class PullTargetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player).TappedByOther || (ObjectManager.Aggressors.Any() && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.GetTarget(ObjectManager.Player).Guid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) < 27 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(LightningBolt) && ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For("PullWithLightningBoltDelay", 100))
                {
                    if (!ObjectManager.Player.IsInCombat)
                        ObjectManager.Player.CastSpell(LightningBolt);

                    if (ObjectManager.Player.IsCasting || ObjectManager.Player.IsInCombat)
                    {
                        ObjectManager.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(BotContext));
                    }
                }
                return;
            }

            Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[1]);
        }
    }
}
