using GameData.Core.Enums;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class MovementBlockUpdate
    {
        // Speeds
        public float WalkSpeed { get; set; }
        public float RunSpeed { get; set; }
        public float RunBackSpeed { get; set; }
        public float SwimSpeed { get; set; }
        public float SwimBackSpeed { get; set; }
        public float TurnRate { get; set; }

        // Spline Data
        public SplineFlags? SplineFlags { get; set; }
        public Position? SplineFinalPoint { get; set; }
        public ulong? SplineTargetGuid { get; set; }
        public float? SplineFinalOrientation { get; set; }

        public int? SplineTimePassed { get; set; }
        public int? SplineDuration { get; set; }
        public uint? SplineId { get; set; }

        public List<Position>? SplineNodes { get; set; } = [];
        public Position? SplineFinalDestination { get; set; }
        public SplineType SplineType { get; internal set; }
        public ulong FacingTargetGuid { get; internal set; }
        public float FacingAngle { get; internal set; }
        public Position FacingSpot { get; internal set; } = new Position(0, 0, 0);
        public uint SplineTimestamp { get; internal set; }
        public List<Position> SplinePoints { get; internal set; } = [];
        public uint HighGuid { get; internal set; }
        public uint UpdateAll { get; internal set; }
        public ulong TargetGuid { get; internal set; }

        public MovementBlockUpdate Clone()
        {
            return new MovementBlockUpdate
            {
                WalkSpeed = WalkSpeed,
                RunSpeed = RunSpeed,
                RunBackSpeed = RunBackSpeed,
                SwimSpeed = SwimSpeed,
                SwimBackSpeed = SwimBackSpeed,
                TurnRate = TurnRate,
                SplineFlags = SplineFlags,
                SplineFinalPoint = SplineFinalPoint,
                SplineTargetGuid = SplineTargetGuid,
                SplineFinalOrientation = SplineFinalOrientation,
                SplineTimePassed = SplineTimePassed,
                SplineDuration = SplineDuration,
                SplineId = SplineId,
                SplineNodes = SplineNodes != null ? [.. SplineNodes] : null,
                SplineFinalDestination = SplineFinalDestination
            };
        }
    }
    public class MovementInfoUpdate
    {
        public ulong Guid { get; set; }
        public ulong TargetGuid { get; set; }
        public uint HighGuid { get; set; }
        public uint UpdateAll { get; set; }
        public uint MovementCounter { get; set; }

        public MovementFlags MovementFlags { get; set; }
        public uint LastUpdated { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Facing { get; set; }

        // Transport Info
        public bool HasTransport => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ONTRANSPORT);
        public ulong? TransportGuid { get; set; }
        public Position? TransportOffset { get; set; }
        public float? TransportOrientation { get; set; }
        public uint? TransportLastUpdated { get; set; }

        // Swim Info
        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);
        public float? SwimPitch { get; set; }

        // Fall / Jump Info
        public uint FallTime { get; set; }
        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_JUMPING);
        public float? JumpVerticalSpeed { get; set; }
        public float? JumpSinAngle { get; set; }
        public float? JumpCosAngle { get; set; }
        public float? JumpHorizontalSpeed { get; set; }

        // Spline Elevation
        public bool HasSplineElevation => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ELEVATION);
        public float? SplineElevation { get; set; }

        public MovementBlockUpdate? MovementBlockUpdate { get; set; }
        public bool HasSpline => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ENABLED);

        public MovementInfoUpdate Clone()
        {
            return new MovementInfoUpdate
            {
                Guid = Guid,
                MovementFlags = MovementFlags,
                LastUpdated = LastUpdated,
                X = X,
                Y = Y,
                Z = Z,
                Facing = Facing,
                TransportGuid = TransportGuid,
                TransportOffset = TransportOffset,
                TransportOrientation = TransportOrientation,
                TransportLastUpdated = TransportLastUpdated,
                SwimPitch = SwimPitch,
                FallTime = FallTime,
                JumpVerticalSpeed = JumpVerticalSpeed,
                JumpSinAngle = JumpSinAngle,
                JumpCosAngle = JumpCosAngle,
                JumpHorizontalSpeed = JumpHorizontalSpeed,
                SplineElevation = SplineElevation,
                MovementBlockUpdate = MovementBlockUpdate,
            };
        }
    }
}
