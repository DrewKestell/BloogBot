using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace HunterBeastMastery.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public override void PerformCombatRotation()
        {

        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(28))
                return;

            ObjectManager.Player.StopAllMovement();

            IWoWItem gun = ObjectManager.GetEquippedItem(EquipSlot.Ranged);
            bool canUseRanged = gun != null && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) > 5 && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 34;
            if (gun == null)
            {
                ObjectManager.Player.StartMeleeAttack();
            }
            else if (canUseRanged && ObjectManager.Player.ManaPercent < 60)
            {
                ObjectManager.Player.StartRangedAttack();
            }
            else if (gun != null && canUseRanged)
            {
                //if (!target.HasDebuff(HuntersMark)) 
                //{
                //     TryCastSpell(HuntersMark, 0, 34);
                //}
                //else 
                if (!ObjectManager.Player.Target.HasDebuff(SerpentSting))
                {
                    TryCastSpell(SerpentSting, 0, 34);
                }
                else if (ObjectManager.Player.ManaPercent > 60)
                {
                    TryCastSpell(ArcaneShot, 0, 34);
                }
                return;


                //TryCastSpell(ConcussiveShot, 0, 34);
            }
            else
            {
                // melee rotation
                TryCastSpell(RaptorStrike, 0, 5);
            }
        }
    }
}
