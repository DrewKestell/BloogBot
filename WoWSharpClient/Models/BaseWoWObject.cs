using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public abstract class BaseWoWObject<T>(HighGuid highGuid, WoWObjectType objectType)
        : IWoWObject, IDeepCloneable<T> where T : BaseWoWObject<T>
    {
        public HighGuid HighGuid => highGuid;
        public ulong Guid => HighGuid.FullGuid;
        public WoWObjectType ObjectType => objectType;

        public uint Entry { get; set; }
        public float ScaleX { get; set; }
        public Position Position { get; set; } = new(0, 0, 0);
        public float Facing { get; set; }
        public uint LastUpdated { get; set; }

        public virtual void CopyFrom(T source)
        {
            Entry = source.Entry;
            ScaleX = source.ScaleX;
            Position = source.Position;
            Facing = source.Facing;
            LastUpdated = source.LastUpdated;
        }

        public abstract T Clone();
    }
}
