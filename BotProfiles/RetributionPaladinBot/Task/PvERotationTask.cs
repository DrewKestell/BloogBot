using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RetributionPaladinBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string DevotionAura = "Devotion Aura";
        const string Exorcism = "Exorcism";
        const string HammerOfJustice = "Hammer of Justice";
        const string HolyLight = "Holy Light";
        const string HolyShield = "Holy Shield";
        const string Judgement = "Judgement";
        const string JudgementOfTheCrusader = "Judgement of the Crusader";
        const string Purify = "Purify";
        const string RetributionAura = "Retribution Aura";
        const string SanctityAura = "Sanctity Aura";
        const string SealOfCommand = "Seal of Command";
        const string SealOfRighteousness = "Seal of Righteousness";
        const string SealOfTheCrusader = "Seal of the Crusader";
        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (Container.Player.HealthPercent < 30 && Container.HostileTarget.HealthPercent > 50 && Container.Player.Mana >= Container.Player.GetManaCost(HolyLight))
            {
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

            if (Update(target, 3))
                return;

            TryCastSpell(Purify, Container.Player.IsPoisoned || Container.Player.IsDiseased, castOnSelf: true);

            TryCastSpell(DevotionAura, !Container.Player.HasBuff(DevotionAura) && !Spellbook.Instance.IsSpellReady(RetributionAura) && !Spellbook.Instance.IsSpellReady(SanctityAura));

            TryCastSpell(RetributionAura, !Container.Player.HasBuff(RetributionAura) && !Spellbook.Instance.IsSpellReady(SanctityAura));

            TryCastSpell(SanctityAura, !Container.Player.HasBuff(SanctityAura));

            TryCastSpell(Exorcism, Container.HostileTarget.CreatureType == CreatureType.Undead || Container.HostileTarget.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, Container.HostileTarget.CreatureType != CreatureType.Humanoid || (target.CreatureType == CreatureType.Humanoid && Container.HostileTarget.HealthPercent < 20));
            
            TryCastSpell(SealOfTheCrusader, !Container.Player.HasBuff(SealOfTheCrusader) && !target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !Container.Player.HasBuff(SealOfRighteousness) && Container.HostileTarget.HasDebuff(JudgementOfTheCrusader) && !Spellbook.Instance.IsSpellReady(SealOfCommand));

            TryCastSpell(SealOfCommand, !Container.Player.HasBuff(SealOfCommand) && Container.HostileTarget.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(HolyShield, !Container.Player.HasBuff(HolyShield) && Container.HostileTarget.HealthPercent > 50);

            TryCastSpell(Judgement, Container.Player.HasBuff(SealOfTheCrusader) || ((Container.Player.HasBuff(SealOfRighteousness) || Container.Player.HasBuff(SealOfCommand)) && (Container.Player.ManaPercent >= 95 || Container.HostileTarget.HealthPercent <= 3)));
        }
    }
}
