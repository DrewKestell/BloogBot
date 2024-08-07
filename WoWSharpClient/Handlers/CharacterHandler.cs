using WoWSharpClient.Models;
using System.Text;
using WoWSharpClient.Manager;
using BotRunner.Interfaces;
using BotRunner.Constants;

namespace WoWSharpClient.Handlers
{
    public static class CharacterHandler
    {
        public static void HandleCharEnum(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            byte numChars = reader.ReadByte();

            ObjectManager.Instance.CharacterSelects.Clear();

            for (int i = 0; i < numChars; i++)
            {
                var character = new CharacterSelect
                {
                    Guid = reader.ReadUInt64(),
                    Name = ReadString(reader),
                    Race = reader.ReadByte(),
                    CharacterClass = reader.ReadByte(),
                    Gender = reader.ReadByte(),
                    Skin = reader.ReadByte(),
                    Face = reader.ReadByte(),
                    HairStyle = reader.ReadByte(),
                    HairColor = reader.ReadByte(),
                    FacialHair = reader.ReadByte(),
                    Level = reader.ReadByte(),
                    ZoneId = reader.ReadUInt32(),
                    MapId = reader.ReadUInt32(),
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle(),
                    Z = reader.ReadSingle(),
                    GuildId = reader.ReadUInt32(),
                    Flags = reader.ReadUInt32(),
                    FirstLogin = reader.ReadByte(),
                    Equipment = new byte[20]
                };
                for (int j = 0; j < 20; j++)
                {
                    character.Equipment[j] = reader.ReadByte();
                }

                ObjectManager.Instance.CharacterSelects.Add(character);
            }

            WoWSharpEventEmitter.Instance.FireOnCharacterListLoaded();
        }

        private static string ReadString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            char ch;
            while ((ch = reader.ReadChar()) != '\0')
            {
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }

        public static void HandleSetRestStart(Opcodes opcode, byte[] data)
        {
            WoWSharpEventEmitter.Instance.FireOnSetRestStart();
        }

        public static void HandleCharCreate(Opcodes opcode, byte[] data)
        {
            var result = (CreateCharacterResult)data[0];
            WoWSharpEventEmitter.Instance.FireOnCharacterCreateResponse(new CharCreateResponse(result));
        }

        public static void HandleCharDelete(Opcodes opcode, byte[] data)
        {
            var result = (DeleteCharacterResult)data[0];
            WoWSharpEventEmitter.Instance.FireOnCharacterDeleteResponse(new CharDeleteResponse(result));
        }

        public static void HandleAddonInfo(Opcodes opcode, byte[] data)
        {
            int index = 0;
            while (index < data.Length)
            {
                byte addonType = data[index++];
                // Handle addon information here if necessary
                //Console.WriteLine($"Addon Info: {addonType:X2}");
            }
        }
    }
}
