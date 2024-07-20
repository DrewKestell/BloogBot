using Ionic.Zlib;
using WoWSlimClient.Handlers;
using WoWSlimClient.Models;

namespace WoWSlimClient.Client
{
    internal class OpCodeDispatcher
    {
        public static OpCodeDispatcher Instance { get; } = new OpCodeDispatcher();
        private readonly Dictionary<Opcodes, Action<Opcodes, byte[]>> _handlers = new Dictionary<Opcodes, Action<Opcodes, byte[]>>();

        private OpCodeDispatcher()
        {
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _handlers[Opcodes.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[Opcodes.SMSG_ACCOUNT_DATA_TIMES] = AccountDataHandler.HandleAccountData;
            _handlers[Opcodes.SMSG_UPDATE_ACCOUNT_DATA] = AccountDataHandler.HandleAccountData;
            _handlers[Opcodes.SMSG_UPDATE_OBJECT] = HandleUpdateObject;
            _handlers[Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT] = HandleCompressedUpdateObject;
            _handlers[Opcodes.SMSG_CHAR_ENUM] = CharacterHandler.HandleCharEnum;
            _handlers[Opcodes.SMSG_ADDON_INFO] = CharacterHandler.HandleAddonInfo;
            _handlers[Opcodes.SMSG_INITIAL_SPELLS] = SpellHandler.HandleInitialSpells;
            _handlers[Opcodes.SMSG_MESSAGECHAT] = ChatHandler.HandleServerChatMessage;
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
                //Console.WriteLine($"Unhandled opcode: {opcode} {BitConverter.ToString(data)}");
            }
        }

        private void HandleAuthResponse(Opcodes opCode, byte[] body)
        {
            if (body.Length < 4)
            {
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

        private void HandleUpdateObject(Opcodes opcode, byte[] data)
        {
            // Implement the handler logic here
        }

        private void HandleCompressedUpdateObject(Opcodes opcode, byte[] data)
        {
            // Decompress and then call HandleUpdateObject
            byte[] decompressedData = ZlibStream.UncompressBuffer(data);
            HandleUpdateObject(opcode, decompressedData);
        }
    }
}
