using BotCommLayer;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using Pathfinding;

namespace BotRunner.Clients
{
    public class PathfindingClient(string ipAddress, int port, ILogger logger)
        : ProtobufSocketClient<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        public Position[] GetPath(uint mapId, Position start, Position end, bool smoothPath = false)
        {
            var request = new PathfindingRequest
            {
                Path = new PathRequest
                {
                    MapId = mapId,
                    Start = start.ToProto(),
                    End = end.ToProto(),
                    SmoothPath = smoothPath
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            return response.Path.Path
                .Select(p => new Position(p.X, p.Y, p.Z))
                .ToArray();
        }

        public float GetPathingDistance(uint mapId, Position start, Position end)
        {
            var request = new PathfindingRequest
            {
                Distance = new DistanceRequest
                {
                    MapId = mapId,
                    Start = start.ToProto(),
                    End = end.ToProto()
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            return response.Distance.Distance;
        }

        public ZQueryResult GetZQuery(uint mapId, Position position)
        {
            var request = new PathfindingRequest
            {
                ZQuery = new ZQueryRequest
                {
                    MapId = mapId,
                    Position = position.ToProto()
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            // Maps directly to proto-generated ZQueryResult
            var z = response.ZQuery.ZResult;
            return new ZQueryResult
            {
                FloorZ = z.FloorZ,
                RaycastZ = z.RaycastZ,
                TerrainZ = z.TerrainZ,
                AdtZ = z.AdtZ,
                LocationZ = z.LocationZ
            };
        }

        public bool IsInLineOfSight(uint mapId, Position from, Position to)
        {
            var request = new PathfindingRequest
            {
                LosQuery = new LOSRequest
                {
                    MapId = mapId,
                    From = from.ToProto(),
                    To = to.ToProto()
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            return response.LosQuery.IsInLos;
        }

        public (uint Flags, int AdtId, int RootId, int GroupId) GetAreaInfo(uint mapId, Position position)
        {
            var request = new PathfindingRequest
            {
                AreaInfo = new AreaInfoRequest
                {
                    MapId = mapId,
                    Position = position.ToProto()
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            var a = response.AreaInfo;
            return (a.AreaFlags, a.AdtId, a.RootId, a.GroupId);
        }

        public (float Level, float Floor, uint Type) GetLiquidLevel(uint mapId, Position position, byte liquidType)
        {
            var request = new PathfindingRequest
            {
                LiquidLevel = new LiquidLevelRequest
                {
                    MapId = mapId,
                    Position = position.ToProto(),
                    ReqLiquidType = liquidType
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            var l = response.LiquidLevel;
            return (l.Level, l.Floor, l.Type);
        }
    }

    public static class ProtoInteropExtensions
    {
        public static Game.Position ToProto(this XYZ xyz) =>
            new() { X = xyz.X, Y = xyz.Y, Z = xyz.Z };

        public static XYZ ToXYZ(this Game.Position p) =>
            new(p.X, p.Y, p.Z);

        public static Game.Position ToProto(this Position p) =>
            new() { X = p.X, Y = p.Y, Z = p.Z };
    }
}
