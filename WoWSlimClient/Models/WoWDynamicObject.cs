

namespace WoWSlimClient.Models
{
    public class WoWDynamicObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.DynamicObj) : WoWGameObject(lowGuid, highGuid, objectType)
    {
        public ulong Caster { get; set; }
        public byte[] Bytes { get; set; }
        public uint SpellId { get; set; }
        public float Radius { get; set; }
    }
}
