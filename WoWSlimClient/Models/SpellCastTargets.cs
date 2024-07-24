using System.Text;

namespace WoWSlimClient.Models
{
    public class SpellCastTargets
    {
        public ulong UnitTargetGUID { get; set; }
        public ulong GOTargetGUID { get; set; }
        public ulong CorpseTargetGUID { get; set; }
        public ulong ItemTargetGUID { get; set; }
        public uint ItemTargetEntry { get; set; }
        public float SrcX { get; set; }
        public float SrcY { get; set; }
        public float SrcZ { get; set; }
        public float DestX { get; set; }
        public float DestY { get; set; }
        public float DestZ { get; set; }
        public string StrTarget { get; set; }
        public ushort TargetMask { get; set; }

        public void Read(BinaryReader reader)
        {
            // Reading the SpellCastTargets structure with length checks
            if (reader.BaseStream.Position + sizeof(ulong) * 4 + sizeof(uint) + sizeof(float) * 6 + sizeof(ushort) > reader.BaseStream.Length)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading SpellCastTargets.");
            }

            UnitTargetGUID = reader.ReadUInt64();
            GOTargetGUID = reader.ReadUInt64();
            CorpseTargetGUID = reader.ReadUInt64();
            ItemTargetGUID = reader.ReadUInt64();
            ItemTargetEntry = reader.ReadUInt32();
            SrcX = reader.ReadSingle();
            SrcY = reader.ReadSingle();
            SrcZ = reader.ReadSingle();
            DestX = reader.ReadSingle();
            DestY = reader.ReadSingle();
            DestZ = reader.ReadSingle();
            TargetMask = reader.ReadUInt16();

            // Read the string target if there's enough data
            StrTarget = ReadString(reader);
        }

        private string ReadString(BinaryReader reader)
        {
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
            {
                return string.Empty;
            }

            var length = reader.ReadByte();
            if (reader.BaseStream.Position + length > reader.BaseStream.Length)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading string target.");
            }

            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
