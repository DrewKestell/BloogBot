using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace FeralDruidBot
{
    class CombatTask : CombatRotationTask, IBotTask
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

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;
        
        Location targetLastLocation;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 30)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
            this.target = targets[0];
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && player.Mana >= player.GetManaCost(HealingTouch))
            {
                if (player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);

                if (player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);

                Wait.RemoveAll();
                botTasks.Push(new HealTask(container, botTasks, target));
                return;
            }

            if (target.TappedByOther)
            {
                player.StopMovement(ControlBits.Nothing);
                Wait.RemoveAll();
                botTasks.Pop();
                return;
            }

            if (target.Health == 0)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    player.StopMovement(ControlBits.Nothing);
                    botTasks.Pop();
                    botTasks.Push(new LootTask(container, botTasks, target));
                    Wait.Remove(waitKey);
                }

                return;
            }

            if (player.TargetGuid == player.Guid)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Location)) player.Face(target.Location);

            // ensure auto-attack is turned on
            Lua.Instance.Execute(AutoAttackLuaScript);

            // if less than level 13, use spellcasting
            if (player.Level <= 12)
            {
                // if low on mana, move into melee range
                if (player.ManaPercent < 20 && player.Location.GetDistanceTo(target.Location) > 5)
                {
                    player.MoveToward(target.Location);
                    return;
                }
                else player.StopMovement(ControlBits.Nothing);

                TryCastSpell(Moonfire, 0, 10, !target.HasDebuff(Moonfire));

                TryCastSpell(Wrath, 10, 30);
            }
            // bear form
            else if (player.Level > 12 && player.Level < 20)
            {
                // ensure we're in melee range
                if ((player.Location.GetDistanceTo(target.Location) > 3 && player.CurrentShapeshiftForm == BearForm && target.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && player.Casting == 0))
                {
                    var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
                    player.MoveToward(nextWaypoint[0]);
                }
                else
                    player.StopMovement(ControlBits.Nothing);

                TryCastSpell(BearForm, 0, 50, player.CurrentShapeshiftForm != BearForm && Wait.For("BearFormDelay", 1000, true));

                if (ObjectManager.Instance.Aggressors.Count() > 1)
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
                if ((player.Location.GetDistanceTo(target.Location) > 3 && player.CurrentShapeshiftForm == CatForm && target.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && player.Casting == 0))
                {
                    var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
                    player.MoveToward(nextWaypoint[0]);
                }
                else
                    player.StopMovement(ControlBits.Nothing);

                TryCastSpell(CatForm, 0, 50, player.CurrentShapeshiftForm != CatForm);

                TryUseCatAbility(TigersFury, 30, condition: target.HealthPercent > 30 && !player.HasBuff(TigersFury));

                TryUseCatAbility(Rake, 35, condition: target.HealthPercent > 50 && !target.HasDebuff(Rake));

                TryUseCatAbility(Claw, 40);

                //TryUseCatAbility(Rip, 30, true, (target.HealthPercent < 70 && !target.HasDebuff(Rip)));
            }

            targetLastLocation = target.Location;
        }

        void TryUseBearAbility(string name, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (Spellbook.Instance.IsSpellReady(name) && player.Rage >= requiredRage && !player.IsStunned && player.CurrentShapeshiftForm == BearForm && condition)
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void TryUseCatAbility(string name, int requiredEnergy = 0, bool requiresComboPoints = false, bool condition = true, Action callback = null)
        {
            if (Spellbook.Instance.IsSpellReady(name) && player.Energy >= requiredEnergy && (!requiresComboPoints || player.ComboPoints > 0) && !player.IsStunned && player.CurrentShapeshiftForm == CatForm && condition)
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void CastSpell(string name)
        {
            if (Spellbook.Instance.IsSpellReady(name) && player.Casting == 0)
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null)
        {
            var distanceToTarget = player.Location.GetDistanceTo(target.Location);

            if (Spellbook.Instance.IsSpellReady(name) && player.Mana >= player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !player.IsStunned && player.Casting == 0 && player.Channeling == 0)
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        bool TargetMovingTowardPlayer =>
            targetLastLocation != null &&
            targetLastLocation.GetDistanceTo(player.Location) > target.Location.GetDistanceTo(player.Location);
    }
}
