using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace RogueCombat.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public override void PerformCombatRotation()
        {
        }

        public void Update()
        {
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Aggressors.Any(x => x.TargetGuid == ObjectManager.Player.Guid))
            {
                Update(0);
                return;
            }

            if (ObjectManager.CasterAggressors.Any(x => x.TargetGuid == ObjectManager.Player.Guid))
            {
                if (MoveBehindTankSpot(15))
                    return;
            }

            AssignDPSTarget();

            if (!ObjectManager.PartyLeader.IsMoving && ObjectManager.Player.Target != null && ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.PartyLeader.Position) <= 5)
            {
                if (MoveBehindTarget(3))
                    return;
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
                    ObjectManager.Player.StartMeleeAttack();

                    TryUseAbility(AdrenalineRush, 0, ObjectManager.Aggressors.Count() == 3 && ObjectManager.Player.HealthPercent > 80);

                    TryUseAbilityById(BloodFury, 3, 0, ObjectManager.Player.Target.HealthPercent > 80);

                    TryUseAbility(Evasion, 0, ObjectManager.Aggressors.Count() > 1);

                    TryUseAbility(BladeFlurry, 25, ObjectManager.Aggressors.Count() > 1);

                    TryUseAbility(SliceAndDice, 25, !ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.Player.Target.HealthPercent > 70 && ObjectManager.Player.ComboPoints == 2);

                    TryUseAbility(Riposte, 10, ObjectManager.Player.CanRiposte);

                    TryUseAbility(Kick, 25, ReadyToInterrupt(ObjectManager.Player.Target));

                    TryUseAbility(Gouge, 45, ReadyToInterrupt(ObjectManager.Player.Target) && !ObjectManager.Player.IsSpellReady(Kick));

                    bool readyToEviscerate =
                        ObjectManager.Player.Target.HealthPercent <= 15 && ObjectManager.Player.ComboPoints >= 2
                        || ObjectManager.Player.Target.HealthPercent <= 25 && ObjectManager.Player.ComboPoints >= 3
                        || ObjectManager.Player.Target.HealthPercent <= 35 && ObjectManager.Player.ComboPoints >= 4
                        || ObjectManager.Player.ComboPoints == 5;
                    TryUseAbility(Eviscerate, 35, readyToEviscerate);

                    TryUseAbility(SinisterStrike, 45, ObjectManager.Player.ComboPoints < 5);
                }
            }
            else
                ObjectManager.Player.StopAllMovement();
        }

        private bool ReadyToInterrupt(IWoWUnit target) => ObjectManager.Player.Target.Mana > 0 && (target.IsCasting || ObjectManager.Player.Target.IsChanneling);
    }
}
