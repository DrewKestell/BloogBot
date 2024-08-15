using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace PaladinRetribution.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (ObjectManager.Player.HealthPercent < 30 && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HolyLight))
            {
                BotTasks.Push(new HealTask(BotContext));
                return;
            }

            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.GetTarget(ObjectManager.Player) == null || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(3))
                return;

            TryCastSpell(Purify, ObjectManager.Player.IsPoisoned || ObjectManager.Player.IsDiseased, castOnSelf: true);

            TryCastSpell(DevotionAura, !ObjectManager.Player.HasBuff(DevotionAura) && !ObjectManager.Player.IsSpellReady(RetributionAura) && !ObjectManager.Player.IsSpellReady(SanctityAura));

            TryCastSpell(RetributionAura, !ObjectManager.Player.HasBuff(RetributionAura) && !ObjectManager.Player.IsSpellReady(SanctityAura));

            TryCastSpell(SanctityAura, !ObjectManager.Player.HasBuff(SanctityAura));

            TryCastSpell(Exorcism, ObjectManager.GetTarget(ObjectManager.Player).CreatureType == CreatureType.Undead || ObjectManager.GetTarget(ObjectManager.Player).CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Humanoid || (ObjectManager.GetTarget(ObjectManager.Player).CreatureType == CreatureType.Humanoid && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent < 20));
            
            TryCastSpell(SealOfTheCrusader, !ObjectManager.Player.HasBuff(SealOfTheCrusader) && !ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !ObjectManager.Player.HasBuff(SealOfRighteousness) && ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(JudgementOfTheCrusader) && !ObjectManager.Player.IsSpellReady(SealOfCommand));

            TryCastSpell(SealOfCommand, !ObjectManager.Player.HasBuff(SealOfCommand) && ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(HolyShield, !ObjectManager.Player.HasBuff(HolyShield) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50);

            TryCastSpell(Judgement, ObjectManager.Player.HasBuff(SealOfTheCrusader) || ((ObjectManager.Player.HasBuff(SealOfRighteousness) || ObjectManager.Player.HasBuff(SealOfCommand)) && (ObjectManager.Player.ManaPercent >= 95 || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 3)));
        }
    }
}
