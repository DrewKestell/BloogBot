using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWGameObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.GameObj) : WoWObject(highGuid, objectType), IWoWGameObject
    {
        public HighGuid CreatedBy { get; private set; } = new(new byte[4], new byte[4]);
        public uint DisplayId { get; set; }
        public uint Flags { get; set; }
        public float[] Rotation { get; private set; } = new float[32];
        public GOState GoState { get; set; }
        public DynamicFlags DynamicFlags { get; set; }
        public uint FactionTemplate { get; set; }
        public uint TypeId { get; set; }
        public uint Level { get; set; }
        public uint ArtKit { get; set; }
        public uint AnimProgress { get; set; }
        public string Name { get; set; } = string.Empty;

        public override WoWObject Clone()
        {
            var clone = new WoWGameObject(HighGuid, ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);

            if (sourceBase is not WoWGameObject source) return;

            CreatedBy = source.CreatedBy;
            DisplayId = source.DisplayId;
            Flags = source.Flags;
            GoState = source.GoState;
            DynamicFlags = source.DynamicFlags;
            FactionTemplate = source.FactionTemplate;
            TypeId = source.TypeId;
            Level = source.Level;
            ArtKit = source.ArtKit;
            AnimProgress = source.AnimProgress;
            Name = source.Name;

            if (Rotation.Length != source.Rotation.Length)
                Rotation = new float[source.Rotation.Length];

            Array.Copy(source.Rotation, Rotation, Rotation.Length);
        }

        public Position GetPointBehindUnit(float distance)
        {
            throw new NotImplementedException();
        }

        public void Interact()
        {
            throw new NotImplementedException();
        }
    }
}
