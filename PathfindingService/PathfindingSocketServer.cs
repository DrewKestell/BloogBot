using BotCommLayer;
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
            Models.Position[] path = PathingAndDistance.Instance.CalculatePath(payload.MapId, startPosition, endPosition, payload.SmoothPath);

            IEnumerable<PositionDTO> convertedPath = path.Select(x =>
                new PositionDTO()
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
