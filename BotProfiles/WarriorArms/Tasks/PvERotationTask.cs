using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

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

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count() == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.GetTarget(ObjectManager.Player) == null || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(3))
                return;

            // Use these abilities when fighting any number of mobs.   
            TryUseAbility(Bloodrage, condition: ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50);

            TryUseAbilityById(BloodFury, 4, condition: ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 80);

            //TryUseAbility(Overpower, 5, ObjectManager.Player.Ca);

            TryUseAbility(Execute, 15, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent < 20);

            // Use these abilities if you are fighting exactly one mob.
            if (ObjectManager.Aggressors.Count() == 1)
            {
                TryUseAbility(Hamstring, 10, (ObjectManager.GetTarget(ObjectManager.Player).Name.Contains("Plainstrider") || ObjectManager.GetTarget(ObjectManager.Player).CreatureType == CreatureType.Humanoid) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent < 30 && !ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(Hamstring));

                TryUseAbility(BattleShout, 10, !ObjectManager.Player.HasBuff(BattleShout));

                TryUseAbility(Rend, 10, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50 && !ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(Rend) && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Elemental && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Undead);

                ISpellEffect sunderDebuff = ObjectManager.GetTarget(ObjectManager.Player).GetDebuffs().FirstOrDefault(f => f.Icon == SunderArmorIcon);
                TryUseAbility(SunderArmor, 15, (sunderDebuff == null || sunderDebuff.StackCount < 5) && ObjectManager.GetTarget(ObjectManager.Player).Level >= ObjectManager.Player.Level - 2 && ObjectManager.GetTarget(ObjectManager.Player).Health > 40 && SunderTargets.Any(s => ObjectManager.GetTarget(ObjectManager.Player).Name.Contains(s)));

                TryUseAbility(MortalStrike, 30);

                TryUseAbility(HeroicStrike, ObjectManager.Player.Level < 30 ? 15 : 45, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 30);
            }

            // Use these abilities if you are fighting TWO OR MORE mobs at once.
            if (ObjectManager.Aggressors.Count() >= 2)
            {
                TryUseAbility(IntimidatingShout, 25, !(ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10) && !ObjectManager.Units.Any(u => u.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid && u.Position.DistanceTo(ObjectManager.Player.Position) < 10 && u.UnitReaction == UnitReaction.Neutral));

                TryUseAbility(Retaliation, 0, ObjectManager.Player.IsSpellReady(Retaliation) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10) && !ObjectManager.Aggressors.Any(a => a.HasDebuff(IntimidatingShout)));

                TryUseAbility(DemoralizingShout, 10, ObjectManager.Aggressors.Any(a => !a.HasDebuff(DemoralizingShout) && a.HealthPercent > 50) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 10) && (!ObjectManager.Player.IsSpellReady(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && !ObjectManager.Units.Any(u => (u.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid && u.Position.DistanceTo(ObjectManager.Player.Position) < 10 && u.UnitReaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(ThunderClap, 20, ObjectManager.Aggressors.Any(a => !a.HasDebuff(ThunderClap) && a.HealthPercent > 50) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(ObjectManager.Player.Position) < 8) && (!ObjectManager.Player.IsSpellReady(IntimidatingShout) || ObjectManager.Player.HasBuff(Retaliation)) && !ObjectManager.Units.Any(u => (u.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid && u.Position.DistanceTo(ObjectManager.Player.Position) < 8 && u.UnitReaction == UnitReaction.Neutral) || u.HasDebuff(IntimidatingShout)));

                TryUseAbility(SweepingStrikes, 30, !ObjectManager.Player.HasBuff(SweepingStrikes) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 30);

                bool thunderClapCondition = ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(ThunderClap) || !ObjectManager.Player.IsSpellReady(ThunderClap) || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent < 50;
                bool demoShoutCondition = ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(DemoralizingShout) || !ObjectManager.Player.IsSpellReady(DemoralizingShout) || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent < 50;
                bool sweepingStrikesCondition = ObjectManager.Player.HasBuff(SweepingStrikes) || !ObjectManager.Player.IsSpellReady(SweepingStrikes);
                if (thunderClapCondition && demoShoutCondition && sweepingStrikesCondition)
                {
                    TryUseAbility(Rend, 10, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50 && !ObjectManager.GetTarget(ObjectManager.Player).HasDebuff(Rend) && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Elemental && ObjectManager.GetTarget(ObjectManager.Player).CreatureType != CreatureType.Undead);

                    TryUseAbility(MortalStrike, 30);

                    TryUseAbility(HeroicStrike, ObjectManager.Player.Level < 30 ? 15 : 45, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 30);
                }
            }
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }
    }
}
