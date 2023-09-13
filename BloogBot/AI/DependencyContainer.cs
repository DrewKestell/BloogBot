using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
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

        IDictionary<string, PlayerTracker> PlayerTrackers { get; } = new Dictionary<string, PlayerTracker>();

        public DependencyContainer(
            Func<WoWUnit, bool> targetingCriteria,
            Func<Stack<IBotState>, IDependencyContainer, IBotState> createRestState,
            Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> createMoveToTargetState,
            Func<Stack<IBotState>, IDependencyContainer, WoWUnit, WoWPlayer, IBotState> createPowerlevelCombatState,
            BotSettings botSettings,
            Probe probe,
            IEnumerable<Hotspot> hotspots)
        {
            this.targetingCriteria = targetingCriteria;

            CreateRestState = createRestState;
            CreateMoveToTargetState = createMoveToTargetState;
            CreatePowerlevelCombatState = createPowerlevelCombatState;
            BotSettings = botSettings;
            Probe = probe;
            Hotspots = hotspots;
        }

        public Func<Stack<IBotState>, IDependencyContainer, IBotState> CreateRestState { get; }

        public Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> CreateMoveToTargetState { get; }

        public Func<Stack<IBotState>, IDependencyContainer, WoWUnit, WoWPlayer, IBotState> CreatePowerlevelCombatState { get; }

        public BotSettings BotSettings { get; }

        public Probe Probe { get; }

        public IEnumerable<Hotspot> Hotspots { get; }

        // this is broken up into multiple sub-expressions to improve readability and debuggability
        public WoWUnit FindThreat()
        {
            var potentialThreats = ObjectManager.Units
                .Where(u =>
                    u.TargetGuid == ObjectManager.Player.Guid ||
                    u.TargetGuid == ObjectManager.Pet?.Guid &&
                    !Probe.BlacklistedMobIds.Contains(u.Guid));

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
                // only consider units that have not been blacklisted
                .Where(u => !Probe?.BlacklistedMobIds?.Contains(u.Guid) ?? true)
                // exclude elites, unless their names have been explicitly included in the targeting settings
                .Where(u => u.CreatureRank == CreatureRank.Normal || BotSettings.TargetingIncludedNames.Any(n => u.Name != null && u.Name.Contains(n)))
                // if included targets are specified, only consider units part of that list
                .Where(u => string.IsNullOrWhiteSpace(BotSettings.TargetingIncludedNames) || BotSettings.TargetingIncludedNames.Split('|').Any(m => u.Name != null && u.Name.Contains(m)))
                // if excluded targets are specified, do not consider units part of that list
                .Where(u => string.IsNullOrWhiteSpace(BotSettings.TargetingExcludedNames) || !BotSettings.TargetingExcludedNames.Split('|').Any(m => u.Name != null && u.Name.Contains(m)))
                // filter units by unit reactions as specified in targeting settings
                .Where(u => BotSettings.UnitReactions.Count == 0 || BotSettings.UnitReactions.Contains(u.UnitReaction.ToString()))
                // filter units by creature type as specified in targeting settings. also include things like totems and slimes.
                .Where(u => BotSettings.CreatureTypes.Count == 0 || u.CreatureType == CreatureType.Mechanical || (u.CreatureType == CreatureType.Totem && u.Position.DistanceTo(ObjectManager.Player?.Position) <= 20) || BotSettings.CreatureTypes.Contains(u.CreatureType.ToString()) || oozeNames.Contains(u.Name))
                // filter by the level range specified in targeting settings
                .Where(u => u.Level <= ObjectManager.Player?.Level + BotSettings.LevelRangeMax && u.Level >= ObjectManager.Player?.Level - BotSettings.LevelRangeMin)
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

        public Hotspot GetCurrentHotspot() => BotSettings.GrindingHotspot;

        public void CheckForTravelPath(Stack<IBotState> botStates, bool reverse, bool needsToRest = true)
        {
            var currentHotspot = BotSettings.GrindingHotspot;
            var travelPath = currentHotspot?.TravelPath;
            if (travelPath != null)
            {
                Position[] waypoints;
                if (reverse)
                    waypoints = travelPath.Waypoints.Reverse().ToArray();
                else
                    waypoints = travelPath.Waypoints;

                var closestTravelPathWaypoint = waypoints.OrderBy(l => l.DistanceTo(ObjectManager.Player.Position)).First();

                Position destination;
                if (reverse)
                    destination = waypoints.Last();
                else
                    destination = currentHotspot.Waypoints.OrderBy(l => l.DistanceTo(ObjectManager.Player.Position)).First();

                if (closestTravelPathWaypoint.DistanceTo(ObjectManager.Player.Position) < destination.DistanceTo(ObjectManager.Player.Position))
                {
                    var startingIndex = waypoints.ToList().IndexOf(closestTravelPathWaypoint);
                    botStates.Push(new TravelState(botStates, this, waypoints, startingIndex));
                    botStates.Push(new MoveToPositionState(botStates, this, closestTravelPathWaypoint));

                    if (reverse && needsToRest)
                        botStates.Push(CreateRestState(botStates, this));
                }
            }
        }

        public bool RunningErrands { get; set; }

        int beepTimer;

        public bool UpdatePlayerTrackers()
        {
            var stopBot = false;

            try
            {
                var me = ObjectManager.Player;
                var otherPlayers = ObjectManager.Players.Where(p => p.Name != me.Name && p.Position.DistanceTo(me.Position) < 80);

                // remove old players
                foreach (var player in PlayerTrackers)
                {
                    if (!otherPlayers.Any(p => p.Name == player.Key))
                        PlayerTrackers.Remove(player);
                }

                // start tracking new players
                foreach (var player in otherPlayers)
                {
                    if (!PlayerTrackers.ContainsKey(player.Name))
                        PlayerTrackers.Add(player.Name, new PlayerTracker(Environment.TickCount));

                    if (PlayerTrackers.ContainsKey(player.Name))
                    {
                        if (player.TargetGuid == me.Guid)
                        {
                            if (!PlayerTrackers[player.Name].TargetingMe)
                            {
                                PlayerTrackers[player.Name].FirstTargetedMe = Environment.TickCount;
                                PlayerTrackers[player.Name].TargetingMe = true;
                            }
                        }
                        else
                        {
                            PlayerTrackers[player.Name].TargetingMe = false;
                        }
                    }
                }

                // check if we're catching heat
                foreach (var player in PlayerTrackers)
                {
                    var targetWarningTriggered = Environment.TickCount - player.Value.FirstTargetedMe > BotSettings.TargetingWarningTimer && player.Value.TargetingMe && !player.Value.TargetWarning && BotSettings.UsePlayerTargetingKillswitch;
                    if (targetWarningTriggered)
                    {
                        player.Value.TargetWarning = true;
                        DiscordClientWrapper.SendMessage($"{player.Key} has been targeting {ObjectManager.Player.Name} for over {BotSettings.TargetingWarningTimer}ms.");
                    }

                    var proximityWarningTriggered = Environment.TickCount - player.Value.FirstSeen > BotSettings.ProximityWarningTimer && !player.Value.ProximityWarning && BotSettings.UsePlayerProximityKillswitch;
                    if (proximityWarningTriggered)
                    {
                        player.Value.ProximityWarning = true;
                        DiscordClientWrapper.SendMessage($"{player.Key} has been in range of {ObjectManager.Player.Name} for over {BotSettings.ProximityWarningTimer}ms.");
                    }

                    if (Environment.TickCount - player.Value.FirstTargetedMe > BotSettings.TargetingStopTimer && player.Value.TargetingMe && BotSettings.UsePlayerTargetingKillswitch)
                    {
                        DiscordClientWrapper.SendMessage($"{player.Key} has been targeting {ObjectManager.Player.Name} for over {BotSettings.TargetingStopTimer}ms. Stopping.");
                        Console.Beep(1000, 250);
                        stopBot = true;
                    }

                    if (Environment.TickCount - player.Value.FirstSeen > BotSettings.ProximityStopTimer && BotSettings.UsePlayerProximityKillswitch)
                    {
                        DiscordClientWrapper.SendMessage($"{player.Key} has been in range of {ObjectManager.Player.Name} for over {BotSettings.ProximityStopTimer}ms. Stopping.");
                        Console.Beep(1000, 250);
                        stopBot = true;
                    }

                    if ((player.Value.TargetWarning || player.Value.ProximityWarning) && Environment.TickCount - beepTimer > 2000)
                    {
                        Console.Beep(1000, 250);
                        beepTimer = Environment.TickCount;
                    }
                }
            }
            catch (Exception) { /* swallow it */ }

            return stopBot;
        }

        public bool DisableTeleportChecker { get; set; }
    }
}
