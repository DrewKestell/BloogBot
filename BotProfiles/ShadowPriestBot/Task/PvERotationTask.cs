using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ShadowPriestBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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
        readonly LocalPlayer player;

        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Instance.PartyMembers.Any(x => x.HealthPercent < 70) && Container.Player.Mana >= Container.Player.GetManaCost(LesserHeal))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 30))
                return;

            bool hasWand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged) != null;
            bool useWand = hasWand && !Container.Player.IsCasting && !Container.Player.IsChanneling && (Container.Player.ManaPercent <= 10 || Container.HostileTarget.CreatureType == CreatureType.Totem || Container.HostileTarget.HealthPercent <= 10);
            if (useWand)
                Lua.Instance.Execute(WandLuaScript);
            else
            {
                List<WoWUnit> aggressors = ObjectManager.Instance.Aggressors;

                //TryCastSpell(ShadowForm, 0, int.MaxValue, !Container.Player.HasBuff(ShadowForm));

                //TryCastSpell(VampiricEmbrace, 0, 29, Container.Player.HealthPercent < 100 && !target.HasDebuff(VampiricEmbrace) && Container.HostileTarget.HealthPercent > 50);

                bool noNeutralsNearby = !ObjectManager.Instance.Units.Any(u => u.Guid != Container.HostileTarget.Guid && u.Reaction == UnitReaction.Neutral && u.Location.GetDistanceTo(Container.Player.Location) <= 10);
                TryCastSpell(PsychicScream, 0, 7, (target.Location.GetDistanceTo(Container.Player.Location) < 8 && !Container.Player.HasBuff(PowerWordShield)) || ObjectManager.Instance.Aggressors.Count() > 1 && Container.HostileTarget.CreatureType != CreatureType.Elemental);

                TryCastSpell(ShadowWordPain, 0, 29, Container.HostileTarget.HealthPercent > 70 && !target.HasDebuff(ShadowWordPain));

                TryCastSpell(DispelMagic, 0, int.MaxValue, Container.Player.HasMagicDebuff, castOnSelf: true);

                if (Spellbook.Instance.IsSpellReady(AbolishDisease))
                    TryCastSpell(AbolishDisease, 0, int.MaxValue, Container.Player.IsDiseased && !Container.Player.HasBuff(ShadowForm), castOnSelf: true);
                else if (Spellbook.Instance.IsSpellReady(CureDisease))
                    TryCastSpell(CureDisease, 0, int.MaxValue, Container.Player.IsDiseased && !Container.Player.HasBuff(ShadowForm), castOnSelf: true);

                TryCastSpell(InnerFire, 0, int.MaxValue, !Container.Player.HasBuff(InnerFire));

                //TryCastSpell(PowerWordShield, 0, int.MaxValue, !Container.Player.HasDebuff(WeakenedSoul) && !Container.Player.HasBuff(PowerWordShield) && (target.HealthPercent > 20 || Container.Player.HealthPercent < 10), castOnSelf: true);

                //TryCastSpell(MindBlast, 0, 29);

                //if (Spellbook.Instance.IsSpellReady(MindFlay) && Container.HostileTarget.Location.GetDistanceTo(Container.Player.Location) <= 19 && (!Spellbook.Instance.IsSpellReady(PowerWordShield) || Container.Player.HasBuff(PowerWordShield)))
                //    TryCastSpell(MindFlay, 0, 19);
                //else
                //    TryCastSpell(Smite, 0, 29, !Container.Player.HasBuff(ShadowForm));
            }
        }
    }
}
