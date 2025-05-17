using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWDynamicObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.DynamicObj) : WoWObject(highGuid, objectType), IWoWDynamicObject
    {
        public HighGuid Caster { get; } = new HighGuid(new byte[4], new byte[4]);
        public byte[] Bytes { get; set; } = new byte[4];
        public uint SpellId { get; set; }
        public float Radius { get; set; }
    }
}
