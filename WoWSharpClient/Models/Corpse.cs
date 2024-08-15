using BotRunner.Base;
using BotRunner.Interfaces;
using BotRunner.Models;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class Corpse : Object, IWoWCorpse
    {
        public Corpse(HighGuid highGuid) : base(highGuid, WoWObjectType.Corpse)
        {
            OwnerGuid = new HighGuid(new byte[4], new byte[4]);
            GhostTime = 0;
            Type = CorpseType.CORPSE_BONES;
            Position = new Position(0, 0, 0);
            Angle = 0.0f;
        }

        public HighGuid OwnerGuid { get; set; }
        public uint GhostTime { get; set; }
        public CorpseType Type { get; set; }
        public float Angle { get; set; }
        public CorpseFlags Flags { get; set; }
        public uint Guild { get; set; }
        public uint[] Items { get; set; } = new uint[19];
        public byte[] Bytes2 { get; set; } = new byte[4];
        public byte[] Bytes1 { get; set; } = new byte[4];

        public bool IsBones()
        {
            return Type == CorpseType.CORPSE_BONES;
        }

        public bool IsPvP()
        {
            return Type == CorpseType.CORPSE_RESURRECTABLE_PVP;
        }
    }
}
