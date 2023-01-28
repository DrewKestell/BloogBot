using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BalanceDruidBot
{
    class CombatState : CombatStateBase, IBotState
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

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;
        readonly WoWUnit target;
        WoWUnit secondaryTarget;

        bool castingEntanglingRoots;
        bool backpedaling;
        int backpedalStartTime;

        Action EntanglingRootsCallback => () =>
        {
            castingEntanglingRoots = true;
        };

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 30)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (castingEntanglingRoots)
            {
                if (secondaryTarget.HasDebuff(EntanglingRoots))
                {
                    backpedaling = true;
                    backpedalStartTime = Environment.TickCount;
                    player.StartMovement(ControlBits.Back);
                }

                player.SetTarget(target.Guid);
                player.Target = target;
                castingEntanglingRoots = false;
            }

            // handle backpedaling during entangling roots
            if (Environment.TickCount - backpedalStartTime > 1500)
            {
                player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return;

            // heal self if we're injured
            if (player.HealthPercent < 30 && (player.Mana >= player.GetManaCost(HealingTouch) || player.Mana >= player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                botStates.Push(new HealSelfState(botStates, target));
                return;
            }

            if (base.Update())
                return;

            // if we get an add, root it with Entangling Roots
            if (ObjectManager.Aggressors.Count() == 2 && secondaryTarget == null)
                secondaryTarget = ObjectManager.Aggressors.Single(u => u.Guid != target.Guid);

            if (secondaryTarget != null && !secondaryTarget.HasDebuff(EntanglingRoots))
            {
                player.SetTarget(secondaryTarget.Guid);
                player.Target = secondaryTarget;
                TryCastSpell(EntanglingRoots, 0, 30, !secondaryTarget.HasDebuff(EntanglingRoots), EntanglingRootsCallback);
            }

            TryCastSpell(MoonkinForm, !player.HasBuff(MoonkinForm));

            TryCastSpell(Innervate, player.ManaPercent < 10, castOnSelf: true);

            TryCastSpell(RemoveCurse, 0, int.MaxValue, player.IsCursed && !player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(AbolishPoison, 0, int.MaxValue, player.IsPoisoned && !player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(InsectSwarm, 0, 30, !target.HasDebuff(InsectSwarm) && target.HealthPercent > 20 && !ImmuneToNatureDamage.Any(s => target.Name.Contains(s)));

            TryCastSpell(Moonfire, 0, 30, !target.HasDebuff(Moonfire));

            TryCastSpell(Wrath, 0, 30, !ImmuneToNatureDamage.Any(s => target.Name.Contains(s)));
        }
    }
}
