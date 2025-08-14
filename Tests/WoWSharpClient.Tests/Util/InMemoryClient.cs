using BotRunner.Clients;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService;
using PathfindingService.Repository;
using PhysicsInput = Pathfinding.PhysicsInput;

public class InMemoryPathfindingClient(Navigation nav) : PathfindingClient
{
    private readonly Navigation _nav = nav;

    public override Position[] GetPath(uint mapId, Position start, Position end, bool smoothPath = false)
        => [.. _nav.CalculatePath(mapId, start.ToXYZ(), end.ToXYZ(), smoothPath).Select(x => new Position(x.X, x.Y, x.Z))];

    public override float GetPathingDistance(uint mapId, Position start, Position end)
    {
        var path = _nav.CalculatePath(mapId, start.ToXYZ(), end.ToXYZ(), true).Select(x => new Position(x.X, x.Y, x.Z)).ToArray();
        float dist = 0f;
        for (int i = 0; i < path.Length - 1; i++)
            dist += path[i].DistanceTo(path[i + 1]);
        return dist;
    }

    public override bool IsInLineOfSight(uint mapId, Position from, Position to)
        => _nav.LineOfSight(mapId, from.ToXYZ(), to.ToXYZ());

    public override Pathfinding.PhysicsOutput PhysicsStep(PhysicsInput physicsInput)
        => _nav.StepPhysics(physicsInput.ToPhysicsInput(), physicsInput.DeltaTime).ToPhysicsOutput();
}
