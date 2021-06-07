using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ElementalShamanBot
{
    class CombatState : CombatStateBase, IBotState
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

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;
        Position targetLastPosition;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 30)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(HealingWave))
            {
                botStates.Push(new HealSelfState(botStates, container));
                return;
            }

            if (base.Update())
                return;

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Aggressors.Any(a => a.IsCasting && target.Mana > 0));

            TryCastSpell(EarthShock, 0, 20, !natureImmuneCreatures.Contains(target.Name) && (target.IsCasting || target.IsChanneling || player.HasBuff(Clearcasting)));

            TryCastSpell(LightningBolt, 0, 30, !natureImmuneCreatures.Contains(target.Name) && ((TargetMovingTowardPlayer && target.Position.DistanceTo(player.Position) > 15) || (!TargetMovingTowardPlayer && target.Position.DistanceTo(player.Position) > 5) || (player.HasBuff(FocusedCasting) && target.HealthPercent > 20 && Wait.For("FocusedLightningBoltDelay", 4000, true))));
            
            TryCastSpell(TremorTotem, 0, int.MaxValue, fearingCreatures.Contains(target.Name) && !ObjectManager.Units.Any(u => u.Position.DistanceTo(player.Position) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Aggressors.Count() > 1);

            TryCastSpell(StoneskinTotem, 0, int.MaxValue, target.Mana == 0 && !ObjectManager.Units.Any(u => u.Position.DistanceTo(player.Position) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneclawTotem) || u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, target.HealthPercent > 70 && !fireImmuneCreatures.Contains(target.Name) && target.Position.DistanceTo(player.Position) < 20 && !ObjectManager.Units.Any(u => u.Position.DistanceTo(player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Units.Any(u => u.Position.DistanceTo(player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(FlameShock, 0, 20, !target.HasDebuff(FlameShock) && (target.HealthPercent >= 50 || natureImmuneCreatures.Contains(target.Name)) && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(LightningShield, 0, int.MaxValue, !natureImmuneCreatures.Contains(target.Name) && !player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, player.KnowsSpell(RockbiterWeapon) && (fireImmuneCreatures.Contains(target.Name) || !player.MainhandIsEnchanted && !player.KnowsSpell(FlametongueWeapon)));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, player.KnowsSpell(FlametongueWeapon) && !player.MainhandIsEnchanted && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(ElementalMastery, 0, int.MaxValue);

            targetLastPosition = target.Position;
        }

        bool TargetMovingTowardPlayer =>
            targetLastPosition != null &&
            targetLastPosition.DistanceTo(player.Position) > target.Position.DistanceTo(player.Position);

        bool TargetIsFleeing =>
            targetLastPosition != null &&
            targetLastPosition.DistanceTo(player.Position) < target.Position.DistanceTo(player.Position);
    }
}
