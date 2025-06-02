using BotRunner.Clients;
using GameData.Core.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using WoWSharpClient;
using WoWSharpClient.Handlers;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    public class OpcodeHandler_Tests
    {
        private readonly WoWSharpObjectManager _objectManager;
        private readonly Mock<PathfindingClient> _pathfindingClientMock = new();
        private readonly Mock<Logger<WoWSharpObjectManager>> _logger = new();
        private readonly OpCodeDispatcher _dispatcher;
        private readonly MovementHandler _movementHandler;
        private readonly AccountDataHandler _accountDataHandler;
        private readonly SpellHandler _spellHandler;
        private readonly WorldStateHandler _worldStateHandler;
        private readonly LoginHandler _loginHandler;

        public OpcodeHandler_Tests()
        {
            _objectManager = new("127.0.0.1", _pathfindingClientMock.Object, _logger.Object);
            _dispatcher = new OpCodeDispatcher(_objectManager);
            _movementHandler = new MovementHandler(_objectManager);
            _accountDataHandler = new AccountDataHandler(_objectManager);
            _spellHandler = new SpellHandler(_objectManager);
            _worldStateHandler = new WorldStateHandler(_objectManager);
            _loginHandler = new LoginHandler(_objectManager);
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
                switch (handlerType)
                {
                    case "movement":
                        _movementHandler.HandleUpdateMovement(opcode, data);
                        break;
                    case "accountData":
                        _accountDataHandler.HandleAccountData(opcode, data);
                        break;
                    case "dispatcher":
                        _dispatcher.Dispatch(opcode, data);
                        break;
                    case "worldState":
                        _worldStateHandler.HandleInitWorldStates(opcode, data);
                        break;
                    case "login":
                        _loginHandler.HandleLoginVerifyWorld(opcode, data);
                        break;
                    case "spellInitial":
                        _spellHandler.HandleInitialSpells(opcode, data);
                        break;
                    case "spellLogMiss":
                        _spellHandler.HandleSpellLogMiss(opcode, data);
                        break;
                    case "spellGo":
                        _spellHandler.HandleSpellGo(opcode, data);
                        break;
                    default:
                        throw new NotSupportedException($"Handler type {handlerType} is not supported.");
                }
            }
        }
    }
}
