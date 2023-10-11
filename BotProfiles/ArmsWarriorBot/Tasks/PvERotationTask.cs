using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ArmsWarriorBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                botTasks.Pop();
                return;
            }

            if (target == null || target.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(Bloodrage, condition: target.HealthPercent > 50);

            TryUseAbilityById(BloodFury, 4, condition: target.HealthPercent > 80);

            TryUseAbility(Overpower, 5, player.CanOverpower);

            TryUseAbility(Execute, 15, target.HealthPercent < 20);

            // Use these abilities if you are fighting exactly one mob.
            if (ObjectManager.Instance.Aggressors.Count() == 1)
            {
                TryUseAbility(Hamstring, 10, (target.Name.Contains("Plainstrider") || target.CreatureType == CreatureType.Humanoid) && target.HealthPercent < 30 && !target.HasDebuff(Hamstring));

                TryUseAbility(BattleShout, 10, !player.HasBuff(BattleShout));

                TryUseAbility(Rend, 10, target.HealthPercent > 50 && !target.HasDebuff(Rend) && target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead);

                var sunderDebuff = target.GetDebuffs().FirstOrDefault(f => f.Icon == SunderArmorIcon);
                TryUseAbility(SunderArmor, 15, (sunderDebuff == null || sunderDebuff.StackCount < 5) && target.Level >= player.Level - 2 && target.Health > 40 && SunderTargets.Any(s => target.Name.Contains(s)));

                TryUseAbility(MortalStrike, 30);

                TryUseAbility(HeroicStrike, player.Level < 30 ? 15 : 45, target.HealthPercent > 30);
            }

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (ObjectManager.Instance.Aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(target.HasDebuff(IntimidatingShout) || player.HasBuff(Retaliation)) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(player.Location) < 10) && !ObjectManager.Instance.Units.Any(u => u.Guid != target.Guid && u.Location.GetDistanceTo(player.Location) < 10 && u.Reaction == UnitReaction.Neutral));

                TryUseAbility(Retaliation, 0, Spellbook.Instance.IsSpellReady(Retaliation) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(player.Location) < 10) && !ObjectManager.Instance.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));

                TryUseAbility(DemoralizingShout, 10, ObjectManager.Instance.Aggressors.Any(a => !a.HasDebuff(DemoralizingShout) && a.HealthPercent > 50) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(player.Location) < 10) && (!Spellbook.Instance.IsSpellReady(IntimidatingShout) || player.HasBuff(Retaliation)) && !ObjectManager.Instance.Units.Any(u => (u.Guid != target.Guid && u.Location.GetDistanceTo(player.Location) < 10 && u.Reaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(ThunderClap, 20, ObjectManager.Instance.Aggressors.Any(a => !a.HasDebuff(ThunderClap) && a.HealthPercent > 50) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(player.Location) < 8) && (!Spellbook.Instance.IsSpellReady(IntimidatingShout) || player.HasBuff(Retaliation)) && !ObjectManager.Instance.Units.Any(u => (u.Guid != target.Guid && u.Location.GetDistanceTo(player.Location) < 8 && u.Reaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(SweepingStrikes, 30, !player.HasBuff(SweepingStrikes) && target.HealthPercent > 30);

                var thunderClapCondition = target.HasDebuff(ThunderClap) || !Spellbook.Instance.IsSpellReady(ThunderClap) || target.HealthPercent < 50;
                var demoShoutCondition = target.HasDebuff(DemoralizingShout) || !Spellbook.Instance.IsSpellReady(DemoralizingShout) || target.HealthPercent < 50;
                var sweepingStrikesCondition = player.HasBuff(SweepingStrikes) || !Spellbook.Instance.IsSpellReady(SweepingStrikes);
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
