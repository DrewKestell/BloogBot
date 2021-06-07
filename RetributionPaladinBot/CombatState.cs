using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace RetributionPaladinBot
{
    class CombatState : CombatStateBase, IBotState
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

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 3)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(HolyLight))
            {
                botStates.Push(new HealSelfState(botStates, container));
                return;
            }

            if (base.Update())
                return;

            TryCastSpell(Purify, player.IsPoisoned || player.IsDiseased, castOnSelf: true);

            TryCastSpell(DevotionAura, !player.HasBuff(DevotionAura) && !player.KnowsSpell(RetributionAura) && !player.KnowsSpell(SanctityAura));

            TryCastSpell(RetributionAura, !player.HasBuff(RetributionAura) && !player.KnowsSpell(SanctityAura));

            TryCastSpell(SanctityAura, !player.HasBuff(SanctityAura));

            TryCastSpell(Exorcism, target.CreatureType == CreatureType.Undead || target.CreatureType == CreatureType.Demon);

            TryCastSpell(HammerOfJustice, (target.CreatureType != CreatureType.Humanoid || (target.CreatureType == CreatureType.Humanoid && target.HealthPercent < 20)));
            
            TryCastSpell(SealOfTheCrusader, !player.HasBuff(SealOfTheCrusader) && !target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(SealOfRighteousness, !player.HasBuff(SealOfRighteousness) && target.HasDebuff(JudgementOfTheCrusader) && !player.KnowsSpell(SealOfCommand));

            TryCastSpell(SealOfCommand, !player.HasBuff(SealOfCommand) && target.HasDebuff(JudgementOfTheCrusader));

            TryCastSpell(HolyShield, !player.HasBuff(HolyShield) && target.HealthPercent > 50);

            TryCastSpell(Judgement, player.HasBuff(SealOfTheCrusader) || ((player.HasBuff(SealOfRighteousness) || player.HasBuff(SealOfCommand)) && (player.ManaPercent >= 95 || target.HealthPercent <= 3)));
        }
    }
}
