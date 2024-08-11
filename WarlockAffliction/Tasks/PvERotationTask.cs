using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace WarlockAffliction.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            AssignDPSTarget();

            if (ObjectManager.Player.Target == null) return;

            //if (Container.State.TankInPosition)
            //{
            //    if (MoveTowardsTarget())
            //        return;

            //    PerformCombatRotation();
            //}
            //else if (MoveBehindTankSpot(15))
            //    return;
            //else
            //    ObjectManager.Player.StopAllMovement();
        }
        public override void PerformCombatRotation()
        {
            ObjectManager.Player.StopAllMovement();
            ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
            ObjectManager.Pet?.Attack();

            TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Player.HealthPercent > 85 && ObjectManager.Player.ManaPercent < 80);

            // if target is low on health, turn off wand and cast drain soul
            if (ObjectManager.Player.Target.HealthPercent <= 20)
            {
                ObjectManager.Player.StopWand();
                TryCastSpell(DrainSoul, 0, 29);
            }
            else
            {
                TryCastSpell(CurseOfAgony, 0, 28, !ObjectManager.Player.Target.HasDebuff(CurseOfAgony) && ObjectManager.Player.Target.HealthPercent > 90);

                TryCastSpell(Immolate, 0, 28, !ObjectManager.Player.Target.HasDebuff(Immolate) && ObjectManager.Player.Target.HealthPercent > 30);

                TryCastSpell(Corruption, 0, 28, !ObjectManager.Player.Target.HasDebuff(Corruption) && ObjectManager.Player.Target.HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 28, !ObjectManager.Player.Target.HasDebuff(SiphonLife) && ObjectManager.Player.Target.HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 28, ObjectManager.Player.Target.HealthPercent > 40);
            }
        }
    }
}
