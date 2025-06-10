using GameData.Core.Enums;
using WoWSharpClient.Handlers;
using WoWSharpClient.Utils;

namespace WoWSharpClient
{
    public class OpCodeDispatcher
    {
        private readonly Dictionary<Opcode, Action<Opcode, byte[]>> _handlers = [];
        private readonly Queue<Action> _queue;
        private readonly Task _runnerTask;
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;
        private readonly ObjectUpdateHandler _objectUpdateHandler;
        private readonly MovementHandler _movementHandler;
        private readonly AccountDataHandler _accountDataHandler;
        private readonly ChatHandler _chatHandlerHandler;
        private readonly CharacterSelectHandler _characterHandler;
        private readonly LoginHandler _loginHandler;
        private readonly SpellHandler _spellHandler;
        private readonly StandStateHandler _standStateHandler;
        private readonly WorldStateHandler _worldStateHandler;

        public OpCodeDispatcher(WoWSharpObjectManager objectManager)
        {
            _woWSharpEventEmitter = objectManager.EventEmitter;

            _objectUpdateHandler = new(objectManager);
            _movementHandler = new(objectManager);
            _accountDataHandler = new(objectManager);
            _chatHandlerHandler = new(objectManager);
            _characterHandler = new(objectManager);
            _loginHandler = new(objectManager);
            _spellHandler = new(objectManager);
            _standStateHandler = new(objectManager);
            _worldStateHandler = new(objectManager);

            _queue = new Queue<Action>();

            RegisterHandlers();

            _runnerTask = Runner();
        }

        private void RegisterHandlers()
        {
            _handlers[Opcode.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[Opcode.SMSG_CLIENT_CONTROL_UPDATE] = HandleSMSGClientControlUpdate;

            _handlers[Opcode.SMSG_UPDATE_OBJECT] = _objectUpdateHandler.HandleUpdateObject;
            _handlers[Opcode.SMSG_COMPRESSED_UPDATE_OBJECT] = _objectUpdateHandler.HandleUpdateObject;

            _handlers[Opcode.SMSG_COMPRESSED_MOVES] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_FEATHER_FALL] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_KNOCK_BACK] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_LAND_WALK] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_NORMAL_FALL] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_SET_FLIGHT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_SET_HOVER] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_UNSET_FLIGHT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_UNSET_HOVER] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_WATER_WALK] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_MOVE_ROOT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_MOVE_UNROOT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_RUN_SPEED_CHANGE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_RUN_BACK_SPEED_CHANGE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_SWIM_SPEED_CHANGE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_FEATHER_FALL] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_LAND_WALK] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_NORMAL_FALL] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_ROOT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_SET_HOVER] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_SET_RUN_MODE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_SET_WALK_MODE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_START_SWIM] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_STOP_SWIM] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_UNROOT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_UNSET_HOVER] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_WATER_WALK] = _movementHandler.HandleUpdateMovement;

            _handlers[Opcode.MSG_MOVE_TELEPORT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_TELEPORT_ACK] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_TIME_SKIPPED] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_JUMP] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_FALL_LAND] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_FORWARD] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_BACKWARD] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_STOP] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_STRAFE_LEFT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_STRAFE_RIGHT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_STOP_STRAFE] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_TURN_LEFT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_TURN_RIGHT] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_STOP_TURN] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_SET_FACING] = _movementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_HEARTBEAT] = _movementHandler.HandleUpdateMovement;

            _handlers[Opcode.SMSG_ACCOUNT_DATA_TIMES] = _accountDataHandler.HandleAccountData;
            _handlers[Opcode.SMSG_UPDATE_ACCOUNT_DATA] = _accountDataHandler.HandleAccountData;

            _handlers[Opcode.SMSG_MESSAGECHAT] = _chatHandlerHandler.HandleServerChatMessage;

            _handlers[Opcode.SMSG_CHAR_ENUM] = _characterHandler.HandleCharEnum;
            _handlers[Opcode.SMSG_ADDON_INFO] = CharacterSelectHandler.HandleAddonInfo;
            _handlers[Opcode.SMSG_NAME_QUERY_RESPONSE] = _characterHandler.HandleNameQueryResponse;

            _handlers[Opcode.SMSG_LOGIN_VERIFY_WORLD] = _loginHandler.HandleLoginVerifyWorld;

            _handlers[Opcode.SMSG_INITIAL_SPELLS] = _spellHandler.HandleInitialSpells;
            _handlers[Opcode.SMSG_SPELLLOGMISS] = _spellHandler.HandleSpellLogMiss;
            _handlers[Opcode.SMSG_SPELL_GO] = _spellHandler.HandleSpellGo;

            _handlers[Opcode.SMSG_STANDSTATE_UPDATE] = _standStateHandler.HandleStandStateUpdate;

            _handlers[Opcode.SMSG_INIT_WORLD_STATES] = _worldStateHandler.HandleInitWorldStates;
            _handlers[Opcode.SMSG_SET_REST_START] = _worldStateHandler.HandleInitWorldStates;
        }

        public void Dispatch(Opcode opcode, byte[] data)
        {
            if (_handlers.TryGetValue(opcode, out var handler))
            {
                _queue.Enqueue(() => handler(opcode, data));
            }
            else
            {
                //Console.WriteLine($"Unhandled opcode: {opcode} byte[{data.Length}]");
            }
        }
        private async Task Runner()
        {
            while (true)
            {
                if (_queue.Count > 0)
                {
                    try
                    {
                        var action = _queue.Dequeue();
                        action();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error in OpCodeDispatcher.Runner: {e}\n");
                    }
                }
                await Task.Delay(50);
            }
        }

        private void HandleAuthResponse(Opcode opCode, byte[] body)
        {
            if (body.Length < 4)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                return;
            }

            uint result = BitConverter.ToUInt32([.. body.Take(4)], 0);

            if (result != (uint)ResponseCode.AUTH_OK) // AUTH_OK
                _woWSharpEventEmitter.FireOnWorldSessionEnd();
        }

        public void HandleSMSGClientControlUpdate(Opcode opCode, byte[] payload)
        {
            BinaryReader reader = new(new MemoryStream(payload));
            ulong guid = ReaderUtils.ReadPackedGuid(reader);
            bool canControl = reader.ReadByte() != 0;

            // Respond with movement or heartbeat to indicate readiness
            _woWSharpEventEmitter.FireOnClientControlUpdate();
        }
    }
}
