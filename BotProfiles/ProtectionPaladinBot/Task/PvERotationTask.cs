using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionPaladinBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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
        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (ObjectManager.Player.HealthPercent < 30 && Container.HostileTarget.HealthPercent > 50 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HolyLight))
            {
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
                Container.HostileTarget = ObjectManager.Aggressors.First();
            }

            if (Update(Container.HostileTarget, 4))
                return;

            TryCastSpell(LayOnHands, ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HolyLight) && ObjectManager.Player.HealthPercent < 10, castOnSelf: true);

            TryCastSpell(Purify, ObjectManager.Player.IsPoisoned || ObjectManager.Player.IsDiseased, castOnSelf: true);

            TryCastSpell(RighteousFury, !ObjectManager.Player.HasBuff(RighteousFury));

            TryCastSpell(DevotionAura, !ObjectManager.Player.HasBuff(DevotionAura) && !ObjectManager.Player.IsSpellReady(RetributionAura));

            TryCastSpell(RetributionAura, !ObjectManager.Player.HasBuff(RetributionAura) && ObjectManager.Player.IsSpellReady(RetributionAura));

            TryCastSpell(Exorcism, 0, 30, Container.HostileTarget.CreatureType == CreatureType.Undead || Container.HostileTarget.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, 0, 10, Container.HostileTarget.CreatureType != CreatureType.Humanoid || (Container.HostileTarget.CreatureType == CreatureType.Humanoid && Container.HostileTarget.HealthPercent < 20));

            TryCastSpell(Consecration, ObjectManager.Aggressors.Count() > 1);

            // do we need to use JudgementOfWisdom? prot pally seems to always be at full mana.

            TryCastSpell(JudgementOfLight, 0, 10, !Container.HostileTarget.HasDebuff(JudgementOfLight) && ObjectManager.Player.Buffs.Any(b => b.Name.StartsWith("Seal of")));

            TryCastSpell(SealOfTheCrusader, !ObjectManager.Player.HasBuff(SealOfTheCrusader) && !Container.HostileTarget.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !ObjectManager.Player.HasBuff(SealOfRighteousness) && (Container.HostileTarget.HasDebuff(JudgementOfTheCrusader) || !ObjectManager.Player.IsSpellReady(JudgementOfTheCrusader)));

            TryCastSpell(HolyShield, !ObjectManager.Player.HasBuff(HolyShield) && Container.HostileTarget.HealthPercent > 50);
        }
    }
}
