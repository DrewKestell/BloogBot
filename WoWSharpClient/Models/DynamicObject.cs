using BotRunner.Base;
using BotRunner.Models;

namespace WoWSharpClient.Models
{
    public class DynamicObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.DynamicObj) : GameObject(highGuid, objectType)
    {
        public HighGuid Caster { get; set; } = new HighGuid(new byte[4], new byte[4]);
        public uint SpellId { get; set; } = 0;
        public float Radius { get; set; } = 0.0f;
        public byte[] Bytes { get; set; } = new byte[4];

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
