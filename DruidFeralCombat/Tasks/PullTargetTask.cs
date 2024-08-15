using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace DruidFeral.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IBotContext botContext) : base(botContext) { }
        public void Update()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player).TappedByOther || (ObjectManager.Aggressors.Any() && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.GetTarget(ObjectManager.Player).Guid)))
            {
                ObjectManager.Player.StopAllMovement();
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }
            
            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) < 27 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(Wrath) && ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 100))
                {
                    if (!ObjectManager.Player.IsInCombat)
                        ObjectManager.Player.CastSpell(Wrath);

                    if (ObjectManager.Player.IsCasting || ObjectManager.Player.CurrentShapeshiftForm != "Human Form" || ObjectManager.Player.IsInCombat)
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
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
