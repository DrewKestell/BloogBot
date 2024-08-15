using System.Text;

namespace WoWSharpClient.Utils
{
    public static class ReaderUtils
    {
        public static ulong ReadPackedGuid(BinaryReader reader)
        {
            ulong guid = 0;
            byte mask = reader.ReadByte();
            int bitIndex = 0;

            while (mask != 0)
            {
                if ((mask & 1) != 0)
                {
                    byte guidByte = reader.ReadByte();
                    guid |= (ulong)guidByte << (bitIndex * 8);
                }
                mask >>= 1;
                bitIndex++;
            }

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
    }
}
