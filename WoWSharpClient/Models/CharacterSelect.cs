namespace WoWSharpClient.Models
{
    public class CharacterSelect
    {
        public ulong Guid;
        public string Name = string.Empty;
        public byte Race;
        public byte CharacterClass;
        public byte Gender;
        public byte Skin;
        public byte Face;
        public byte HairStyle;
        public byte HairColor;
        public byte FacialHair;
        public byte Level;

        public uint ZoneId;
        public uint MapId;
        public float X;
        public float Y;
        public float Z;

        public byte[] Equipment = [];

        public uint GuildId { get; internal set; }
        public uint Flags { get; internal set; }
        public byte FirstLogin { get; internal set; }
    }
}
