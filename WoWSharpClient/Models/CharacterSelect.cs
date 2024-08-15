using BotRunner.Constants;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class CharacterSelect
    {
        public ulong Guid { get; set; }
        public string Name { get; set; } = string.Empty;
        public Race Race { get; set; }
        public Class Class { get; set; }
        public byte Gender { get; set; }
        public byte Skin { get; set; }
        public byte Face { get; set; }
        public byte HairStyle { get; set; }
        public byte HairColor { get; set; }
        public byte FacialHair { get; set; }
        public byte Level { get; set; }
        public uint ZoneId { get; set; }
        public uint MapId { get; set; }
        public Position Position { get; set; } = new Position(0, 0, 0);
        public uint GuildId { get; set; }
        public CharacterFlags CharacterFlags { get; set; }
        public AtLoginFlags FirstLogin { get; set; }
        public uint PetDisplayId { get; set; }
        public uint PetLevel { get; set; }
        public uint PetFamily { get; set; }
        public List<(uint DisplayId, InventoryType InventoryType)> Equipment { get; set; } = [];
        public uint FirstBagDisplayId { get; set; }
        public byte FirstBagInventoryType { get; set; }
    }

    [Flags]
    public enum CharacterFlags : uint
    {
        CHARACTER_FLAG_NONE = 0x00000000,
        CHARACTER_FLAG_UNK1 = 0x00000001,
        CHARACTER_FLAG_UNK2 = 0x00000002,
        CHARACTER_LOCKED_FOR_TRANSFER = 0x00000004,
        CHARACTER_FLAG_UNK4 = 0x00000008,
        CHARACTER_FLAG_UNK5 = 0x00000010,
        CHARACTER_FLAG_UNK6 = 0x00000020,
        CHARACTER_FLAG_UNK7 = 0x00000040,
        CHARACTER_FLAG_UNK8 = 0x00000080,
        CHARACTER_FLAG_UNK9 = 0x00000100,
        CHARACTER_FLAG_UNK10 = 0x00000200,
        CHARACTER_FLAG_HIDE_HELM = 0x00000400,
        CHARACTER_FLAG_HIDE_CLOAK = 0x00000800,
        CHARACTER_FLAG_UNK13 = 0x00001000,
        CHARACTER_FLAG_GHOST = 0x00002000,
        CHARACTER_FLAG_RENAME = 0x00004000,
        CHARACTER_FLAG_UNK16 = 0x00008000,
        CHARACTER_FLAG_UNK17 = 0x00010000,
        CHARACTER_FLAG_UNK18 = 0x00020000,
        CHARACTER_FLAG_UNK19 = 0x00040000,
        CHARACTER_FLAG_UNK20 = 0x00080000,
        CHARACTER_FLAG_UNK21 = 0x00100000,
        CHARACTER_FLAG_UNK22 = 0x00200000,
        CHARACTER_FLAG_UNK23 = 0x00400000,
        CHARACTER_FLAG_UNK24 = 0x00800000,
        CHARACTER_FLAG_LOCKED_BY_BILLING = 0x01000000,
        CHARACTER_FLAG_DECLINED = 0x02000000,
        CHARACTER_FLAG_UNK27 = 0x04000000,
        CHARACTER_FLAG_UNK28 = 0x08000000,
        CHARACTER_FLAG_UNK29 = 0x10000000,
        CHARACTER_FLAG_UNK30 = 0x20000000,
        CHARACTER_FLAG_UNK31 = 0x40000000,
        CHARACTER_FLAG_UNK32 = 0x80000000
    }

    [Flags]
    public enum AtLoginFlags
    {
        AT_LOGIN_NONE = 0x00,
        AT_LOGIN_RENAME = 0x01,
        AT_LOGIN_RESET_SPELLS = 0x02,
        AT_LOGIN_RESET_TALENTS = 0x04,
        // AT_LOGIN_CUSTOMIZE         = 0x08, -- used in post-3.x
        // AT_LOGIN_RESET_PET_TALENTS = 0x10, -- used in post-3.x
        AT_LOGIN_FIRST = 0x20,
    }
}
