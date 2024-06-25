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

        public override void PerformCombatRotation()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            if (ObjectManager.Player.HealthPercent < 30 && ObjectManager.Player.Target.HealthPercent > 50 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HolyLight))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(4))
                return;

            TryCastSpell(LayOnHands, ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HolyLight) && ObjectManager.Player.HealthPercent < 10, castOnSelf: true);

            TryCastSpell(Purify, ObjectManager.Player.IsPoisoned || ObjectManager.Player.IsDiseased, castOnSelf: true);

            TryCastSpell(RighteousFury, !ObjectManager.Player.HasBuff(RighteousFury));

            TryCastSpell(DevotionAura, !ObjectManager.Player.HasBuff(DevotionAura) && !ObjectManager.Player.IsSpellReady(RetributionAura));

            TryCastSpell(RetributionAura, !ObjectManager.Player.HasBuff(RetributionAura) && ObjectManager.Player.IsSpellReady(RetributionAura));

            TryCastSpell(Exorcism, 0, 30, ObjectManager.Player.Target.CreatureType == CreatureType.Undead || ObjectManager.Player.Target.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, 0, 10, ObjectManager.Player.Target.CreatureType != CreatureType.Humanoid || (ObjectManager.Player.Target.CreatureType == CreatureType.Humanoid && ObjectManager.Player.Target.HealthPercent < 20));

            TryCastSpell(Consecration, ObjectManager.Aggressors.Count() > 1);

            // do we need to use JudgementOfWisdom? prot pally seems to always be at full mana.

            TryCastSpell(JudgementOfLight, 0, 10, !ObjectManager.Player.Target.HasDebuff(JudgementOfLight) && ObjectManager.Player.Buffs.Any(b => b.Name.StartsWith("Seal of")));

            TryCastSpell(SealOfTheCrusader, !ObjectManager.Player.HasBuff(SealOfTheCrusader) && !ObjectManager.Player.Target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !ObjectManager.Player.HasBuff(SealOfRighteousness) && (ObjectManager.Player.Target.HasDebuff(JudgementOfTheCrusader) || !ObjectManager.Player.IsSpellReady(JudgementOfTheCrusader)));

            TryCastSpell(HolyShield, !ObjectManager.Player.HasBuff(HolyShield) && ObjectManager.Player.Target.HealthPercent > 50);
        }
    }
}
