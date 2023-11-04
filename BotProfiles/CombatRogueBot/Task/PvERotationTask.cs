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
        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (Container.HostileTarget == null || Container.HostileTarget.HealthPercent <= 0)
            {
                Container.HostileTarget = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(Container.HostileTarget, 3))
                return;

            TryUseAbility(AdrenalineRush, 0, ObjectManager.Instance.Aggressors.Count() == 3 && Container.Player.HealthPercent > 80);

            TryUseAbilityById(BloodFury, 3, 0, Container.HostileTarget.HealthPercent > 80);

            TryUseAbility(Evasion, 0, ObjectManager.Instance.Aggressors.Count() > 1);

            TryUseAbility(BladeFlurry, 25, ObjectManager.Instance.Aggressors.Count() > 1);

            TryUseAbility(SliceAndDice, 25, !Container.Player.HasBuff(SliceAndDice) && Container.HostileTarget.HealthPercent > 70 && Container.Player.ComboPoints == 2);

            TryUseAbility(Riposte, 10, Container.Player.CanRiposte);

            TryUseAbility(Kick, 25, ReadyToInterrupt(Container.HostileTarget));

            TryUseAbility(Gouge, 45, ReadyToInterrupt(Container.HostileTarget) && !Spellbook.Instance.IsSpellReady(Kick));

            bool readyToEviscerate =
                Container.HostileTarget.HealthPercent <= 15 && Container.Player.ComboPoints >= 2
                || Container.HostileTarget.HealthPercent <= 25 && Container.Player.ComboPoints >= 3
                || Container.HostileTarget.HealthPercent <= 35 && Container.Player.ComboPoints >= 4
                || Container.Player.ComboPoints == 5;
            TryUseAbility(Eviscerate, 35, readyToEviscerate);

            TryUseAbility(SinisterStrike, 45, Container.Player.ComboPoints < 5);
        }

        bool ReadyToInterrupt(WoWUnit target) => Container.HostileTarget.Mana > 0 && (target.IsCasting || Container.HostileTarget.IsChanneling);
    }
}
