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
    class CombatState : IBotState
    {
        const string AutoAttackLuaScript = "if IsCurrentAction('12') == nil then CastSpellByName('Attack') end";

        // Shapeshifting
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";
        const string HumanForm = "Human Form";

        // Bear
        const string Maul = "Maul";
        const string Enrage = "Enrage";
        const string DemoralizingRoar = "Demoralizing Roar";

        // Cat
        const string Claw = "Claw";
        const string Rake = "Rake";
        const string Rip = "Rip";
        const string TigersFury = "Tiger's Fury";

        // Human
        const string HealingTouch = "Healing Touch";
        const string Moonfire = "Moonfire";
        const string Wrath = "Wrath";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;
        
        Position targetLastPosition;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            this.target = target;
        }

        public void Update()
        {
            if (player.HealthPercent < 30 && player.Mana >= player.GetManaCost(HealingTouch))
            {
                if (player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);

                if (player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);

                Wait.RemoveAll();
                botStates.Push(new HealSelfState(botStates, container, target));
                return;
            }

            if (target.TappedByOther)
            {
                player.StopAllMovement();
                Wait.RemoveAll();
                botStates.Pop();
                return;
            }

            if (target.Health == 0)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    player.StopAllMovement();
                    botStates.Pop();
                    botStates.Push(new LootState(botStates, container, target));
                    Wait.Remove(waitKey);
                }

                return;
            }

            if (player.TargetGuid == player.Guid)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Position)) player.Face(target.Position);

            // ensure auto-attack is turned on
            player.LuaCall(AutoAttackLuaScript);

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

                TryCastSpell(Wrath, 10, 30);
            }
            // bear form
            else if (player.Level > 12 && player.Level < 20)
            {
                // ensure we're in melee range
                if ((player.Position.DistanceTo(target.Position) > 3 && player.CurrentShapeshiftForm == BearForm && target.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && !player.IsCasting))
                {
                    var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                    player.MoveToward(nextWaypoint);
                }
                else
                    player.StopAllMovement();

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
                // ensure we're in melee range
                if ((player.Position.DistanceTo(target.Position) > 3 && player.CurrentShapeshiftForm == CatForm && target.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && !player.IsCasting))
                {
                    var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                    player.MoveToward(nextWaypoint);
                }
                else
                    player.StopAllMovement();

                TryCastSpell(CatForm, 0, 50, player.CurrentShapeshiftForm != CatForm);

                TryUseCatAbility(TigersFury, 30, condition: target.HealthPercent > 30 && !player.HasBuff(TigersFury));

                TryUseCatAbility(Rake, 35, condition: target.HealthPercent > 50 && !target.HasDebuff(Rake));

                TryUseCatAbility(Claw, 40);

                //TryUseCatAbility(Rip, 30, true, (target.HealthPercent < 70 && !target.HasDebuff(Rip)));
            }

            targetLastPosition = target.Position;
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

        void CastSpell(string name)
        {
            if (player.IsSpellReady(name) && !player.IsCasting)
                player.LuaCall($"CastSpellByName(\"{name}\")");
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null)
        {
            var distanceToTarget = player.Position.DistanceTo(target.Position);

            if (player.IsSpellReady(name) && player.Mana >= player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !player.IsStunned && !player.IsCasting && !player.IsChanneling)
            {
                player.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        bool TargetMovingTowardPlayer =>
            targetLastPosition != null &&
            targetLastPosition.DistanceTo(player.Position) > target.Position.DistanceTo(player.Position);
    }
}
