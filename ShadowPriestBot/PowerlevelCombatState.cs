using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using BloogBot.AI.SharedStates;
using BloogBot.Game.Enums;

namespace ShadowPriestBot
{
    class PowerlevelCombatState : IBotState
    {
        const string AutoAttackLuaScript = "if IsCurrentAction('12') == nil then CastSpellByName('Attack') end";
        const string LosErrorMessage = "Target not in line of sight";
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        const string AbolishDisease = "Abolish Disease";
        const string CureDisease = "Cure Disease";
        const string DispelMagic = "Dispel Magic";
        const string InnerFire = "Inner Fire";
        const string LesserHeal = "Lesser Heal";
        const string MindBlast = "Mind Blast";
        const string MindFlay = "Mind Flay";
        const string PowerWordShield = "Power Word: Shield";
        const string PsychicScream = "Psychic Scream";
        const string ShadowForm = "Shadowform";
        const string ShadowWordPain = "Shadow Word: Pain";
        const string Smite = "Smite";
        const string VampiricEmbrace = "Vampiric Embrace";
        const string WeakenedSoul = "Weakened Soul";

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

            WoWEventHandler.OnErrorMessage += OnErrorMessageCallback;
        }

        ~PowerlevelCombatState()
        {
            WoWEventHandler.OnErrorMessage -= OnErrorMessageCallback;
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

            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(LesserHeal))
            {
                botStates.Push(new HealSelfState(botStates, container));
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
            if (target.Health == 0 || target.TappedByOther || checkTarget == null)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    botStates.Pop();
                    botStates.Push(new LootState(botStates, container, target));
                    Wait.Remove(waitKey);
                }

                return;
            }

            if (player.TargetGuid != target.Guid)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Position)) player.Face(target.Position);

            // make sure we get into mind flay range for casters
            if ((target.IsCasting || target.IsChanneling) && player.Position.DistanceTo(target.Position) > 19)
                player.MoveToward(target.Position);
            else if (player.IsMoving)
                player.StopAllMovement();

            var hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;

            // ensure auto-attack is turned on only if we don't have a wand
            if (!hasWand)
                player.LuaCall(AutoAttackLuaScript);

            // ----- COMBAT ROTATION -----
            var useWand = (hasWand && player.ManaPercent <= 10 && !player.IsCasting && !player.IsChanneling) || target.CreatureType == CreatureType.Totem;
            if (useWand)
                player.LuaCall(WandLuaScript);
            else
            {
                var aggressors = ObjectManager.Aggressors;

                TryCastSpell(ShadowForm, 0, int.MaxValue, !player.HasBuff(ShadowForm));

                TryCastSpell(VampiricEmbrace, 0, 29, player.HealthPercent < 100 && !target.HasDebuff(VampiricEmbrace) && target.HealthPercent > 50);

                var noNeutralsNearby = !ObjectManager.Units.Any(u => u.Guid != target.Guid && u.UnitReaction == UnitReaction.Neutral && u.Position.DistanceTo(player.Position) <= 10);
                TryCastSpell(PsychicScream, 0, 7, (target.Position.DistanceTo(player.Position) < 8 && !player.HasBuff(PowerWordShield)) || ObjectManager.Aggressors.Count() > 1 && target.CreatureType != CreatureType.Elemental);

                TryCastSpell(ShadowWordPain, 0, 29, target.HealthPercent > 70 && !target.HasDebuff(ShadowWordPain));

                TryCastSpell(DispelMagic, 0, int.MaxValue, player.HasMagicDebuff, castOnSelf: true);

                if (player.KnowsSpell(AbolishDisease))
                    TryCastSpell(AbolishDisease, 0, int.MaxValue, player.IsDiseased && !player.HasBuff(ShadowForm), castOnSelf: true);
                else if (player.KnowsSpell(CureDisease))
                    TryCastSpell(CureDisease, 0, int.MaxValue, player.IsDiseased && !player.HasBuff(ShadowForm), castOnSelf: true);

                TryCastSpell(InnerFire, 0, int.MaxValue, !player.HasBuff(InnerFire));

                TryCastSpell(PowerWordShield, 0, int.MaxValue, !player.HasDebuff(WeakenedSoul) && !player.HasBuff(PowerWordShield) && (target.HealthPercent > 20 || player.HealthPercent < 10), castOnSelf: true);

                TryCastSpell(MindBlast, 0, 29);

                if (player.KnowsSpell(MindFlay) && target.Position.DistanceTo(player.Position) <= 19 && (!player.KnowsSpell(PowerWordShield) || player.HasBuff(PowerWordShield)))
                    TryCastSpell(MindFlay, 0, 19);
                else
                    TryCastSpell(Smite, 0, 29, !player.HasBuff(ShadowForm));

                if (powerlevelTarget.HealthPercent < 50)
                {
                    player.SetTarget(powerlevelTarget.Guid);
                    TryCastSpell(LesserHeal, 0, 40, player.Mana > player.GetManaCost(LesserHeal));
                }
            }
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            var distanceToTarget = player.Position.DistanceTo(target.Position);

            if (player.IsSpellReady(name) && player.Mana >= player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !player.IsStunned && !player.IsCasting && !player.IsChanneling)
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                player.LuaCall($"CastSpellByName(\"{name}\"{castOnSelfString})");
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
    }
}
