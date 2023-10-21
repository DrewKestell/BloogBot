using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
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

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        WoWUnit target;
        WoWUnit secondaryTarget;

        bool castingEntanglingRoots;
        bool backpedaling;
        int backpedalStartTime;

        Action EntanglingRootsCallback => () =>
        {
            castingEntanglingRoots = true;
        };

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (castingEntanglingRoots)
            {
                if (secondaryTarget.HasDebuff(EntanglingRoots))
                {
                    backpedaling = true;
                    backpedalStartTime = Environment.TickCount;
                    Container.Player.StartMovement(ControlBits.Back);
                }

                Container.Player.SetTarget(target.Guid);
                castingEntanglingRoots = false;
            }

            // handle backpedaling during entangling roots
            if (Environment.TickCount - backpedalStartTime > 1500)
            {
                Container.Player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return;

            // heal self if we're injured
            if (Container.Player.HealthPercent < 30 && (Container.Player.Mana >= Container.Player.GetManaCost(HealingTouch) || Container.Player.Mana >= Container.Player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors[0];
            }

            if (Update(target, 30))
                return;

            // if we get an add, root it with Entangling Roots
            if (ObjectManager.Instance.Aggressors.Count() == 2 && secondaryTarget == null)
                secondaryTarget = ObjectManager.Instance.Aggressors.Single(u => u.Guid != Container.HostileTarget.Guid);

            if (secondaryTarget != null && !secondaryTarget.HasDebuff(EntanglingRoots))
            {
                Container.Player.SetTarget(secondaryTarget.Guid);
                TryCastSpell(EntanglingRoots, 0, 30, !secondaryTarget.HasDebuff(EntanglingRoots), EntanglingRootsCallback);
            }

            TryCastSpell(MoonkinForm, !Container.Player.HasBuff(MoonkinForm));

            TryCastSpell(Innervate, Container.Player.ManaPercent < 10, castOnSelf: true);

            TryCastSpell(RemoveCurse, 0, int.MaxValue, Container.Player.IsCursed && !Container.Player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(AbolishPoison, 0, int.MaxValue, Container.Player.IsPoisoned && !Container.Player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(InsectSwarm, 0, 30, !target.HasDebuff(InsectSwarm) && Container.HostileTarget.HealthPercent > 20 && !ImmuneToNatureDamage.Any(s => Container.HostileTarget.Name.Contains(s)));

            TryCastSpell(Moonfire, 0, 30, !target.HasDebuff(Moonfire));

            TryCastSpell(Wrath, 0, 30, !ImmuneToNatureDamage.Any(s => Container.HostileTarget.Name.Contains(s)));
        }
    }
}
