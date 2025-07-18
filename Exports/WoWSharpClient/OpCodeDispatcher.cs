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

        public OpCodeDispatcher()
        {
            _queue = new Queue<Action>();

            RegisterHandlers();

            _runnerTask = Runner();
        }

        private void RegisterHandlers()
        {
            _handlers[Opcode.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[Opcode.SMSG_CLIENT_CONTROL_UPDATE] = HandleSMSGClientControlUpdate;

            _handlers[Opcode.SMSG_UPDATE_OBJECT] = ObjectUpdateHandler.HandleUpdateObject;
            _handlers[Opcode.SMSG_COMPRESSED_UPDATE_OBJECT] = ObjectUpdateHandler.HandleUpdateObject;

            _handlers[Opcode.SMSG_COMPRESSED_MOVES] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_FEATHER_FALL] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_KNOCK_BACK] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_LAND_WALK] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_NORMAL_FALL] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_SET_FLIGHT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_SET_HOVER] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_UNSET_FLIGHT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_UNSET_HOVER] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_MOVE_WATER_WALK] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_MOVE_ROOT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_MOVE_UNROOT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_RUN_SPEED_CHANGE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_RUN_BACK_SPEED_CHANGE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_SWIM_SPEED_CHANGE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_FEATHER_FALL] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_LAND_WALK] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_NORMAL_FALL] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_ROOT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_SET_HOVER] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_SET_RUN_MODE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_SET_WALK_MODE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_START_SWIM] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_STOP_SWIM] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_UNROOT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_UNSET_HOVER] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.SMSG_SPLINE_MOVE_WATER_WALK] = MovementHandler.HandleUpdateMovement;

            _handlers[Opcode.MSG_MOVE_TELEPORT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_TELEPORT_ACK] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_TIME_SKIPPED] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_JUMP] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_FALL_LAND] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_FORWARD] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_BACKWARD] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_STOP] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_STRAFE_LEFT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_STRAFE_RIGHT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_STOP_STRAFE] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_TURN_LEFT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_START_TURN_RIGHT] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_STOP_TURN] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_SET_FACING] = MovementHandler.HandleUpdateMovement;
            _handlers[Opcode.MSG_MOVE_HEARTBEAT] = MovementHandler.HandleUpdateMovement;

            _handlers[Opcode.SMSG_ACCOUNT_DATA_TIMES] = AccountDataHandler.HandleAccountData;
            _handlers[Opcode.SMSG_UPDATE_ACCOUNT_DATA] = AccountDataHandler.HandleAccountData;

            _handlers[Opcode.SMSG_MESSAGECHAT] = ChatHandler.HandleServerChatMessage;

            _handlers[Opcode.SMSG_CHAR_ENUM] = CharacterSelectHandler.HandleCharEnum;
            _handlers[Opcode.SMSG_ADDON_INFO] = CharacterSelectHandler.HandleAddonInfo;
            _handlers[Opcode.SMSG_NAME_QUERY_RESPONSE] = CharacterSelectHandler.HandleNameQueryResponse;

            _handlers[Opcode.SMSG_LOGIN_VERIFY_WORLD] = LoginHandler.HandleLoginVerifyWorld;
            _handlers[Opcode.SMSG_LOGIN_SETTIMESPEED] = LoginHandler.HandleSetTimeSpeed;
            _handlers[Opcode.SMSG_QUERY_TIME_RESPONSE] = LoginHandler.HandleTimeQueryResponse;

            _handlers[Opcode.SMSG_INITIAL_SPELLS] = SpellHandler.HandleInitialSpells;
            _handlers[Opcode.SMSG_SPELLLOGMISS] = SpellHandler.HandleSpellLogMiss;
            _handlers[Opcode.SMSG_SPELL_GO] = SpellHandler.HandleSpellGo;

            _handlers[Opcode.SMSG_STANDSTATE_UPDATE] = StandStateHandler.HandleStandStateUpdate;

            _handlers[Opcode.SMSG_INIT_WORLD_STATES] = WorldStateHandler.HandleInitWorldStates;
            _handlers[Opcode.SMSG_SET_REST_START] = WorldStateHandler.HandleInitWorldStates;
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
                WoWSharpEventEmitter.Instance.FireOnWorldSessionEnd();
        }

        public static void HandleSMSGClientControlUpdate(Opcode opCode, byte[] payload)
        {
            BinaryReader reader = new(new MemoryStream(payload));
            ulong guid = ReaderUtils.ReadPackedGuid(reader);
            bool canControl = reader.ReadByte() != 0;

            // Respond with movement or heartbeat to indicate readiness
            WoWSharpEventEmitter.Instance.FireOnClientControlUpdate();
        }
    }
}
