using System.Diagnostics;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using static WoWActivityMember.Constants.Enums;

namespace ProtectionWarriorBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string BattleStance = "Battle Stance";
        const string DefensiveStance = "Defensive Stance";
        const string BerserkerStance = "Berserker Stance";

        const string BattleShout = "Battle Shout";
        const string Berserking = "Berserking";
        const string Bloodrage = "Bloodrage";
        const string ConcussionBlow = "Concussion Blow";
        const string DemoralizingShout = "Demoralizing Shout";
        const string Execute = "Execute";
        const string HeroicStrike = "Heroic Strike";
        const string LastStand = "Last Stand";
        const string Overpower = "Overpower";
        const string Rend = "Rend";
        const string Retaliation = "Retaliation";
        const string Revenge = "Revenge";
        const string ShieldBash = "Shield Bash";
        const string ShieldSlam = "Shield Slam";
        const string SunderArmor = "Sunder Armor";
        const string Taunt = "Taunt";
        const string ThunderClap = "Thunder Clap";

        readonly Stopwatch overpowerStopwatch = new();

        readonly Position tankSpot;
        WoWUnit currentDPSTarget;
        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            WoWEventHandler.Instance.OnBlockParryDodge += Instance_OnBlockParryDodge;
        }
        ~PvERotationTask()
        {
            WoWEventHandler.Instance.OnBlockParryDodge -= Instance_OnBlockParryDodge;
        }

        private void Instance_OnBlockParryDodge(object sender, EventArgs e)
        {
            overpowerStopwatch.Restart();
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            ObjectManager.Player.StartAttack();

            List<WoWUnit> looseUnits = ObjectManager.Aggressors.Where(x => x.TargetGuid != ObjectManager.Player.Guid).OrderBy(x => x.Position.DistanceTo(ObjectManager.Player.Position)).ToList();
            WoWUnit nearestHostile = ObjectManager.Hostiles.Where(x => !x.IsInCombat).OrderBy(x => x.Position.DistanceTo(ObjectManager.Player.Position)).First();

            if (looseUnits.Count > 0)
            {
                WoWUnit looseUnit = looseUnits.First();

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

                    Functions.LuaCall("SetRaidTarget('target', 8)");
                }
                else
                {
                    currentDPSTarget = ObjectManager.Units.First(x => x.Guid == ObjectManager.SkullTargetGuid);
                }

                if (tankSpot.DistanceTo(ObjectManager.Player.Position) > 5)
                {
                    Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, tankSpot, true);

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
            if (ObjectManager.Player.Target == null) return;

            TryUseAbility(Bloodrage, condition: ObjectManager.Player.Target.HealthPercent > 50);

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

            TryUseAbility(ShieldBash, 10, ObjectManager.Player.Target.IsCasting && ObjectManager.Player.Target.Mana > 0);

            TryUseAbility(Rend, 10, !ObjectManager.Player.Target.HasDebuff(Rend) && ObjectManager.Player.Target.HealthPercent > 50 && ObjectManager.Player.Target.CreatureType != CreatureType.Elemental && ObjectManager.Player.Target.CreatureType != CreatureType.Undead);

            TryUseAbility(ShieldSlam, 20, ObjectManager.Player.Target.HealthPercent > 30);

            TryUseAbility(SunderArmor, 15);

            TryUseAbility(HeroicStrike, 40, ObjectManager.Player.Target.HealthPercent > 40 && !ObjectManager.Player.IsCasting);

            TryUseAbility(DemoralizingShout, 10, !ObjectManager.Player.Target.HasDebuff(DemoralizingShout));

            TryUseAbility(LastStand, condition: ObjectManager.Player.HealthPercent <= 8);

            TryUseAbility(Berserking, 5, ObjectManager.Player.CurrentStance == BerserkerStance && ObjectManager.Player.HealthPercent < 30);

            TryUseAbility(BattleShout, 10, !ObjectManager.Player.HasBuff(BattleShout));

            TryUseAbility(ConcussionBlow, 15, !ObjectManager.Player.Target.IsStunned && ObjectManager.Player.Target.HealthPercent > 40);

            TryUseAbility(Execute, 20, ObjectManager.Player.Target.HealthPercent < 20);

            if (overpowerStopwatch.ElapsedMilliseconds > 5000)
            {
                overpowerStopwatch.Stop();
            }
        }
    }
}
