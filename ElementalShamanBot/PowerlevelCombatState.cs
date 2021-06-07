using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElementalShamanBot
{
    class PowerlevelCombatState : IBotState
    {

        const string LosErrorMessage = "Target not in line of sight";
        const string AutoAttackLuaScript = "if IsCurrentAction('12') == nil then CastSpellByName('Attack') end";

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
        const string LesserHealingWave = "Lesser Healing Wave";

        readonly string[] fearingCreatures = new[] { "Scorpid Terror" };
        readonly string[] fireImmuneCreatures = new[] { "Rogue Flame Spirit", "Burning Destroyer" };
        readonly string[] natureImmuneCreatures = new[] { "Swirling Vortex", "Gusting Vortex", "Dust Stormer" };

        
        Position targetLastPosition;

        bool noLos;
        int noLosStartTime;

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly WoWPlayer powerlevelTarget;
        readonly LocalPlayer player;

        public PowerlevelCombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target, WoWPlayer powerlevelTarget)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            this.powerlevelTarget = powerlevelTarget;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (Environment.TickCount - noLosStartTime > 1000)
            {
                player.StopAllMovement();
                noLos = false;
            }

            if (noLos)
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
                return;
            }


            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(HealingWave))
            {
                botStates.Push(new HealSelfState(botStates, container));
                return;
            }

            // pop state when the target is dead
            if (target.Health == 0)
            {
                botStates.Pop();

                if (player.ManaPercent < 20)
                    botStates.Push(new RestState(botStates, container));

                return;
            }

            if (player.TargetGuid != player.Guid & !player.IsCasting)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Position)) player.Face(target.Position);

            // ensure auto-attack is turned on
            player.LuaCall(AutoAttackLuaScript);

            // ensure we're in melee range
            if (player.Position.DistanceTo(target.Position) > 35 || (natureImmuneCreatures.Contains(target.Name) || player.Mana < player.GetManaCost(LightningBolt) && (player.Position.DistanceTo(target.Position) > 3)))
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
            }
            else
                player.StopAllMovement();


            // ----- COMBAT ROTATION -----
            var partyMembers = ObjectManager.GetPartyMembers();
            var healTarget = partyMembers.FirstOrDefault(p => p.HealthPercent < 50);

            if (healTarget != null && player.Mana > player.GetManaCost(HealingWave))
            {
                player.SetTarget(healTarget.Guid);
                TryCastSpell(HealingWave);
            }

            TryCastSpell(LightningBolt, player.ManaPercent > 50 && target.HealthPercent < 90 && !natureImmuneCreatures.Contains(target.Name) && ((TargetMovingTowardPlayer && target.Position.DistanceTo(player.Position) > 15) || (!TargetMovingTowardPlayer && target.Position.DistanceTo(player.Position) > 5) || (player.HasBuff(FocusedCasting) && target.HealthPercent > 20 && Wait.For("FocusedLightningBoltDelay", 4000, true))));

            TryCastSpell(FlameShock, player.ManaPercent > 50 && target.HealthPercent < 90 && !target.HasDebuff(FlameShock) && (target.HealthPercent >= 50 || natureImmuneCreatures.Contains(target.Name)) && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(LightningShield, !natureImmuneCreatures.Contains(target.Name) && !player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, player.KnowsSpell(RockbiterWeapon) && (fireImmuneCreatures.Contains(target.Name) || !player.MainhandIsEnchanted && !player.KnowsSpell(FlametongueWeapon)));

            TryCastSpell(FlametongueWeapon, player.KnowsSpell(FlametongueWeapon) && !player.MainhandIsEnchanted && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(ManaSpringTotem, !ObjectManager.Units.Any(u => u.Position.DistanceTo(player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(ElementalMastery);

            targetLastPosition = target.Position;
        }

        void TryCastSpell(string name, bool condition = true, Action callback = null)
        {
            var distanceToTarget = player.Position.DistanceTo(target.Position);

            if (player.IsSpellReady(name) && player.Mana >= player.GetManaCost(name) && condition && !player.IsStunned && !player.IsCasting && !player.IsChanneling)
            {
                player.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        bool TargetMovingTowardPlayer =>
            targetLastPosition != null &&
            targetLastPosition.DistanceTo(player.Position) > target.Position.DistanceTo(player.Position);

        bool TargetIsFleeing =>
            targetLastPosition != null &&
            targetLastPosition.DistanceTo(player.Position) < target.Position.DistanceTo(player.Position);

        void OnErrorMessageCallback(object sender, OnUiMessageArgs e)
        {
            if (e.Message == LosErrorMessage)
            {
                noLos = true;
                noLosStartTime = Environment.TickCount;
            }
        }
    }
}
