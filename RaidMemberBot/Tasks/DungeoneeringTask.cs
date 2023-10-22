using Newtonsoft.Json;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Models;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using ObjectManager = RaidMemberBot.Game.Statics.ObjectManager;

namespace RaidMemberBot.AI.SharedStates
{
    public class DungeoneeringTask : BotTask, IBotTask
    {
        readonly bool isPartyLeader;

        List<Location> dungeonWaypoints;
        List<Location> majorWaypoints;
        Dictionary<Location, List<Location>> minorWaypoints;

        Location currentMajorWaypoint;
        Location currentMinorWaypoint;
        Location destination;

        Location lastLocation;
        int lastTickTime;
        int stuckDuration;
        private bool CanProceed => ObjectManager.Instance.PartyMembers.All(x => (x.ManaPercent < 0 || x.ManaPercent > 80) && x.HealthPercent > 85);
        private bool NeedsGuidance => Container.CurrentWaypoint.DistanceToPlayer() < 3 || !Container.Player.IsFacing(Container.CurrentWaypoint) || !Container.Player.IsMoving;
        public DungeoneeringTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary)
        {
            isPartyLeader = ObjectManager.Instance.PartyLeader?.Guid == Container.Player.Guid;

            NavigationClient.Instance.isRaidLeader = isPartyLeader;

            if (isPartyLeader)
            {
                CreateWaypointMap();
            }

            Container.CurrentWaypoint = Container.Player.Location;

            WoWEventHandler.Instance.OnUnitKilled += Instance_OnUnitKilled;
        }

        private void Instance_OnUnitKilled(object sender, EventArgs e)
        {
            Console.WriteLine($"Unit Killed");
        }

        public void Update()
        {
            if (isPartyLeader)
            {
                CleanupWaypoints();

                // if there are hostile enemies to clear
                if (ObjectManager.Instance.Hostiles.Count(x => Container.Player.InLosWith(x) && x.Location.DistanceToPlayer() < 25) > 0)
                {
                    //Console.WriteLine($"DUNGEON: if there are hostile enemies to clear");
                    // if the party is ready to pull
                    if (CanProceed)
                    {
                        //Console.WriteLine($"DUNGEON: if the party is ready to pull");
                        Container.HostileTarget = ObjectManager.Instance.Hostiles.Where(x => Container.Player.InLosWith(x)).OrderBy(x => x.Location.DistanceToPlayer()).First();
                        Container.Player.SetTarget(Container.HostileTarget);

                        BotTasks.Push(Container.CreatePullTargetTask(Container, BotTasks));
                        return;
                    }
                    else
                    {
                        //Console.WriteLine($"DUNGEON: hodl up");
                        // hodl up
                        Container.Player.StopAllMovement();
                    }
                }
                else
                {
                    if (NeedsGuidance)
                    {
                        //Console.WriteLine($"DUNGEON: NeedsGuidance");
                        // if we are near our destination?
                        if (destination.DistanceToPlayer() < 3)
                        {
                            //Console.WriteLine($"DUNGEON: if we are near our destination?");
                            SetNextWaypoint();
                        }
                    }

                    ApproachDestination();
                }
            }
            else
            {
                if (ObjectManager.Instance.PartyLeader == null && Container.Player.MapId != 1 && Container.Player.MapId != 0)
                {
                    Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, new Location(), true);
                    destination = locations[locations.Length - 1];

                    ApproachDestination();
                }
                else if (ObjectManager.Instance.PartyLeader?.Location.DistanceToPlayer() > 15)
                {
                    Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, ObjectManager.Instance.PartyLeader.Location, true);
                    destination = locations[locations.Length - 1];

                    ApproachDestination();
                }
                else
                {
                    if (ObjectManager.Instance.Aggressors.Count > 0)
                    {
                        Container.Player.StopAllMovement();
                        BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                        return;
                    }
                    Container.Player.StopAllMovement();
                }
            }

            if (!CanProceed)
            {
                Container.Player.StopAllMovement();
                BotTasks.Push(Container.CreateRestTask(Container, BotTasks));
            }
            BotTasks.Push(Container.CreateBuffTask(Container, BotTasks));
        }

        private void SetNextWaypoint()
        {
            if (minorWaypoints.TryGetValue(currentMajorWaypoint, out List<Location> minorWaypointsListFinal))
            {
                if (minorWaypointsListFinal.Count > 0)
                {
                    currentMinorWaypoint = minorWaypointsListFinal.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, x, currentMajorWaypoint, true)).Reverse().First();
                    destination = currentMinorWaypoint;
                } else
                {
                    destination = currentMajorWaypoint;
                }
            }
            else
            {
                if (majorWaypoints.Count > 0)
                {
                    currentMajorWaypoint = majorWaypoints[0];
                    destination = currentMajorWaypoint;
                } else
                {
                    Console.WriteLine($"Job's Finished!");
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                }
            }
        }

        private void CleanupWaypoints()
        {
            //Console.WriteLine($"DUNGEON: CleanupWaypoints");
            dungeonWaypoints.RemoveAll(x => Container.Player.InLosWith(x) && NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, Container.Player.Location, x, true) < 20);

            Location[] minorWaypointKeys = minorWaypoints.Keys.ToArray();

            foreach (var location in minorWaypointKeys)
            {
                if (minorWaypoints.TryGetValue(location, out List<Location> minorWaypointsList))
                {
                    minorWaypointsList.RemoveAll(x => Container.Player.InLosWith(x) && NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, Container.Player.Location, x, true) < 20);

                    if (Container.Player.InLosWith(location) && minorWaypointsList.Count == 0 && NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, Container.Player.Location, location, true) < 20)
                    {
                        majorWaypoints.Remove(location);
                        minorWaypoints.Remove(location);
                    }
                }
            }
        }

        private void ApproachDestination()
        {
            //Console.WriteLine($"DUNGEON: ApproachDestination");
            Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, destination, true);

            if (locations.Length > 1)
            {
                Container.CurrentWaypoint = locations[1];

                if (lastLocation != null && Container.Player.Location.GetDistanceTo(lastLocation) <= 0.05)
                    stuckDuration += Environment.TickCount - lastTickTime;

                if (stuckDuration >= 1000)
                {
                    stuckDuration = 0;
                }

                lastLocation = Container.Player.Location;
                lastTickTime = Environment.TickCount;

                Container.Player.MoveToward(Container.CurrentWaypoint);
            }
        }

        private void CreateWaypointMap()
        {
            Console.WriteLine($"DUNGEONEERING TASK: Sorting encounter data");
            List<Creature> encounters = DatabaseClient.Instance.GetCreaturesByMapId((int)ObjectManager.Instance.Player.MapId);
            majorWaypoints = new List<Location>();
            minorWaypoints = new Dictionary<Location, List<Location>>();

            dungeonWaypoints = GetWaypointsListFromEncounters(encounters);

            destination = dungeonWaypoints.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, Container.Player.Location, x, true)).First();

            for (int i = 0; i < encounters.Count; i++)
            {
                if (encounters.Count(x => x.Id == encounters[i].Id) == 1)
                {
                    majorWaypoints.Add(new Location(encounters[i].LocationX, encounters[i].LocationY, encounters[i].LocationZ));
                }
            }

            majorWaypoints = majorWaypoints.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, destination, x, true)).ToList();

            float[,] weightedMatrix = new float[majorWaypoints.Count, majorWaypoints.Count];

            for (int i = 0; i < majorWaypoints.Count; i++)
            {
                for (int j = 0; j < majorWaypoints.Count; j++)
                {
                    if (i == j)
                    {
                        weightedMatrix[i, j] = 0;
                    }
                    else
                    {
                        weightedMatrix[i, j] = NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, majorWaypoints[i], majorWaypoints[j], true);
                    }
                }
            }

            Console.WriteLine($"DUNGEONEERING TASK: Optimizing Boss route[{majorWaypoints.Count}]");
            majorWaypoints = TravelingDungeonCrawler(majorWaypoints, 0);

            for (int i = 0; i < majorWaypoints.Count; i++)
            {
                minorWaypoints.Add(majorWaypoints[i], new List<Location>());

                if (i < majorWaypoints.Count - 1)
                {
                    Console.WriteLine($"DUNGEONEERING TASK: {JsonConvert.SerializeObject(majorWaypoints[i])}\t=> {JsonConvert.SerializeObject(majorWaypoints[i + 1])}");
                }
            }

            //Console.WriteLine($"DUNGEONEERING TASK: Optimizing sub routes");
            for (int i = 0; i < dungeonWaypoints.Count; i++)
            {
                if (!majorWaypoints.Contains(dungeonWaypoints[i]))
                {
                    if (minorWaypoints.TryGetValue(majorWaypoints.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, dungeonWaypoints[i], x, true)).First(), out List<Location> minorWaypointsList))
                    {
                        minorWaypointsList.Add(dungeonWaypoints[i]);
                    }
                }
            }

            foreach (var minorWaypoint in minorWaypoints)
            {
                if (minorWaypoints.TryGetValue(minorWaypoint.Key, out List<Location> minorWaypointsList))
                {
                    if (minorWaypointsList.Count < 10)
                    {
                        Console.WriteLine($"DUNGEONEERING TASK: Optimizing sub route[{minorWaypointsList.Count}]");

                        minorWaypointsList = TravelingDungeonCrawler(minorWaypointsList, 0);
                    }
                }
            }

            currentMajorWaypoint = majorWaypoints.First();

            if (minorWaypoints.TryGetValue(currentMajorWaypoint, out List<Location> minorWaypointsListFinal))
            {
                currentMinorWaypoint = minorWaypointsListFinal.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, Container.Player.Location, x, true)).First();
                destination = currentMinorWaypoint;
            }
        }

        // implementation of Traveling Salesman Problem
        private List<Location> TravelingDungeonCrawler(List<Location> locations, int s)
        {
            float[,] graph = new float[locations.Count, locations.Count];

            for (int i = 0; i < locations.Count; i++)
            {
                for (int j = 0; j < locations.Count; j++)
                {
                    if (i == j)
                    {
                        graph[i, j] = 0;
                    }
                    else
                    {
                        graph[i, j] = NavigationClient.Instance.CalculatePathingDistance(Container.Player.MapId, locations[i], locations[j], true);
                    }
                }
            }

            List<int> vertex = new List<int>();

            for (int i = 0; i < graph.GetLength(0); i++)
                if (i != s)
                    vertex.Add(i);

            List<int> pathSequence = vertex.ToList();
            // store minimum weight
            // Hamiltonian Cycle.
            float min_path = float.MaxValue;
            do
            {
                // store current Path weight(cost)
                float current_pathweight = 0;
                int k = s;

                // compute current path weight
                for (int i = 0; i < vertex.Count; i++)
                {
                    current_pathweight += graph[k, vertex[i]];
                    k = vertex[i];
                }

                current_pathweight += graph[k, s];

                // update minimum
                if (current_pathweight < min_path)
                {
                    pathSequence = vertex.ToList();
                    min_path = current_pathweight;
                }

            } while (FindNextPermutation(vertex));

            List<Location> proposedPath = new List<Location>() { destination };
            for (int i = 0; i < pathSequence.Count; i++)
            {
                proposedPath.Add(locations[pathSequence[i]]);
            }
            return proposedPath;
        }

        // Function to swap the data resent in the left and
        // right indices
        private List<int> Swap(List<int> data, int left,
                                     int right)
        {
            // Swap the data
            int temp = data[left];
            data[left] = data[right];
            data[right] = temp;

            // Return the updated array
            return data;
        }

        // Function to reverse the sub-array starting from left
        // to the right both inclusive
        private List<int> Reverse(List<int> data,
                                        int left, int right)
        {
            // Reverse the sub-array
            while (left < right)
            {
                int temp = data[left];
                data[left++] = data[right];
                data[right--] = temp;
            }

            // Return the updated array
            return data;
        }

        // Function to find the next permutation of the given
        // integer array
        private bool FindNextPermutation(List<int> data)
        {
            // If the given dataset is empty
            // or contains only one element
            // next_permutation is not possible
            if (data.Count <= 1)
                return false;
            int last = data.Count - 2;

            // find the longest non-increasing
            // suffix and find the pivot
            while (last >= 0)
            {
                if (data[last] < data[last + 1])
                    break;
                last--;
            }

            // If there is no increasing pair
            // there is no higher order permutation
            if (last < 0)
                return false;
            int nextGreater = data.Count - 1;

            // Find the rightmost successor
            // to the pivot
            for (int i = data.Count - 1; i > last; i--)
            {
                if (data[i] > data[last])
                {
                    nextGreater = i;
                    break;
                }
            }

            // Swap the successor and
            // the pivot
            data = Swap(data, nextGreater, last);

            // Reverse the suffix
            data = Reverse(data, last + 1, data.Count - 1);

            // Return true as the
            // next_permutation is done
            return true;
        }

        private List<Location> GetWaypointsListFromEncounters(List<Creature> encounters)
        {
            List<Location> waypoints = new List<Location>();

            for (int i = 0; i < encounters.Count; i++)
            {
                Dictionary<int, HashSet<int>> creatureLinkedMapping = new Dictionary<int, HashSet<int>>();

                CreatureTemplate creatureTemplate = DatabaseClient.Instance.GetCreatureTemplateById(encounters[i].Id);
                List<CreatureLinking> creatureLinkings = DatabaseClient.Instance.GetCreatureLinkedByGuid(encounters[i].Guid);

                foreach (var creatureLinking in creatureLinkings)
                {
                    if (!creatureLinkedMapping.ContainsKey(creatureLinking.MasterGuid))
                    {
                        creatureLinkedMapping.Add(creatureLinking.MasterGuid, new HashSet<int>());
                    }

                    if (creatureLinkedMapping.TryGetValue(creatureLinking.MasterGuid, out HashSet<int> creatures))
                    {
                        creatures.Add(encounters[i].Guid);
                    }
                }

                bool isLinked = false;

                foreach (var creatureLinkMappingValues in creatureLinkedMapping)
                {
                    if (creatureLinkMappingValues.Value.Any(x => x == encounters[i].Guid))
                    {
                        isLinked = true;

                        float centerX = 0;
                        float centerY = 0;
                        float centerZ = 0;

                        foreach (var packMember in creatureLinkMappingValues.Value)
                        {
                            Creature creature = encounters.First(x => x.Guid == packMember);

                            centerX += creature.LocationX;
                            centerY += creature.LocationY;
                            centerZ += creature.LocationZ;
                        }
                        Location centerPoint = new Location(centerX / creatureLinkMappingValues.Value.Count,
                                                            centerY / creatureLinkMappingValues.Value.Count,
                                                            centerZ / creatureLinkMappingValues.Value.Count);
                        Location centerSpawnLocation = creatureLinkMappingValues.Value
                            .Select(x => encounters.First(u => u.Guid == x))
                            .Select(x => new Location(x.LocationX, x.LocationY, x.LocationZ))
                            .OrderBy(x => x.GetDistanceTo(centerPoint))
                            .First();

                        waypoints.Add(centerSpawnLocation);
                    }
                }

                if (!isLinked)
                {
                    waypoints.Add(new Location(encounters[i].LocationX, encounters[i].LocationY, encounters[i].LocationZ));
                }
            }

            return waypoints;
        }

    }
}
