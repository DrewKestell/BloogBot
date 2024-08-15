using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace PriestShadow.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private readonly string pullingSpell;
        private Position currentWaypoint;
        internal PullTargetTask(IBotContext botContext) : base(botContext)
        {
            if (ObjectManager.Player.HasBuff(ShadowForm))
                pullingSpell = MindBlast;
            else if (ObjectManager.Player.IsSpellReady(HolyFire))
                pullingSpell = HolyFire;
            else
                pullingSpell = Smite;
        }

        public void Update()
        {
            if (ObjectManager.Hostiles.Any())
            {
                IWoWUnit potentialNewTarget = ObjectManager.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid)
                {
                    ObjectManager.Player.SetTarget(potentialNewTarget.Guid);
                }
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position);
            if (distanceToTarget < 27)
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(pullingSpell))
                {
                    if (!ObjectManager.Player.IsSpellReady(PowerWordShield) || ObjectManager.Player.HasBuff(PowerWordShield) || ObjectManager.Player.IsInCombat)
                    {
                        if (Wait.For("ShadowPriestPullDelay", 250))
                        {
                            ObjectManager.Player.SetTarget(ObjectManager.GetTarget(ObjectManager.Player).Guid);
                            Wait.Remove("ShadowPriestPullDelay");

                            if (!ObjectManager.Player.IsInCombat)
                                ObjectManager.Player.CastSpell(pullingSpell);

                            ObjectManager.Player.StopAllMovement();
                            BotTasks.Pop();
                            BotTasks.Push(new PvERotationTask(BotContext));
                        }
                    }

                    if (ObjectManager.Player.IsSpellReady(PowerWordShield) && !ObjectManager.Player.HasDebuff(WeakenedSoul) && !ObjectManager.Player.HasBuff(PowerWordShield))
                        ObjectManager.Player.CastSpell(PowerWordShield, castOnSelf: true);

                    return;
                }
            }
            else
            {
                Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);
                if (nextWaypoint.Length > 1)
                {
                    currentWaypoint = nextWaypoint[1];
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }

                ObjectManager.Player.MoveToward(currentWaypoint);
            }
        }
    }
}
