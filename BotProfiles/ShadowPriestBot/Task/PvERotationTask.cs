using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ShadowPriestBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(72) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(72) ~= nil then CastSpellByName('Shoot') end";

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

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(30))
                return;

            bool hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;
            bool useWand = hasWand && !ObjectManager.Player.IsCasting;
            if (useWand)
                Functions.LuaCall(WandLuaScript);
            else
            {
                List<WoWUnit> aggressors = ObjectManager.Aggressors;

                //TryCastSpell(ShadowForm, 0, int.MaxValue, !ObjectManager.Player.HasBuff(ShadowForm));

                //TryCastSpell(VampiricEmbrace, 0, 29, ObjectManager.Player.HealthPercent < 100 && !ObjectManager.Player.Target.HasDebuff(VampiricEmbrace) && ObjectManager.Player.Target.HealthPercent > 50);

                //bool noNeutralsNearby = !ObjectManager.Units.Any(u => u.Guid != ObjectManager.Player.TargetGuid && u.UnitReaction == UnitReaction.Neutral && u.Position.DistanceTo(ObjectManager.Player.Position) <= 10);
                //TryCastSpell(PsychicScream, 0, 7, (ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.Player.Position) < 8 && !ObjectManager.Player.HasBuff(PowerWordShield)) || ObjectManager.Aggressors.Count() > 1 && ObjectManager.Player.Target.CreatureType != CreatureType.Elemental);

                TryCastSpell(ShadowWordPain, 0, 29, ObjectManager.Player.Target.HealthPercent > 70 && !ObjectManager.Player.Target.HasDebuff(ShadowWordPain));

                TryCastSpell(DispelMagic, 0, int.MaxValue, ObjectManager.Player.HasMagicDebuff, castOnSelf: true);

                if (ObjectManager.Player.IsSpellReady(AbolishDisease))
                    TryCastSpell(AbolishDisease, 0, int.MaxValue, ObjectManager.Player.IsDiseased && !ObjectManager.Player.HasBuff(ShadowForm), castOnSelf: true);
                else if (ObjectManager.Player.IsSpellReady(CureDisease))
                    TryCastSpell(CureDisease, 0, int.MaxValue, ObjectManager.Player.IsDiseased && !ObjectManager.Player.HasBuff(ShadowForm), castOnSelf: true);

                TryCastSpell(InnerFire, 0, int.MaxValue, !ObjectManager.Player.HasBuff(InnerFire));

                //TryCastSpell(PowerWordShield, 0, int.MaxValue, !ObjectManager.Player.HasDebuff(WeakenedSoul) && !ObjectManager.Player.HasBuff(PowerWordShield) && (ObjectManager.Player.Target.HealthPercent > 20 || ObjectManager.Player.HealthPercent < 10), castOnSelf: true);

                //TryCastSpell(MindBlast, 0, 29);

                //if (ObjectManager.Player.IsSpellReady(MindFlay) && ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.Player.Position) <= 19 && (!ObjectManager.Player.IsSpellReady(PowerWordShield) || ObjectManager.Player.HasBuff(PowerWordShield)))
                //    TryCastSpell(MindFlay, 0, 19);
                //else
                //    TryCastSpell(Smite, 0, 29, !ObjectManager.Player.HasBuff(ShadowForm));
            }
        }
    }
}
