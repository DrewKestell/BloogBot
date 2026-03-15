using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FeralDruidBot
{
    class CombatState : CombatStateBase, IBotState
    {
        // Shapeshifting
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";

        // Bear
        const string Maul = "Maul";
        const string Enrage = "Enrage";
        const string DemoralizingRoar = "Demoralizing Roar";

        // Cat
        const string Claw = "Claw";
        const string Rake = "Rake";
        const string Rip = "Rip";
        const string TigersFury = "Tiger's Fury";
        const string FeralCharge = "Feral Charge - Cat";
        const string FaerieFire = "Faerie Fire (Feral)";
        const string FerociousBite = "Ferocious Bite";
        const string Mangle = "Mangle (Cat)";
        const string Berserk = "Berserk";

        // Human
        const string HealingTouch = "Healing Touch";
        const string Moonfire = "Moonfire";
        const string Wrath = "Wrath";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) :
            base(botStates, container, target, desiredRange: ObjectManager.Player.Level <= 12 ? 30 : 4)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && player.Mana >= player.GetManaCost(HealingTouch))
            {
                Wait.RemoveAll();
                botStates.Push(new HealSelfState(botStates, container, target));
                return;
            }

            if (base.Update())
            {
                return;
            }

            // if less than level 13, use spellcasting
            if (player.Level <= 12)
            {
                // if low on mana, move into melee range
                if (player.ManaPercent < 20 && player.Position.DistanceTo(target.Position) > 5)
                {
                    player.MoveToward(target.Position);
                    return;
                }
                else player.StopAllMovement();

                TryCastSpell(Moonfire, 0, 10, !target.HasDebuff(Moonfire));

                TryCastSpell(Wrath, 0, 30);
            }
            // bear form
            else if (player.Level > 12 && player.Level < 20)
            {
                TryCastSpell(BearForm, 0, 50, player.CurrentShapeshiftForm != BearForm && Wait.For("BearFormDelay", 1000, true));

                if (ObjectManager.Aggressors.Count() > 1)
                {
                    TryUseBearAbility(DemoralizingRoar, 10, !target.HasDebuff(DemoralizingRoar) && player.CurrentShapeshiftForm == BearForm);
                }

                TryUseBearAbility(Enrage, condition: player.CurrentShapeshiftForm == BearForm);

                TryUseBearAbility(Maul, Math.Max(15 - (player.Level - 9), 10), player.CurrentShapeshiftForm == BearForm);
            }
            // cat form
            else if (player.Level >= 20)
            {
                if (player.Position.DistanceTo(target.Position) > 8)
                {
                    TryUseCatAbility(FeralCharge, requiredEnergy: 10);
                }

                TryCastSpell(CatForm, 0, int.MaxValue, player.CurrentShapeshiftForm != CatForm);

                TryUseCatAbility(Berserk, 0, condition: target.HealthPercent > 30 && !player.HasBuff(Berserk));

                TryUseCatAbility(TigersFury, 30, condition: target.HealthPercent > 30 && !player.HasBuff(TigersFury));

                TryCastSpell(FaerieFire, condition: !target.HasDebuff(FaerieFire));

                TryUseCatAbility(Rip, 30, true, condition:
                    !target.HasDebuff(Rip) &&
                    player.ComboPoints >= 5);

                TryUseCatAbility(FerociousBite, 35, true, condition: player.ComboPoints >= 5);

                TryUseCatAbility(Rake, 35, false, condition: target.HealthPercent > 50 && !target.HasDebuff(Rake));

                TryUseCatAbility(Mangle, 40, false, condition: player.KnowsSpell(Mangle));

                TryUseCatAbility(Claw, 40, false, condition: !player.KnowsSpell(Mangle));
            }
        }

        void TryUseBearAbility(string name, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (player.IsSpellReady(name) && player.Rage >= requiredRage && !player.IsStunned && player.CurrentShapeshiftForm == BearForm && condition)
            {
                player.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void TryUseCatAbility(string name, int requiredEnergy = 0, bool requiresComboPoints = false, bool condition = true, Action callback = null)
        {
            if (player.IsSpellReady(name) && player.Energy >= requiredEnergy && (!requiresComboPoints || player.ComboPoints > 0) && !player.IsStunned && player.CurrentShapeshiftForm == CatForm && condition)
            {
                player.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }
    }
}
