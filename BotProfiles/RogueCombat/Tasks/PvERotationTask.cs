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

            if (!ObjectManager.PartyLeader.IsMoving && ObjectManager.GetTarget(ObjectManager.Player) != null && ObjectManager.GetTarget(ObjectManager.Player).Position.DistanceTo(ObjectManager.PartyLeader.Position) <= 5)
            {
                if (MoveBehindTarget(3))
                    return;
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    ObjectManager.Player.Face(ObjectManager.GetTarget(ObjectManager.Player).Position);
                    ObjectManager.Player.StartMeleeAttack();

                    TryUseAbility(AdrenalineRush, 0, ObjectManager.Aggressors.Count() == 3 && ObjectManager.Player.HealthPercent > 80);

                    TryUseAbilityById(BloodFury, 3, 0, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 80);

                    TryUseAbility(Evasion, 0, ObjectManager.Aggressors.Count() > 1);

                    TryUseAbility(BladeFlurry, 25, ObjectManager.Aggressors.Count() > 1);

                    TryUseAbility(SliceAndDice, 25, !ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 70 && ObjectManager.Player.ComboPoints == 2);

                    TryUseAbility(Riposte, 10, ObjectManager.Player.CanRiposte);

                    TryUseAbility(Kick, 25, ReadyToInterrupt(ObjectManager.GetTarget(ObjectManager.Player)));

                    TryUseAbility(Gouge, 45, ReadyToInterrupt(ObjectManager.GetTarget(ObjectManager.Player)) && !ObjectManager.Player.IsSpellReady(Kick));

                    bool readyToEviscerate =
                        ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 15 && ObjectManager.Player.ComboPoints >= 2
                        || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 25 && ObjectManager.Player.ComboPoints >= 3
                        || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 35 && ObjectManager.Player.ComboPoints >= 4
                        || ObjectManager.Player.ComboPoints == 5;
                    TryUseAbility(Eviscerate, 35, readyToEviscerate);

                    TryUseAbility(SinisterStrike, 45, ObjectManager.Player.ComboPoints < 5);
                }
            }
            else
                ObjectManager.Player.StopAllMovement();
        }

        private bool ReadyToInterrupt(IWoWUnit target) => ObjectManager.GetTarget(ObjectManager.Player).Mana > 0 && (target.IsCasting || ObjectManager.GetTarget(ObjectManager.Player).IsChanneling);
    }
}
