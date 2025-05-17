using System.Text;

namespace WoWSharpClient.Utils
{
    public static class ReaderUtils
    {
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
