using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace WarriorFury.Tasks
{
    public class PullTargetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsInCombat)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(BotContext));
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);
            if (distanceToTarget < 25 && distanceToTarget > 8 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady("Charge") && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                if (ObjectManager.Player.IsCasting)
                    ObjectManager.Player.CastSpell(Charge);
            }

            if (distanceToTarget < 3)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(BotContext));
                return;
            }

            Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
