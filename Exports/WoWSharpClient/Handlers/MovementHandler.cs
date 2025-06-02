using GameData.Core.Enums;
using GameData.Core.Interfaces;
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
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    ParseCompressedMove(reader);
                }
            }
            else
            {
                switch (opcode)
                {
                    case Opcode.MSG_MOVE_TELEPORT:
                        _eventEmitter.FireOnTeleport(ParseAcknowledgementPacket(reader));
                        break;
                    case Opcode.MSG_MOVE_TELEPORT_ACK:
                        ulong guid = ReaderUtils.ReadPackedGuid(reader);
                        uint movementCounter = reader.ReadUInt32();
                        MovementPacketHandler.ParseMovementInfo(reader, (WoWUnit)_objectManager.Objects.First(x => x.Guid == guid));

                        _eventEmitter.FireOnTeleport(new RequiresAcknowledgementArgs(guid, movementCounter));
                        break;
                    case Opcode.SMSG_FORCE_MOVE_ROOT:
                        _eventEmitter.FireOnForceMoveRoot(ParseGuidCounterPacket(reader));
                        break;
                    case Opcode.SMSG_FORCE_MOVE_UNROOT:
                        _eventEmitter.FireOnForceMoveUnroot(ParseGuidCounterPacket(reader));
                        break;
                    case Opcode.SMSG_FORCE_RUN_SPEED_CHANGE:
                        _eventEmitter.FireOnForceRunSpeedChange(ParseGuidCounterPacket(reader));
                        break;
                    case Opcode.SMSG_FORCE_RUN_BACK_SPEED_CHANGE:
                        _eventEmitter.FireOnForceRunBackSpeedChange(ParseGuidCounterPacket(reader));
                        break;
                    case Opcode.SMSG_FORCE_SWIM_SPEED_CHANGE:
                        _eventEmitter.FireOnForceSwimSpeedChange(ParseGuidCounterPacket(reader));
                        break;
                    case Opcode.SMSG_MOVE_KNOCK_BACK:
                        _eventEmitter.FireOnForceMoveKnockBack(ParseGuidCounterPacket(reader));
                        break;
                    case Opcode.MSG_MOVE_TIME_SKIPPED:
                        _eventEmitter.FireOnMoveTimeSkipped(ParseGuidCounterPacket(reader));
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
                        Console.WriteLine($"{opcode} not handled");
                        break;
                }
            }
        }

        private RequiresAcknowledgementArgs ParseAcknowledgementPacket(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var counter = reader.ReadUInt32();

            MovementPacketHandler.ParseMovementInfo(reader, (WoWUnit)_objectManager.Objects.First(x => x.Guid == guid));

            return new(guid, counter);
        }

        private static RequiresAcknowledgementArgs ParseGuidCounterPacket(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var counter = reader.ReadUInt32();

            return new(guid, counter);
        }

        private ulong ParseMessageMove(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            WoWUnit woWUnit = (WoWUnit)_objectManager.Objects.First(x => x.Guid == packedGuid);

            MovementPacketHandler.ParseMovementInfo(reader, woWUnit);

            return packedGuid;
        }

        private static void ParseSplineSetRunSpeed(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            var packedSpeed = reader.ReadSingle();
        }

        private void ParseMonsterMoveTransport(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            var packedTransportGuid = ReaderUtils.ReadPackedGuid(reader);

            ParseMonsterMove(reader, packedGuid);
        }

        private void ParseCompressedMove(BinaryReader reader)
        {
            var size = reader.ReadByte();
            var compressedOpCode = (Opcode)reader.ReadUInt16();
            var guid = ReaderUtils.ReadPackedGuid(reader); // DO NOT read again in sub-method

            switch (compressedOpCode)
            {
                case Opcode.SMSG_SPLINE_SET_RUN_SPEED:
                    ParseSplineSetRunSpeed(reader);
                    break;
                case Opcode.SMSG_MONSTER_MOVE:
                    ParseMonsterMove(reader, guid); // pass guid
                    break;
                case Opcode.SMSG_MONSTER_MOVE_TRANSPORT:
                    ParseMonsterMoveTransport(reader);
                    break;
            }
        }

        private void ParseMonsterMove(BinaryReader reader, ulong guid)
        {
            WoWUnit woWUnit = new(new HighGuid(guid));
            if (_objectManager.Objects.FirstOrDefault(x => x.Guid == guid) is WoWUnit existingUnit)
                woWUnit = existingUnit;

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
                    float fx = reader.ReadSingle();
                    float fy = reader.ReadSingle();
                    float fz = reader.ReadSingle();
                    break;
            }

            if (splineType != SplineType.Stop)
            {
                var splineFlags = (SplineFlags)reader.ReadUInt32();
                var timestamp = reader.ReadUInt32();

                uint splineCount = reader.ReadUInt32();

                // First uncompressed point
                float firstX = reader.ReadSingle();
                float firstY = reader.ReadSingle();
                float firstZ = reader.ReadSingle();

                var splinePoints = new List<Position> { new(firstX, firstY, firstZ) };

                // Packed deltas — usually signed 11/11/10 bits
                for (int i = 1; i < splineCount; i++)
                {
                    uint packed = reader.ReadUInt32();

                    // Interpret as SIGNED deltas from previous point
                    int dx = SignExtend(packed & 0x7FF, 11);
                    int dy = SignExtend((packed >> 11) & 0x7FF, 11);
                    int dz = SignExtend((packed >> 22) & 0x3FF, 10);

                    float px = firstX + dx / 4.0f;
                    float py = firstY + dy / 4.0f;
                    float pz = firstZ + dz / 4.0f;

                    splinePoints.Add(new Position(px, py, pz));
                }
            }
        }

        private static int SignExtend(uint value, int bits)
        {
            int shift = 32 - bits;
            return (int)(value << shift) >> shift;
        }
    }
}
