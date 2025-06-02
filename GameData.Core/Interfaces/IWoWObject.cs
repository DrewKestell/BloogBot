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
        uint LastUpdated { get; }
    }
}