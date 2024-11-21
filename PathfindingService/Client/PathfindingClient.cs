using BotCommLayer;
using Pathfinding;
using PathfindingService.Models;

namespace PathfindingService.Client
{
    public class PathfindingClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        public Position[] GetPath(uint mapId, Position startPosition, Position endPosition, bool smoothPath)
        {
            return SendMessage(new PathfindingRequest()
            {
                MapId = mapId,
                Start = new Game.Position()
                {
                    X = startPosition.X,
                    Y = startPosition.Y,
                    Z = startPosition.Z
                },
                End = new Game.Position()
                {
                    X = endPosition.X,
                    Y = endPosition.Y,
                    Z = endPosition.Z
                },
                SmoothPath = smoothPath
            }).Path.Select(pos => new Position(pos.X, pos.Y, pos.Z)).ToArray();
        }

        public float GetPathingDistance(uint mapId, Position startPosition, Position endPosition, bool smoothPath)
        {
            float distance = 0f;
            List<Position> positions = [.. SendMessage(new PathfindingRequest()
            {
                MapId = mapId,
                Start = new Game.Position()
                {
                    X = startPosition.X,
                    Y = startPosition.Y,
                    Z = startPosition.Z
                },
                End = new Game.Position()
                {
                    X = endPosition.X,
                    Y = endPosition.Y,
                    Z = endPosition.Z
                },
                SmoothPath = smoothPath
            }).Path.Select(pos => new Position(pos.X, pos.Y, pos.Z))];

            for (int i = 0; i < positions.Count - 1; i++)
            {
                distance += positions[i].DistanceTo(positions[i + 1]);
            }
            return distance;
        }
    }
}
