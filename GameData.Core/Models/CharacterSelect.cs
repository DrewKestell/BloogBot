using GameData.Core.Enums;

namespace GameData.Core.Models
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
}
