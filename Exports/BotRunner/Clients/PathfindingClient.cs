using BotCommLayer;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using Pathfinding;
using System;
using System.Linq;

namespace BotRunner.Clients
{
    public class PathfindingClient : ProtobufSocketClient<PathfindingRequest, PathfindingResponse>
    {
        public PathfindingClient() : base() { }

        public PathfindingClient(string ipAddress, int port, ILogger logger)
            : base(ipAddress, port, logger) { }

        public virtual Position[] GetPath(uint mapId, Position start, Position end, bool smoothPath = false)
        {
            var request = new PathfindingRequest
            {
                Path = new CalculatePathRequest
                {
                    MapId = mapId,
                    Start = start.ToProto(),
                    End = end.ToProto(),
                    Straight = smoothPath
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            return response.Path.Corners
                .Select(p => new Position(p.X, p.Y, p.Z))
                .ToArray();
        }

        public virtual float GetPathingDistance(uint mapId, Position start, Position end)
        {
            var path = GetPath(mapId, start, end);
            float distance = 0f;

            for (int i = 0; i < path.Length - 1; i++)
                distance += path[i].DistanceTo(path[i + 1]);

            return distance;
        }

        public virtual bool IsInLineOfSight(uint mapId, Position from, Position to)
        {
            var request = new PathfindingRequest
            {
                Los = new LineOfSightRequest
                {
                    MapId = mapId,
                    From = from.ToProto(),
                    To = to.ToProto()
                }
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            return response.Los.InLos;
        }

        public virtual PhysicsOutput PhysicsStep(PhysicsInput physicsInput)
        {
            var request = new PathfindingRequest
            {
                Step = physicsInput
            };

            var response = SendMessage(request);

            if (response.PayloadCase == PathfindingResponse.PayloadOneofCase.Error)
                throw new Exception(response.Error.Message);

            return response.Step;
        }
    }

    public static class ProtoInteropExtensions
    {
        public static Game.Position ToProto(this Position p) =>
            new() { X = p.X, Y = p.Y, Z = p.Z };
    }
}