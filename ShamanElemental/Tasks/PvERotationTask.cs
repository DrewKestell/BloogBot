using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace ShamanElemental.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private const string Clearcasting = "Clearcasting";
        private const string EarthShock = "Earth Shock";
        private const string ElementalMastery = "Elemental Mastery";
        private const string FlameShock = "Flame Shock";
        private const string FlametongueWeapon = "Flametongue Weapon";
        private const string FocusedCasting = "Focused Casting";
        private const string GroundingTotem = "Grounding Totem";
        private const string ManaSpringTotem = "Mana Spring Totem";
        private const string HealingWave = "Healing Wave";
        private const string LightningBolt = "Lightning Bolt";
        private const string LightningShield = "Lightning Shield";
        private const string RockbiterWeapon = "Rockbiter Weapon";
        private const string SearingTotem = "Searing Totem";
        private const string StoneclawTotem = "Stoneclaw Totem";
        private const string StoneskinTotem = "Stoneskin Totem";
        private const string TremorTotem = "Tremor Totem";
        private readonly string[] fearingCreatures = ["Scorpid Terror"];
        private readonly string[] fireImmuneCreatures = ["Rogue Flame Spirit", "Burning Destroyer"];
        private readonly string[] natureImmuneCreatures = ["Swirling Vortex", "Gusting Vortex", "Dust Stormer"];

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        ~PvERotationTask()
        {

        }

        public override void PerformCombatRotation()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
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
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (Update(12))
            {
                return;
            }

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Aggressors.Any(a => a.IsCasting && ObjectManager.Player.Target.Mana > 0));

            TryCastSpell(EarthShock, 0, 20, !natureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && (ObjectManager.Player.Target.IsCasting || ObjectManager.Player.Target.IsChanneling || ObjectManager.Player.HasBuff(Clearcasting)));

            TryCastSpell(LightningBolt, 0, 30, !natureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && ObjectManager.Player.ManaPercent > 30 || (ObjectManager.Player.HasBuff(FocusedCasting) && ObjectManager.Player.Target.HealthPercent > 20 && Wait.For("FocusedLightningBoltDelay", 4000, true)));

            TryCastSpell(TremorTotem, 0, int.MaxValue, fearingCreatures.Contains(ObjectManager.Player.Target.Name) && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            //TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Aggressors.Count() > 1);

            //TryCastSpell(StoneskinTotem, 0, int.MaxValue, ObjectManager.Player.Target.Mana == 0 && !ObjectManager.Units.Any(u => u.Position.GetDistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, ObjectManager.Player.Target.HealthPercent > 70 && !fireImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.Player.Position) < 20 && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(FlameShock, 0, 20, !ObjectManager.Player.Target.HasDebuff(FlameShock) && (ObjectManager.Player.Target.HealthPercent >= 50 || natureImmuneCreatures.Contains(ObjectManager.Player.Target.Name)) && !fireImmuneCreatures.Contains(ObjectManager.Player.Target.Name));

            TryCastSpell(LightningShield, 0, int.MaxValue, !natureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && !ObjectManager.Player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, ObjectManager.Player.IsSpellReady(RockbiterWeapon) && (fireImmuneCreatures.Contains(ObjectManager.Player.Target.Name) || !ObjectManager.Player.MainhandIsEnchanted && !ObjectManager.Player.IsSpellReady(FlametongueWeapon)));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, ObjectManager.Player.IsSpellReady(FlametongueWeapon) && !ObjectManager.Player.MainhandIsEnchanted && !fireImmuneCreatures.Contains(ObjectManager.Player.Target.Name));

            TryCastSpell(ElementalMastery, 0, int.MaxValue);
        }
    }
}
