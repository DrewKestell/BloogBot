using BotRunner.Clients;
using GameData.Core.Constants;
using GameData.Core.Enums;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService.Repository;

public class InMemoryPathfindingClient : PathfindingClient
{
    private readonly Navigation _nav;

    public InMemoryPathfindingClient(Navigation nav)
    {
        _nav = nav;
    }

    public override Position[] GetPath(uint mapId, Position start, Position end, bool smoothPath = false)
        => _nav.CalculatePath(mapId, start, end, smoothPath);

    public override float GetPathingDistance(uint mapId, Position start, Position end)
    {
        var path = _nav.CalculatePath(mapId, start, end, true);
        float dist = 0f;
        for (int i = 0; i < path.Length - 1; i++)
            dist += path[i].DistanceTo(path[i + 1]);
        return dist;
    }

    public override bool IsInLineOfSight(uint mapId, Position from, Position to)
        => _nav.IsLineOfSight(mapId, from, to);

    public override TerrainProbeResponse ProbeTerrain(uint mapId, Position feet, Race race)
    {
        var (radius, height) = RaceDimensions.GetCapsuleForRace(race);
        return _nav.GetTerrainProbe(mapId, feet.ToProto(), radius, height);
    }
}
