using GameData.Core.Enums;
using GameData.Core.Models;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Parsers;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    internal class MovementHandler(WoWSharpObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _eventEmitter = objectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = objectManager;
        public void HandleUpdateMovement(Opcode opcode, byte[] data)
        {
            if (opcode == Opcode.SMSG_COMPRESSED_MOVES)
                data = PacketManager.Decompress([.. data.Skip(4)]);

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            if (opcode == Opcode.SMSG_COMPRESSED_MOVES)
            {
                var size = reader.ReadByte();
                var compressedOpCode = (Opcode)reader.ReadUInt16();
                var guid = ReaderUtils.ReadPackedGuid(reader);

                switch (compressedOpCode)
                {
                    case Opcode.SMSG_MONSTER_MOVE:
                        ParseMonsterMove(reader);

                        break;
                    case Opcode.SMSG_MONSTER_MOVE_TRANSPORT:
                        ParseMonsterMoveTransport(reader);
                        break;
                    case Opcode.SMSG_SPLINE_SET_RUN_SPEED:
                        ParseSplineSetRunSpeed(reader);
                        break;
                    case Opcode.SMSG_SPLINE_MOVE_UNROOT:
                    case Opcode.SMSG_SPLINE_MOVE_SET_RUN_MODE:
                    case Opcode.SMSG_SPLINE_MOVE_SET_WALK_MODE:
                        var packedGuid = ReaderUtils.ReadPackedGuid(reader);
                        break;
                }
            }
            else
            {
                try
                {
                    switch (opcode)
                    {
                        case Opcode.SMSG_MONSTER_MOVE:
                            ParseMonsterMove(reader);

                            break;
                        case Opcode.SMSG_MONSTER_MOVE_TRANSPORT:
                            ParseMonsterMoveTransport(reader);
                            break;
                        case Opcode.SMSG_SPLINE_SET_RUN_SPEED:
                            ParseSplineSetRunSpeed(reader);
                            break;
                        case Opcode.SMSG_SPLINE_MOVE_UNROOT:
                        case Opcode.SMSG_SPLINE_MOVE_SET_RUN_MODE:
                        case Opcode.SMSG_SPLINE_MOVE_SET_WALK_MODE:
                            ParseRunMode(reader);
                            break;
                        case Opcode.SMSG_FORCE_MOVE_UNROOT:
                            ParseForceMoveUnroot(reader);
                            break;
                        case Opcode.MSG_MOVE_TIME_SKIPPED:
                            ParseTimeSkipped(reader);
                            break;
                        case Opcode.MSG_MOVE_JUMP:
                            _eventEmitter.FireOnCharacterJumpStart(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_FALL_LAND:
                            _eventEmitter.FireOnCharacterFallLand(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_START_FORWARD:
                            _eventEmitter.FireOnCharacterStartForward(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_STOP:
                            _eventEmitter.FireOnCharacterMoveStop(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_START_STRAFE_LEFT:
                            _eventEmitter.FireOnCharacterStartStrafeLeft(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_START_STRAFE_RIGHT:
                            _eventEmitter.FireOnCharacterStartStrafeRight(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_STOP_STRAFE:
                            _eventEmitter.FireOnCharacterStopStrafe(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_START_TURN_LEFT:
                            _eventEmitter.FireOnCharacterStartTurnLeft(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_START_TURN_RIGHT:
                            _eventEmitter.FireOnCharacterStartTurnRight(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_STOP_TURN:
                            _eventEmitter.FireOnCharacterStopTurn(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_SET_FACING:
                            _eventEmitter.FireOnCharacterSetFacing(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_START_BACKWARD:
                            _eventEmitter.FireOnCharacterStartBackwards(ParseMessageMove(reader));
                            break;
                        case Opcode.MSG_MOVE_HEARTBEAT:
                            ParseMessageMove(reader);
                            break;
                        default:
                            //Console.WriteLine($"{opcode} not handled");
                            break;
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine($"[{opcode}] {e}");
                }
            }
        }

        private static void ParseTimeSkipped(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var timeSkipped = reader.ReadUInt32();
        }

        private static void ParseForceMoveUnroot(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var counter = reader.ReadUInt32();
        }

        private static void ParseRunMode(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
        }

        private ulong ParseMessageMove(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            WoWUnit woWUnit = (WoWUnit)_objectManager.Objects.First(x => x.Guid == packedGuid);

            MovementParser.ParseMovementInfo(reader, woWUnit);

            return packedGuid;
        }

        private static void ParseSplineSetRunSpeed(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            var packedSpeed = reader.ReadSingle();
        }

        private static void ParseMonsterMoveTransport(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            var packedTransportGuid = ReaderUtils.ReadPackedGuid(reader);

            ParseMonsterMove(reader);
        }

        private static void ParseMonsterMove(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            var splineId = reader.ReadUInt32();
            var splineType = (SplineType)reader.ReadByte();

            switch (splineType)
            {
                case SplineType.FacingTarget:
                    var targetGuid = reader.ReadUInt64();
                    break;
                case SplineType.FacingAngle:
                    var angle = reader.ReadSingle();
                    break;
                case SplineType.FacingSpot:
                    float x1 = reader.ReadSingle();
                    float y1 = reader.ReadSingle();
                    float z1 = reader.ReadSingle();
                    break;
            }

            if (splineType != SplineType.Stop)
            {
                var splineFlag = (SplineFlags)reader.ReadUInt32();
                var timestamp = reader.ReadUInt32();

                uint splineCount = reader.ReadUInt32();

                // First uncompressed spline point
                float firstX = reader.ReadSingle();
                float firstY = reader.ReadSingle();
                float firstZ = reader.ReadSingle();
                var splinePoints = new List<Position>
                                {
                                    new(firstX, firstY, firstZ)
                                };

                // Read remaining spline points as packed uint32
                for (int i = 1; i < splineCount; i++)
                {
                    uint packed = reader.ReadUInt32();
                    float xPacked = (packed & 0x7FF) / 4.0f;
                    float yPacked = ((packed >> 11) & 0x7FF) / 4.0f;
                    float zPacked = ((packed >> 22) & 0x3FF) / 4.0f;

                    splinePoints.Add(new Position(xPacked, yPacked, zPacked));
                }
            }
        }
    }
}
