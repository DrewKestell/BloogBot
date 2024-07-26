namespace WoWSlimClient.Models
{
    public class WoWDynamicObject : WoWGameObject
    {
        public WoWDynamicObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.DynamicObj)
            : base(lowGuid, highGuid, objectType)
        {
            Caster = 0;
            SpellId = 0;
            Radius = 0.0f;
        }

        public ulong Caster { get; set; }
        public uint SpellId { get; set; }
        public float Radius { get; set; }
        public byte[] Bytes { get; set; }

        public void SetCaster(ulong caster)
        {
            Caster = caster;
        }

        public void SetSpellId(uint spellId)
        {
            SpellId = spellId;
        }

        public void SetRadius(float radius)
        {
            Radius = radius;
        }

        public void Remove()
        {
            // Implementation to remove dynamic object
        }

        public void Update(uint time)
        {
            // Implementation to update dynamic object state
        }
    }

    public enum DynamicObjectType
    {
        Portal = 0x0,          // unused
        AreaSpell = 0x1,
        FarsightFocus = 0x2,
    }
}
