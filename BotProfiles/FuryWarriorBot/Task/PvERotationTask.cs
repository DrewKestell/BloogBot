using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
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

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        WoWUnit target;
        readonly LocalPlayer player;

        bool slamReady;
        int slamReadyStartTime;

        bool losBackpedaling;
        int losBackpedalStartTime;

        bool backpedaling;
        int backpedalStartTime;
        int backpedalDuration;

        bool initialized;
        int combatStateEnterTime = Environment.TickCount;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;

            WoWEventHandler.Instance.OnSlamReady += OnSlamReadyCallback;
        }

        ~PvERotationTask()
        {
            WoWEventHandler.Instance.OnSlamReady -= OnSlamReadyCallback;
        }

        public void Update()
        {
            if (Environment.TickCount - backpedalStartTime > backpedalDuration)
            {
                Container.Player.StopMovement(ControlBits.Back);
                // Container.Player.StopMovement(ControlBits.StrafeLeft);
                // Container.Player.StopMovement(ControlBits.Right);
                backpedaling = false;
            }

            if (backpedaling)
                return;

            if (Environment.TickCount - slamReadyStartTime > 250)
            {
                slamReady = false;
            }

            //if (!FacingAllTargets && ObjectManager.Instance.Aggressors.Count() >= 2 && AggressorsInMelee)
            //{
            //    WalkBack(50);
            //    return;
            //}

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 5))
                return;

            var currentStance = Container.Player.CurrentStance;
            var spellcastingAggressors = ObjectManager.Instance.Aggressors
                .Where(a => a.Mana > 0);
            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(BerserkerStance, condition: Container.Player.Level >= 30 && currentStance == BattleStance && (target.HasDebuff(Rend) || Container.HostileTarget.HealthPercent < 80 || Container.HostileTarget.CreatureType == CreatureType.Elemental || Container.HostileTarget.CreatureType == CreatureType.Undead));

            TryUseAbility(Pummel, 10, currentStance == BerserkerStance && Container.HostileTarget.Mana > 0 && (target.IsCasting || Container.HostileTarget.IsChanneling));

            // TryUseAbility(Rend, 10, (currentStance == BattleStance && Container.HostileTarget.HealthPercent > 50 && !target.HasDebuff(Rend) && (target.CreatureType != CreatureType.Elemental && Container.HostileTarget.CreatureType != CreatureType.Undead)));

            TryUseAbility(DeathWish, 10, Spellbook.Instance.IsSpellReady(DeathWish) && Container.HostileTarget.HealthPercent > 80);

            TryUseAbility(BattleShout, 10, !Container.Player.HasBuff(BattleShout));

            TryUseAbilityById(BloodFury, 4, 0, Container.HostileTarget.HealthPercent > 80);

            TryUseAbility(Bloodrage, condition: Container.HostileTarget.HealthPercent > 50);

            TryUseAbility(Execute, 15, Container.HostileTarget.HealthPercent < 20);

            TryUseAbility(BerserkerRage, condition: Container.HostileTarget.HealthPercent > 70 && currentStance == BerserkerStance);

            TryUseAbility(Overpower, 5, currentStance == BattleStance && Container.Player.CanOverpower);

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (ObjectManager.Instance.Aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(target.HasDebuff(IntimidatingShout) || Container.Player.HasBuff(Retaliation)) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(Container.Player.Location) < 10));

                TryUseAbility(DemoralizingShout, 10, !target.HasDebuff(DemoralizingShout));

                // TryUseAbility(Cleave, 20, Container.HostileTarget.HealthPercent > 20 && FacingAllTargets);

                TryUseAbility(Whirlwind, 25, Container.HostileTarget.HealthPercent > 20 && currentStance == BerserkerStance && !target.HasDebuff(IntimidatingShout) && AggressorsInMelee);

                // if our target uses melee, but there's a caster attacking us, do not use retaliation
                TryUseAbility(Retaliation, 0, Spellbook.Instance.IsSpellReady(Retaliation) && spellcastingAggressors.Count() == 0 && currentStance == BattleStance && FacingAllTargets && !ObjectManager.Instance.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));
            }

            // Use these abilities if you are fighting only one mob at a time, or multiple and one or more are not in melee range.
            if (ObjectManager.Instance.Aggressors.Count() >= 1 || (ObjectManager.Instance.Aggressors.Count() > 1 && !AggressorsInMelee))
            {
                TryUseAbility(Slam, 15, Container.HostileTarget.HealthPercent > 20 && slamReady, SlamCallback);
                
                // TryUseAbility(Rend, 10, (currentStance == BattleStance && Container.HostileTarget.HealthPercent > 50 && !target.HasDebuff(Rend) && (target.CreatureType != CreatureType.Elemental && Container.HostileTarget.CreatureType != CreatureType.Undead)));

                TryUseAbility(Bloodthirst, 30);

                TryUseAbility(Hamstring, 10, Container.HostileTarget.CreatureType == CreatureType.Humanoid && !target.HasDebuff(Hamstring));

                TryUseAbility(HeroicStrike, Container.Player.Level < 30 ? 15 : 45, Container.HostileTarget.HealthPercent > 30);

                TryUseAbility(Execute, 15, Container.HostileTarget.HealthPercent < 20);

                TryUseAbility(SunderArmor, 15, Container.HostileTarget.HealthPercent < 80 && !target.HasDebuff(SunderArmor));
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

        // Check to see if toon is facing all the targets and they are within melee, used to determine if player should walkbackwards to reposition targets in front of mob.
        bool FacingAllTargets
        {
            get
            {
                return ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(Container.Player.Location) < 7);
            }
        }

        // Check to see if toon is with melee distance of mobs.  This is used to determine if player should use single mob rotation or multi-mob rotation.
        bool AggressorsInMelee
        {
            get
            {
                return ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(Container.Player.Location) < 7);
            }
        }

        void WalkBack(int milleseconds)
        {
            backpedaling = true;
            backpedalStartTime = Environment.TickCount;
            backpedalDuration = milleseconds;
            Container.Player.StartMovement(ControlBits.Back);
            // Container.Player.StartMovement(ControlBits.StrafeLeft);
            // Container.Player.StartMovement(ControlBits.Right);
        }
    }
}
