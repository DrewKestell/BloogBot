using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class CombatState : CombatStateBase, IBotState
    {
        const string WandLuaScript = "if IsAutoRepeatAction(12) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(12) ~= nil then CastSpellByName('Shoot') end";

        const string AbolishDisease = "Abolish Disease";
        const string CureDisease = "Cure Disease";
        const string DispelMagic = "Dispel Magic";
        const string InnerFire = "Inner Fire";
        const string LesserHeal = "Lesser Heal";
        const string MindBlast = "Mind Blast";
        const string MindFlay = "Mind Flay";
        const string PowerWordShield = "Power Word: Shield";
        const string PsychicScream = "Psychic Scream";
        const string ShadowForm = "Shadowform";
        const string ShadowWordPain = "Shadow Word: Pain";
        const string Smite = "Smite";
        const string VampiricEmbrace = "Vampiric Embrace";
        const string WeakenedSoul = "Weakened Soul";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 30)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(LesserHeal))
            {
                botStates.Push(new HealSelfState(botStates, container));
                return;
            }

            if (base.Update())
                return;

            var hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;
            var useWand = hasWand && !player.IsCasting && !player.IsChanneling && (player.ManaPercent <= 10 || target.CreatureType == CreatureType.Totem || target.HealthPercent <= 10);
            if (useWand)
                player.LuaCall(WandLuaScript);
            else
            {
                var aggressors = ObjectManager.Aggressors;
                
                TryCastSpell(ShadowForm, 0, int.MaxValue, !player.HasBuff(ShadowForm));

                TryCastSpell(VampiricEmbrace, 0, 29, player.HealthPercent < 100 && !target.HasDebuff(VampiricEmbrace) && target.HealthPercent > 50);

                var noNeutralsNearby = !ObjectManager.Units.Any(u => u.Guid != target.Guid && u.UnitReaction == UnitReaction.Neutral && u.Position.DistanceTo(player.Position) <= 10);
                TryCastSpell(PsychicScream, 0, 7, (target.Position.DistanceTo(player.Position) < 8 && !player.HasBuff(PowerWordShield)) || ObjectManager.Aggressors.Count() > 1 && target.CreatureType != CreatureType.Elemental);

                TryCastSpell(ShadowWordPain, 0, 29, target.HealthPercent > 70 && !target.HasDebuff(ShadowWordPain));

                TryCastSpell(DispelMagic, 0, int.MaxValue, player.HasMagicDebuff, castOnSelf: true);
                
                if (player.KnowsSpell(AbolishDisease))
                    TryCastSpell(AbolishDisease, 0, int.MaxValue, player.IsDiseased && !player.HasBuff(ShadowForm), castOnSelf: true);
                else if (player.KnowsSpell(CureDisease))
                    TryCastSpell(CureDisease, 0, int.MaxValue, player.IsDiseased && !player.HasBuff(ShadowForm), castOnSelf: true);

                TryCastSpell(InnerFire, 0, int.MaxValue, !player.HasBuff(InnerFire));

                TryCastSpell(PowerWordShield, 0, int.MaxValue, !player.HasDebuff(WeakenedSoul) && !player.HasBuff(PowerWordShield) && (target.HealthPercent > 20 || player.HealthPercent < 10), castOnSelf: true);

                TryCastSpell(MindBlast, 0, 29);

                if (player.KnowsSpell(MindFlay) && target.Position.DistanceTo(player.Position) <= 19 && (!player.KnowsSpell(PowerWordShield) || player.HasBuff(PowerWordShield)))
                    TryCastSpell(MindFlay, 0, 19);
                else
                    TryCastSpell(Smite, 0, 29, !player.HasBuff(ShadowForm));
            }
        }
    }
}
