using BotRunner.Base;
using BotRunner.Models;
using PathfindingService.Models;

namespace BotRunner.Interfaces
{
    public interface IWoWObject
    {
        nint Pointer { get; }
        ulong Guid { get; }
        HighGuid HighGuid { get; }
        WoWObjectType ObjectType { get; }
        uint LastUpated { get; }
        uint Entry { get; }
        float ScaleX { get; }
        float Height { get; }
        float Facing { get; }
        Position Position { get; }
        bool InWorld { get; }
        bool InLosWith(Position position);
        bool InLosWith(IWoWObject objc);
        bool IsFacing(Position position);
        bool IsFacing(IWoWObject objc);
        bool IsBehind(IWoWObject target);
    }
}