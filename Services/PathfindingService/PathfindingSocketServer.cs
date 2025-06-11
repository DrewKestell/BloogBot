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
                    PathfindingRequest.PayloadOneofCase.ZQuery => HandleZQuery(request.ZQuery),
                    PathfindingRequest.PayloadOneofCase.LosQuery => HandleLOS(request.LosQuery),
                    PathfindingRequest.PayloadOneofCase.AreaInfo => HandleAreaInfo(request.AreaInfo),
                    PathfindingRequest.PayloadOneofCase.LiquidLevel => HandleLiquidLevel(request.LiquidLevel),
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

        private PathfindingResponse HandleZQuery(ZQueryRequest req)
        {
            if (!CheckPosition(req.MapId, req.Position, out var err))
                return err;

            var pos = new Position(req.Position.ToXYZ());
            var z = _navigation.QueryZ(req.MapId, pos);

            var resp = new ZQueryResponse
            {
                ZResult = new ZQueryResult
                {
                    FloorZ = z.FloorZ,
                    RaycastZ = z.RaycastZ,
                    TerrainZ = z.TerrainZ,
                    AdtZ = z.AdtZ,
                    LocationZ = z.LocationZ
                }
            };
            return new PathfindingResponse { ZQuery = resp };
        }

        private PathfindingResponse HandleLOS(LOSRequest req)
        {
            if (!CheckPosition(req.MapId, req.From, req.To, out var err))
                return err;

            var a = new Position(req.From.ToXYZ());
            var b = new Position(req.To.ToXYZ());
            var los = _navigation.IsInLineOfSight(req.MapId, a, b);

            var resp = new LOSResponse { IsInLos = los };
            return new PathfindingResponse { LosQuery = resp };
        }

        private PathfindingResponse HandleAreaInfo(AreaInfoRequest req)
        {
            if (!CheckPosition(req.MapId, req.Position, out var err))
                return err;

            var pos = new Position(req.Position.ToXYZ());
            if (_navigation.TryGetAreaInfo(req.MapId, pos, out uint flags, out int adtId, out int rootId, out int groupId))
            {
                var resp = new AreaInfoResponse
                {
                    AreaFlags = flags,
                    AdtId = adtId,
                    RootId = rootId,
                    GroupId = groupId
                };
                return new PathfindingResponse { AreaInfo = resp };
            }
            return ErrorResponse("Area info query failed.");
        }

        private PathfindingResponse HandleLiquidLevel(LiquidLevelRequest req)
        {
            if (!CheckPosition(req.MapId, req.Position, out var err))
                return err;

            var pos = new Position(req.Position.ToXYZ());
            if (_navigation.TryGetLiquidLevel(req.MapId, pos, (byte)req.ReqLiquidType, out float level, out float floor, out uint type))
            {
                var resp = new LiquidLevelResponse
                {
                    Level = level,
                    Floor = floor,
                    Type = type
                };
                return new PathfindingResponse { LiquidLevel = resp };
            }
            return ErrorResponse("Liquid level query failed.");
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

        private bool CheckPosition(uint mapId, Game.Position a, out PathfindingResponse error)
        {
            if (mapId == 0 || a == null)
            {
                error = ErrorResponse("Missing or invalid MapId/position.");
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
