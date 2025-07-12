using BotRunner.Clients;
using GameData.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WoWSharpClient.Client;
using WoWSharpClient.Handlers;
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
            // No unmanaged state needs disposal for now
        }
    }
    public class OpcodeHandler_Tests
    {
        private readonly OpCodeDispatcher _dispatcher;

        public OpcodeHandler_Tests()
        {
            _dispatcher = new OpCodeDispatcher();
        }

        public static IEnumerable<object[]> OpcodeTestData => new List<object[]>
        {
            new object[] { Opcode.MSG_MOVE_FALL_LAND, "movement" },
            new object[] { Opcode.MSG_MOVE_TIME_SKIPPED, "movement" },
            new object[] { Opcode.SMSG_ACCOUNT_DATA_TIMES, "accountData" },
            new object[] { Opcode.SMSG_ACTION_BUTTONS, "dispatcher" },
            new object[] { Opcode.SMSG_AUTH_RESPONSE, "dispatcher" },
            new object[] { Opcode.SMSG_BINDPOINTUPDATE, "dispatcher" },
            new object[] { Opcode.SMSG_COMPRESSED_MOVES, "movement" },
            new object[] { Opcode.SMSG_DESTROY_OBJECT, "dispatcher" },
            new object[] { Opcode.SMSG_FRIEND_LIST, "dispatcher" },
            new object[] { Opcode.SMSG_GROUP_LIST, "dispatcher" },
            new object[] { Opcode.SMSG_IGNORE_LIST, "dispatcher" },
            new object[] { Opcode.SMSG_INITIALIZE_FACTIONS, "dispatcher" },
            new object[] { Opcode.SMSG_INITIAL_SPELLS, "spellInitial" },
            new object[] { Opcode.SMSG_INIT_WORLD_STATES, "worldState" },
            new object[] { Opcode.SMSG_LOGIN_SETTIMESPEED, "dispatcher" },
            new object[] { Opcode.SMSG_LOGIN_VERIFY_WORLD, "login" },
            new object[] { Opcode.SMSG_SET_FLAT_SPELL_MODIFIER, "dispatcher" },
            new object[] { Opcode.SMSG_SET_PCT_SPELL_MODIFIER, "dispatcher" },
            new object[] { Opcode.SMSG_SET_PROFICIENCY, "dispatcher" },
            new object[] { Opcode.SMSG_SET_REST_START, "worldState" },
            new object[] { Opcode.SMSG_SPELLLOGMISS, "spellLogMiss" },
            new object[] { Opcode.SMSG_SPELL_GO, "spellGo" },
            new object[] { Opcode.SMSG_TUTORIAL_FLAGS, "dispatcher" },
            new object[] { Opcode.SMSG_UPDATE_AURA_DURATION, "dispatcher" },
            new object[] { Opcode.SMSG_WEATHER, "dispatcher" }
        };

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
