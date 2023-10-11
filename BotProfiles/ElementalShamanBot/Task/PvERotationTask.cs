using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElementalShamanBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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
        readonly LocalPlayer player;
        WoWUnit target;
        Location targetLastLocation;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        ~PvERotationTask()
        {

        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                botTasks.Pop();
                return;
            }

            if (target == null || target.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
                ObjectManager.Instance.Player.SetTarget(target);
            }

            if (player.HealthPercent < 30 && player.Mana >= player.GetManaCost(HealingWave))
            {
                botTasks.Push(new HealTask(container, botTasks, target));
                return;
            }

            if (Update(target, 30))
            {
                return;
            }

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Instance.Aggressors.Any(a => a.IsCasting && target.Mana > 0));

            TryCastSpell(EarthShock, 0, 20, !natureImmuneCreatures.Contains(target.Name) && (target.IsCasting || target.IsChanneling || player.HasBuff(Clearcasting)));

            TryCastSpell(LightningBolt, 0, 30, !natureImmuneCreatures.Contains(target.Name) && player.ManaPercent > 30 || (player.HasBuff(FocusedCasting) && target.HealthPercent > 20 && Wait.For("FocusedLightningBoltDelay", 4000, true)));

            TryCastSpell(TremorTotem, 0, int.MaxValue, fearingCreatures.Contains(target.Name) && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(player.Location) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Instance.Aggressors.Count() > 1);

            TryCastSpell(StoneskinTotem, 0, int.MaxValue, target.Mana == 0 && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(player.Location) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneclawTotem) || u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, target.HealthPercent > 70 && !fireImmuneCreatures.Contains(target.Name) && target.Location.GetDistanceTo(player.Location) < 20 && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(player.Location) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(player.Location) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(FlameShock, 0, 20, !target.HasDebuff(FlameShock) && (target.HealthPercent >= 50 || natureImmuneCreatures.Contains(target.Name)) && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(LightningShield, 0, int.MaxValue, !natureImmuneCreatures.Contains(target.Name) && !player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, Spellbook.Instance.IsSpellReady(RockbiterWeapon) && (fireImmuneCreatures.Contains(target.Name) || !player.IsMainhandEnchanted() && !Spellbook.Instance.IsSpellReady(FlametongueWeapon)));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, Spellbook.Instance.IsSpellReady(FlametongueWeapon) && !player.IsMainhandEnchanted() && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(ElementalMastery, 0, int.MaxValue);

            targetLastLocation = target.Location;
        }

        bool TargetMovingTowardPlayer =>
            targetLastLocation != null &&
            targetLastLocation.GetDistanceTo(player.Location) > target.Location.GetDistanceTo(player.Location);

        bool TargetIsFleeing =>
            targetLastLocation != null &&
            targetLastLocation.GetDistanceTo(player.Location) < target.Location.GetDistanceTo(player.Location);
    }
}
