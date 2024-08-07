using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace PriestShadow.Tasks
{
    internal class PvERotationTask(IBotContext botContext) : CombatRotationTask(botContext), IBotTask
    {
        public void Update()
        {
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            IWoWUnit woWUnit = ObjectManager.Aggressors.FirstOrDefault(x => x.TargetGuid == ObjectManager.Player.Guid);
            if (ObjectManager.PartyMembers.Any(x => x.HealthPercent < 70) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(LesserHeal))
            {
                List<IWoWPlayer> unhealthyMembers = ObjectManager.PartyMembers.Where(x => x.HealthPercent < 70).OrderBy(x => x.Health).ToList();

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
                    ObjectManager.Player.StopWand();

                    ObjectManager.Player.StopAllMovement();

                    if (!ObjectManager.Player.Target.HasBuff(Renew))
                        ObjectManager.Player.CastSpell(Renew);
                    if (ObjectManager.Player.IsSpellReady(LesserHeal))
                        ObjectManager.Player.CastSpell(LesserHeal);

                    return;
                }
                else
                {
                    Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

                    if (nextWaypoint.Length > 1)
                    {
                        ObjectManager.Player.MoveToward(nextWaypoint[1]);
                    }
                    else
                    {
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

                //if (Container.State.TankInPosition)
                //{
                //    if (MoveTowardsTarget())
                //        return;

                //    PerformCombatRotation();
                //}
                //else if (MoveBehindTankSpot(15))
                //    return;
            }
        }

        public override void PerformCombatRotation()
        {
            ObjectManager.Player.StopAllMovement();
            ObjectManager.Player.Face(ObjectManager.Player.Target.Position);

            if (ObjectManager.Player.Target.HasDebuff(ShadowWordPain) || ObjectManager.Player.ManaPercent < 50)
                ObjectManager.Player.StartWand();
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
