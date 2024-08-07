using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace ShamanEnhancement.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (ObjectManager.Player.HealthPercent < 30
                && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HealingWave))
            {
                BotTasks.Push(new HealTask(BotContext));
                return;
            }

            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            AssignDPSTarget();

            if (ObjectManager.Player.Target == null) return;

            //if (ObjectManager.CasterAggressors.Any(x => x.ManaPercent > 0 && !natureImmuneCreatures.Contains(ObjectManager.Player.Target.Name)))
            //{
            //    WoWUnit castingUnit = ObjectManager.CasterAggressors.First(x =>
            //    x.ManaPercent > 0
            //    && !natureImmuneCreatures.Contains(ObjectManager.Player.Target.Name));
            //    WoWUnit nearestHostile = ObjectManager.Hostiles.Where(x => !x.IsInCombat).OrderBy(x => x.Position.DistanceTo(castingUnit.Position)).First();

            //    if (nearestHostile.Position.DistanceTo(castingUnit.Position) > 20)
            //    {
            //        ObjectManager.Player.SetTarget(castingUnit.Guid);

            //        if (ObjectManager.Player.Target == null) return;

            //        if (castingUnit.TargetGuid > 0)
            //        {
            //            if (Update(20))
            //            {
            //                Container.State.Action = "Moving to interrupt";
            //            }
            //            else
            //            {
            //                Container.State.Action = "Interrupting spellcaster";
            //                ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
            //                TryCastSpell(EarthShock, 0, 20, ObjectManager.Player.Target.IsCasting || ObjectManager.Player.Target.IsChanneling);
            //            }
            //        }
            //        else if (MoveBehindTankSpot(45))
            //        {
            //            Container.State.Action = "Has spellcaster aggro/running behind tank spot";
            //        }
            //        else
            //        {
            //            Container.State.Action = "In position to interrupt";
            //            ObjectManager.Player.StopAllMovement();
            //            ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
            //        }
            //    }
            //    else
            //    {
            //        Container.State.Action = "Hostile too close to interrupt spellcaster";
            //    }
            //}
            //else 
            //if (Container.State.TankInPosition)
            //{
            //    if (MoveBehindTarget(3))
            //        return;
            //    else
            //        PerformCombatRotation();
            //}
            //else
            //{
            //    if (MoveTowardsTank())
            //        return;
            //}
        }

        public override void PerformCombatRotation()
        {
            ObjectManager.Player.StopAllMovement();
            ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
            ObjectManager.Player.StartMeleeAttack();

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Aggressors.Any(a => a.IsCasting && ObjectManager.Player.Target.Mana > 0));

            TryCastSpell(TremorTotem, 0, int.MaxValue, FearingCreatures.Contains(ObjectManager.Player.Target.Name) && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            TryCastSpell(WindfuryWeapon, 0, int.MaxValue, !ObjectManager.Player.MainhandIsEnchanted && ObjectManager.Player.IsSpellReady(WindfuryWeapon));

            //TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Aggressors.Count() > 1);

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(StoneskinTotem, 0, int.MaxValue, ObjectManager.Player.Target.Mana == 0 && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneclawTotem) || u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, ObjectManager.Player.Target.HealthPercent > 70 && !FireImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && ObjectManager.Player.Target.Position.DistanceTo(ObjectManager.Player.Position) < 20 && !ObjectManager.Units.Any(u => u.Position.DistanceTo(ObjectManager.Player.Position) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(Stormstrike, 0, 5);

            TryCastSpell(FlameShock, 0, 20, !ObjectManager.Player.Target.HasDebuff(FlameShock) && ObjectManager.Player.Target.HealthPercent > 70 || NatureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && !FireImmuneCreatures.Contains(ObjectManager.Player.Target.Name));

            //TryCastSpell(EarthShock, 0, 20, !natureImmuneCreatures.Contains(target.Name) && !ObjectManager.Player.IsSpellReady(Stormstrike) && ObjectManager.Player.Target.HealthPercent < 70 || ObjectManager.Player.Target.HasDebuff(Stormstrike) || ObjectManager.Player.Target.IsCasting || ObjectManager.Player.Target.IsChanneling || ObjectManager.Player.HasBuff(Clearcasting));

            TryCastSpell(LightningShield, 0, int.MaxValue, !NatureImmuneCreatures.Contains(ObjectManager.Player.Target.Name) && !ObjectManager.Player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, !ObjectManager.Player.MainhandIsEnchanted && ObjectManager.Player.IsSpellReady(RockbiterWeapon) && !ObjectManager.Player.IsSpellReady(FlametongueWeapon) && !ObjectManager.Player.IsSpellReady(WindfuryWeapon));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, !ObjectManager.Player.MainhandIsEnchanted && ObjectManager.Player.IsSpellReady(FlametongueWeapon) && !ObjectManager.Player.IsSpellReady(WindfuryWeapon));
        }
    }
}
