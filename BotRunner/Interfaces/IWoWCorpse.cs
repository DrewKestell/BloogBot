using BotRunner.Models;

namespace BotRunner.Interfaces
{
    public interface IWoWCorpse : IWoWObject
    {
        HighGuid OwnerGuid { get; set; }
        uint GhostTime { get; set; }
        CorpseType Type { get; set; }
        float Angle { get; set; }
        CorpseFlags Flags { get; set; }
        uint Guild { get; set; }
        uint[] Items { get; }
        byte[] Bytes2 { get; }
        byte[] Bytes1 { get; }

        bool IsBones();
        bool IsPvP();
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