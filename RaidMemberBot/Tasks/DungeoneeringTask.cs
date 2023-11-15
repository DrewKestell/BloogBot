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

        List<Position> dungeonWaypoints;
        List<Position> majorWaypoints;
        Dictionary<Position, List<Position>> minorWaypoints;

        Position currentMajorWaypoint;
        Position currentMinorWaypoint;
        Position destination;

        Position lastPosition;
        int lastTickTime;
        int stuckDuration;
        private bool CanProceed => ObjectManager.PartyMembers.All(x => (x.ManaPercent < 0 || x.ManaPercent > 80) && x.HealthPercent > 85);
        private bool NeedsGuidance => Container.CurrentWaypoint.DistanceTo(ObjectManager.Player.Position) < 3 || !ObjectManager.Player.IsFacing(Container.CurrentWaypoint) || !ObjectManager.Player.IsMoving;
        public DungeoneeringTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary)
        {
            isPartyLeader = container.State.RaidLeader == container.State.CharacterName;

            NavigationClient.Instance.isRaidLeader = isPartyLeader;

            if (isPartyLeader)
            {
                CreateWaypointMap();
            }

            Container.CurrentWaypoint = ObjectManager.Player.Position;

            WoWEventHandler.Instance.OnUnitKilled += Instance_OnUnitKilled;
        }

        private void Instance_OnUnitKilled(object sender, EventArgs e)
        {
            //Console.WriteLine($"Unit Killed");
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count > 0)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                return;
            }

            if (isPartyLeader)
            {
                CleanupWaypoints();
                // if there are hostile enemies to clear
                if (CanProceed)
                {
                    // if the party is ready to pull
                    if (ObjectManager.Hostiles.Count(x => ObjectManager.Player.InLosWith(x.Position) && x.Position.DistanceTo(ObjectManager.Player.Position) < 25) > 0)
                    {
                        ObjectManager.Player.StopAllMovement();
                        //Console.WriteLine($"DUNGEON: if the party is ready to pull");
                        Container.HostileTarget = ObjectManager.Hostiles.Where(x => ObjectManager.Player.InLosWith(x.Position)).OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x.Position, true)).First();
                        ObjectManager.Player.SetTarget(Container.HostileTarget.Guid);

                        BotTasks.Push(Container.CreatePullTargetTask(Container, BotTasks));
                        return;
                    }
                    else
                    {
                        if (NeedsGuidance)
                        {
                            //Console.WriteLine($"DUNGEON: NeedsGuidance");
                            // if we are near our destination?
                            if (destination.DistanceTo(ObjectManager.Player.Position) < 3)
                            {
                                //Console.WriteLine($"DUNGEON: if we are near our destination?");
                                SetNextWaypoint();
                            }
                        }

                        ApproachDestination();
                    }
                }
            }
            else
            {
                if (ObjectManager.PartyLeader == null && ObjectManager.MapId != 1 && ObjectManager.MapId != 0)
                {
                    Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, new Position(0, 0, 0), true);
                    destination = locations[locations.Length - 1];

                    ApproachDestination();
                }
                else if (ObjectManager.PartyLeader?.Position.DistanceTo(ObjectManager.Player.Position) > 15)
                {
                    Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.PartyLeader.Position, true);
                    destination = locations[locations.Length - 1];

                    ApproachDestination();
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                }
            }

            if (!CanProceed)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Push(Container.CreateRestTask(Container, BotTasks));
            }
            BotTasks.Push(Container.CreateBuffTask(Container, BotTasks));
        }

        private void SetNextWaypoint()
        {
            if (minorWaypoints.TryGetValue(currentMajorWaypoint, out List<Position> minorWaypointsListFinal))
            {
                if (minorWaypointsListFinal.Count > 0)
                {
                    currentMinorWaypoint = minorWaypointsListFinal.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, x, currentMajorWaypoint, true)).Reverse().First();
                    destination = currentMinorWaypoint;
                }
                else
                {
                    minorWaypoints.Remove(currentMajorWaypoint);
                    SetNextWaypoint();
                }
            }
            else
            {
                if (majorWaypoints.Count > 0)
                {
                    currentMajorWaypoint = majorWaypoints[0];
                    destination = currentMajorWaypoint;
                }
                else
                {
                    Console.WriteLine($"DUNGEONEERING TASK: Job's Finished!");
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                }
            }
        }

        private void CleanupWaypoints()
        {
            //Console.WriteLine($"DUNGEON: CleanupWaypoints");
            dungeonWaypoints.RemoveAll(x => ObjectManager.Player.InLosWith(x) && NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true) < 20);

            Position[] minorWaypointKeys = minorWaypoints.Keys.ToArray();

            foreach (Position location in minorWaypointKeys)
            {
                if (minorWaypoints.TryGetValue(location, out List<Position> minorWaypointsList))
                {
                    minorWaypointsList.RemoveAll(x => !dungeonWaypoints.Contains(x));
                }
            }
        }

        private void ApproachDestination()
        {
            //Console.WriteLine($"DUNGEON: ApproachDestination");
            Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, destination, true);

            if (locations.Length > 1)
            {
                Container.CurrentWaypoint = locations[1];

                if (lastPosition != null && ObjectManager.Player.Position.DistanceTo(lastPosition) <= 0.05)
                    stuckDuration += Environment.TickCount - lastTickTime;

                if (stuckDuration >= 1000)
                {
                    stuckDuration = 0;
                }

                lastPosition = ObjectManager.Player.Position;
                lastTickTime = Environment.TickCount;

                ObjectManager.Player.MoveToward(Container.CurrentWaypoint);
            }
        }

        private void CreateWaypointMap()
        {
            Console.WriteLine($"[DUNGEONEERING TASK] Sorting encounter data");
            List<Creature> encounters = DatabaseClient.Instance.GetCreaturesByMapId((int)ObjectManager.MapId);
            majorWaypoints = new List<Position>();
            minorWaypoints = new Dictionary<Position, List<Position>>();

            dungeonWaypoints = GetWaypointsListFromEncounters(encounters);

            destination = dungeonWaypoints.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true)).First();

            for (int i = 0; i < encounters.Count; i++)
            {
                if (encounters.Count(x => x.Id == encounters[i].Id) == 1)
                {
                    majorWaypoints.Add(new Position(encounters[i].PositionX, encounters[i].PositionY, encounters[i].PositionZ));
                }
            }

            majorWaypoints = majorWaypoints.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, destination, x, true)).ToList();

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
                        weightedMatrix[i, j] = NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, majorWaypoints[i], majorWaypoints[j], true);
                    }
                }
            }

            Console.WriteLine($"DUNGEONEERING TASK: Optimizing Boss route[{majorWaypoints.Count}]");
            majorWaypoints = TravelingDungeonCrawler(majorWaypoints, 0);

            for (int i = 0; i < majorWaypoints.Count; i++)
            {
                minorWaypoints.Add(majorWaypoints[i], new List<Position>());

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
                    if (minorWaypoints.TryGetValue(majorWaypoints.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, dungeonWaypoints[i], x, true)).First(), out List<Position> minorWaypointsList))
                    {
                        minorWaypointsList.Add(dungeonWaypoints[i]);
                    }
                }
            }

            foreach (KeyValuePair<Position, List<Position>> minorWaypoint in minorWaypoints)
            {
                if (minorWaypoints.TryGetValue(minorWaypoint.Key, out List<Position> minorWaypointsList))
                {
                    if (minorWaypointsList.Count < 10)
                    {
                        Console.WriteLine($"DUNGEONEERING TASK: Optimizing sub route[{minorWaypointsList.Count}]");

                        minorWaypointsList = TravelingDungeonCrawler(minorWaypointsList, 0);
                    }
                }
            }

            currentMajorWaypoint = majorWaypoints.First();

            if (minorWaypoints.TryGetValue(currentMajorWaypoint, out List<Position> minorWaypointsListFinal))
            {
                currentMinorWaypoint = minorWaypointsListFinal.OrderBy(x => NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true)).First();
                destination = currentMinorWaypoint;
            }
        }

        // implementation of Traveling Salesman Problem
        private List<Position> TravelingDungeonCrawler(List<Position> locations, int s)
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
                        graph[i, j] = NavigationClient.Instance.CalculatePathingDistance(ObjectManager.MapId, locations[i], locations[j], true);
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

            List<Position> proposedPath = new List<Position>() { destination };
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

        private List<Position> GetWaypointsListFromEncounters(List<Creature> encounters)
        {
            List<Position> waypoints = new List<Position>();

            for (int i = 0; i < encounters.Count; i++)
            {
                Dictionary<int, HashSet<int>> creatureLinkedMapping = new Dictionary<int, HashSet<int>>();

                CreatureTemplate creatureTemplate = DatabaseClient.Instance.GetCreatureTemplateById(encounters[i].Id);
                List<CreatureLinking> creatureLinkings = DatabaseClient.Instance.GetCreatureLinkedByGuid(encounters[i].Guid);

                foreach (CreatureLinking creatureLinking in creatureLinkings)
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

                foreach (KeyValuePair<int, HashSet<int>> creatureLinkMappingValues in creatureLinkedMapping)
                {
                    if (creatureLinkMappingValues.Value.Any(x => x == encounters[i].Guid))
                    {
                        isLinked = true;

                        float centerX = 0;
                        float centerY = 0;
                        float centerZ = 0;

                        foreach (int packMember in creatureLinkMappingValues.Value)
                        {
                            Creature creature = encounters.First(x => x.Guid == packMember);

                            centerX += creature.PositionX;
                            centerY += creature.PositionY;
                            centerZ += creature.PositionZ;
                        }
                        Position centerPoint = new Position(centerX / creatureLinkMappingValues.Value.Count,
                                                            centerY / creatureLinkMappingValues.Value.Count,
                                                            centerZ / creatureLinkMappingValues.Value.Count);
                        Position centerSpawnPosition = creatureLinkMappingValues.Value
                            .Select(x => encounters.First(u => u.Guid == x))
                            .Select(x => new Position(x.PositionX, x.PositionY, x.PositionZ))
                            .OrderBy(x => x.DistanceTo(centerPoint))
                            .First();

                        waypoints.Add(centerSpawnPosition);
                    }
                }

                if (!isLinked)
                {
                    waypoints.Add(new Position(encounters[i].PositionX, encounters[i].PositionY, encounters[i].PositionZ));
                }
            }

            return waypoints;
        }

    }
}
