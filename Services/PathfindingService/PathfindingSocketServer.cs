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
                    PathfindingRequest.PayloadOneofCase.CapsuleOverlap => HandleOverlap(request.CapsuleOverlap),
                    PathfindingRequest.PayloadOneofCase.LosQuery => HandleLineOfSight(request.LosQuery),
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

            var adtZs = _navigation.GetADTHeight(req.MapId, req.Position.X, req.Position.Y);
            float raycastZ = _navigation.GetFloorHeight(req.MapId, req.Position.X, req.Position.Y, req.Position.Z);

            var resp = new ZQueryResponse
            {
                ZResult = new ZQueryResult
                {
                    TerrainZ = raycastZ,
                    AdtZ = adtZs.Item1,
                    LocationZ = float.NegativeInfinity,
                    WaterLevel = adtZs.Item2,
                }
            };
            return new PathfindingResponse { ZQuery = resp };
        }
        private PathfindingResponse HandleLineOfSight(LOSRequest req)
        {
            if (!CheckPosition(req.MapId, req.From, req.To, out var err))
                return err;

            var from = new Position(req.From.ToXYZ());
            var to = new Position(req.To.ToXYZ());

            bool hasLOS = _navigation.IsLineOfSight(req.MapId, from, to);

            return new PathfindingResponse
            {
                LosQuery = new LOSResponse { IsInLos = hasLOS }
            };
        }
        private PathfindingResponse HandleOverlap(CapsuleOverlapRequest req)
        {
            if (!CheckPosition(req.MapId, req.Position, out var err))
                return err;

            var pos = new Position(req.Position.ToXYZ());
            var (radius, height) = RaceDimensions.GetCapsuleForRace(req.Race);

            var polys = _navigation.GetCapsuleOverlaps(req.MapId, pos, radius, height);

            var overlapResp = new CapsuleOverlapResponse();
            foreach (var poly in polys)
            {
                overlapResp.Hits.Add(new NavPolyHit
                {
                    RefId = poly.RefId,
                    Area = poly.Area,
                    Flags = poly.Flags,
                    VertCount = poly.VertCount,
                    Verts = { poly.Verts.Select(v => v.ToProto()) }
                });
            }

            return new PathfindingResponse { CapsuleOverlap = overlapResp };
        }
        // ------------- Validation and Helpers ----------------

        private static bool CheckPosition(uint mapId, Game.Position a, Game.Position b, out PathfindingResponse error)
        {
            if (mapId == 0 || a == null || b == null)
            {
                error = ErrorResponse("Missing or invalid MapId/start/end.");
                return false;
            }
            error = null!;
            return true;
        }

        private static bool CheckPosition(uint mapId, Game.Position a, out PathfindingResponse error)
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
