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
