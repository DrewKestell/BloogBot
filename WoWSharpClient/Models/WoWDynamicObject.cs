using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWDynamicObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.DynamicObj) : WoWObject(highGuid, objectType), IWoWDynamicObject
    {
        public HighGuid Caster { get; private set; } = new(new byte[4], new byte[4]);
        public byte[] Bytes { get; set; } = new byte[4];
        public uint SpellId { get; set; }
        public float Radius { get; set; }

        public override WoWDynamicObject Clone()
        {
            var clone = new WoWDynamicObject(this.HighGuid, this.ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);

            if (sourceBase is not WoWDynamicObject source) return;

            Caster = source.Caster;
            SpellId = source.SpellId;
            Radius = source.Radius;

            if (Bytes.Length != source.Bytes.Length)
                Bytes = new byte[source.Bytes.Length];

            Array.Copy(source.Bytes, Bytes, Bytes.Length);
        }
    }

}
