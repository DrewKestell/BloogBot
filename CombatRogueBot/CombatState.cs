using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CombatRogueBot
{
    class CombatState : CombatStateBase, IBotState
    {
        const string AdrenalineRush = "Adrenaline Rush";
        const string BladeFlurry = "Blade Flurry";
        const string Evasion = "Evasion";
        const string Eviscerate = "Eviscerate";
        const string Gouge = "Gouge";
        const string BloodFury = "Blood Fury";
        const string Kick = "Kick";
        const string Riposte = "Riposte";
        const string SinisterStrike = "Sinister Strike";
        const string SliceAndDice = "Slice and Dice";

        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 3)
        {
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            TryUseAbility(AdrenalineRush, 0, ObjectManager.Aggressors.Count() == 3 && player.HealthPercent > 80);

            TryUseAbilityById(BloodFury, 3, 0, target.HealthPercent > 80);

            TryUseAbility(Evasion, 0, ObjectManager.Aggressors.Count() > 1);

            TryUseAbility(BladeFlurry, 25, ObjectManager.Aggressors.Count() > 1);

            TryUseAbility(SliceAndDice, 25, !player.HasBuff(SliceAndDice) && target.HealthPercent > 70 && player.ComboPoints == 2);

            TryUseAbility(Riposte, 10, player.CanRiposte);

            TryUseAbility(Kick, 25, ReadyToInterrupt(target));

            TryUseAbility(Gouge, 45, ReadyToInterrupt(target) && !player.IsSpellReady(Kick));

            var readyToEviscerate =
                target.HealthPercent <= 15 && player.ComboPoints >= 2
                || target.HealthPercent <= 25 && player.ComboPoints >= 3
                || target.HealthPercent <= 35 && player.ComboPoints >= 4
                || player.ComboPoints == 5;
            TryUseAbility(Eviscerate, 35, readyToEviscerate);

            TryUseAbility(SinisterStrike, 45, player.ComboPoints < 5);
        }

        bool ReadyToInterrupt(WoWUnit target) => target.Mana > 0 && (target.IsCasting || target.IsChanneling);
    }
}
