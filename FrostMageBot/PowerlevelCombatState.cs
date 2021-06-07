using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostMageBot
{
    class PowerlevelCombatState : IBotState
    {
        const string LosErrorMessage = "Target not in line of sight";
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";

        readonly string[] FireWardTargets = new[] { "Fire", "Flame", "Infernal", "Searing", "Hellcaller" };
        readonly string[] FrostWardTargets = new[] { "Ice", "Frost" };

        const string ConeOfCold = "Cone of Cold";
        const string Counterspell = "Counterspell";
        const string Evocation = "Evocation";
        const string Fireball = "Fireball";
        const string FireBlast = "Fire Blast";
        const string FireWard = "Fire Ward";
        const string FrostNova = "Frost Nova";
        const string FrostWard = "Frost Ward";
        const string Frostbite = "Frostbite";
        const string Frostbolt = "Frostbolt";
        const string IceBarrier = "Ice Barrier";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly string nuke;
        readonly int range;

        bool noLos;
        int noLosStartTime;

        bool backpedaling;
        int backpedalStartTime;

        Action FrostNovaCallback => () =>
        {
            backpedaling = true;
            backpedalStartTime = Environment.TickCount;
            player.StartMovement(ControlBits.Back);
        };

        public PowerlevelCombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target, WoWPlayer powerlevelTarget)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;

            WoWEventHandler.OnErrorMessage += OnErrorMessageCallback;

            if (player.Level >= 8)
                nuke = Frostbolt;
            else if (player.Level >= 6 || !player.KnowsSpell(Frostbolt))
                nuke = Fireball;
            else if (player.Level >= 4 && player.KnowsSpell(Frostbolt))
                nuke = Frostbolt;
            else
                nuke = Fireball;

            range = 29 + (ObjectManager.GetTalentRank(3, 11) * 3);
        }

        ~PowerlevelCombatState()
        {
            WoWEventHandler.OnErrorMessage -= OnErrorMessageCallback;
        }

        public void Update()
        {
            if (player.IsChanneling)
                return;

            if (Environment.TickCount - backpedalStartTime > 1500)
            {
                player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }

            if (backpedaling)
                return;

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

            if (target.TappedByOther)
            {
                botStates.Pop();
                return;
            }

            // when killing certain summoned units (like totems), our local reference to target will still have 100% health even after the totem is destroyed
            // so we need to lookup the target again in the object manager, and if it's null, we can assume it's dead and leave combat.
            var checkTarget = ObjectManager.Units.FirstOrDefault(u => u.Guid == target.Guid);
            if (target.Health == 0 || checkTarget == null)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    botStates.Pop();

                    if (!player.IsSwimming)
                        botStates.Push(new RestState(botStates, container));

                    Wait.Remove(waitKey);
                }

                return;
            }

            // ensure the correct target is set
            if (player.TargetGuid != target.Guid)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Position)) player.Face(target.Position);

            // ensure we're in melee range
            if (player.Position.DistanceTo(target.Position) > range && !player.IsCasting)
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
            }
            else
                player.StopAllMovement();

            // ----- COMBAT ROTATION -----
            TryCastSpell(Evocation, 0, Int32.MaxValue, (player.HealthPercent > 50 || player.HasBuff(IceBarrier)) && player.ManaPercent < 8 && target.HealthPercent > 15);

            var wand = Inventory.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && player.ManaPercent <= 10 && !player.IsCasting && !player.IsChanneling)
                player.LuaCall(WandLuaScript);
            else
            {
                TryCastSpell(FireWard, 0, Int32.MaxValue, FireWardTargets.Any(c => target.Name.Contains(c)) && (target.HealthPercent > 20 || player.HealthPercent < 10));

                TryCastSpell(FrostWard, 0, Int32.MaxValue, FrostWardTargets.Any(c => target.Name.Contains(c)) && (target.HealthPercent > 20 || player.HealthPercent < 10));

                TryCastSpell(Counterspell, 0, 29, target.Mana > 0 && target.IsCasting);

                TryCastSpell(IceBarrier, 0, 50, !player.HasBuff(IceBarrier) && player.HealthPercent < 95 && player.ManaPercent > 40 && (target.HealthPercent > 20 || player.HealthPercent < 10));

                TryCastSpell(FrostNova, 0, 10, target.TargetGuid == player.Guid && target.HealthPercent > 20 && !IsTargetFrozen && !ObjectManager.Units.Any(u => u.Guid != target.Guid && u.HealthPercent > 0 && u.Guid != player.Guid && u.Position.DistanceTo(player.Position) <= 12), callback: FrostNovaCallback);

                TryCastSpell(ConeOfCold, 0, 8, player.Level >= 30 && target.HealthPercent > 20 && IsTargetFrozen);

                TryCastSpell(FireBlast, 0, 19, !IsTargetFrozen);

                // Either Frostbolt or Fireball depending on what is stronger. Will always use Frostbolt at level 8+.
                TryCastSpell(nuke, 0, range);
            }
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null)
        {
            var distanceToTarget = player.Position.DistanceTo(target.Position);

            if (player.IsSpellReady(name) && player.Mana >= player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !player.IsStunned && !player.IsCasting && !player.IsChanneling)
            {
                player.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }

        void OnErrorMessageCallback(object sender, OnUiMessageArgs e)
        {
            if (e.Message == LosErrorMessage)
            {
                noLos = true;
                noLosStartTime = Environment.TickCount;
            }
        }

        bool IsTargetFrozen => target.HasDebuff(Frostbite) || target.HasDebuff(FrostNova);
    }
}
