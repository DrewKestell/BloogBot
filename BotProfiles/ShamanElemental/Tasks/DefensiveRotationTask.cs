namespace ElementalShamanBot
{
    class DefensiveRotationTask : CombatRotationTask, IBotTask
    {
        const string Clearcasting = "Clearcasting";
        const string EarthShock = "Earth Shock";
        const string ElementalMastery = "Elemental Mastery";
        const string FlameShock = "Flame Shock";
        const string FlametongueWeapon = "Flametongue Weapon";
        const string FocusedCasting = "Focused Casting";
        const string GroundingTotem = "Grounding Totem";
        const string ManaSpringTotem = "Mana Spring Totem";
        const string HealingWave = "Healing Wave";
        const string LightningBolt = "Lightning Bolt";
        const string LightningShield = "Lightning Shield";
        const string RockbiterWeapon = "Rockbiter Weapon";
        const string SearingTotem = "Searing Totem";
        const string StoneclawTotem = "Stoneclaw Totem";
        const string StoneskinTotem = "Stoneskin Totem";
        const string TremorTotem = "Tremor Totem";

        readonly string[] fearingCreatures = new[] { "Scorpid Terror" };
        readonly string[] fireImmuneCreatures = new[] { "Rogue Flame Spirit", "Burning Destroyer" };
        readonly string[] natureImmuneCreatures = new[] { "Swirling Vortex", "Gusting Vortex", "Dust Stormer" };

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        Location targetLastLocation;

        internal DefensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 30)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = targets[0];
        }

        ~DefensiveRotationTask()
        {

        }

        public new void Update()
        {
            Console.WriteLine($"{ObjectManager.Instance.Player.GetManaCost(HealingWave)}");
            if (ObjectManager.Instance.PartyMembers.Any(x => x.HealthPercent < 30) && ObjectManager.Instance.Player.Mana >= ObjectManager.Instance.Player.GetManaCost(HealingWave))
            {
                botTasks.Push(new HealTask(container, botTasks, ObjectManager.Instance.PartyMembers.First(x => x.HealthPercent < 30)));
                return;
            }

            if (base.Update())
            {
                return;
            }

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Instance.Aggressors.Any(a => a.IsCasting && target.Mana > 0));

            TryCastSpell(EarthShock, 0, 20, !natureImmuneCreatures.Contains(target.Name) && (target.IsCasting || target.IsChanneling || ObjectManager.Instance.Player.HasBuff(Clearcasting)));

            TryCastSpell(LightningBolt, 0, 30, !natureImmuneCreatures.Contains(target.Name) && ObjectManager.Instance.Player.ManaPercent > 30 || (ObjectManager.Instance.Player.HasBuff(FocusedCasting) && target.HealthPercent > 20 && Wait.For("FocusedLightningBoltDelay", 4000, true)));
            
            TryCastSpell(TremorTotem, 0, int.MaxValue, fearingCreatures.Contains(target.Name) && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Instance.Aggressors.Count() > 1);

            TryCastSpell(StoneskinTotem, 0, int.MaxValue, target.Mana == 0 && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneskinTotem) || u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, target.HealthPercent > 70 && !fireImmuneCreatures.Contains(target.Name) && target.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) < 20 && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(ObjectManager.Instance.Player.Location) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(FlameShock, 0, 20, !target.HasDebuff(FlameShock) && (target.HealthPercent >= 50 || natureImmuneCreatures.Contains(target.Name)) && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(LightningShield, 0, int.MaxValue, !natureImmuneCreatures.Contains(target.Name) && !ObjectManager.Instance.Player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, Spellbook.Instance.IsSpellReady(RockbiterWeapon) && (fireImmuneCreatures.Contains(target.Name) || !ObjectManager.Instance.Player.IsMainhandEnchanted() && !Spellbook.Instance.IsSpellReady(FlametongueWeapon)));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, Spellbook.Instance.IsSpellReady(FlametongueWeapon) && !ObjectManager.Instance.Player.IsMainhandEnchanted() && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(ElementalMastery, 0, int.MaxValue);

            targetLastLocation = target.Location;
        }

        bool TargetMovingTowardPlayer =>
            targetLastLocation != null &&
            targetLastLocation.GetDistanceTo(ObjectManager.Instance.Player.Location) > target.Location.GetDistanceTo(ObjectManager.Instance.Player.Location);

        bool TargetIsFleeing =>
            targetLastLocation != null &&
            targetLastLocation.GetDistanceTo(ObjectManager.Instance.Player.Location) < target.Location.GetDistanceTo(ObjectManager.Instance.Player.Location);
    }
}
