using BotRunner.Constants;
using WoWSharpClient.Handlers;
using WoWSharpClient.Manager;

namespace WoWSharpClient
{
    internal class OpCodeDispatcher
    {
        private readonly Dictionary<Opcodes, Action<Opcodes, byte[]>> _handlers = [];
        private readonly Queue<Action> _queue;
        private readonly Task _runnerTask;
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;
        private readonly ObjectManager _objectManager;
        private readonly ObjectUpdateHandler _objectUpdateHandler;
        private readonly AccountDataHandler _accountDataHandler;
        private readonly ChatHandler _chatHandlerHandler;
        private readonly CharacterSelectHandler _characterHandler;
        private readonly LoginHandler _loginHandler;
        private readonly SpellHandler _spellHandler;
        private readonly StandStateHandler _standStateHandler;
        private readonly WorldStateHandler _worldStateHandler;

        public OpCodeDispatcher(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
        {
            _woWSharpEventEmitter = woWSharpEventEmitter;
            _objectManager = objectManager;

            _objectUpdateHandler = new ObjectUpdateHandler(_woWSharpEventEmitter, _objectManager);
            _accountDataHandler = new AccountDataHandler(_woWSharpEventEmitter, _objectManager);
            _chatHandlerHandler = new ChatHandler(_woWSharpEventEmitter, _objectManager);
            _characterHandler = new CharacterSelectHandler(_woWSharpEventEmitter, _objectManager);
            _loginHandler = new LoginHandler(_woWSharpEventEmitter, _objectManager);
            _spellHandler = new SpellHandler(_woWSharpEventEmitter, _objectManager);
            _standStateHandler = new StandStateHandler(_woWSharpEventEmitter, _objectManager);
            _worldStateHandler = new WorldStateHandler(_woWSharpEventEmitter, _objectManager);

            _queue = new Queue<Action>();
            _runnerTask = Runner();

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _handlers[Opcodes.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[Opcodes.SMSG_UPDATE_OBJECT] = _objectUpdateHandler.HandleUpdateObject;
            _handlers[Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT] = _objectUpdateHandler.HandleUpdateObject;

            _handlers[Opcodes.SMSG_ACCOUNT_DATA_TIMES] = _accountDataHandler.HandleAccountData;
            _handlers[Opcodes.SMSG_UPDATE_ACCOUNT_DATA] = _accountDataHandler.HandleAccountData;

            _handlers[Opcodes.SMSG_MESSAGECHAT] = _chatHandlerHandler.HandleServerChatMessage;

            _handlers[Opcodes.SMSG_CHAR_ENUM] = _characterHandler.HandleCharEnum;
            _handlers[Opcodes.SMSG_ADDON_INFO] = _characterHandler.HandleAddonInfo;

            _handlers[Opcodes.SMSG_LOGIN_VERIFY_WORLD] = _loginHandler.HandleLoginVerifyWorld;

            _handlers[Opcodes.SMSG_INITIAL_SPELLS] = _spellHandler.HandleInitialSpells;
            _handlers[Opcodes.SMSG_SPELLLOGMISS] = _spellHandler.HandleSpellLogMiss;
            _handlers[Opcodes.SMSG_SPELL_GO] = _spellHandler.HandleSpellGo;

            _handlers[Opcodes.SMSG_STANDSTATE_UPDATE] = _standStateHandler.HandleStandStateUpdate;

            _handlers[Opcodes.SMSG_INIT_WORLD_STATES] = _worldStateHandler.HandleInitWorldStates;
            _handlers[Opcodes.SMSG_SET_REST_START] = _worldStateHandler.HandleInitWorldStates;
            // Add more handlers as needed
        }

        public void Dispatch(Opcodes opcode, byte[] data)
        {
            //GenerateTestFile(opcode, data);

            if (_handlers.TryGetValue(opcode, out var handler))
            {
                _queue.Enqueue(() => handler(opcode, data));
            }
            else
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine($"Unhandled opcode: {opcode} {BitConverter.ToString(data)}");
            }
        }
        public static void GenerateTestFile(Opcodes opcode, byte[] data)
        {
            try
            {
                // Create a directory for the opcode if it doesn't exist
                string opcodeDirectory = Path.Combine(Directory.GetCurrentDirectory(), opcode.ToString());
                if (!Directory.Exists(opcodeDirectory))
                {
                    Directory.CreateDirectory(opcodeDirectory);
                }

                // Create a unique filename based on the current timestamp
                string fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}.bin";
                string filePath = Path.Combine(opcodeDirectory, fileName);

                // Write the byte array to the file
                File.WriteAllBytes(filePath, data);

                Console.WriteLine($"Test file generated: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating test file: {ex.Message}");
            }
        }

        private async Task Runner()
        {
            while (true)
            {
                if (_queue.Count > 0)
                {
                    var action = _queue.Dequeue();
                    action();
                }
                await Task.Delay(1);
            }
        }

        private void HandleAuthResponse(Opcodes opCode, byte[] body)
        {
            if (body.Length < 4)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[WorldClient] Incomplete SMSG_AUTH_RESPONSE packet.");
                return;
            }

            uint result = BitConverter.ToUInt32(body.Take(4).ToArray(), 0);

            if (result == (uint)ResponseCodes.AUTH_OK) // AUTH_OK
                _woWSharpEventEmitter.FireOnWorldSessionStart();
            else
                _woWSharpEventEmitter.FireOnWorldSessionEnd();
        }
    }
}
