using GameData.Core.Constants;
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
            Console.WriteLine($"[NameQuery] Received response - Opcode: {opcode}, Data size: {data?.Length ?? 0} bytes");

            if (data == null || data.Length == 0)
            {
                Console.WriteLine("[ERROR] [NameQuery] Received null or empty data packet");
                return;
            }

            try
            {
                using var reader = new BinaryReader(new MemoryStream(data));

                // Read packet data
                var guid = reader.ReadUInt64();
                var name = ReaderUtils.ReadCString(reader);
                var realm = ReaderUtils.ReadCString(reader);
                var race = (Race)reader.ReadUInt32();
                var gender = (Gender)reader.ReadUInt32();
                var classId = (Class)reader.ReadUInt32();

                // Log parsed values
                Console.WriteLine($"[INFO] [NameQuery] Parsed player info - GUID: 0x{guid:X16}, Name: '{name}', " +
                                 $"Realm: '{realm}', Race: {race}, Gender: {gender}, Class: {classId}");

                // Try to find in active game objects
                var gameObject = WoWSharpObjectManager.Instance.Objects
                    .FirstOrDefault(x => x.Guid == guid) as WoWPlayer;

                if (gameObject != null)
                {
                    var oldName = gameObject.Name;
                    var oldRace = gameObject.Race;
                    var oldGender = gameObject.Gender;
                    var oldClass = gameObject.Class;

                    gameObject.Name = name;
                    gameObject.Race = race;
                    gameObject.Gender = gender;
                    gameObject.Class = classId;

                    Console.WriteLine($"[INFO] [NameQuery] Updated WoWPlayer object - GUID: 0x{guid:X16}");

                    // Log if any values changed
                    if (oldName != name)
                        Console.WriteLine($"[DEBUG] [NameQuery] Name changed: '{oldName}' -> '{name}'");
                    if (oldRace != race)
                        Console.WriteLine($"[DEBUG] [NameQuery] Race changed: {oldRace} -> {race}");
                    if (oldGender != gender)
                        Console.WriteLine($"[DEBUG] [NameQuery] Gender changed: {oldGender} -> {gender}");
                    if (oldClass != classId)
                        Console.WriteLine($"[DEBUG] [NameQuery] Class changed: {oldClass} -> {classId}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] [NameQuery] Player 0x{guid:X16} not found in active objects, checking character select");

                    // Try to find in character select screen
                    var characterSelect = WoWSharpObjectManager.Instance.CharacterSelectScreen?
                        .CharacterSelects?.FirstOrDefault(x => x.Guid == guid);

                    if (characterSelect != null)
                    {
                        var oldName = characterSelect.Name;

                        characterSelect.Name = name;
                        characterSelect.Race = race;
                        characterSelect.Gender = gender;
                        characterSelect.Class = classId;

                        Console.WriteLine($"[INFO] [NameQuery] Updated CharacterSelect entry - GUID: 0x{guid:X16}, " +
                                        $"Name: '{name}' (was '{oldName}')");
                    }
                    else
                    {
                        Console.WriteLine($"[WARN] [NameQuery] Could not find object with GUID 0x{guid:X16} " +
                                        $"in either active objects or character select. Player info cached: " +
                                        $"'{name}' ({race} {gender} {classId})");

                        // You might want to cache this for later use
                        // PlayerCache.Add(guid, new PlayerInfo { Name = name, Race = race, ... });
                    }
                }

                // Check for any remaining data in the packet
                var remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
                if (remainingBytes > 0)
                {
                    Console.WriteLine($"[WARN] [NameQuery] {remainingBytes} bytes remaining in packet after parsing");
                }
            }
            catch (EndOfStreamException ex)
            {
                Console.WriteLine($"[ERROR] [NameQuery] Packet ended unexpectedly while reading: {ex.Message}");
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine($"[ERROR] [NameQuery] Invalid enum value in packet: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] [NameQuery] Unexpected error processing name query response: {ex}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            }
        }
    }
}
