using BotRunner.Clients;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService;
using PathfindingService.Repository;

public class InMemoryPathfindingClient(Navigation nav) : PathfindingClient
{
    private readonly Navigation _nav = nav;

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

    public override PhysicsOutput PhysicsStep(PhysicsInput physicsInput)
        => _nav.StepPhysics(physicsInput.ToPhysicsInput(), physicsInput.DeltaTime).ToPhysicsOutput();
}
