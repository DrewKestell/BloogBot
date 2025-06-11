using BotCommLayer;
using GameData.Core.Models;
using Pathfinding; // Proto-generated C# files
using PathfindingService.Repository;

namespace PathfindingService
{
    public class PathfindingSocketServer(string ipAddress, int port, ILogger logger)
        : ProtobufSocketServer<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        private readonly Navigation _navigation = new();

        protected override PathfindingResponse HandleRequest(PathfindingRequest request)
        {
            try
            {
                // Dispatch on request type using oneof
                return request.PayloadCase switch
                {
                    PathfindingRequest.PayloadOneofCase.Path => HandlePath(request.Path),
                    PathfindingRequest.PayloadOneofCase.Distance => HandleDistance(request.Distance),
                    _ => ErrorResponse("Unknown or unset request type."),
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"[PathfindingSocketServer] Error: {ex.Message}\n{ex.StackTrace}");
                return ErrorResponse($"Internal error: {ex.Message}");
            }
        }

        private PathfindingResponse HandlePath(PathRequest req)
        {
            if (!CheckPosition(req.MapId, req.Start, req.End, out var err))
                return err;

            var start = new Position(req.Start.ToXYZ());
            var end = new Position(req.End.ToXYZ());
            var path = _navigation.CalculatePath(req.MapId, start, end, req.SmoothPath);

            var resp = new PathResponse();
            resp.Path.AddRange(path.Select(p => p.ToXYZ().ToProto()));

            return new PathfindingResponse { Path = resp };
        }

        private PathfindingResponse HandleDistance(DistanceRequest req)
        {
            if (!CheckPosition(req.MapId, req.Start, req.End, out var err))
                return err;

            var start = new Position(req.Start.ToXYZ());
            var end = new Position(req.End.ToXYZ());
            var path = _navigation.CalculatePath(req.MapId, start, end, false);

            float totalDistance = 0f;
            for (int i = 0; i < path.Length - 1; i++)
                totalDistance += path[i].DistanceTo(path[i + 1]);

            var resp = new DistanceResponse { Distance = totalDistance };
            return new PathfindingResponse { Distance = resp };
        }

        // ------------- Validation and Helpers ----------------

        private bool CheckPosition(uint mapId, Game.Position a, Game.Position b, out PathfindingResponse error)
        {
            if (mapId == 0 || a == null || b == null)
            {
                error = ErrorResponse("Missing or invalid MapId/start/end.");
                return false;
            }
            error = null!;
            return true;
        }

        private static PathfindingResponse ErrorResponse(string msg)
        {
            return new PathfindingResponse
            {
                Error = new Error { Message = msg }
            };
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
