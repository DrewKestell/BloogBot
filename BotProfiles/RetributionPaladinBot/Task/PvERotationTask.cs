using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
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

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

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
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors[0].Guid);
            }

            if (Update(3))
                return;

            TryCastSpell(Purify, ObjectManager.Player.IsPoisoned || ObjectManager.Player.IsDiseased, castOnSelf: true);

            TryCastSpell(DevotionAura, !ObjectManager.Player.HasBuff(DevotionAura) && !ObjectManager.Player.IsSpellReady(RetributionAura) && !ObjectManager.Player.IsSpellReady(SanctityAura));

            TryCastSpell(RetributionAura, !ObjectManager.Player.HasBuff(RetributionAura) && !ObjectManager.Player.IsSpellReady(SanctityAura));

            TryCastSpell(SanctityAura, !ObjectManager.Player.HasBuff(SanctityAura));

            TryCastSpell(Exorcism, ObjectManager.Player.Target.CreatureType == CreatureType.Undead || ObjectManager.Player.Target.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, ObjectManager.Player.Target.CreatureType != CreatureType.Humanoid || (ObjectManager.Player.Target.CreatureType == CreatureType.Humanoid && ObjectManager.Player.Target.HealthPercent < 20));
            
            TryCastSpell(SealOfTheCrusader, !ObjectManager.Player.HasBuff(SealOfTheCrusader) && !ObjectManager.Player.Target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !ObjectManager.Player.HasBuff(SealOfRighteousness) && ObjectManager.Player.Target.HasDebuff(JudgementOfTheCrusader) && !ObjectManager.Player.IsSpellReady(SealOfCommand));

            TryCastSpell(SealOfCommand, !ObjectManager.Player.HasBuff(SealOfCommand) && ObjectManager.Player.Target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(HolyShield, !ObjectManager.Player.HasBuff(HolyShield) && ObjectManager.Player.Target.HealthPercent > 50);

            TryCastSpell(Judgement, ObjectManager.Player.HasBuff(SealOfTheCrusader) || ((ObjectManager.Player.HasBuff(SealOfRighteousness) || ObjectManager.Player.HasBuff(SealOfCommand)) && (ObjectManager.Player.ManaPercent >= 95 || ObjectManager.Player.Target.HealthPercent <= 3)));
        }
    }
}
