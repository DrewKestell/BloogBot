using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using static WoWActivityMember.Constants.Enums;

namespace WarriorArms.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private static readonly string[] SunderTargets = [ "Snapjaw", "Snapper", "Tortoise", "Spikeshell", "Burrower", "Borer", // turtles
            "Bear", "Grizzly", "Ashclaw", "Mauler", "Shardtooth", "Plaguebear", "Bristlefur", "Thistlefur", // bears
            "Scorpid", "Flayer", "Stinger", "Lasher", "Pincer", // scorpids
            "Crocolisk", "Vicejaw", "Deadmire", "Snapper", "Daggermaw", // crocs
            "Crawler", "Crustacean", // crabs
            "Stag" ]; // other

        private const string SunderArmorIcon = "Interface\\Icons\\Ability_Warrior_Sunder";
        private const string BattleShout = "Battle Shout";
        private const string Bloodrage = "Bloodrage";
        private const string BloodFury = "Blood Fury";
        private const string DemoralizingShout = "Demoralizing Shout";
        private const string Execute = "Execute";
        private const string Hamstring = "Hamstring";
        private const string HeroicStrike = "Heroic Strike";
        private const string MortalStrike = "Mortal Strike";
        private const string Overpower = "Overpower";
        private const string Rend = "Rend";
        private const string Retaliation = "Retaliation";
        private const string SunderArmor = "Sunder Armor";
        private const string SweepingStrikes = "Sweeping Strikes";
        private const string ThunderClap = "Thunder Clap";
        private const string IntimidatingShout = "Intimidating Shout";

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(3))
                return;

            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(Bloodrage, condition: ObjectManager.Player.Target.HealthPercent > 50);

            TryUseAbilityById(BloodFury, 4, condition: ObjectManager.Player.Target.HealthPercent > 80);

            //TryUseAbility(Overpower, 5, ObjectManager.Player.Ca);

            TryUseAbility(Execute, 15, ObjectManager.Player.Target.HealthPercent < 20);

            // Use these abilities if you are fighting exactly one mob.
            if (ObjectManager.Aggressors.Count() == 1)
            {
                TryUseAbility(Hamstring, 10, (ObjectManager.Player.Target.Name.Contains("Plainstrider") || ObjectManager.Player.Target.CreatureType == CreatureType.Humanoid) && ObjectManager.Player.Target.HealthPercent < 30 && !ObjectManager.Player.Target.HasDebuff(Hamstring));

                TryUseAbility(BattleShout, 10, !ObjectManager.Player.HasBuff(BattleShout));

                TryUseAbility(Rend, 10, ObjectManager.Player.Target.HealthPercent > 50 && !ObjectManager.Player.Target.HasDebuff(Rend) && ObjectManager.Player.Target.CreatureType != CreatureType.Elemental && ObjectManager.Player.Target.CreatureType != CreatureType.Undead);

                SpellEffect sunderDebuff = ObjectManager.Player.Target.GetDebuffs(LuaTarget.Target).FirstOrDefault(f => f.Icon == SunderArmorIcon);
                TryUseAbility(SunderArmor, 15, (sunderDebuff == null || sunderDebuff.StackCount < 5) && ObjectManager.Player.Target.Level >= ObjectManager.Player.Level - 2 && ObjectManager.Player.Target.Health > 40 && SunderTargets.Any(s => ObjectManager.Player.Target.Name.Contains(s)));

                TryUseAbility(MortalStrike, 30);

                TryUseAbility(HeroicStrike, ObjectManager.Player.Level < 30 ? 15 : 45, ObjectManager.Player.Target.HealthPercent > 30);
            }

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (ObjectManager.Aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(ObjectManager.Player.Target.HasDebuff(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10) && !ObjectManager.Units.Any(u => u.Guid != ObjectManager.Player.TargetGuid && u.Position.DistanceTo(ObjectManager.Player.Position) < 10 && u.UnitReaction == UnitReaction.Neutral));

                TryUseAbility(Retaliation, 0, ObjectManager.Player.IsSpellReady(Retaliation) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10) && !ObjectManager.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));

                TryUseAbility(DemoralizingShout, 10, ObjectManager.Aggressors.Any(a => !a.HasDebuff(DemoralizingShout) && a.HealthPercent > 50) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10) && (!ObjectManager.Player.IsSpellReady(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && !ObjectManager.Units.Any(u => (u.Guid != ObjectManager.Player.TargetGuid && u.Position.DistanceTo(ObjectManager.Player.Position) < 10 && u.UnitReaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(ThunderClap, 20, ObjectManager.Aggressors.Any(a => !a.HasDebuff(ThunderClap) && a.HealthPercent > 50) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 8) && (!ObjectManager.Player.IsSpellReady(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && !ObjectManager.Units.Any(u => (u.Guid != ObjectManager.Player.TargetGuid && u.Position.DistanceTo(ObjectManager.Player.Position) < 8 && u.UnitReaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(SweepingStrikes, 30, !ObjectManager.Player.HasBuff(SweepingStrikes) && ObjectManager.Player.Target.HealthPercent > 30);

                bool thunderClapCondition = ObjectManager.Player.Target.HasDebuff(ThunderClap) || !ObjectManager.Player.IsSpellReady(ThunderClap) || ObjectManager.Player.Target.HealthPercent < 50;
                bool demoShoutCondition = ObjectManager.Player.Target.HasDebuff(DemoralizingShout) || !ObjectManager.Player.IsSpellReady(DemoralizingShout) || ObjectManager.Player.Target.HealthPercent < 50;
                bool sweepingStrikesCondition = ObjectManager.Player.HasBuff(SweepingStrikes) || !ObjectManager.Player.IsSpellReady(SweepingStrikes);
                if (thunderClapCondition && demoShoutCondition && sweepingStrikesCondition)
                {
                    TryUseAbility(Rend, 10, ObjectManager.Player.Target.HealthPercent > 50 && !ObjectManager.Player.Target.HasDebuff(Rend) && ObjectManager.Player.Target.CreatureType != CreatureType.Elemental && ObjectManager.Player.Target.CreatureType != CreatureType.Undead);

                    TryUseAbility(MortalStrike, 30);

                    TryUseAbility(HeroicStrike, ObjectManager.Player.Level < 30 ? 15 : 45, ObjectManager.Player.Target.HealthPercent > 30);
                }
            }
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }
    }
}
