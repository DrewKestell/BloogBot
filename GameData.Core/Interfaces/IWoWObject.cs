using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWObject
    {
        HighGuid HighGuid { get; }
        ulong Guid { get; }
        WoWObjectType ObjectType { get; }
        uint Entry { get; }
        float ScaleX { get; }
        Position? Position { get; }
        float Facing { get; }
        ulong TransportGuid { get; }
        float TransportOrientation { get; }
        float SwimPitch { get; }
        float JumpVerticalSpeed { get; }
        float JumpSinAngle { get; }
        float JumpCosAngle { get; }
        float JumpHorizontalSpeed { get; }
        float SplineElevation { get; }
        uint LastUpdated { get; }
        uint TransportLastUpdated { get; }
        SplineFlags SplineFlags { get; set; }
        Position SplineFinalPoint { get; set; }
        ulong SplineTargetGuid { get; set; }
        float SplineFinalOrientation { get; set; }

        int SplineTimePassed { get; set; }
        int SplineDuration { get; set; }
        uint SplineId { get; set; }
        List<Position> SplineNodes { get; set; }
        Position SplineFinalDestination { get; set; }
    }
}