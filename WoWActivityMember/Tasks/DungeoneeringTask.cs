using MaNGOSDBDomain.Models;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using ObjectManager = WoWActivityMember.Game.Statics.ObjectManager;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class DungeoneeringTask : BotTask, IBotTask
    {
        private readonly bool isPartyLeader;
        private List<Position> dungeonWaypoints;
        private List<Position> majorWaypoints;
        private Dictionary<Position, List<Position>> minorWaypoints;
        private Position currentMajorWaypoint;
        private Position currentMinorWaypoint;
        private Position destination;
        private Position currentWaypoint;
        private Position lastPosition;
        private int lastTickTime;
        private int stuckDuration;
        public DungeoneeringTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary)
        {
            if (isPartyLeader)
            {
                CreateWaypointMap();
            }
            currentWaypoint = ObjectManager.Player.Position;

            WoWEventHandler.Instance.OnUnitKilled += Instance_OnUnitKilled;
            Console.WriteLine($"[DUNGEONEERING TASK] Sorting encounter data");
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
                    if (ObjectManager.Hostiles.Count(x => ObjectManager.Player.InLosWith(x) && x.Position.DistanceTo(ObjectManager.Player.Position) < 25) > 0)
                    {
                        ObjectManager.Player.StopAllMovement();

                        WoWUnit target = ObjectManager.Hostiles.Where(x => ObjectManager.Player.InLosWith(x)).OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x.Position, true)).First();

                        Console.WriteLine($"** PULLING ** {target.Pointer.ToString("X")} {IntPtr.Add(target.Pointer, MemoryAddresses.WoWUnit_BoundingRadiusOffset)} {IntPtr.Add(target.Pointer, MemoryAddresses.WoWUnit_CombatReachOffset)}");
                        //if (Container.State.Encounters.Any(x => x.Any(y => y.Guid == target.CreatureId)))
                        //{
                        //    List<Creature> creatures = Container.State.Encounters.FirstOrDefault(x => x.Any(y => y.Guid == target.CreatureId));
                        //    Console.WriteLine($"Got it! {JsonConvert.SerializeObject(creatures)}");
                        //}

                        ObjectManager.Player.SetTarget(target.Guid);
                        Functions.LuaCall($"SetRaidTarget('target', 8)");

                        BotTasks.Push(Container.CreatePullTargetTask(Container, BotTasks));
                        return;
                    }
                    else
                    {
                        if (NeedsGuidance)
                        {
                            if (destination.DistanceTo(ObjectManager.Player.Position) < 3)
                            {
                                SetNextWaypoint();
                            }
                        }

                        ApproachDestination();
                    }
                }
            }
            else
            {
                if (ObjectManager.PartyLeader == null)
                {
                    Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, new Position(0, 0, 0), true);

                    if (locations.Length > 1)
                    {
                        destination = locations[1];

                        ApproachDestination();
                    }
                }
                else if (ObjectManager.PartyLeader?.Position.DistanceTo(ObjectManager.Player.Position) > 15)
                {
                    Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.PartyLeader.Position, true);

                    if (locations.Length > 1)
                    {
                        destination = locations[1];

                        ApproachDestination();
                    }
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
            if (dungeonWaypoints.Count > 0)
            {
                destination = dungeonWaypoints.OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true)).First();
            }
            else
            {
                Functions.LuaCall("DoEmote(\"CHEER\")");
                BotTasks.Pop();
                return;
            }
        }

        private void CleanupWaypoints()
        {
            List<Position> positions = dungeonWaypoints.Where(x => ObjectManager.Player.Position.InLosWith(x) && Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true) < 5).ToList();
            //Container.State.VisitedWaypoints.AddRange(positions);
            dungeonWaypoints.RemoveAll(x => positions.Contains(x));

            Position[] minorWaypointKeys = [.. minorWaypoints.Keys];

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
            Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, destination, true);

            if (locations.Length > 1)
            {
                currentWaypoint = locations[1];
                if (lastPosition != null && ObjectManager.Player.Position.DistanceTo(lastPosition) <= 0.05)
                    stuckDuration += Environment.TickCount - lastTickTime;

                if (stuckDuration >= 1000)
                {
                    stuckDuration = 0;
                }

                lastPosition = ObjectManager.Player.Position;
                lastTickTime = Environment.TickCount;

                ObjectManager.Player.MoveToward(locations[1]);
            }
        }

        private void CreateWaypointMap()
        {
            Console.WriteLine($"[DUNGEONEERING TASK] Sorting encounter data");
            List<Creature> encounters = Container.MaNGOSDBClient.GetCreaturesByMapId((int)ObjectManager.MapId);
            majorWaypoints = [];
            minorWaypoints = [];

            dungeonWaypoints = GetWaypointsListFromEncounters(encounters);
            dungeonWaypoints.AddRange(GetWaypointsListFromPathing(encounters));

            destination = dungeonWaypoints.OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true)).First();

            for (int i = 0; i < encounters.Count; i++)
            {
                if (encounters.Count(x => x.Id == encounters[i].Id) == 1)
                {
                    majorWaypoints.Add(new Position(encounters[i].PositionX, encounters[i].PositionY, encounters[i].PositionZ));
                }
            }

            majorWaypoints = [.. majorWaypoints.OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, destination, x, true))];

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
                        weightedMatrix[i, j] = Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, majorWaypoints[i], majorWaypoints[j], true);
                    }
                }
            }

            Console.WriteLine($"[DUNGEONEERING TASK] Optimizing Boss route[{majorWaypoints.Count}]");
            majorWaypoints = TravelingDungeonCrawler(majorWaypoints, 0);

            for (int i = 0; i < majorWaypoints.Count; i++)
            {
                minorWaypoints.Add(majorWaypoints[i], []);
            }

            for (int i = 0; i < dungeonWaypoints.Count; i++)
            {
                if (!majorWaypoints.Contains(dungeonWaypoints[i]))
                {
                    if (minorWaypoints.TryGetValue(majorWaypoints.OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, dungeonWaypoints[i], x, true)).First(), out List<Position> minorWaypointsList))
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
                        minorWaypointsList = TravelingDungeonCrawler(minorWaypointsList, 0);
                    }
                }
            }

            currentMajorWaypoint = majorWaypoints.First();

            if (minorWaypoints.TryGetValue(currentMajorWaypoint, out List<Position> minorWaypointsListFinal))
            {
                currentMinorWaypoint = minorWaypointsListFinal.OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true)).First();
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
                        graph[i, j] = Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, locations[i], locations[j], true);
                    }
                }
            }

            List<int> vertex = [];

            for (int i = 0; i < graph.GetLength(0); i++)
                if (i != s)
                    vertex.Add(i);

            List<int> pathSequence = [.. vertex];
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
                    pathSequence = [.. vertex];
                    min_path = current_pathweight;
                }

            } while (FindNextPermutation(vertex));

            List<Position> proposedPath = [destination];
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
            List<Position> waypoints = [];
            Dictionary<int, HashSet<int>> creatureLinkedMapping = [];

            for (int i = 0; i < encounters.Count; i++)
            {
                CreatureTemplate creatureTemplate = Container.MaNGOSDBClient.GetCreatureTemplateById(encounters[i].Id);
                List<CreatureGrouping> creatureLinkings = Container.MaNGOSDBClient.GetCreatureMappingByMemberGuid(encounters[i].Guid);
                foreach (CreatureGrouping creatureLinking in creatureLinkings)
                {
                    if (!creatureLinkedMapping.ContainsKey(creatureLinking.LeaderGuid))
                    {
                        creatureLinkedMapping.Add(creatureLinking.LeaderGuid, [creatureLinking.LeaderGuid]);
                    }

                    if (creatureLinkedMapping.TryGetValue(creatureLinking.LeaderGuid, out HashSet<int> creatures))
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

                        Position centerPoint = new(centerX / creatureLinkMappingValues.Value.Count,
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

            for (int i = 0; i < encounters.Count; i++)
            {
                //HashSet<int> hashSet = creatureLinkedMapping.Values.FirstOrDefault(x => x.Any(y => y == encounters[i].Guid));
                //if (hashSet == null || hashSet.Count == 0)
                //{
                //    Container.State.Encounters.Add([encounters[i]]);
                //}
                //else if (!Container.State.Encounters.Any(x => x.Any(y => y.Guid == encounters[i].Guid)))
                //{
                //    List<Creature> creatures = [];
                //    foreach (int creatureGuid in hashSet)
                //    {
                //        creatures.Add(encounters.First(x => x.Guid == creatureGuid));
                //    }
                //    Container.State.Encounters.Add(creatures);
                //}
            }
            //Console.WriteLine($"** {JsonConvert.SerializeObject(Container.State.Encounters, Formatting.Indented)}");
            return waypoints;
        }

        private List<Position> GetWaypointsListFromPathing(List<Creature> encounters)
        {
            List<Position> waypoints = [];

            foreach (Creature creature in encounters)
            {
                waypoints.Add(new Position(creature.PositionX, creature.PositionY, creature.PositionZ));
            }

            return waypoints;
        }
        private bool CanProceed => ObjectManager.PartyMembers.All(x => (x.ManaPercent < 0 || x.ManaPercent > 80) && x.HealthPercent > 85);
        private bool NeedsGuidance => currentWaypoint.DistanceTo(ObjectManager.Player.Position) < 3 || !ObjectManager.Player.IsFacing(currentWaypoint) || !ObjectManager.Player.IsMoving;

    }
}
