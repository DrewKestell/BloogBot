using BotRunner.Clients;
using GameData.Core.Enums;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using WoWSharpClient.Handlers;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    public class SMSG_CHAR_ENUM_Tests
    {
        private readonly CharacterSelectHandler _characterSelectHandler;
        private readonly WoWSharpObjectManager _objectManager;
        private readonly Mock<PathfindingClient> _pathfindingClientMock;
        private readonly Mock<Logger<WoWSharpObjectManager>> _logger = new();

        public SMSG_CHAR_ENUM_Tests()
        {
            _pathfindingClientMock = new Mock<PathfindingClient>();
            // Initialize your dependencies using mocks or stubs
            _objectManager = new WoWSharpObjectManager("127.0.0.1", _pathfindingClientMock.Object, _logger.Object);

            // Initialize ObjectUpdateHandler with mocked dependencies
            _characterSelectHandler = new CharacterSelectHandler(_objectManager);
        }

        [Fact]
        public void ShouldParseCharacterList()
        {
            var opcode = Opcode.SMSG_CHAR_ENUM;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\20240815.bin");

            // Call the HandleUpdateObject method on ObjectUpdateHandler
            _characterSelectHandler.HandleCharEnum(opcode, data);

            // Verify that 10 characters were parsed and added to the ObjectManager
            Assert.Equal(10, _objectManager.CharacterSelectScreen.CharacterSelects.Count);

            // Define expected values for assertions (these values are hypothetical, replace them with the correct expected values)
            var expectedCharacters = new List<CharacterSelect>
            {
                new() { Guid = 150, Name = "Greenbarbie", Race = Race.Orc, Class = Class.Warrior, Level = 60, ZoneId = 1637, MapId = 1, Position = new Position(1984.09f, -4792.23f, 55.8203f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (34215, InventoryType.Head), (9860, InventoryType.Neck), (34253, InventoryType.Shoulders),
                        (7904, InventoryType.Shirt), (33983, InventoryType.Chest), (33990, InventoryType.Waist),
                        (33986, InventoryType.Legs), (33989, InventoryType.Feet), (33982, InventoryType.Wrists),
                        (33984, InventoryType.Hands), (9835, InventoryType.Finger), (9840, InventoryType.Finger),
                        (24776, InventoryType.Trinket), (6337, InventoryType.Trinket), (29827, InventoryType.Cloak),
                        (31866, InventoryType.Weapon), (34110, InventoryType.Shield), (35071, InventoryType.RangedRight), (15817, InventoryType.Tabard)} },
                new() { Guid = 153, Name = "Beefburgers", Race = Race.Tauren, Class = Class.Warrior, Level = 1, ZoneId = 215, MapId = 1, Position = new Position(-2917.58f, -257.98f, 52.9968f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9995, InventoryType.Shirt), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9988, InventoryType.Legs), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (8690, InventoryType.TwoHander), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 154, Name = "Slimslash", Race = Race.Human, Class = Class.Warrior, Level = 1, ZoneId = 12, MapId = 0, Position = new Position(-8949.9502f, -132.492996f, 83.5311966f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                       (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9891, InventoryType.Shirt), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9892, InventoryType.Legs), (10141, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (1542, InventoryType.MainHand), (18730, InventoryType.Shield), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 155, Name = "Trollspear", Race = Race.Troll, Class = Class.Rogue, Level = 1, ZoneId = 14, MapId = 1, Position = new Position(-618.518005f, -4251.66992f, 38.7179985f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (10112, InventoryType.Shirt), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (10114, InventoryType.Legs), (10115, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (6442, InventoryType.Weapon), (0, InventoryType.NonEquippable), (20777, InventoryType.Thrown), (0, InventoryType.NonEquippable)} },
                new() { Guid = 156, Name = "Manastone", Race = Race.Dwarf, Class = Class.Paladin, Level = 1, ZoneId = 1, MapId = 0, Position = new Position(-6240.31982f, 331.03299f, 382.757996f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9972, InventoryType.Shirt), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9974, InventoryType.Legs), (10272, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (8690, InventoryType.TwoHander), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 157, Name = "Grimdoc", Race = Race.Troll, Class = Class.Mage, Level = 1, ZoneId = 14, MapId = 1, Position = new Position(-618.518005f, -4251.66992f, 38.7179985f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (2163, InventoryType.Shirt), (12649, InventoryType.Robe), (0, InventoryType.NonEquippable),
                        (9924, InventoryType.Legs), (9929, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (472, InventoryType.TwoHander), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 158, Name = "Jimmer", Race = Race.Undead, Class = Class.Priest, Level = 1, ZoneId = 85, MapId = 0, Position = new Position(1676.70996f, 1678.31006f, 121.669998f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9944, InventoryType.Shirt), (12680, InventoryType.Robe), (0, InventoryType.NonEquippable),
                        (9945, InventoryType.Legs), (9946, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (5194, InventoryType.MainHand), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 159, Name = "Treehugger", Race = Race.NightElf, Class = Class.Druid, Level = 1, ZoneId = 141, MapId = 1, Position = new Position(10311.2998f, 832.463013f, 1326.41003f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (12683, InventoryType.Robe), (0, InventoryType.NonEquippable),
                        (9987, InventoryType.Legs), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (18530, InventoryType.TwoHander), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 160, Name = "Gnomlocker", Race = Race.Gnome, Class = Class.Warlock, Level = 1, ZoneId = 1, MapId = 0, Position = new Position(-6240.31982f, 331.03299f, 382.757996f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                       (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (2470, InventoryType.Shirt), (12645, InventoryType.Robe), (0, InventoryType.NonEquippable),
                        (3260, InventoryType.Legs), (3261, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (6442, InventoryType.Weapon), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable)} },
                new() { Guid = 161, Name = "Steinshooter", Race = Race.Dwarf, Class = Class.Hunter, Level = 1, ZoneId = 1, MapId = 0, Position = new Position(-6240.31982f, 331.03299f, 382.757996f), GuildId = 0, CharacterFlags = CharacterFlags.CHARACTER_FLAG_NONE, FirstLogin = AtLoginFlags.AT_LOGIN_NONE, PetDisplayId = 0, PetLevel = 0, PetFamily = 0, FirstBagDisplayId = 0, FirstBagInventoryType = 0, Equipment = {
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9976, InventoryType.Shirt), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (9975, InventoryType.Legs), (9977, InventoryType.Feet), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable), (0, InventoryType.NonEquippable),
                        (14029, InventoryType.MainHand), (0, InventoryType.NonEquippable), (6606, InventoryType.RangedRight), (0, InventoryType.NonEquippable)} },
            };

            foreach (var expectedCharacter in expectedCharacters)
            {
                var parsedCharacter = _objectManager.CharacterSelectScreen.CharacterSelects.First(o => o.Guid == expectedCharacter.Guid);
                Assert.NotNull(parsedCharacter);

                Assert.Equal(expectedCharacter.Name, parsedCharacter.Name);
                Assert.Equal(expectedCharacter.Race, parsedCharacter.Race);
                Assert.Equal(expectedCharacter.Class, parsedCharacter.Class);
                Assert.Equal(expectedCharacter.Level, parsedCharacter.Level);
                Assert.Equal(expectedCharacter.ZoneId, parsedCharacter.ZoneId);
                Assert.Equal(expectedCharacter.MapId, parsedCharacter.MapId);
                Assert.Equal(expectedCharacter.Position.X, parsedCharacter.Position.X);
                Assert.Equal(expectedCharacter.Position.Y, parsedCharacter.Position.Y);
                Assert.Equal(expectedCharacter.Position.Z, parsedCharacter.Position.Z);
                Assert.Equal(expectedCharacter.GuildId, parsedCharacter.GuildId);
                Assert.Equal(expectedCharacter.CharacterFlags, parsedCharacter.CharacterFlags);
                Assert.Equal(expectedCharacter.FirstLogin, parsedCharacter.FirstLogin);
                Assert.Equal(expectedCharacter.PetDisplayId, parsedCharacter.PetDisplayId);
                Assert.Equal(expectedCharacter.PetLevel, parsedCharacter.PetLevel);
                Assert.Equal(expectedCharacter.PetFamily, parsedCharacter.PetFamily);
                Assert.Equal(expectedCharacter.FirstBagDisplayId, parsedCharacter.FirstBagDisplayId);
                Assert.Equal(expectedCharacter.FirstBagInventoryType, parsedCharacter.FirstBagInventoryType);
                Assert.Equal(expectedCharacter.Equipment.Count, parsedCharacter.Equipment.Count);
                for (int i = 0; i < expectedCharacter.Equipment.Count; i++)
                {
                    Assert.Equal(expectedCharacter.Equipment[i].DisplayId, parsedCharacter.Equipment[i].DisplayId);
                    Assert.Equal(expectedCharacter.Equipment[i].InventoryType, parsedCharacter.Equipment[i].InventoryType);
                }
            }
        }
    }
}
