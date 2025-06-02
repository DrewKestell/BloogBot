using BotCommLayer;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService.Repository;

namespace PathfindingService
{
    public class PathfindingSocketServer(string ipAddress, int port, ILogger logger)
        : ProtobufSocketServer<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        private readonly Navigation _navigation = new();
        protected override PathfindingResponse HandleRequest(PathfindingRequest payload)
        {
            PathfindingResponse response = new() { ResponseType = payload.RequestType };

            if (payload.MapId == null)
            {
                logger.LogWarning("Missing MapId in request.");
                return response;
            }

            if (payload.Start == null || payload.End == null)
            {
                logger.LogWarning("Missing start or end position.");
                return response;
            }

            bool useSmoothing = payload.SmoothPath ?? false;

            try
            {
                switch (payload.RequestType)
                {
                    case PathfindingRequestType.Path:
                        {
                            var start = new Position(payload.Start.ToXYZ());
                            var end = new Position(payload.End.ToXYZ());
                            var path = _navigation.CalculatePath(payload.MapId.Value, start, end, useSmoothing);

                            response.Path.AddRange(path.Select(p => p.ToXYZ().ToProto()));
                            break;
                        }

                    case PathfindingRequestType.Distance:
                        {
                            var start = new Position(payload.Start.ToXYZ());
                            var end = new Position(payload.End.ToXYZ());
                            var path = _navigation.CalculatePath(payload.MapId.Value, start, end, useSmoothing);

                            float totalDistance = 0f;
                            for (int i = 0; i < path.Length - 1; i++)
                                totalDistance += path[i].DistanceTo(path[i + 1]);

                            response.ZPoint = totalDistance;
                            break;
                        }

                    case PathfindingRequestType.ZCheck:
                        {
                            var sample = new Position(payload.Start.ToXYZ());
                            response.ZPoint = _navigation.GetFloorZ(payload.MapId.Value, sample);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"[PathfindingSocketServer] Error during request handling: {ex.Message}\n{ex.StackTrace}");
            }

            return response;
        }
    }

    public static class ProtoInteropExtensions
    {
        public static Game.Position ToProto(this XYZ xyz) =>
            new() { X = xyz.X, Y = xyz.Y, Z = xyz.Z };

        public static XYZ ToXYZ(this Game.Position p) =>
            new(p.X, p.Y, p.Z);
    }
}