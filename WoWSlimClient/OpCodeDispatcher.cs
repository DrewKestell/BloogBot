using WoWSlimClient.Handlers;
using WoWSlimClient.Models;

namespace WoWSlimClient.Client
{
    internal class OpCodeDispatcher
    {
        public static OpCodeDispatcher Instance { get; } = new OpCodeDispatcher();
        private readonly Dictionary<Opcodes, Action<Opcodes, byte[]>> _handlers = [];

        private OpCodeDispatcher()
        {
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _handlers[Opcodes.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[Opcodes.SMSG_UPDATE_OBJECT] = ObjectUpdateHandler.HandleUpdateObject;
            _handlers[Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT] = ObjectUpdateHandler.HandleUpdateObject;

            _handlers[Opcodes.SMSG_ACCOUNT_DATA_TIMES] = AccountDataHandler.HandleAccountData;
            _handlers[Opcodes.SMSG_UPDATE_ACCOUNT_DATA] = AccountDataHandler.HandleAccountData;

            _handlers[Opcodes.SMSG_MESSAGECHAT] = ChatHandler.HandleServerChatMessage;

            _handlers[Opcodes.SMSG_CHAR_ENUM] = CharacterHandler.HandleCharEnum;
            _handlers[Opcodes.SMSG_ADDON_INFO] = CharacterHandler.HandleAddonInfo;

            _handlers[Opcodes.SMSG_LOGIN_VERIFY_WORLD] = LoginHandler.HandleLoginVerifyWorld;

            _handlers[Opcodes.SMSG_INITIAL_SPELLS] = SpellHandler.HandleInitialSpells;
            _handlers[Opcodes.SMSG_SPELLLOGMISS] = SpellHandler.HandleSpellLogMiss;
            _handlers[Opcodes.SMSG_SPELL_GO] = SpellHandler.HandleSpellGo;

            _handlers[Opcodes.SMSG_STANDSTATE_UPDATE] = StandStateHandler.HandleStandStateUpdate;

            _handlers[Opcodes.SMSG_INIT_WORLD_STATES] = WorldStateHandler.HandleInitWorldStates;
            _handlers[Opcodes.SMSG_SET_REST_START] = WorldStateHandler.HandleInitWorldStates;
            // Add more handlers as needed
        }

        public void Dispatch(Opcodes opcode, byte[] data)
        {
            if (_handlers.TryGetValue(opcode, out var handler))
            {
                handler(opcode, data);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine($"Unhandled opcode: {opcode} {BitConverter.ToString(data)}");
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
            {
                WoWEventHandler.Instance.FireOnWorldSessionStart();
            }
            else
            {
                WoWEventHandler.Instance.FireOnWorldSessionEnd();
            }
        }
    }
}
