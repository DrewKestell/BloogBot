using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmsWarriorBot
{
    class CombatState : CombatStateBase, IBotState
    {
        static readonly string[] SunderTargets = { "Snapjaw", "Snapper", "Tortoise", "Spikeshell", "Burrower", "Borer", // turtles
            "Bear", "Grizzly", "Ashclaw", "Mauler", "Shardtooth", "Plaguebear", "Bristlefur", "Thistlefur", // bears
            "Scorpid", "Flayer", "Stinger", "Lasher", "Pincer", // scorpids
            "Crocolisk", "Vicejaw", "Deadmire", "Snapper", "Daggermaw", // crocs
            "Crawler", "Crustacean", // crabs
            "Stag" }; // other

        const string SunderArmorIcon = "Interface\\Icons\\Ability_Warrior_Sunder";

        const string BattleShout = "Battle Shout";
        const string Bloodrage = "Bloodrage";
        const string BloodFury = "Blood Fury";
        const string DemoralizingShout = "Demoralizing Shout";
        const string Execute = "Execute";
        const string Hamstring = "Hamstring";
        const string HeroicStrike = "Heroic Strike";
        const string MortalStrike = "Mortal Strike";
        const string Overpower = "Overpower";
        const string Rend = "Rend";
        const string Retaliation = "Retaliation";
        const string SunderArmor = "Sunder Armor";
        const string SweepingStrikes = "Sweeping Strikes";
        const string ThunderClap = "Thunder Clap";
        const string IntimidatingShout = "Intimidating Shout";

        readonly WoWUnit target;
        readonly LocalPlayer player;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 5)
        {
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            var aggressors = ObjectManager.Aggressors.ToList();

            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(Bloodrage, condition: target.HealthPercent > 50);

            TryUseAbilityById(BloodFury, 4, condition: target.HealthPercent > 80);

            TryUseAbility(Overpower, 5, player.CanOverpower);

            TryUseAbility(Execute, 15, target.HealthPercent < 20);

            // Use these abilities if you are fighting exactly one mob.
            if (aggressors.Count() == 1)
            {
                TryUseAbility(Hamstring, 10, (target.Name.Contains("Plainstrider") || target.CreatureType == CreatureType.Humanoid) && target.HealthPercent < 30 && !target.HasDebuff(Hamstring));

                TryUseAbility(BattleShout, 10, !player.HasBuff(BattleShout));

                TryUseAbility(Rend, 10, target.HealthPercent > 50 && !target.HasDebuff(Rend) && target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead);

                var sunderDebuff = target.GetDebuffs(LuaTarget.Target).FirstOrDefault(f => f.Icon == SunderArmorIcon);
                TryUseAbility(SunderArmor, 15, (sunderDebuff == null || sunderDebuff.StackCount < 5) && target.Level >= player.Level - 2 && target.Health > 40 && SunderTargets.Any(s => target.Name.Contains(s)));

                TryUseAbility(MortalStrike, 30);

                TryUseAbility(HeroicStrike, player.Level < 30 ? 15 : 45, target.HealthPercent > 30);
            }

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(target.HasDebuff(IntimidatingShout) || player.HasBuff(Retaliation)) && aggressors.All(a => a.Position.DistanceTo(player.Position) < 10) && !ObjectManager.Units.Any(u => u.Guid != target.Guid && u.Position.DistanceTo(player.Position) < 10 && u.UnitReaction == UnitReaction.Neutral));

                TryUseAbility(Retaliation, 0, player.IsSpellReady(Retaliation) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(player.Position) < 10) && !ObjectManager.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));

                TryUseAbility(DemoralizingShout, 10, aggressors.Any(a => !a.HasDebuff(DemoralizingShout) && a.HealthPercent > 50) && aggressors.All(a => a.Position.DistanceTo(player.Position) < 10) && (!player.IsSpellReady(IntimidatingShout) || player.HasBuff(Retaliation)) && !ObjectManager.Units.Any(u => (u.Guid != target.Guid && u.Position.DistanceTo(player.Position) < 10 && u.UnitReaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(ThunderClap, 20, aggressors.Any(a => !a.HasDebuff(ThunderClap) && a.HealthPercent > 50) && aggressors.All(a => a.Position.DistanceTo(player.Position) < 8) && (!player.IsSpellReady(IntimidatingShout) || player.HasBuff(Retaliation)) && !ObjectManager.Units.Any(u => (u.Guid != target.Guid && u.Position.DistanceTo(player.Position) < 8 && u.UnitReaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(SweepingStrikes, 30, !player.HasBuff(SweepingStrikes) && target.HealthPercent > 30);

                var thunderClapCondition = target.HasDebuff(ThunderClap) || !player.KnowsSpell(ThunderClap) || target.HealthPercent < 50;
                var demoShoutCondition = target.HasDebuff(DemoralizingShout) || !player.KnowsSpell(DemoralizingShout) || target.HealthPercent < 50;
                var sweepingStrikesCondition = player.HasBuff(SweepingStrikes) || !player.IsSpellReady(SweepingStrikes);
                if (thunderClapCondition && demoShoutCondition && sweepingStrikesCondition)
                {
                    TryUseAbility(Rend, 10, target.HealthPercent > 50 && !target.HasDebuff(Rend) && target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead);

                    TryUseAbility(MortalStrike, 30);

                    TryUseAbility(HeroicStrike, player.Level < 30 ? 15 : 45, target.HealthPercent > 30);
                }
            }
        }
    }
}
