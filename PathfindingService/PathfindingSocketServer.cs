using BotCommLayer;
using Game;
using Pathfinding;
using PathfindingService.Repository;

namespace PathfindingService
{
    public class PathfindingSocketServer(string ipAddress, int port, ILogger logger) : ProtobufSocketServer<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        protected override PathfindingResponse HandleRequest(PathfindingRequest payload)
        {
            Models.Position startPosition = new(payload.Start.X, payload.Start.Y, payload.Start.Z);
            Models.Position endPosition = new(payload.End.X, payload.End.Y, payload.End.Z);
            Models.Position[] path = Navigation.CalculatePath(payload.MapId, startPosition, endPosition, payload.SmoothPath);

            IEnumerable<Position> convertedPath = path.Select(x =>
                new Position()
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
