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
            if (ObjectManager.Player.Target.TappedByOther || (ObjectManager.Aggressors.Any() && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.Player.TargetGuid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 27 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(LightningBolt) && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
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

            Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[1]);
        }
    }
}
