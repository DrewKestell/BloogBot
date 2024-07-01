using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace PaladinProtection.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private const string Consecration = "Consecration";
        private const string DevotionAura = "Devotion Aura";
        private const string Exorcism = "Exorcism";
        private const string HammerOfJustice = "Hammer of Justice";
        private const string HolyLight = "Holy Light";
        private const string HolyShield = "Holy Shield";
        private const string Judgement = "Judgement";
        private const string JudgementOfLight = "Judgement of Light";
        private const string JudgementOfWisdom = "Judgement of Wisdom";
        private const string JudgementOfTheCrusader = "Judgement of the Crusader";
        private const string LayOnHands = "Lay on Hands";
        private const string Purify = "Purify";
        private const string RetributionAura = "Retribution Aura";
        private const string RighteousFury = "Righteous Fury";
        private const string SealOfRighteousness = "Seal of Righteousness";
        private const string SealOfTheCrusader = "Seal of the Crusader";
        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public override void PerformCombatRotation()
        {

        }

        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
