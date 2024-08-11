using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace ShamanElemental.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        ~PvERotationTask()
        {

        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (ObjectManager.Player.HealthPercent < 30 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HealingWave))
            {
                BotTasks.Push(new HealTask(BotContext));
                return;
            }

            if (Update(12))
            {
                return;
            }

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Aggressors.Any(a => a.IsCasting && ObjectManager.Player.Target.Mana > 0));

            TryCastSpell(EarthShock, 0, 20, !NatureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && (ObjectManager.Player.Target.IsCasting || ObjectManager.Player.Target.IsChanneling || ObjectManager.Player.HasBuff(Clearcasting)));

            TryCastSpell(LightningBolt, 0, 30, !NatureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && ObjectManager.Player.ManaPercent > 30 || (ObjectManager.Player.HasBuff(FocusedCasting) && ObjectManager.Player.Target.HealthPercent > 20 && Wait.For("FocusedLightningBoltDelay", 4000, true)));

            TryCastSpell(TremorTotem, 0, int.MaxValue, FearingCreatures.Contains(ObjectManager.Player.Target.Name) && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            //TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Aggressors.Count() > 1);

            //TryCastSpell(StoneskinTotem, 0, int.MaxValue, ObjectManager.Player.Target.Mana == 0 && !ObjectManager.Units.Any(u => u.Position.GetDistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, ObjectManager.Player.Target.HealthPercent > 70 && !FireImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.Player.Position) < 20 && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(FlameShock, 0, 20, !ObjectManager.Player.Target.HasDebuff(FlameShock) && (ObjectManager.Player.Target.HealthPercent >= 50 || NatureImmuneCreatures.Contains(ObjectManager.Player.Target.Name)) && !FireImmuneCreatures.Contains(ObjectManager.Player.Target.Name));

            TryCastSpell(LightningShield, 0, int.MaxValue, !NatureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && !ObjectManager.Player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, ObjectManager.Player.IsSpellReady(RockbiterWeapon) && (FireImmuneCreatures.Contains(ObjectManager.Player.Target.Name) || !ObjectManager.Player.MainhandIsEnchanted && !ObjectManager.Player.IsSpellReady(FlametongueWeapon)));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, ObjectManager.Player.IsSpellReady(FlametongueWeapon) && !ObjectManager.Player.MainhandIsEnchanted && !FireImmuneCreatures.Contains(ObjectManager.Player.Target.Name));

            TryCastSpell(ElementalMastery, 0, int.MaxValue);
        }
    }
}
