using BotRunner.Clients;
using GameData.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WoWSharpClient.Client;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    public class ObjectManagerFixture : IDisposable
    {
        public readonly Mock<WoWClient> _woWClient;
        public readonly Mock<PathfindingClient> _pathfindingClient;
        private readonly ILogger<WoWSharpObjectManager> _logger = NullLoggerFactory.Instance.CreateLogger<WoWSharpObjectManager>();
        public ObjectManagerFixture()
        {
            _woWClient = new();
            _pathfindingClient = new();

            WoWSharpObjectManager.Instance.Initialize(_woWClient.Object, _pathfindingClient.Object, _logger);
        }

        public void Dispose()
        {
            WoWSharpObjectManager.Instance.Initialize(_woWClient.Object, _pathfindingClient.Object, _logger);
        }
    }
    [CollectionDefinition("Sequential ObjectManager tests", DisableParallelization = true)]
    public class SequentialCollection : ICollectionFixture<ObjectManagerFixture> { }
    
    [Collection("Sequential ObjectManager tests")]
    public class OpcodeHandler_Tests(ObjectManagerFixture _) : IClassFixture<ObjectManagerFixture>
    {
        private readonly OpCodeDispatcher _dispatcher = new();

        public static IEnumerable<object[]> OpcodeTestData =>
        [
            [Opcode.MSG_MOVE_FALL_LAND, "movement"],
            [Opcode.MSG_MOVE_TIME_SKIPPED, "movement"],
            [Opcode.SMSG_ACCOUNT_DATA_TIMES, "accountData"],
            [Opcode.SMSG_ACTION_BUTTONS, "dispatcher"],
            [Opcode.SMSG_AUTH_RESPONSE, "dispatcher"],
            [Opcode.SMSG_BINDPOINTUPDATE, "dispatcher"],
            [Opcode.SMSG_COMPRESSED_MOVES, "movement"],
            [Opcode.SMSG_DESTROY_OBJECT, "dispatcher"],
            [Opcode.SMSG_FRIEND_LIST, "dispatcher"],
            [Opcode.SMSG_GROUP_LIST, "dispatcher"],
            [Opcode.SMSG_IGNORE_LIST, "dispatcher"],
            [Opcode.SMSG_INITIALIZE_FACTIONS, "dispatcher"],
            [Opcode.SMSG_INITIAL_SPELLS, "spellInitial"],
            [Opcode.SMSG_INIT_WORLD_STATES, "worldState"],
            [Opcode.SMSG_LOGIN_SETTIMESPEED, "dispatcher"],
            [Opcode.SMSG_LOGIN_VERIFY_WORLD, "login"],
            [Opcode.SMSG_SET_FLAT_SPELL_MODIFIER, "dispatcher"],
            [Opcode.SMSG_SET_PCT_SPELL_MODIFIER, "dispatcher"],
            [Opcode.SMSG_SET_PROFICIENCY, "dispatcher"],
            [Opcode.SMSG_SET_REST_START, "worldState"],
            [Opcode.SMSG_SPELLLOGMISS, "spellLogMiss"],
            [Opcode.SMSG_SPELL_GO, "spellGo"],
            [Opcode.SMSG_TUTORIAL_FLAGS, "dispatcher"],
            [Opcode.SMSG_UPDATE_AURA_DURATION, "dispatcher"],
            [Opcode.SMSG_WEATHER, "dispatcher"]
        ];

        //TODO: Test might be useless or redundant
        [Theory]
        [MemberData(nameof(OpcodeTestData))]
        public void ShouldHandleOpcodePackets(Opcode opcode, string handlerType)
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString());

            var files = Directory.GetFiles(directoryPath, "20240815_*.bin")
                .OrderBy(path =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var parts = fileName.Split('_');
                    return parts.Length > 1 && int.TryParse(parts[1], out var index) ? index : int.MaxValue;
                });

            foreach (var filePath in files)
            {
                byte[] data = FileReader.ReadBinaryFile(filePath);

                _dispatcher.Dispatch(opcode, data);
            }
        }
    }
}
