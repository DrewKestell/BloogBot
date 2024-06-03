using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace FuryWarriorBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string BattleStance = "Battle Stance";
        const string BerserkerStance = "Berserker Stance";

        const string BattleShout = "Battle Shout";
        const string BerserkerRage = "Berserker Rage";
        const string Berserking = "Berserking";
        const string BloodFury = "Blood Fury";
        const string Bloodrage = "Bloodrage";
        const string Bloodthirst = "Bloodthirst";
        const string Cleave = "Cleave";
        const string DeathWish = "Death Wish";
        const string DemoralizingShout = "Demoralizing Shout";
        const string Execute = "Execute";
        const string HeroicStrike = "Heroic Strike";
        const string Overpower = "Overpower";
        const string Pummel = "Pummel";
        const string Rend = "Rend";
        const string Retaliation = "Retaliation";
        const string Slam = "Slam";
        const string SunderArmor = "Sunder Armor";
        const string ThunderClap = "Thunder Clap";
        const string Hamstring = "Ham String";
        const string IntimidatingShout = "Intimidating Shout";
        const string Whirlwind = "Whirlwind";

        bool slamReady;
        int slamReadyStartTime;

        bool backpedaling;
        int backpedalStartTime;
        int backpedalDuration;

        readonly Stopwatch overpowerStopwatch = new Stopwatch();

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

        void OnSlamReadyCallback(object sender, EventArgs e)
        {
            OnSlamReady();
        }

        void OnSlamReady()
        {
            slamReady = true;
            slamReadyStartTime = Environment.TickCount;
        }

        void SlamCallback()
        {
            slamReady = false;
        }

        // Check to see if toon is facing all the ObjectManager.Player.Targets and they are within melee, used to determine if player should walkbackwards to reposition ObjectManager.Player.Targets in front of mob.
        bool FacingAllTargets
        {
            get
            {
                return ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 7);
            }
        }

        // Check to see if toon is with melee distance of mobs.  This is used to determine if player should use single mob rotation or multi-mob rotation.
        bool AggressorsInMelee
        {
            get
            {
                return ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 7);
            }
        }

        void WalkBack(int milleseconds)
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
