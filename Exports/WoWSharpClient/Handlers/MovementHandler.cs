using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Parsers;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    public class MovementHandler(WoWSharpObjectManager objectManager)
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
                try
                {
                    switch (opcode)
                    {
                        case Opcode.MSG_MOVE_TELEPORT:
                            _eventEmitter.FireOnTeleport(ParseAcknowledgementPacket(reader));
                            break;
                        case Opcode.MSG_MOVE_TELEPORT_ACK:
                            ulong guid = ReaderUtils.ReadPackedGuid(reader);
                            uint movementCounter = reader.ReadUInt32();
                            MovementInfoUpdate movementUpdateData = MovementPacketHandler.ParseMovementInfo(reader);
                            movementUpdateData.MovementCounter = movementCounter;

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
                        case Opcode.SMSG_SPLINE_MOVE_SET_RUN_MODE:
                            ulong splineRunGuid = ReaderUtils.ReadPackedGuid(reader);
                            Console.WriteLine($"{splineRunGuid} Now running");
                            break;
                        case Opcode.SMSG_SPLINE_MOVE_SET_WALK_MODE:
                            ulong splineWalkGuid = ReaderUtils.ReadPackedGuid(reader);
                            Console.WriteLine($"{splineWalkGuid} Now walking");
                            break;
                        case Opcode.SMSG_SPLINE_MOVE_ROOT:
                            ulong splineMoveRootGuid = ReaderUtils.ReadPackedGuid(reader);
                            Console.WriteLine($"{splineMoveRootGuid} Now rooted");
                            break;
                        case Opcode.SMSG_SPLINE_MOVE_UNROOT:
                            ulong splineMoveUnrootGuid = ReaderUtils.ReadPackedGuid(reader);
                            Console.WriteLine($"{splineMoveUnrootGuid} Now unrooted");
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
                catch (Exception ex)
                {
                    Console.WriteLine($"[MovementHandler] {ex}");
                }
            }
        }
        private RequiresAcknowledgementArgs ParseAcknowledgementPacket(BinaryReader reader)
        {
            var packedGuid = ReaderUtils.ReadPackedGuid(reader);
            var counter = reader.ReadUInt32();

            MovementInfoUpdate movementData = MovementPacketHandler.ParseMovementInfo(reader);
            movementData.MovementCounter = counter;

            _objectManager.QueueUpdate(new ObjectStateUpdate(packedGuid, ObjectUpdateOperation.Update, WoWObjectType.Player, movementData, []));

            return new(packedGuid, counter);
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

            // Parse raw movement data
            var movementData = MovementPacketHandler.ParseMovementInfo(reader);

            // Queue an update for the object's movement
            _objectManager.QueueUpdate(new ObjectStateUpdate(packedGuid, ObjectUpdateOperation.Update, WoWObjectType.None, movementData, []));

            return packedGuid;
        }

        private void ParseCompressedMove(BinaryReader reader)
        {
            reader.ReadByte(); // size
            var compressedOpCode = (Opcode)reader.ReadUInt16();
            var guid = ReaderUtils.ReadPackedGuid(reader);

            //Console.WriteLine($"[MovementHandler] {compressedOpCode}");
            switch (compressedOpCode)
            {
                case Opcode.SMSG_MONSTER_MOVE:
                    var moveData = ParseMonsterMove(reader);
                    _objectManager.QueueUpdate(new ObjectStateUpdate(guid, ObjectUpdateOperation.Update, WoWObjectType.Unit, moveData, []));
                    break;
                case Opcode.SMSG_MONSTER_MOVE_TRANSPORT:
                    ReaderUtils.ReadPackedGuid(reader); // skip transport guid
                    moveData = ParseMonsterMove(reader);
                    _objectManager.QueueUpdate(new ObjectStateUpdate(guid, ObjectUpdateOperation.Update, WoWObjectType.Unit, moveData, []));
                    break;
                case Opcode.SMSG_SPLINE_SET_RUN_SPEED:
                    ReaderUtils.ReadPackedGuid(reader);
                    reader.ReadSingle();
                    break;
            }
        }

        private static MovementInfoUpdate ParseMonsterMove(BinaryReader reader)
        {
            MovementInfoUpdate data = new()
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
                MovementBlockUpdate = new()
                {
                    SplineId = reader.ReadUInt32(),
                    SplineType = (SplineType)reader.ReadByte()
                }
            };

            switch (data.MovementBlockUpdate.SplineType)
            {
                case SplineType.FacingTarget:
                    data.MovementBlockUpdate.FacingTargetGuid = reader.ReadUInt64();
                    break;
                case SplineType.FacingAngle:
                    data.MovementBlockUpdate.FacingAngle = reader.ReadSingle();
                    break;
                case SplineType.FacingSpot:
                    float fx = reader.ReadSingle();
                    float fy = reader.ReadSingle();
                    float fz = reader.ReadSingle();
                    data.MovementBlockUpdate.FacingSpot = new Position(fx, fy, fz);
                    break;
            }

            if (data.MovementBlockUpdate.SplineType != SplineType.Stop)
            {
                data.MovementBlockUpdate.SplineFlags = (SplineFlags)reader.ReadUInt32();
                data.MovementBlockUpdate.SplineTimestamp = reader.ReadUInt32();

                uint splineCount = reader.ReadUInt32();
                float firstX = reader.ReadSingle();
                float firstY = reader.ReadSingle();
                float firstZ = reader.ReadSingle();

                data.MovementBlockUpdate.SplinePoints.Add(new Position(firstX, firstY, firstZ));

                for (int i = 1; i < splineCount; i++)
                {
                    uint packed = reader.ReadUInt32();
                    int dx = SignExtend(packed & 0x7FF, 11);
                    int dy = SignExtend((packed >> 11) & 0x7FF, 11);
                    int dz = SignExtend((packed >> 22) & 0x3FF, 10);

                    float px = firstX + dx / 4.0f;
                    float py = firstY + dy / 4.0f;
                    float pz = firstZ + dz / 4.0f;

                    data.MovementBlockUpdate.SplinePoints.Add(new Position(px, py, pz));
                }
            }

            return data;
        }

        private static int SignExtend(uint value, int bits)
        {
            int shift = 32 - bits;
            return (int)(value << shift) >> shift;
        }
    }
}
