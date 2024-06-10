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
        const string Fade = "Fade";
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
        const string Heal = "Heal";
        const string Renew = "Renew";
        private bool hasWand;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            WoWUnit woWUnit = ObjectManager.Aggressors.FirstOrDefault(x => x.TargetGuid == ObjectManager.Player.Guid);
            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                List<WoWPlayer> unhealthyMembers = ObjectManager.PartyMembers.Where(x => x.HealthPercent < 70).OrderBy(x => x.Health).ToList();

                if (unhealthyMembers.Count > 0 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
                {
                    if (ObjectManager.Player.Target == null || ObjectManager.Player.TargetGuid != unhealthyMembers[0].Guid)
                    {
                        ObjectManager.Player.SetTarget(unhealthyMembers[0].Guid);
                        return;
                    }
                }

                if (ObjectManager.Player.IsCasting || ObjectManager.Player.Target == null)
                    return;

                if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 40 && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
                {
                    ObjectManager.Player.StopAllMovement();
                    Functions.LuaCall(TurnOffWandLuaScript);

                    Container.State.Action = $"Healing {ObjectManager.Player.Target.Name}";
                    ObjectManager.Player.StopAllMovement();

                    if (!ObjectManager.Player.Target.HasBuff(Renew))
                        Functions.LuaCall($"CastSpellByName('{Renew}')");
                    if (ObjectManager.Player.IsSpellReady(LesserHeal))
                        Functions.LuaCall($"CastSpellByName('{LesserHeal}')");

                    return;
                }
                else
                {
                    Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

                    if (nextWaypoint.Length > 1)
                    {
                        Container.State.Action = $"Moving to heal {ObjectManager.Player.Target.Name}";
                        ObjectManager.Player.MoveToward(nextWaypoint[1]);
                    }
                    else
                    {
                        Container.State.Action = $"Can't move to heal {ObjectManager.Player.Target.Name}";
                        ObjectManager.Player.StopAllMovement();
                    }
                    return;
                }
            }
            else if (woWUnit != null)
            {
                TryCastSpell(Fade, 0, int.MaxValue);

                if (woWUnit.ManaPercent > 5)
                {
                    if (MoveBehindTankSpot(45))
                        return;
                    else
                        ObjectManager.Player.StopAllMovement();
                }
                else if (MoveBehindTankSpot(3))
                    return;
                else
                    ObjectManager.Player.StopAllMovement();
            }
            else
            {
                AssignDPSTarget();

                if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.UnitReaction == UnitReaction.Friendly) return;

                if (Container.State.TankInPosition)
                {
                    if (MoveTowardsTarget())
                        return;

                    PerformCombatRotation();
                }
                else if (MoveBehindTankSpot(15))
                    return;
            }
        }

        public override void PerformCombatRotation()
        {
            ObjectManager.Player.StopAllMovement();
            ObjectManager.Player.Face(ObjectManager.Player.Target.Position);

            if (hasWand && (ObjectManager.Player.Target.HasDebuff(ShadowWordPain) || ObjectManager.Player.ManaPercent < 50))
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

                TryCastSpell(ShadowWordPain, 0, 29, ObjectManager.Player.Target.HealthPercent > 10 && !ObjectManager.Player.Target.HasDebuff(ShadowWordPain) && ObjectManager.Player.ManaPercent > 50);

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
