using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.GameObj) : IWoWObject
    {
        public HighGuid HighGuid => highGuid;
        public ulong Guid => HighGuid.FullGuid;
        public WoWObjectType ObjectType => objectType;
        public uint Entry { get; set; }
        public float ScaleX { get; set; }
        public Position Position { get; set; } = new Position(0, 0, 0);
        public float Facing { get; set; }
        public ulong TransportGuid { get; internal set; }
        public Position TransportOffset { get; } = new Position(0, 0, 0);
        public float TransportOrientation { get; set; }
        public float SwimPitch { get; set; }
        public float JumpVerticalSpeed { get; set; }
        public float JumpSinAngle { get; set; }
        public float JumpCosAngle { get; set; }
        public float JumpHorizontalSpeed { get; set; }
        public float SplineElevation { get; set; }
        public uint LastUpdated { get; set; }
        public uint TransportLastUpdated { get; set; }
        public SplineFlags SplineFlags { get; set; }
        public Position SplineFinalPoint { get; set; } = new Position(0, 0, 0);
        public ulong SplineTargetGuid { get; set; }
        public float SplineFinalOrientation { get; set; }

        public int SplineTimePassed { get; set; }
        public int SplineDuration { get; set; }
        public uint SplineId { get; set; }
        public List<Position> SplineNodes { get; set; } = [];
        public Position SplineFinalDestination { get; set; } = new Position(0, 0, 0);

    }
}
