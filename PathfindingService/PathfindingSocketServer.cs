using BotCommLayer;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService.Repository;

namespace PathfindingService
{
    public class PathfindingSocketServer(string ipAddress, int port, ILogger logger) : ProtobufSocketServer<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        protected override PathfindingResponse HandleRequest(PathfindingRequest payload)
        {
            Position startPosition = new(payload.Start.X, payload.Start.Y, payload.Start.Z);
            Position endPosition = new(payload.End.X, payload.End.Y, payload.End.Z);
            Position[] path = Navigation.CalculatePath(payload.MapId, startPosition, endPosition, payload.SmoothPath);

            IEnumerable<Game.Position> convertedPath = path.Select(x =>
                new Game.Position()
                {
                    X = x.X,
                    Y = x.Y,
                    Z = x.Z
                }
             );

            PathfindingResponse response = new();
            response.Path.AddRange(convertedPath);
            return response;
        }
    }
}
