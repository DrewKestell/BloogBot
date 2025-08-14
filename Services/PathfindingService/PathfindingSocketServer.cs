using BotCommLayer;
using GameData.Core.Models;
using Pathfinding;
using PathfindingService.Repository;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using GameData.Core.Constants;
using GameData.Core.Enums;

namespace PathfindingService
{
    public class PathfindingSocketServer(string ipAddress, int port, ILogger logger) : ProtobufSocketServer<PathfindingRequest, PathfindingResponse>(ipAddress, port, logger)
    {
        private readonly Navigation _navigation = new();

        protected override PathfindingResponse HandleRequest(PathfindingRequest request)
        {
            try
            {
                return request.PayloadCase switch
                {
                    PathfindingRequest.PayloadOneofCase.Path => HandlePath(request.Path),
                    PathfindingRequest.PayloadOneofCase.Los => HandleLineOfSight(request.Los),
                    PathfindingRequest.PayloadOneofCase.Step => HandlePhysics(request.Step),
                    _ => ErrorResponse("Unknown or unset request type.")
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"[PathfindingSocketServer] Error: {ex.Message}\n{ex.StackTrace}");
                return ErrorResponse($"Internal error: {ex.Message}");
            }
        }

        private PathfindingResponse HandlePhysics(Pathfinding.PhysicsInput step)
        {
            var physicsInput = step.ToPhysicsInput();
            var physicsOutput = _navigation.StepPhysics(physicsInput, step.DeltaTime);
            return new PathfindingResponse { Step = physicsOutput.ToPhysicsOutput() };
        }

        private PathfindingResponse HandlePath(CalculatePathRequest req)
        {
            if (!CheckPosition(req.MapId, req.Start, req.End, out var err))
                return err;

            var start = new XYZ(req.Start.X, req.Start.Y, req.Start.Z);
            var end = new XYZ(req.End.X, req.End.Y, req.End.Z);
            var path = _navigation.CalculatePath(req.MapId, start, end, req.Straight);

            var resp = new CalculatePathResponse();
            resp.Corners.AddRange(path.Select(p => new Game.Position { X = p.X, Y = p.Y, Z = p.Z }));

            return new PathfindingResponse { Path = resp };
        }

        private PathfindingResponse HandleLineOfSight(LineOfSightRequest req)
        {
            if (!CheckPosition(req.MapId, req.From, req.To, out var err))
                return err;

            var from = new XYZ(req.From.X, req.From.Y, req.From.Z);
            var to = new XYZ(req.To.X, req.To.Y, req.To.Z);

            bool hasLOS = _navigation.LineOfSight(req.MapId, from, to);

            return new PathfindingResponse
            {
                Los = new LineOfSightResponse { InLos = hasLOS }
            };
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
        // Convert from Protobuf PhysicsInput to Navigation.PhysicsInput
        public static Repository.PhysicsInput ToPhysicsInput(this Pathfinding.PhysicsInput proto)
        {
            (float radius, float height) value = RaceDimensions.GetCapsuleForRace((Race)proto.Race, (Gender)proto.Gender);
            return new Repository.PhysicsInput
            {
                // Position and orientation
                x = proto.PosX,
                y = proto.PosY,
                z = proto.PosZ,
                orientation = proto.Facing,
                pitch = proto.SwimPitch,

                // Movement speeds
                walkSpeed = proto.WalkSpeed,
                runSpeed = proto.RunSpeed,
                swimSpeed = proto.SwimSpeed,
                flightSpeed = 7.0f, // Default flight speed
                runBackSpeed = proto.RunBackSpeed,

                // State
                moveFlags = proto.MovementFlags,
                mapId = proto.MapId,

                // Velocity
                vx = proto.VelX,
                vy = proto.VelY,
                vz = proto.VelZ,

                // Collision
                height = value.height,
                radius = value.radius,

                // Spline (not used)
                hasSplinePath = false,
                splineSpeed = 0,
                splinePoints = IntPtr.Zero,
                splinePointCount = 0,
                currentSplineIndex = 0,

                // Time
                deltaTime = proto.DeltaTime
            };
        }

        // Convert from Navigation.PhysicsOutput to Protobuf PhysicsOutput
        public static Pathfinding.PhysicsOutput ToPhysicsOutput(this Repository.PhysicsOutput nav)
        {
            return new Pathfinding.PhysicsOutput
            {
                NewPosX = nav.x,
                NewPosY = nav.y,
                NewPosZ = nav.z,
                NewVelX = nav.vx,
                NewVelY = nav.vy,
                NewVelZ = nav.vz,
                MovementFlags = nav.moveFlags,
                Orientation = nav.orientation,
                Pitch = nav.pitch,
                IsGrounded = nav.isGrounded,
                IsSwimming = nav.isSwimming,
                IsFlying = nav.isFlying,
                FallTime = nav.fallTime,
                CurrentSplineIndex = nav.currentSplineIndex,
                SplineProgress = nav.splineProgress
            };
        }
    }
}