using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace MageFrost.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private const string waitKey = "FrostMagePull";
        private readonly string pullingSpell;
        private readonly int range;

        internal PullTargetTask(IBotContext botContext) : base(botContext)
        {
            if (ObjectManager.Player.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;

            range = 28 + (ObjectManager.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            if (ObjectManager.GetTarget(ObjectManager.Player).TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position);
            if (distanceToTarget <= range && ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For(waitKey, 250))
                {
                    ObjectManager.Player.StopAllMovement();
                    Wait.Remove(waitKey);

                    if (!ObjectManager.Player.IsInCombat)
                        ObjectManager.Player.CastSpell(pullingSpell);

                    BotTasks.Pop();
                    BotTasks.Push(new PvERotationTask(BotContext));
                    return;
                }
            }
            else
            {
                Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
