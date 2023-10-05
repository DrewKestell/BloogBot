using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionPaladinBot
{
    class CombatTask : CombatRotationTask, IBotTask
    {
        const string Consecration = "Consecration";
        const string DevotionAura = "Devotion Aura";
        const string Exorcism = "Exorcism";
        const string HammerOfJustice = "Hammer of Justice";
        const string HolyLight = "Holy Light";
        const string HolyShield = "Holy Shield";
        const string Judgement = "Judgement";
        const string JudgementOfLight = "Judgement of Light";
        const string JudgementOfWisdom = "Judgement of Wisdom";
        const string JudgementOfTheCrusader = "Judgement of the Crusader";
        const string LayOnHands = "Lay on Hands";
        const string Purify = "Purify";
        const string RetributionAura = "Retribution Aura";
        const string RighteousFury = "Righteous Fury";
        const string SealOfRighteousness = "Seal of Righteousness";
        const string SealOfTheCrusader = "Seal of the Crusader";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 4)
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

            TryCastSpell(LayOnHands, player.Mana < player.GetManaCost(HolyLight) && player.HealthPercent < 10, castOnSelf: true);

            TryCastSpell(Purify, player.IsPoisoned || player.IsDiseased, castOnSelf: true);

            TryCastSpell(RighteousFury, !player.HasBuff(RighteousFury));

            TryCastSpell(DevotionAura, !player.HasBuff(DevotionAura) && !Spellbook.Instance.IsSpellReady(RetributionAura));

            TryCastSpell(RetributionAura, !player.HasBuff(RetributionAura) && Spellbook.Instance.IsSpellReady(RetributionAura));

            TryCastSpell(Exorcism, 0, 30, target.CreatureType == CreatureType.Undead || target.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, 0, 10, target.CreatureType != CreatureType.Humanoid || (target.CreatureType == CreatureType.Humanoid && target.HealthPercent < 20));

            TryCastSpell(Consecration, ObjectManager.Instance.Aggressors.Count() > 1);

            // do we need to use JudgementOfWisdom? prot pally seems to always be at full mana.

            TryCastSpell(JudgementOfLight, 0, 10, !target.HasDebuff(JudgementOfLight) && player.Buffs.Any(b => b.Name.StartsWith("Seal of")));

            TryCastSpell(SealOfTheCrusader, !player.HasBuff(SealOfTheCrusader) && !target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !player.HasBuff(SealOfRighteousness) && (target.HasDebuff(JudgementOfTheCrusader) || !Spellbook.Instance.IsSpellReady(JudgementOfTheCrusader)));

            TryCastSpell(HolyShield, !player.HasBuff(HolyShield) && target.HealthPercent > 50);
        }
    }
}
