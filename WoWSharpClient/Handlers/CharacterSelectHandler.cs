using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using WoWSharpClient.Models;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    public class CharacterSelectHandler(WoWSharpObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = objectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = objectManager;

        public void HandleCharEnum(Opcode opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            byte numChars = reader.ReadByte();

            _objectManager.CharacterSelectScreen.CharacterSelects.Clear();

            for (int i = 0; i < numChars; i++)
            {
                var character = new CharacterSelect
                {
                    Guid = reader.ReadUInt64(),
                    Name = ReaderUtils.ReadCString(reader),
                    Race = (Race)reader.ReadByte(),
                    Class = (Class)reader.ReadByte(),
                    Gender = reader.ReadByte(),

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
                _objectManager.CharacterSelectScreen.CharacterSelects.Add(character);
            }

            // Trigger an event once the character list is loaded
            _woWSharpEventEmitter.FireOnCharacterListLoaded();
        }

        public void HandleSetRestStart(Opcode opcode, byte[] data)
        {
            _woWSharpEventEmitter.FireOnSetRestStart();
        }

        public void HandleCharCreate(Opcode opcode, byte[] data)
        {
            var result = (CreateCharacterResult)data[0];
            _woWSharpEventEmitter.FireOnCharacterCreateResponse(new CharCreateResponse(result));
        }

        public void HandleCharDelete(Opcode opcode, byte[] data)
        {
            var result = (DeleteCharacterResult)data[0];
            _woWSharpEventEmitter.FireOnCharacterDeleteResponse(new CharDeleteResponse(result));
        }

        public void HandleAddonInfo(Opcode opcode, byte[] data)
        {
            int index = 0;
            while (index < data.Length)
            {
                byte addonType = data[index++];
                // Handle addon information here if necessary
                //Console.WriteLine($"Addon Info: {addonType:X2}");
            }
        }

        public void HandleNameQueryResponse(Opcode opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var name = ReaderUtils.ReadCString(reader);
            var race = (Race)reader.ReadUInt32();
            var gender = (Gender)reader.ReadUInt32();
            var classId = (Class)reader.ReadUInt32();

            var gameObject = (WoWPlayer)_objectManager.GameObjects.First(x => x.Guid == guid);
            gameObject.Name = name;
            gameObject.Race = race;
            gameObject.Gender = gender;
            gameObject.Class = classId;
        }
    }
}
