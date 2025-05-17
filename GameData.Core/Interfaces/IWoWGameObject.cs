using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWGameObject : IWoWObject
    {
        HighGuid CreatedBy { get; }
        uint DisplayId { get; }
        uint Flags { get; }
        float[] Rotation { get; }
        GOState GoState { get; }
        DynamicFlags DynamicFlags { get; }
        uint FactionTemplate { get; }
        uint TypeId { get; }
        uint Level { get; }
        uint ArtKit { get; }
        uint AnimProgress { get; }
        bool CanBeLooted => DynamicFlags.HasFlag(DynamicFlags.CanBeLooted);
        bool TappedByOther => DynamicFlags.HasFlag(DynamicFlags.Tapped) && !DynamicFlags.HasFlag(DynamicFlags.TappedByMe);
        Position GetPointBehindUnit(float distance);
        void Interact();
        bool InLosWith(Position position);
        bool InLosWith(IWoWObject objc);
        bool IsFacing(Position position);
        bool IsFacing(IWoWObject objc);
        bool IsBehind(IWoWObject target);
        string Name { get; }
    }
}
