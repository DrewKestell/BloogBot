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
    }
}
