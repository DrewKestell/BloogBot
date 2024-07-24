using Ionic.Zlib;
using System.Security.Cryptography;
using System.Text;

namespace WoWSlimClient.Manager
{
    internal class PacketManager
    {
        public static byte[] GenerateClientProof(string username, uint clientSeed, byte[] serverSeed, byte[] sessionKey)
        {
            using SHA1 sha1 = SHA1.Create();
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(Encoding.UTF8.GetBytes(username)); // Ensure username is correct
            writer.Write(new byte[4]); // Write t (all zeroes)
            writer.Write(clientSeed);
            writer.Write(serverSeed);
            writer.Write(sessionKey);

            return sha1.ComputeHash(ms.ToArray());
        }

        public static byte[] GenerateAddonInfo()
        {
            string addonData =
                "Blizzard_AuctionUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_BattlefieldMinimap\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_BindingUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_CombatText\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_CraftUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_GMSurveyUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_InspectUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_MacroUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_RaidUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_TalentUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_TradeSkillUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_TrainerUI\x00\x01mw\x1cL\x00\x00\x00\x00";

            return Encoding.UTF8.GetBytes(addonData);
        }

        public static byte[] Compress(byte[] addonInfo)
        {
            using (var outputStream = new MemoryStream())
            using (var compressionStream = new ZlibStream(outputStream, CompressionMode.Compress, CompressionLevel.Default))
            {
                compressionStream.Write(addonInfo, 0, addonInfo.Length);
                return outputStream.ToArray();
            }
        }
        public static Stream Decompress(Stream data)
        {
            using (var zlibStream = new ZlibStream(data, CompressionMode.Decompress))
            {
                return zlibStream;
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var decompressedStream = new MemoryStream())
            {
                using (var zlibStream = new ZlibStream(compressedStream, CompressionMode.Decompress))
                {
                    zlibStream.CopyTo(decompressedStream);

                    return decompressedStream.ToArray();
                }
            }
        }

        public static async Task<byte[]> ReadAsync(BinaryReader reader, int count)
        {
            byte[] buffer = new byte[count];
            int read = await reader.BaseStream.ReadAsync(buffer, 0, count);
            if (read < count)
            {
                byte[] result = new byte[read];
                Array.Copy(buffer, result, read);
                return result;
            }
            return buffer;
        }

        public static string ReadCString(BinaryReader reader)
        {
            StringBuilder sb = new();
            char c;
            while ((c = reader.ReadChar()) != '\0')
            {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
