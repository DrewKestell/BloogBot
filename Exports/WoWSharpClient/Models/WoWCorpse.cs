using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWCorpse(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Corpse) : WoWGameObject(highGuid, objectType), IWoWCorpse
    {
        public HighGuid OwnerGuid { get; set; } = new HighGuid(new byte[4], new byte[4]);

        public uint GhostTime { get; set; }

        public CorpseType Type { get; set; }

        public float Angle { get; set; }

        public CorpseFlags CorpseFlags { get; set; }

        public uint Guild { get; set; }

        public uint[] Items { get; set; } = new uint[64];

        public byte[] Bytes2 { get; set; } = new byte[4];

        public byte[] Bytes1 { get; set; } = new byte[4];

        public bool IsBones()
        {
            throw new NotImplementedException();
        }

        public bool IsPvP()
        {
            throw new NotImplementedException();
        }

        public override WoWObject Clone()
        {
            var clone = new WoWCorpse(HighGuid, ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);

            if (sourceBase is not WoWCorpse source)
                return;

            OwnerGuid = source.OwnerGuid;
            GhostTime = source.GhostTime;
            Type = source.Type;
            Angle = source.Angle;
            CorpseFlags = source.CorpseFlags;
            Guild = source.Guild;

            Array.Copy(source.Items, Items, Items.Length);
            Array.Copy(source.Bytes1, Bytes1, Bytes1.Length);
            Array.Copy(source.Bytes2, Bytes2, Bytes2.Length);
        }
    }
}
