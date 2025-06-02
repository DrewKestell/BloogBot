using System.Text;
using WoWSharpClient.Models;

namespace WoWSharpClient.Client
{
    public class PacketParser
    {
        public static List<WoWPlayer> ParseCharacterDataPacket(byte[] data)
        {
            var players = new List<WoWPlayer>();

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                // Read character count
                byte characterCount = reader.ReadByte();

                for (int i = 0; i < characterCount; i++)
                {
                    //var player = new WoWPlayer
                    //{
                    //    //Guid = reader.ReadUInt64(),
                    //    //Name = ReadString(reader),
                    //    Race = ((Race)reader.ReadByte()),
                    //    Class = (Class)reader.ReadByte(),
                    //    Gender = reader.ReadByte(),
                    //    Level = reader.ReadByte(),
                    //    Position = new Position(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    //    GuildId = reader.ReadUInt32(),
                    //    UnitFlags = (UnitFlags)reader.ReadUInt32(),
                    //    // Add more fields as necessary
                    //};

                    //players.Add(player);
                }
            }

            return players;
        }

        private static string ReadString(BinaryReader reader)
        {
            var result = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                result.Add(b);
            }
            return Encoding.ASCII.GetString([.. result]);
        }
    }
}
