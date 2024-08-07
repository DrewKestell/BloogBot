using BotRunner.Interfaces;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class Corpse : Object
    {
        public Corpse(byte[] lowGuid, byte[] highGuid) : base(lowGuid, highGuid, WoWObjectType.Corpse)
        {
            OwnerGuid = 0;
            GhostTime = 0;
            Type = CorpseType.CORPSE_BONES;
            Position = new Position(0, 0, 0);
            Angle = 0.0f;
        }

        public ulong OwnerGuid { get; set; }
        public uint GhostTime { get; set; }
        public CorpseType Type { get; set; }
        public float Angle { get; set; }

        public void Create(ulong ownerGuid, float x, float y, float z, float angle)
        {
            OwnerGuid = ownerGuid;
            Position = new Position(x, y, z);
            Angle = angle;
            GhostTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public bool IsBones()
        {
            return Type == CorpseType.CORPSE_BONES;
        }

        public bool IsPvP()
        {
            return Type == CorpseType.CORPSE_RESURRECTABLE_PVP;
        }
    }

    public enum CorpseType
    {
        CORPSE_BONES = 0,
        CORPSE_RESURRECTABLE_PVE = 1,
        CORPSE_RESURRECTABLE_PVP = 2
    }

    public enum CorpseFlags
    {
        CORPSE_FLAG_NONE = 0x00,
        CORPSE_FLAG_BONES = 0x01,
        CORPSE_FLAG_UNK1 = 0x02,
        CORPSE_FLAG_UNK2 = 0x04,
        CORPSE_FLAG_HIDE_HELM = 0x08,
        CORPSE_FLAG_HIDE_CLOAK = 0x10,
        CORPSE_FLAG_LOOTABLE = 0x20
    }
}
