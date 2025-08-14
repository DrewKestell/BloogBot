using BotCommLayer;
using GameData.Core.Enums;
using GameData.Core.Models;
using Pathfinding; // Proto-generated C# files
using PathfindingService.Repository;
using System;
using System.Linq;

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
                return request.PayloadCase switch
                {
                    PathfindingRequest.PayloadOneofCase.Path => HandlePath(request.Path),
                    PathfindingRequest.PayloadOneofCase.Los => HandleLineOfSight(request.Los),
                    PathfindingRequest.PayloadOneofCase.Step => HandlePhysics(request.Step),
                    PathfindingRequest.PayloadOneofCase.VmapHeight => HandleVMapHeight(request.VmapHeight),
                    _ => ErrorResponse("Unknown or unset request type.")
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"[PathfindingSocketServer] Error: {ex.Message}\n{ex.StackTrace}");
                return ErrorResponse($"Internal error: {ex.Message}");
            }
        }

        private PathfindingResponse HandlePhysics(PhysicsInput step)
        {
            Navigation.PhysicsOutput physicsOutput = _navigation.StepPhysics(step.ToPhysicsInput(), step.DeltaTime);
            return new PathfindingResponse { Step = physicsOutput.ToPhysicsOutput() };
        }

        private PathfindingResponse HandlePath(CalculatePathRequest req)
        {
            if (!CheckPosition(req.MapId, req.Start, req.End, out var err))
                return err;

            var start = new Position(req.Start.X, req.Start.Y, req.Start.Z);
            var end = new Position(req.End.X, req.End.Y, req.End.Z);
            var path = _navigation.CalculatePath(req.MapId, start.ToXYZ(), end.ToXYZ(), req.Straight);

            var resp = new CalculatePathResponse();
            resp.Corners.AddRange(path.Select(p => new Game.Position { X = p.X, Y = p.Y, Z = p.Z }));

            return new PathfindingResponse { Path = resp };
        }

        private PathfindingResponse HandleLineOfSight(LineOfSightRequest req)
        {
            if (!CheckPosition(req.MapId, req.From, req.To, out var err))
                return err;

            var from = new Position(req.From.X, req.From.Y, req.From.Z);
            var to = new Position(req.To.X, req.To.Y, req.To.Z);

            bool hasLOS = _navigation.LineOfSight(req.MapId, from.ToXYZ(), to.ToXYZ());

            return new PathfindingResponse
            {
                Los = new LineOfSightResponse { InLos = hasLOS }
            };
        }

        private PathfindingResponse HandleVMapHeight(VMapHeightRequest req)
        {
            if (req.Position == null)
                return ErrorResponse("Position is required");

            var pos = new Position(req.Position.X, req.Position.Y, req.Position.Z);
            float height = _navigation.GetGroundHeight(req.MapId, pos.X, pos.Y, pos.Z, req.SearchDistance);

            return new PathfindingResponse
            {
                VmapHeight = new VMapHeightResponse { Height = height }
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
        public static Navigation.PhysicsInput ToPhysicsInput(this PhysicsInput proto)
        {
            var input = new Navigation.PhysicsInput
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
                flightSpeed = 7.0f, // Default, not in proto
                runBackSpeed = proto.RunBackSpeed,

                // State
                moveFlags = proto.MovementFlags,
                mapId = proto.MapId,

                // Physics modifiers
                vx = 0,
                vy = 0,
                vz = proto.JumpVerticalSpeed,

                // Collision
                height = proto.Height,
                radius = proto.Radius,

                // Spline (not used in current proto)
                hasSplinePath = false,
                splineSpeed = 0,
                splinePoints = IntPtr.Zero,
                splinePointCount = 0,
                currentSplineIndex = 0,

                // Time
                deltaTime = 0.016f // Will be overridden by parameter
            };

            return input;
        }

        // Convert from Navigation.PhysicsOutput to Protobuf PhysicsOutput
        public static PhysicsOutput ToPhysicsOutput(this Navigation.PhysicsOutput nav)
        {
            return new PhysicsOutput
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
                Collided = nav.collided,
                GroundZ = nav.groundZ,
                LiquidZ = nav.liquidZ,
                FallDistance = nav.fallDistance,
                FallTime = nav.fallTime,
                CurrentSplineIndex = nav.currentSplineIndex,
                SplineProgress = nav.splineProgress
            };
        }
    }
}