using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FuryWarriorBot
{
    class CombatState : CombatStateBase, IBotState
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

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
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

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 5)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            this.target = target;

            WoWEventHandler.OnSlamReady += OnSlamReadyCallback;
        }

        ~CombatState()
        {
            WoWEventHandler.OnSlamReady -= OnSlamReadyCallback;
        }

        public new void Update()
        {
            if (Environment.TickCount - backpedalStartTime > backpedalDuration)
            {
                player.StopMovement(ControlBits.Back);
                // player.StopMovement(ControlBits.StrafeLeft);
                // player.StopMovement(ControlBits.Right);
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

            if (base.Update())
                return;

            var currentStance = player.CurrentStance;
            var spellcastingAggressors = ObjectManager.Aggressors
                .Where(a => a.Mana > 0);
            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(BerserkerStance, condition: player.Level >= 30 && currentStance == BattleStance && (target.HasDebuff(Rend) || target.HealthPercent < 80 || target.CreatureType == CreatureType.Elemental || target.CreatureType == CreatureType.Undead));

            TryUseAbility(Pummel, 10, currentStance == BerserkerStance && target.Mana > 0 && (target.IsCasting || target.IsChanneling));

            // TryUseAbility(Rend, 10, (currentStance == BattleStance && target.HealthPercent > 50 && !target.HasDebuff(Rend) && (target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead)));

            TryUseAbility(DeathWish, 10, player.IsSpellReady(DeathWish) && target.HealthPercent > 80);

            TryUseAbility(BattleShout, 10, !player.HasBuff(BattleShout));

            TryUseAbilityById(BloodFury, 4, 0, target.HealthPercent > 80);

            TryUseAbility(Bloodrage, condition: target.HealthPercent > 50);

            TryUseAbility(Execute, 15, target.HealthPercent < 20);

            TryUseAbility(BerserkerRage, condition: target.HealthPercent > 70 && currentStance == BerserkerStance);

            TryUseAbility(Overpower, 5, currentStance == BattleStance && player.CanOverpower);

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (ObjectManager.Aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(target.HasDebuff(IntimidatingShout) || player.HasBuff(Retaliation)) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(player.Position) < 10));

                TryUseAbility(DemoralizingShout, 10, !target.HasDebuff(DemoralizingShout));

                // TryUseAbility(Cleave, 20, target.HealthPercent > 20 && FacingAllTargets);

                TryUseAbility(Whirlwind, 25, target.HealthPercent > 20 && currentStance == BerserkerStance && !target.HasDebuff(IntimidatingShout) && AggressorsInMelee);

                // if our target uses melee, but there's a caster attacking us, do not use retaliation
                TryUseAbility(Retaliation, 0, player.IsSpellReady(Retaliation) && spellcastingAggressors.Count() == 0 && currentStance == BattleStance && FacingAllTargets && !ObjectManager.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));
            }

            // Use these abilities if you are fighting only one mob at a time, or multiple and one or more are not in melee range.
            if (ObjectManager.Aggressors.Count() >= 1 || (ObjectManager.Aggressors.Count() > 1 && !AggressorsInMelee))
            {
                TryUseAbility(Slam, 15, target.HealthPercent > 20 && slamReady, SlamCallback);
                
                // TryUseAbility(Rend, 10, (currentStance == BattleStance && target.HealthPercent > 50 && !target.HasDebuff(Rend) && (target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead)));

                TryUseAbility(Bloodthirst, 30);

                TryUseAbility(Hamstring, 10, target.CreatureType == CreatureType.Humanoid && !target.HasDebuff(Hamstring));

                TryUseAbility(HeroicStrike, player.Level < 30 ? 15 : 45, target.HealthPercent > 30);

                TryUseAbility(Execute, 15, target.HealthPercent < 20);

                TryUseAbility(SunderArmor, 15, target.HealthPercent < 80 && !target.HasDebuff(SunderArmor));
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
                return ObjectManager.Aggressors.All(a => a.Position.DistanceTo(player.Position) < 7 && player.IsInCleave(a.Position));
            }
        }

        // Check to see if toon is with melee distance of mobs.  This is used to determine if player should use single mob rotation or multi-mob rotation.
        bool AggressorsInMelee
        {
            get
            {
                return ObjectManager.Aggressors.All(a => a.Position.DistanceTo(player.Position) < 7);
            }
        }

        void WalkBack(int milleseconds)
        {
            backpedaling = true;
            backpedalStartTime = Environment.TickCount;
            backpedalDuration = milleseconds;
            player.StartMovement(ControlBits.Back);
            // player.StartMovement(ControlBits.StrafeLeft);
            // player.StartMovement(ControlBits.Right);
        }
    }
}
