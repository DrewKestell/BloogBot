using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace RogueCombat.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {

        internal PullTargetTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player).TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position);
            if (distanceToTarget < 25 && !ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Garrote) && !ObjectManager.Player.IsInCombat)
            {
                ObjectManager.Player.CastSpell(Stealth);
            }

            if (distanceToTarget < 15 && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Totem)
            {
                //var delta = ObjectManager.GetTarget(ObjectManager.Player).Position - ObjectManager.Player.Position;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetPosition = ObjectManager.GetTarget(ObjectManager.Player).Position + scaledVector;

                //ObjectManager.Player.CastSpellAtPosition(Distract, targetPosition);
            }

            if (distanceToTarget < 3.5 && ObjectManager.Player.HasBuff(Stealth) && !ObjectManager.Player.IsInCombat && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Totem)
            {
                if (ObjectManager.Player.IsSpellReady(Garrote) && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Elemental && ObjectManager.Player.IsBehind(ObjectManager.GetTarget(ObjectManager.Player)))
                {
                    ObjectManager.Player.CastSpell(Garrote);
                    return;
                }
                else if (ObjectManager.Player.IsSpellReady(CheapShot) && ObjectManager.Player.IsBehind(ObjectManager.GetTarget(ObjectManager.Player)))
                {
                    ObjectManager.Player.CastSpell(CheapShot);
                    return;
                }
            } 

            if (distanceToTarget < 3)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(BotContext));
                return;
            }

            Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
