using BotCommLayer;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using Pathfinding;

namespace BotRunner.Clients
{
    public class PathfindingClient(string ipAddress, int port, ILogger logger)
        : ProtobufSocketClient<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        public Position[] GetPath(uint mapId, Position start, Position end, bool smoothPath)
        {
            var response = SendMessage(new PathfindingRequest
            {
                RequestType = PathfindingRequestType.Path,
                MapId = mapId,
                Start = start.ToProto(),
                End = end.ToProto(),
                SmoothPath = smoothPath
            });

            return response.Path
                .Select(p => new Position(p.X, p.Y, p.Z))
                .ToArray() ?? [];
        }

        public float GetPathingDistance(uint mapId, Position start, Position end, bool smoothPath)
        {
            var response = SendMessage(new PathfindingRequest
            {
                RequestType = PathfindingRequestType.Distance,
                MapId = mapId,
                Start = start.ToProto(),
                End = end.ToProto(),
                SmoothPath = smoothPath
            });

            // FloatValue was flattened to float?, so just use the float directly
            return response.ZPoint.HasValue ? response.ZPoint.Value : 0f;
        }

        public float GetGroundZ(uint mapId, Position position)
        {
            var response = SendMessage(new PathfindingRequest
            {
                RequestType = PathfindingRequestType.ZCheck,
                MapId = mapId,
                Start = position.ToProto(),
                End = position.ToProto(),
                SmoothPath = true
            });

            return response.ZPoint.Value;
        }
    }

    public static class ProtoInteropExtensions
    {
        public static Game.Position ToProto(this XYZ xyz) =>
            new() { X = xyz.X, Y = xyz.Y, Z = xyz.Z };

        public static XYZ ToXYZ(this Position p) =>
            new(p.X, p.Y, p.Z);

        public static Game.Position ToProto(this Position p) =>
            new() { X = p.X, Y = p.Y, Z = p.Z };
    }
}
