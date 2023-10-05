using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ShadowPriestBot
{
    class CombatTask : CombatRotationTask, IBotTask
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

        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 30)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (player.HealthPercent < 30 && target.HealthPercent > 50 && player.Mana >= player.GetManaCost(LesserHeal))
            {
                botTasks.Push(new HealTask(botTasks));
                return;
            }

            if (base.Update())
                return;

            var hasWand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged) != null;
            var useWand = hasWand && player.Casting == 0 && player.Channeling == 0 && (player.ManaPercent <= 10 || target.CreatureType == CreatureType.Totem || target.HealthPercent <= 10);
            if (useWand)
                Lua.Instance.Execute(WandLuaScript);
            else
            {
                var aggressors = ObjectManager.Instance.Aggressors;
                
                TryCastSpell(ShadowForm, 0, int.MaxValue, !player.HasBuff(ShadowForm));

                TryCastSpell(VampiricEmbrace, 0, 29, player.HealthPercent < 100 && !target.HasDebuff(VampiricEmbrace) && target.HealthPercent > 50);

                var noNeutralsNearby = !ObjectManager.Instance.Units.Any(u => u.Guid != target.Guid && u.Reaction == UnitReaction.Neutral && u.Location.GetDistanceTo(player.Location) <= 10);
                TryCastSpell(PsychicScream, 0, 7, (target.Location.GetDistanceTo(player.Location) < 8 && !player.HasBuff(PowerWordShield)) || ObjectManager.Instance.Aggressors.Count() > 1 && target.CreatureType != CreatureType.Elemental);

                TryCastSpell(ShadowWordPain, 0, 29, target.HealthPercent > 70 && !target.HasDebuff(ShadowWordPain));

                TryCastSpell(DispelMagic, 0, int.MaxValue, player.HasMagicDebuff, castOnSelf: true);
                
                if (Spellbook.Instance.IsSpellReady(AbolishDisease))
                    TryCastSpell(AbolishDisease, 0, int.MaxValue, player.IsDiseased && !player.HasBuff(ShadowForm), castOnSelf: true);
                else if (Spellbook.Instance.IsSpellReady(CureDisease))
                    TryCastSpell(CureDisease, 0, int.MaxValue, player.IsDiseased && !player.HasBuff(ShadowForm), castOnSelf: true);

                TryCastSpell(InnerFire, 0, int.MaxValue, !player.HasBuff(InnerFire));

                TryCastSpell(PowerWordShield, 0, int.MaxValue, !player.HasDebuff(WeakenedSoul) && !player.HasBuff(PowerWordShield) && (target.HealthPercent > 20 || player.HealthPercent < 10), castOnSelf: true);

                TryCastSpell(MindBlast, 0, 29);

                if (Spellbook.Instance.IsSpellReady(MindFlay) && target.Location.GetDistanceTo(player.Location) <= 19 && (!Spellbook.Instance.IsSpellReady(PowerWordShield) || player.HasBuff(PowerWordShield)))
                    TryCastSpell(MindFlay, 0, 19);
                else
                    TryCastSpell(Smite, 0, 29, !player.HasBuff(ShadowForm));
            }
        }
    }
}
