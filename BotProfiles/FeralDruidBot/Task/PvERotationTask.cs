using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FeralDruidBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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
        WoWUnit target;
        
        Position targetLastPosition;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            if (ObjectManager.Player.HealthPercent < 30 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HealingTouch))
            {
                if (ObjectManager.Player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);

                if (ObjectManager.Player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);

                Wait.RemoveAll();
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (target.Health == 0)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    BotTasks.Push(new LootTask(Container, BotTasks));
                    Wait.Remove(waitKey);
                }

                return;
            }

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
                ObjectManager.Player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!ObjectManager.Player.IsFacing(target.Position)) ObjectManager.Player.Face(target.Position);

            // ensure auto-attack is turned on
            Functions.LuaCall(AutoAttackLuaScript);

            // if less than level 13, use spellcasting
            if (ObjectManager.Player.Level <= 12)
            {
                // if low on mana, move into melee range
                if (ObjectManager.Player.ManaPercent < 20 && ObjectManager.Player.Position.DistanceTo(target.Position) > 5)
                {
                    ObjectManager.Player.MoveToward(target.Position);
                    return;
                }
                else ObjectManager.Player.StopAllMovement();

                TryCastSpell(Moonfire, 0, 10, !target.HasDebuff(Moonfire));

                TryCastSpell(Wrath, 10, 30);
            }
            // bear form
            else if (ObjectManager.Player.Level > 12 && ObjectManager.Player.Level < 20)
            {
                // ensure we're in melee range
                if ((ObjectManager.Player.Position.DistanceTo(target.Position) > 3 && ObjectManager.Player.CurrentShapeshiftForm == BearForm && Container.HostileTarget.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && ObjectManager.Player.IsCasting))
                {
                    Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);
                    ObjectManager.Player.MoveToward(nextWaypoint[0]);
                }
                else
                    ObjectManager.Player.StopAllMovement();

                TryCastSpell(BearForm, 0, 50, ObjectManager.Player.CurrentShapeshiftForm != BearForm && Wait.For("BearFormDelay", 1000, true));

                if (ObjectManager.Aggressors.Count() > 1)
                {
                    TryUseBearAbility(DemoralizingRoar, 10, !target.HasDebuff(DemoralizingRoar) && ObjectManager.Player.CurrentShapeshiftForm == BearForm);
                }

                TryUseBearAbility(Enrage, condition: ObjectManager.Player.CurrentShapeshiftForm == BearForm);

                TryUseBearAbility(Maul, Math.Max(15 - (ObjectManager.Player.Level - 9), 10), ObjectManager.Player.CurrentShapeshiftForm == BearForm);
            }
            // cat form
            else if (ObjectManager.Player.Level >= 20)
            {
                // ensure we're in melee range
                if ((ObjectManager.Player.Position.DistanceTo(target.Position) > 3 && ObjectManager.Player.CurrentShapeshiftForm == CatForm && Container.HostileTarget.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && ObjectManager.Player.IsCasting))
                {
                    Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);
                    ObjectManager.Player.MoveToward(nextWaypoint[0]);
                }
                else
                    ObjectManager.Player.StopAllMovement();

                TryCastSpell(CatForm, 0, 50, ObjectManager.Player.CurrentShapeshiftForm != CatForm);

                TryUseCatAbility(TigersFury, 30, condition: Container.HostileTarget.HealthPercent > 30 && !ObjectManager.Player.HasBuff(TigersFury));

                TryUseCatAbility(Rake, 35, condition: Container.HostileTarget.HealthPercent > 50 && !target.HasDebuff(Rake));

                TryUseCatAbility(Claw, 40);

                //TryUseCatAbility(Rip, 30, true, (target.HealthPercent < 70 && !target.HasDebuff(Rip)));
            }

            targetLastPosition = Container.HostileTarget.Position;
        }

        void TryUseBearAbility(string name, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Rage >= requiredRage && !ObjectManager.Player.IsStunned && ObjectManager.Player.CurrentShapeshiftForm == BearForm && condition)
            {
                Functions.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void TryUseCatAbility(string name, int requiredEnergy = 0, bool requiresComboPoints = false, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Energy >= requiredEnergy && (!requiresComboPoints || ObjectManager.Player.ComboPoints > 0) && !ObjectManager.Player.IsStunned && ObjectManager.Player.CurrentShapeshiftForm == CatForm && condition)
            {
                Functions.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void CastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && !ObjectManager.Player.IsCasting)
                Functions.LuaCall($"CastSpellByName(\"{name}\")");
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null)
        {
            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(target.Position);

            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !ObjectManager.Player.IsStunned && ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0)
            {
                Functions.LuaCall($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }
    }
}
