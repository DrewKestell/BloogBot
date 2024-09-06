using BotCommLayer;
using Common;
using Pathfinding;
using PathfindingService.Repository;

namespace PathfindingService
{
    public class PathfindingSocketServer : ProtobufSocketServer<PathfindingRequest, PathfindingResponse>
    {
        public PathfindingSocketServer(string ipAddress, int port, ILogger logger) : base(ipAddress, port, logger)
        {
            PathingAndLOS.Initialize();
        }

        protected override PathfindingResponse HandleRequest(PathfindingRequest payload)
        {
            Models.Position startPosition = new(payload.Start.X, payload.Start.Y, payload.Start.Z);
            Models.Position endPosition = new(payload.End.X, payload.End.Y, payload.End.Z);
            Models.Position[] path = PathingAndLOS.CalculatePath(payload.MapId, startPosition, endPosition, payload.SmoothPath);

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
