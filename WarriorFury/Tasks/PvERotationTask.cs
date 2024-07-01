using System.Diagnostics;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;
using static WoWActivityMember.Constants.Enums;

namespace WarriorFury.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private const string BattleStance = "Battle Stance";
        private const string BerserkerStance = "Berserker Stance";
        private const string BattleShout = "Battle Shout";
        private const string BerserkerRage = "Berserker Rage";
        private const string Berserking = "Berserking";
        private const string BloodFury = "Blood Fury";
        private const string Bloodrage = "Bloodrage";
        private const string Bloodthirst = "Bloodthirst";
        private const string Cleave = "Cleave";
        private const string DeathWish = "Death Wish";
        private const string DemoralizingShout = "Demoralizing Shout";
        private const string Execute = "Execute";
        private const string HeroicStrike = "Heroic Strike";
        private const string Overpower = "Overpower";
        private const string Pummel = "Pummel";
        private const string Rend = "Rend";
        private const string Retaliation = "Retaliation";
        private const string Slam = "Slam";
        private const string SunderArmor = "Sunder Armor";
        private const string ThunderClap = "Thunder Clap";
        private const string Hamstring = "Ham String";
        private const string IntimidatingShout = "Intimidating Shout";
        private const string Whirlwind = "Whirlwind";
        private bool slamReady;
        private int slamReadyStartTime;
        private bool backpedaling;
        private int backpedalStartTime;
        private int backpedalDuration;
        private readonly Stopwatch overpowerStopwatch = new();

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            WoWEventHandler.Instance.OnSlamReady += OnSlamReadyCallback;
            WoWEventHandler.Instance.OnBlockParryDodge += Instance_OnBlockParryDodge;
        }

        ~PvERotationTask()
        {
            WoWEventHandler.Instance.OnSlamReady -= OnSlamReadyCallback;
        }

        private void Instance_OnBlockParryDodge(object sender, EventArgs e)
        {
            overpowerStopwatch.Restart();
        }

        public void Update()
        {
            if (Environment.TickCount - backpedalStartTime > backpedalDuration)
            {
                ObjectManager.Player.StopMovement(ControlBits.Back);
                // ObjectManager.Player.StopMovement(ControlBits.StrafeLeft);
                // ObjectManager.Player.StopMovement(ControlBits.Right);
                backpedaling = false;
            }

            if (backpedaling)
                return;

            if (Environment.TickCount - slamReadyStartTime > 250)
            {
                slamReady = false;
            }

            //if (!FacingAllTargets && ObjectManager.Aggressors.Count() >= 2 && AggressorsInMelee)
            //{
            //    WalkBack(50);
            //    return;
            //}

            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(5))
                return;

            string currentStance = ObjectManager.Player.CurrentStance;
            IEnumerable<WoWUnit> spellcastingAggressors = ObjectManager.Aggressors
                .Where(a => a.Mana > 0);
            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(BerserkerStance, condition: ObjectManager.Player.Level >= 30 && currentStance == BattleStance && (ObjectManager.Player.Target.HasDebuff(Rend) || ObjectManager.Player.Target.HealthPercent < 80 || ObjectManager.Player.Target.CreatureType == CreatureType.Elemental || ObjectManager.Player.Target.CreatureType == CreatureType.Undead));

            TryUseAbility(Pummel, 10, currentStance == BerserkerStance && ObjectManager.Player.Target.Mana > 0 && (ObjectManager.Player.Target.IsCasting || ObjectManager.Player.Target.IsChanneling));

            // TryUseAbility(Rend, 10, (currentStance == BattleStance && ObjectManager.Player.Target.HealthPercent > 50 && !ObjectManager.Player.Target.HasDebuff(Rend) && (ObjectManager.Player.Target.CreatureType != CreatureType.Elemental && ObjectManager.Player.Target.CreatureType != CreatureType.Undead)));

            TryUseAbility(DeathWish, 10, ObjectManager.Player.IsSpellReady(DeathWish) && ObjectManager.Player.Target.HealthPercent > 80);

            TryUseAbility(BattleShout, 10, !ObjectManager.Player.HasBuff(BattleShout));

            TryUseAbilityById(BloodFury, 4, 0, ObjectManager.Player.Target.HealthPercent > 80);

            TryUseAbility(Bloodrage, condition: ObjectManager.Player.Target.HealthPercent > 50);

            TryUseAbility(Execute, 15, ObjectManager.Player.Target.HealthPercent < 20);

            TryUseAbility(BerserkerRage, condition: ObjectManager.Player.Target.HealthPercent > 70 && currentStance == BerserkerStance);

            TryUseAbility(Overpower, 5, currentStance == BattleStance && overpowerStopwatch.IsRunning);

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (ObjectManager.Aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(ObjectManager.Player.Target.HasDebuff(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10));

                TryUseAbility(DemoralizingShout, 10, !ObjectManager.Player.Target.HasDebuff(DemoralizingShout));

                // TryUseAbility(Cleave, 20, ObjectManager.Player.Target.HealthPercent > 20 && FacingAllTargets);

                TryUseAbility(Whirlwind, 25, ObjectManager.Player.Target.HealthPercent > 20 && currentStance == BerserkerStance && !ObjectManager.Player.Target.HasDebuff(IntimidatingShout) && AggressorsInMelee);

                // if our ObjectManager.Player.Target uses melee, but there's a caster attacking us, do not use retaliation
                TryUseAbility(Retaliation, 0, ObjectManager.Player.IsSpellReady(Retaliation) && spellcastingAggressors.Count() == 0 && currentStance == BattleStance && FacingAllTargets && !ObjectManager.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));
            }

            // Use these abilities if you are fighting only one mob at a time, or multiple and one or more are not in melee range.
            if (ObjectManager.Aggressors.Count() >= 1 || (ObjectManager.Aggressors.Count() > 1 && !AggressorsInMelee))
            {
                TryUseAbility(Slam, 15, ObjectManager.Player.Target.HealthPercent > 20 && slamReady, SlamCallback);
                
                // TryUseAbility(Rend, 10, (currentStance == BattleStance && ObjectManager.Player.Target.HealthPercent > 50 && !ObjectManager.Player.Target.HasDebuff(Rend) && (ObjectManager.Player.Target.CreatureType != CreatureType.Elemental && ObjectManager.Player.Target.CreatureType != CreatureType.Undead)));

                TryUseAbility(Bloodthirst, 30);

                TryUseAbility(Hamstring, 10, ObjectManager.Player.Target.CreatureType == CreatureType.Humanoid && !ObjectManager.Player.Target.HasDebuff(Hamstring));

                TryUseAbility(HeroicStrike, ObjectManager.Player.Level < 30 ? 15 : 45, ObjectManager.Player.Target.HealthPercent > 30);

                TryUseAbility(Execute, 15, ObjectManager.Player.Target.HealthPercent < 20);

                TryUseAbility(SunderArmor, 15, ObjectManager.Player.Target.HealthPercent < 80 && !ObjectManager.Player.Target.HasDebuff(SunderArmor));
            }

            if (overpowerStopwatch.ElapsedMilliseconds > 5000)
            {
                overpowerStopwatch.Stop();
            }
        }

        private void OnSlamReadyCallback(object sender, EventArgs e)
        {
            OnSlamReady();
        }

        private void OnSlamReady()
        {
            slamReady = true;
            slamReadyStartTime = Environment.TickCount;
        }

        private void SlamCallback()
        {
            slamReady = false;
        }

        // Check to see if toon is facing all the ObjectManager.Player.Targets and they are within melee, used to determine if player should walkbackwards to reposition ObjectManager.Player.Targets in front of mob.
        private bool FacingAllTargets
        {
            get
            {
                return ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 7);
            }
        }

        // Check to see if toon is with melee distance of mobs.  This is used to determine if player should use single mob rotation or multi-mob rotation.
        private bool AggressorsInMelee
        {
            get
            {
                return ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 7);
            }
        }

        private void WalkBack(int milleseconds)
        {
            backpedaling = true;
            backpedalStartTime = Environment.TickCount;
            backpedalDuration = milleseconds;
            ObjectManager.Player.StartMovement(ControlBits.Back);
            // ObjectManager.Player.StartMovement(ControlBits.StrafeLeft);
            // ObjectManager.Player.StartMovement(ControlBits.Right);
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }
    }
}
