using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI
{
    public class DependencyContainer : IDependencyContainer
    {
        static readonly string[] oozeNames = { "Acidic Swamp Ooze", "Black Slime", "Cloned Ectoplasm", "Cloned Ooze", "Corrosive Sap Beast", "Corrosive Swamp Ooze",
            "Cursed Ooze", "Devouring Ectoplasm", "Evolving Ectoplasm", "Gargantuan Ooze", "Glob of Viscidus", "Glutinous Ooze", "Green Sludge", "Irradiated Slime",
            "Jade Ooze", "Muculent Ooze", "Nightmare Ectoplasm", "Noxious Slime", "Plague Slime", "Primal Ooze", "Rotting Slime", "Sap Beast", "Silty Oozeling",
            "Tainted Ooze", "Vile Slime", "The Rot", "Viscidus", "The Ongar" };

        readonly Func<WoWUnit, bool> targetingCriteria;

        public DependencyContainer(
            Func<WoWUnit, bool> targetingCriteria,
            Func<Stack<IBotState>, IDependencyContainer, IBotState> createRestState,
            Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> createMoveToTargetState,
            BotSettings botSettings,
            InstanceUpdate probe)
        {
            this.targetingCriteria = targetingCriteria;

            CreateRestState = createRestState;
            CreateMoveToTargetState = createMoveToTargetState;
            BotSettings = botSettings;
            Probe = probe;
        }

        public Func<Stack<IBotState>, IDependencyContainer, IBotState> CreateRestState { get; }

        public Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> CreateMoveToTargetState { get; }

        public BotSettings BotSettings { get; }

        public InstanceUpdate Probe { get; }

        // this is broken up into multiple sub-expressions to improve readability and debuggability
        public WoWUnit FindThreat()
        {
            var potentialThreats = ObjectManager.Units
                .Where(u =>
                    u.TargetGuid == ObjectManager.Player.Guid ||
                    u.TargetGuid == ObjectManager.Pet?.Guid);

            if (potentialThreats.Any())
                return potentialThreats.First();

            // find totems (these disrupt resting between combat, so kill 'em)
            potentialThreats = ObjectManager.Units
                .Where(u =>
                    u.CreatureType == CreatureType.Totem &&
                    u.Position.DistanceTo(ObjectManager.Player.Position) <= 20 &&
                    u.UnitReaction == UnitReaction.Hostile);

            if (potentialThreats.Any())
                return potentialThreats.First();

            // find stoneclaw totems? for some reason the above will not find these.
            potentialThreats = ObjectManager.Units
                .Where(u =>
                    u.Position.DistanceTo(ObjectManager.Player.Position) < 10 &&
                    Convert.ToBoolean(ObjectManager.Units.FirstOrDefault(ou => ou.Guid == u.TargetGuid)?.Name?.Contains("Stoneclaw Totem")) &&
                    u.IsInCombat);

            if (potentialThreats.Any())
                return potentialThreats.First();

            return null;
        }

        // this is broken up into multiple sub-expressions to improve readability and debuggability
        public WoWUnit FindClosestTarget()
        {
            var threat = FindThreat();
            if (threat != null)
                return threat;

            var potentialTargetsList = ObjectManager.Units
                // only consider units that are not null, and whose name and position are not null
                .Where(u => u != null && u.Name != null && u.Position != null)
                // only consider living units whose health is > 0
                .Where(u => u.Health > 0)
                // only consider units that have not already been tapped by another played
                .Where(u => !u.TappedByOther)
                // exclude units that are pets of another unit
                .Where(u => !u.IsPet)
                // exclude elites, unless their names have been explicitly included in the targeting settings
                //.Where(u => u.CreatureRank == CreatureRank.Normal || BotSettings.TargetingIncludedNames.Any(n => u.Name != null && u.Name.Contains(n)))
                //// if included targets are specified, only consider units part of that list
                //.Where(u => string.IsNullOrWhiteSpace(BotSettings.TargetingIncludedNames) || BotSettings.TargetingIncludedNames.Split('|').Any(m => u.Name != null && u.Name.Contains(m)))
                //// if excluded targets are specified, do not consider units part of that list
                //.Where(u => string.IsNullOrWhiteSpace(BotSettings.TargetingExcludedNames) || !BotSettings.TargetingExcludedNames.Split('|').Any(m => u.Name != null && u.Name.Contains(m)))
                //// filter units by unit reactions as specified in targeting settings
                //.Where(u => BotSettings.UnitReactions.Count == 0 || BotSettings.UnitReactions.Contains(u.UnitReaction.ToString()))
                //// filter units by creature type as specified in targeting settings. also include things like totems and slimes.
                //.Where(u => BotSettings.CreatureTypes.Count == 0 || u.CreatureType == CreatureType.Mechanical || (u.CreatureType == CreatureType.Totem && u.Position.DistanceTo(ObjectManager.Player?.Position) <= 20) || BotSettings.CreatureTypes.Contains(u.CreatureType.ToString()) || oozeNames.Contains(u.Name))
                //// filter by the level range specified in targeting settings
                //.Where(u => u.Level <= ObjectManager.Player?.Level + BotSettings.LevelRangeMax && u.Level >= ObjectManager.Player?.Level - BotSettings.LevelRangeMin)
                // exclude certain factions known to cause targeting issues (like neutral, non attackable NPCs in town)
                .Where(u => u.FactionId != 71 && u.FactionId != 85 && u.FactionId != 474 && u.FactionId != 475 && u.FactionId != 1475)
                // exclude units with the UNIT_FLAG_NON_ATTACKABLE flag
                .Where(u => u.UnitFlags != UnitFlags.UNIT_FLAG_NON_ATTACKABLE)
                // apply bot profile specific targeting criteria
                .Where(u => targetingCriteria(u))
                .ToList();

            var potentialTargets = potentialTargetsList
                .OrderBy(u => u.Position.DistanceTo(ObjectManager.Player?.Position));

            return potentialTargets.FirstOrDefault();
        }

        public bool RunningErrands { get; set; }
        public string AccountName { get; set; }
    }
}
