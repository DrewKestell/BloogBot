using System;
using System.Collections.Generic;
using System.Linq;
using BloogBot.Game;
using BloogBot.Game.Objects;

namespace BloogBot.AI.SharedStates
{
    public class GatherState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly Position[] waypoints;
        readonly LocalPlayer player;
        readonly HashSet<string> nodeNames;
        readonly StuckHelper stuckHelper;
        readonly Dictionary<ulong, int> nodeGatherFailTimestamps = new Dictionary<ulong, int>();

        int nextWaypointIndex = -1;
        int lastWaypointTime = Environment.TickCount;

        public GatherState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            waypoints = container.GetCurrentGatherRoute().TravelPath.Waypoints;
            player = ObjectManager.Player;
            nodeNames = new HashSet<string>(container.GetCurrentGatherRoute().NodeNames.Split('|'));
            stuckHelper = new StuckHelper(botStates, container);
        }

        public void Update()
        {
            if (waypoints.Length == 0)
            {
                Logger.Log("No waypoints found for gather route, stopping bot.");
                botStates.Pop();
                return;
            }

            if (nextWaypointIndex == -1)
            {
                // Move to the closest waypoint to start.
                var closestDistance = player.Position.DistanceTo(waypoints[0]);
                nextWaypointIndex = 0;

                for (var i = 1; i < waypoints.Length; i++)
                {
                    var distance = player.Position.DistanceTo(waypoints[i]);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nextWaypointIndex = i;
                    }
                }
            }

            // If we are in combat, deal with the threat.
            if (player.IsInCombat)
            {
                var target = container.FindClosestTarget();
                if (target != null)
                {
                    botStates.Push(container.CreateCombatState(
                        botStates, container, target, /*loot=*/false));
                    return;
                }
            }

            // Find if there are any gather nodes in render distance.
            var gatherNodes = ObjectManager
                .GameObjects
                .Where(g => nodeNames.Contains(g.Name))
                .Where(g => !container.Probe.BlacklistedMobIds?.Contains(g.Guid) ?? true)
                // Filter out nodes we've recently failed to gather (e.g. skill level too low).
                .Where(g => !(
                    nodeGatherFailTimestamps.ContainsKey(g.Guid) &&
                    Environment.TickCount - nodeGatherFailTimestamps[g.Guid] < 5 * 60 * 1000))
                .OrderBy(g => g.Position.DistanceTo(player.Position));

            if (gatherNodes.Any())
            {
                var gatherNode = gatherNodes.First();
                botStates.Push(new GatherObjectState(
                    botStates, container, gatherNode, onDeadline: () =>
                    {
                        // We failed to gather this node (e.g. skill level too low). Blacklist it
                        // temporarily.
                        nodeGatherFailTimestamps[gatherNode.Guid] = Environment.TickCount;
                    }));
                botStates.Push(new MoveToPositionState(
                    botStates,
                    container,
                    gatherNode.Position,
                    ignoreThreats: true,
                    deadline: Environment.TickCount + 60 * 1000,
                    onDeadline: () =>
                    {
                        // We can't reach this node. Blacklist it.
                        container.Probe.BlacklistedMobIds.Add(gatherNode.Guid);
                        if (container.BotSettings.PermanentlyBlacklistUnreachableTargets)
                        {
                            Repository.AddBlacklistedMob(gatherNode.Guid);
                        }
                    }));
                return;
            }

            // Move to the next waypoint.
            stuckHelper.CheckIfStuck();

            var nextWaypoint = waypoints[nextWaypointIndex];
            player.MoveToward(Navigation.GetNextWaypoint(
                ObjectManager.MapId, player.Position, nextWaypoint, false));

            if (player.Position.DistanceTo(nextWaypoint) < 5)
            {
                NextWaypoint();
            }

            // If we are stuck getting to the waypoint, skip to the next one.
            if (Environment.TickCount - lastWaypointTime > 30 * 1000)
            {
                Logger.Log("Stuck getting to waypoint, skipping to next one.");
                NextWaypoint();
            }
        }

        void NextWaypoint()
        {
            lastWaypointTime = Environment.TickCount;
            nextWaypointIndex = (nextWaypointIndex + 1) % waypoints.Length;
        }
    }
}
