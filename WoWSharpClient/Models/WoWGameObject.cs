using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWGameObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.GameObj) : WoWObject(highGuid, objectType), IWoWGameObject
    {
        public HighGuid CreatedBy { get; } = new HighGuid(new byte[4], new byte[4]);
        public uint DisplayId { get; set; }
        public uint Flags { get; set; }
        public float[] Rotation { get; } = new float[32];
        public GOState GoState { get; set; }
        public DynamicFlags DynamicFlags { get; set; }
        public uint FactionTemplate { get; set; }
        public uint TypeId { get; set; }
        public uint Level { get; set; }
        public uint ArtKit { get; set; }
        public uint AnimProgress { get; set; }
        public bool InLosWith(Position position)
        {
            throw new NotImplementedException();
        }

        public bool InLosWith(IWoWObject objc)
        {
            throw new NotImplementedException();
        }

        public bool IsBehind(IWoWObject target)
        {
            throw new NotImplementedException();
        }

        public bool IsFacing(Position position)
        {
            throw new NotImplementedException();
        }

        public bool IsFacing(IWoWObject objc)
        {
            throw new NotImplementedException();
        }
        public Position GetPointBehindUnit(float distance)
        {
            throw new NotImplementedException();
        }

        public void Interact()
        {
            throw new NotImplementedException();
        }
        public string Name { get; set; } = string.Empty;
    }
}
