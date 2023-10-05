using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RetributionPaladinBot
{
    class CombatTask : CombatRotationTask, IBotTask
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

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 3)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
            this.target = targets[0];
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(HolyLight))
            {
                botTasks.Push(new HealTask(container, botTasks, target));
                return;
            }

            if (base.Update())
                return;

            TryCastSpell(Purify, player.IsPoisoned || player.IsDiseased, castOnSelf: true);

            TryCastSpell(DevotionAura, !player.HasBuff(DevotionAura) && !Spellbook.Instance.IsSpellReady(RetributionAura) && !Spellbook.Instance.IsSpellReady(SanctityAura));

            TryCastSpell(RetributionAura, !player.HasBuff(RetributionAura) && !Spellbook.Instance.IsSpellReady(SanctityAura));

            TryCastSpell(SanctityAura, !player.HasBuff(SanctityAura));

            TryCastSpell(Exorcism, target.CreatureType == CreatureType.Undead || target.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, target.CreatureType != CreatureType.Humanoid || (target.CreatureType == CreatureType.Humanoid && target.HealthPercent < 20));
            
            TryCastSpell(SealOfTheCrusader, !player.HasBuff(SealOfTheCrusader) && !target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !player.HasBuff(SealOfRighteousness) && target.HasDebuff(JudgementOfTheCrusader) && !Spellbook.Instance.IsSpellReady(SealOfCommand));

            TryCastSpell(SealOfCommand, !player.HasBuff(SealOfCommand) && target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(HolyShield, !player.HasBuff(HolyShield) && target.HealthPercent > 50);

            TryCastSpell(Judgement, player.HasBuff(SealOfTheCrusader) || ((player.HasBuff(SealOfRighteousness) || player.HasBuff(SealOfCommand)) && (player.ManaPercent >= 95 || target.HealthPercent <= 3)));
        }
    }
}
