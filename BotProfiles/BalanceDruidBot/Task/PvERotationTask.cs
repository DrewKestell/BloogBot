using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace BalanceDruidBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        static readonly string[] ImmuneToNatureDamage = { "Vortex", "Whirlwind", "Whirling", "Dust", "Cyclone" };

        const string AbolishPoison = "Abolish Poison";
        const string EntanglingRoots = "Entangling Roots";
        const string HealingTouch = "Healing Touch";
        const string Moonfire = "Moonfire";
        const string Rejuvenation = "Rejuvenation";
        const string RemoveCurse = "Remove Curse";
        const string Wrath = "Wrath";
        const string InsectSwarm = "Insect Swarm";
        const string Innervate = "Innervate";
        const string MoonkinForm = "Moonkin Form";

        WoWUnit secondaryTarget;

        bool castingEntanglingRoots;
        bool backpedaling;
        int backpedalStartTime;

        Action EntanglingRootsCallback => () =>
        {
            castingEntanglingRoots = true;
        };

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (castingEntanglingRoots)
            {
                if (secondaryTarget.HasDebuff(EntanglingRoots))
                {
                    backpedaling = true;
                    backpedalStartTime = Environment.TickCount;
                    ObjectManager.Player.StartMovement(ControlBits.Back);
                }

                ObjectManager.Player.SetTarget(Container.HostileTarget.Guid);
                castingEntanglingRoots = false;
            }

            // handle backpedaling during entangling roots
            if (Environment.TickCount - backpedalStartTime > 1500)
            {
                ObjectManager.Player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return;

            // heal self if we're injured
            if (ObjectManager.Player.HealthPercent < 30 && (ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HealingTouch) || ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (Container.HostileTarget == null || Container.HostileTarget.HealthPercent <= 0)
            {
                Container.HostileTarget = ObjectManager.Aggressors[0];
            }

            if (Update(30))
                return;

            // if we get an add, root it with Entangling Roots
            if (ObjectManager.Aggressors.Count() == 2 && secondaryTarget == null)
                secondaryTarget = ObjectManager.Aggressors.Single(u => u.Guid != Container.HostileTarget.Guid);

            if (secondaryTarget != null && !secondaryTarget.HasDebuff(EntanglingRoots))
            {
                ObjectManager.Player.SetTarget(secondaryTarget.Guid);
                TryCastSpell(EntanglingRoots, 0, 30, !secondaryTarget.HasDebuff(EntanglingRoots), EntanglingRootsCallback);
            }

            TryCastSpell(MoonkinForm, !ObjectManager.Player.HasBuff(MoonkinForm));

            TryCastSpell(Innervate, ObjectManager.Player.ManaPercent < 10, castOnSelf: true);

            TryCastSpell(RemoveCurse, 0, int.MaxValue, ObjectManager.Player.IsCursed && !ObjectManager.Player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(AbolishPoison, 0, int.MaxValue, ObjectManager.Player.IsPoisoned && !ObjectManager.Player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(InsectSwarm, 0, 30, !Container.HostileTarget.HasDebuff(InsectSwarm) && Container.HostileTarget.HealthPercent > 20 && !ImmuneToNatureDamage.Any(s => Container.HostileTarget.Name.Contains(s)));

            TryCastSpell(Moonfire, 0, 30, !Container.HostileTarget.HasDebuff(Moonfire));

            TryCastSpell(Wrath, 0, 30, !ImmuneToNatureDamage.Any(s => Container.HostileTarget.Name.Contains(s)));
        }
    }
}
