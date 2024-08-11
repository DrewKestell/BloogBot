using BotRunner.Interfaces;

namespace WoWSharpClient.Models
{
    public class DynamicObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.DynamicObj) : GameObject(lowGuid, highGuid, objectType)
    {
        public ulong Caster { get; set; } = 0;
        public uint SpellId { get; set; } = 0;
        public float Radius { get; set; } = 0.0f;
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
