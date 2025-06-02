using System.Text;

namespace WoWSharpClient.Utils
{
    public static class ReaderUtils
    {
        public static void WritePackedGuid(BinaryWriter writer, ulong guid)
        {
            byte mask = 0;
            List<byte> nonZeroBytes = [];

            for (int i = 0; i < 8; i++)
            {
                byte part = (byte)((guid >> (i * 8)) & 0xFF);
                if (part != 0)
                {
                    mask |= (byte)(1 << i);
                    nonZeroBytes.Add(part);
                }
            }

            writer.Write(mask);
            foreach (byte b in nonZeroBytes)
            {
                writer.Write(b);
            }
        }
        public static ulong ReadPackedGuid(BinaryReader reader)
        {
            ulong guid = 0;
            byte mask = reader.ReadByte();

            if (mask == 0)
                return 0;

            for (var i = 0; i < 8; i++)
                if ((1 << i & mask) != 0)
                    guid |= (ulong)reader.ReadByte() << i * 8;

            return guid;
        }

        public static string ReadCString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                stringBuilder.Append((char)b);
            }
            return stringBuilder.ToString();
        }

        public static string ReadString(BinaryReader reader, uint length)
        {
            var stringBuilder = new StringBuilder();
            while (length - 1 != 0)
            {
                stringBuilder.Append(reader.ReadChar());
                length--;
            }
            return stringBuilder.ToString();
        }
    }
}
