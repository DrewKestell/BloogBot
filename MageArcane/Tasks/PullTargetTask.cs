using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace MageArcane.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private readonly string pullingSpell;

        internal PullTargetTask(IBotContext botContext) : base(botContext)
        {
            if (ObjectManager.Player.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;
        }

        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);
            if (distanceToTarget < 27)
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(pullingSpell) && Wait.For("ArcaneMagePull", 500))
                {
                    ObjectManager.Player.StopAllMovement();
                    Wait.RemoveAll();
                    ObjectManager.Player.CastSpell(pullingSpell);
                    BotTasks.Pop();
                    BotTasks.Push(new PvERotationTask(BotContext));
                    return;
                }
            }
            else
            {
                Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
