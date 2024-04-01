using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
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
        private bool useWand;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            bool hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;
            useWand = hasWand && !ObjectManager.Player.IsCasting;
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                ObjectManager.Player.StopAllMovement();
                Functions.LuaCall(TurnOffWandLuaScript);
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            WoWUnit woWUnit = ObjectManager.Aggressors.FirstOrDefault(x => x.TargetGuid == ObjectManager.Player.Guid);
            if (woWUnit != null)
            {
                if (woWUnit.ManaPercent > 5)
                {
                    if (MoveBehindTankSpot(30))
                    {
                        Container.State.Action = "Moving far behind tank spot";
                        return;
                    }
                }
                else
                {
                    if (MoveBehindTankSpot(3))
                    {
                        Container.State.Action = "Moving close behind tank spot";
                        return;
                    }
                    else
                        ObjectManager.Player.StopAllMovement();
                }
            }

            AssignDPSTarget();

            if (MoveBehindTankSpot(8))
            {
                Container.State.Action = "Moving close behind tank spot";
                return;
            }
            else
            {
                Container.State.Action = "Attacking target";
                ObjectManager.Player.StopAllMovement();
                if (ObjectManager.Player.Target == null) return;

                ObjectManager.Player.Face(ObjectManager.Player.Target.Position);

                if (useWand && ObjectManager.Player.Target.HasDebuff(ShadowWordPain))
                    Functions.LuaCall(WandLuaScript);
                else
                {
                    //TryCastSpell(ShadowForm, 0, int.MaxValue, !ObjectManager.Player.HasBuff(ShadowForm));

                    //TryCastSpell(VampiricEmbrace, 0, 29, ObjectManager.Player.HealthPercent < 100 && !ObjectManager.Player.Target.HasDebuff(VampiricEmbrace) && ObjectManager.Player.Target.HealthPercent > 50);

                    //bool noNeutralsNearby = !ObjectManager.Units.Any(u => u.Guid != ObjectManager.Player.TargetGuid && u.UnitReaction == UnitReaction.Neutral && u.Position.DistanceTo(ObjectManager.Player.Position) <= 10);
                    //TryCastSpell(PsychicScream, 0, 7, (ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.Player.Position) < 8 && !ObjectManager.Player.HasBuff(PowerWordShield)) || ObjectManager.Aggressors.Count() > 1 && ObjectManager.Player.Target.CreatureType != CreatureType.Elemental);

                    TryCastSpell(DispelMagic, 0, int.MaxValue, ObjectManager.Player.HasMagicDebuff, castOnSelf: true);

                    if (ObjectManager.Player.IsSpellReady(AbolishDisease))
                        TryCastSpell(AbolishDisease, 0, int.MaxValue, ObjectManager.Player.IsDiseased && !ObjectManager.Player.HasBuff(ShadowForm), castOnSelf: true);
                    else if (ObjectManager.Player.IsSpellReady(CureDisease))
                        TryCastSpell(CureDisease, 0, int.MaxValue, ObjectManager.Player.IsDiseased && !ObjectManager.Player.HasBuff(ShadowForm), castOnSelf: true);

                    TryCastSpell(InnerFire, 0, int.MaxValue, !ObjectManager.Player.HasBuff(InnerFire));

                    TryCastSpell(ShadowWordPain, 0, 29, ObjectManager.Player.Target.HealthPercent > 10 && !ObjectManager.Player.Target.HasDebuff(ShadowWordPain));

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
}
