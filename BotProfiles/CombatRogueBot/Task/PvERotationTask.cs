using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CombatRogueBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                botTasks.Pop();
                return;
            }

            if (target == null || target.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            TryUseAbility(AdrenalineRush, 0, ObjectManager.Instance.Aggressors.Count() == 3 && player.HealthPercent > 80);

            TryUseAbilityById(BloodFury, 3, 0, target.HealthPercent > 80);

            TryUseAbility(Evasion, 0, ObjectManager.Instance.Aggressors.Count() > 1);

            TryUseAbility(BladeFlurry, 25, ObjectManager.Instance.Aggressors.Count() > 1);

            TryUseAbility(SliceAndDice, 25, !player.HasBuff(SliceAndDice) && target.HealthPercent > 70 && player.ComboPoints == 2);

            TryUseAbility(Riposte, 10, player.CanRiposte);

            TryUseAbility(Kick, 25, ReadyToInterrupt(target));

            TryUseAbility(Gouge, 45, ReadyToInterrupt(target) && !Spellbook.Instance.IsSpellReady(Kick));

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
