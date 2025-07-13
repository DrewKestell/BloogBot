using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using WoWSharpClient.Models;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    public static class CharacterSelectHandler
    {
        public static void HandleCharEnum(Opcode opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            byte numChars = reader.ReadByte();

            WoWSharpObjectManager.Instance.CharacterSelectScreen.CharacterSelects.Clear();

            for (int i = 0; i < numChars; i++)
            {
                var character = new CharacterSelect
                {
                    Guid = reader.ReadUInt64(),
                    Name = ReaderUtils.ReadCString(reader),
                    Race = (Race)reader.ReadByte(),
                    Class = (Class)reader.ReadByte(),
                    Gender = (Gender)reader.ReadByte(),

                    Skin = reader.ReadByte(),
                    Face = reader.ReadByte(),
                    HairStyle = reader.ReadByte(),
                    HairColor = reader.ReadByte(),

                    FacialHair = reader.ReadByte(),

                    Level = reader.ReadByte(),
                    ZoneId = reader.ReadUInt32(),
                    MapId = reader.ReadUInt32(),

                    Position = new Position(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),

                    GuildId = reader.ReadUInt32(),

                    CharacterFlags = (CharacterFlags)reader.ReadUInt32(),
                    FirstLogin = (AtLoginFlags)reader.ReadByte(),

                    PetDisplayId = reader.ReadUInt32(),
                    PetLevel = reader.ReadUInt32(),
                    PetFamily = reader.ReadUInt32(),
                    Equipment = []
                };

                // Read equipment (19 slots)
                for (int j = 0; j < 19; j++)
                {
                    uint displayId = reader.ReadUInt32();
                    InventoryType inventoryType = (InventoryType)reader.ReadByte();
                    character.Equipment.Add((displayId, inventoryType));
                }

                // Read first bag information
                character.FirstBagDisplayId = reader.ReadUInt32();
                character.FirstBagInventoryType = reader.ReadByte();

                // Add the parsed character to the character selection list
                WoWSharpObjectManager.Instance.CharacterSelectScreen.CharacterSelects.Add(character);
            }

            // Trigger an event once the character list is loaded
            WoWSharpEventEmitter.Instance.FireOnCharacterListLoaded();
        }

        public static void HandleSetRestStart(Opcode opcode, byte[] data)
        {
            WoWSharpEventEmitter.Instance.FireOnSetRestStart();
        }

        public static void HandleCharCreate(Opcode opcode, byte[] data)
        {
            var result = (CreateCharacterResult)data[0];
            WoWSharpEventEmitter.Instance.FireOnCharacterCreateResponse(new CharCreateResponse(result));
        }

        public static void HandleCharDelete(Opcode opcode, byte[] data)
        {
            var result = (DeleteCharacterResult)data[0];
            WoWSharpEventEmitter.Instance.FireOnCharacterDeleteResponse(new CharDeleteResponse(result));
        }

        public static void HandleAddonInfo(Opcode opcode, byte[] data)
        {
            int index = 0;
            while (index < data.Length)
            {
                byte addonType = data[index++];
                // Handle addon information here if necessary
                //Console.WriteLine($"Addon Info: {addonType:X2}");
            }
        }

        public static void HandleNameQueryResponse(Opcode opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var guid = reader.ReadUInt64();
            var name = ReaderUtils.ReadCString(reader);
            var realm = ReaderUtils.ReadCString(reader);
            var race = (Race)reader.ReadUInt32();
            var gender = (Gender)reader.ReadUInt32();
            var classId = (Class)reader.ReadUInt32();

            var gameObject = (WoWPlayer)WoWSharpObjectManager.Instance.Objects.First(x => x.Guid == guid);

            if (gameObject != null)
            {
                gameObject.Name = name;
                gameObject.Race = race;
                gameObject.Gender = gender;
                gameObject.Class = classId;
            }
            else
            {
                var characterSelect = WoWSharpObjectManager.Instance.CharacterSelectScreen.CharacterSelects.FirstOrDefault(x => x.Guid == guid);
                if (characterSelect != null)
                {
                    characterSelect.Name = name;
                    characterSelect.Race = race;
                    characterSelect.Gender = gender;
                    characterSelect.Class = classId;
                }
            }
        }
    }
}
