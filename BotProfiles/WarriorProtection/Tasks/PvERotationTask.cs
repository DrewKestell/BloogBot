using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using System.Diagnostics;
using static BotRunner.Constants.Spellbook;

namespace WarriorProtection.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private readonly Stopwatch overpowerStopwatch = new();
        private readonly Position tankSpot;
        private IWoWUnit currentDPSTarget;
        internal PvERotationTask(IBotContext botContext) : base(botContext) => EventHandler.OnBlockParryDodge += Instance_OnBlockParryDodge;
        ~PvERotationTask()
        {
            EventHandler.OnBlockParryDodge -= Instance_OnBlockParryDodge;
        }

        private void Instance_OnBlockParryDodge(object sender, EventArgs e)
        {
            overpowerStopwatch.Restart();
        }

        public void Update()
        {
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            ObjectManager.Player.StartMeleeAttack();

            List<IWoWUnit> looseUnits = [.. ObjectManager.Aggressors.Where(x => x.TargetGuid != ObjectManager.Player.Guid).OrderBy(x => x.Position.DistanceTo(ObjectManager.Player.Position))];
            IWoWUnit nearestHostile = ObjectManager.Hostiles.Where(x => !x.IsInCombat).OrderBy(x => x.Position.DistanceTo(ObjectManager.Player.Position)).First();

            if (looseUnits.Count > 0)
            {
                IWoWUnit looseUnit = looseUnits.First();

                ObjectManager.Player.SetTarget(looseUnit.Guid);

                if ((looseUnit.ManaPercent < 10 || looseUnit.Position.DistanceTo(ObjectManager.Player.Position) < 8) && Update(5))
                {

                }
                else
                {
                    ObjectManager.Player.StopAllMovement();

                    if (ObjectManager.Player.CurrentStance != DefensiveStance)
                        TryCastSpell(DefensiveStance);
                    else if (ObjectManager.Player.IsSpellReady(Taunt))
                        TryUseAbility(Taunt);
                    else
                        PerformCombatRotation();
                }
            }
            else
            {
                if (ObjectManager.SkullTargetGuid == 0 || !ObjectManager.Hostiles.Any(x => x.Guid == ObjectManager.SkullTargetGuid))
                {
                    currentDPSTarget = ObjectManager.Aggressors.OrderBy(x => x.Health).Last();
                    ObjectManager.Player.SetTarget(currentDPSTarget.Guid);

                    ObjectManager.SetRaidTarget(currentDPSTarget, TargetMarker.Skull);
                }
                else
                {
                    currentDPSTarget = ObjectManager.Units.First(x => x.Guid == ObjectManager.SkullTargetGuid);
                }

                if (tankSpot.DistanceTo(ObjectManager.Player.Position) > 5)
                {
                    Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, tankSpot, true);

                    if (locations.Length > 1)
                        ObjectManager.Player.MoveToward(locations[1]);
                    else
                        ObjectManager.Player.StopAllMovement();
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    ObjectManager.Player.Face(currentDPSTarget.Position);

                    PerformCombatRotation();
                }
            }
        }

        public override void PerformCombatRotation()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player) == null) return;

            TryUseAbility(Bloodrage, condition: ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50);

            if (ObjectManager.Aggressors.Count() >= 3)
            {
                TryUseAbility(Retaliation);
            }

            //if (ObjectManager.Aggressors.Count() >= 4 && !ObjectManager.Aggressors.All(u => u.HasDebuff(ThunderClap)))
            //{
            //    if (ObjectManager.Player.CurrentStance != BattleStance)
            //    {
            //        TryCastSpell(BattleStance);
            //    }

            //    TryUseAbility(ThunderClap, 20);

            //    TryUseAbility(Overpower, 5, overpowerStopwatch.IsRunning);
            //}
            //else
            if (ObjectManager.Player.CurrentStance != DefensiveStance)
            {
                TryCastSpell(DefensiveStance);
            }

            TryUseAbility(Revenge, 5, ObjectManager.Player.CurrentStance == DefensiveStance && overpowerStopwatch.IsRunning);

            TryUseAbility(ShieldBash, 10, ObjectManager.GetTarget(ObjectManager.Player).IsCasting && ObjectManager.GetTarget(ObjectManager.Player).Mana > 0);

            TryUseAbility(Rend, 10, !ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(Rend) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50 && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Elemental && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Undead);

            TryUseAbility(ShieldSlam, 20, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 30);

            TryUseAbility(SunderArmor, 15);

            TryUseAbility(HeroicStrike, 40, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 40 && !ObjectManager.Player.IsCasting);

            TryUseAbility(DemoralizingShout, 10, !ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(DemoralizingShout));

            TryUseAbility(LastStand, condition: ObjectManager.Player.HealthPercent <= 8);

            TryUseAbility(Berserking, 5, ObjectManager.Player.CurrentStance == BerserkerStance && ObjectManager.Player.HealthPercent < 30);

            TryUseAbility(BattleShout, 10, !ObjectManager.Player.HasBuff(BattleShout));

            TryUseAbility(ConcussionBlow, 15, !ObjectManager.GetTarget(ObjectManager.Player).IsStunned && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 40);

            TryUseAbility(Execute, 20, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent < 20);

            if (overpowerStopwatch.ElapsedMilliseconds > 5000)
            {
                overpowerStopwatch.Stop();
            }
        }
    }
}
