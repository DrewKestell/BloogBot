using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
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
        
        Location targetLastLocation;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            if (Container.Player.HealthPercent < 30 && Container.Player.Mana >= Container.Player.GetManaCost(HealingTouch))
            {
                if (Container.Player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);

                if (Container.Player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);

                Wait.RemoveAll();
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (target.TappedByOther)
            {
                Container.Player.StopAllMovement();
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (target.Health == 0)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                    BotTasks.Push(new LootTask(Container, BotTasks));
                    Wait.Remove(waitKey);
                }

                return;
            }

            if (Container.Player.TargetGuid == Container.Player.Guid)
                Container.Player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!Container.Player.IsFacing(target.Location)) Container.Player.Face(target.Location);

            // ensure auto-attack is turned on
            Lua.Instance.Execute(AutoAttackLuaScript);

            // if less than level 13, use spellcasting
            if (Container.Player.Level <= 12)
            {
                // if low on mana, move into melee range
                if (Container.Player.ManaPercent < 20 && Container.Player.Location.GetDistanceTo(target.Location) > 5)
                {
                    Container.Player.MoveToward(target.Location);
                    return;
                }
                else Container.Player.StopAllMovement();

                TryCastSpell(Moonfire, 0, 10, !target.HasDebuff(Moonfire));

                TryCastSpell(Wrath, 10, 30);
            }
            // bear form
            else if (Container.Player.Level > 12 && Container.Player.Level < 20)
            {
                // ensure we're in melee range
                if ((Container.Player.Location.GetDistanceTo(target.Location) > 3 && Container.Player.CurrentShapeshiftForm == BearForm && Container.HostileTarget.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && Container.Player.IsCasting))
                {
                    var nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
                    Container.Player.MoveToward(nextWaypoint[0]);
                }
                else
                    Container.Player.StopAllMovement();

                TryCastSpell(BearForm, 0, 50, Container.Player.CurrentShapeshiftForm != BearForm && Wait.For("BearFormDelay", 1000, true));

                if (ObjectManager.Instance.Aggressors.Count() > 1)
                {
                    TryUseBearAbility(DemoralizingRoar, 10, !target.HasDebuff(DemoralizingRoar) && Container.Player.CurrentShapeshiftForm == BearForm);
                }

                TryUseBearAbility(Enrage, condition: Container.Player.CurrentShapeshiftForm == BearForm);

                TryUseBearAbility(Maul, Math.Max(15 - (Container.Player.Level - 9), 10), Container.Player.CurrentShapeshiftForm == BearForm);
            }
            // cat form
            else if (Container.Player.Level >= 20)
            {
                // ensure we're in melee range
                if ((Container.Player.Location.GetDistanceTo(target.Location) > 3 && Container.Player.CurrentShapeshiftForm == CatForm && Container.HostileTarget.IsInCombat && !TargetMovingTowardPlayer) || (!target.IsInCombat && Container.Player.IsCasting))
                {
                    var nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
                    Container.Player.MoveToward(nextWaypoint[0]);
                }
                else
                    Container.Player.StopAllMovement();

                TryCastSpell(CatForm, 0, 50, Container.Player.CurrentShapeshiftForm != CatForm);

                TryUseCatAbility(TigersFury, 30, condition: Container.HostileTarget.HealthPercent > 30 && !Container.Player.HasBuff(TigersFury));

                TryUseCatAbility(Rake, 35, condition: Container.HostileTarget.HealthPercent > 50 && !target.HasDebuff(Rake));

                TryUseCatAbility(Claw, 40);

                //TryUseCatAbility(Rip, 30, true, (target.HealthPercent < 70 && !target.HasDebuff(Rip)));
            }

            targetLastLocation = Container.HostileTarget.Location;
        }

        void TryUseBearAbility(string name, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (Spellbook.Instance.IsSpellReady(name) && Container.Player.Rage >= requiredRage && !Container.Player.IsStunned && Container.Player.CurrentShapeshiftForm == BearForm && condition)
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void TryUseCatAbility(string name, int requiredEnergy = 0, bool requiresComboPoints = false, bool condition = true, Action callback = null)
        {
            if (Spellbook.Instance.IsSpellReady(name) && Container.Player.Energy >= requiredEnergy && (!requiresComboPoints || Container.Player.ComboPoints > 0) && !Container.Player.IsStunned && Container.Player.CurrentShapeshiftForm == CatForm && condition)
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }

        void CastSpell(string name)
        {
            if (Spellbook.Instance.IsSpellReady(name) && !Container.Player.IsCasting)
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null)
        {
            var distanceToTarget = Container.Player.Location.GetDistanceTo(target.Location);

            if (Spellbook.Instance.IsSpellReady(name) && Container.Player.Mana >= Container.Player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !Container.Player.IsStunned && Container.Player.IsCasting && Container.Player.Channeling == 0)
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\")");
                callback?.Invoke();
            }
        }
    }
}
