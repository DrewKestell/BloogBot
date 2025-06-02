using GameData.Core.Enums;
using GameData.Core.Models;
using WoWSharpClient.Models;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Parsers
{
    public static class MovementPacketHandler
    {
        public static byte[] BuildMoveTeleportAckPayload(ulong guid, uint movementCounter, uint timestamp)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(guid);
            writer.Write(movementCounter);
            writer.Write(timestamp);

            return ms.ToArray();
        }
        public static byte[] BuildMovementInfoBuffer(WoWLocalPlayer player, uint timestamp)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Movement flags
            writer.Write((uint)player.MovementFlags);

            // Timestamp
            writer.Write(timestamp);

            // Position
            writer.Write(player.Position.X);
            writer.Write(player.Position.Y);
            writer.Write(player.Position.Z);

            // Orientation (facing)
            writer.Write(player.Facing);

            // If ON_TRANSPORT: write transport data
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ONTRANSPORT) && player.Transport != null)
            {
                writer.Write(player.Transport.Guid); // 64-bit GUID
                writer.Write(player.Transport.Position.X);
                writer.Write(player.Transport.Position.Y);
                writer.Write(player.Transport.Position.Z);
                writer.Write(player.Transport.Facing);
                writer.Write(timestamp); // uint32
            }

            // If SWIMMING: write pitch
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING))
            {
                writer.Write(player.SwimPitch);
            }

            // Fall time (always written)
            writer.Write(player.FallTime);

            // If JUMPING: write jump velocity and angles
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING))
            {
                writer.Write(player.JumpVerticalSpeed);
                writer.Write(player.JumpCosAngle);
                writer.Write(player.JumpSinAngle);
                writer.Write(player.JumpHorizontalSpeed);
            }

            // If SPLINE_ELEVATION: write elevation float
            if (player.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ELEVATION))
            {
                writer.Write(player.SplineElevation);
            }

            return ms.ToArray();
        }
        public static void ParseMovementBlock(BinaryReader reader, WoWUnit currentUnit)
        {
            ReadMovementHeader(reader, currentUnit);
            ReadPositionAndFacing(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ONTRANSPORT))
                ReadTransportData(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING))
                ReadSwimPitch(reader, currentUnit);

            if (!currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ONTRANSPORT))
                currentUnit.FallTime = reader.ReadSingle();

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING))
                ReadJumpData(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ELEVATION))
                currentUnit.SplineElevation = reader.ReadSingle();

            ReadSpeeds(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ENABLED))
                ReadSplineData(reader, currentUnit);
        }
        public static void ParseMovementInfo(BinaryReader reader, WoWUnit currentUnit)
        {
            ReadMovementHeader(reader, currentUnit);
            ReadPositionAndFacing(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ONTRANSPORT))
                ReadTransportData(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING))
                ReadSwimPitch(reader, currentUnit);

            currentUnit.FallTime = reader.ReadSingle();

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING))
                ReadJumpData(reader, currentUnit);

            if (currentUnit.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ELEVATION))
                currentUnit.SplineElevation = reader.ReadSingle();
        }
        private static void ReadMovementHeader(BinaryReader reader, WoWUnit unit)
        {
            unit.MovementFlags = (MovementFlags)reader.ReadUInt32();
            unit.LastUpdated = reader.ReadUInt32();
        }
        private static void ReadPositionAndFacing(BinaryReader reader, WoWUnit unit)
        {
            unit.Position.X = reader.ReadSingle();
            unit.Position.Y = reader.ReadSingle();
            unit.Position.Z = reader.ReadSingle();
            unit.Facing = reader.ReadSingle();
        }
        private static void ReadTransportData(BinaryReader reader, WoWUnit unit)
        {
            unit.TransportGuid = BitConverter.ToUInt64(reader.ReadBytes(4));
            unit.TransportOffset.X = reader.ReadSingle();
            unit.TransportOffset.Y = reader.ReadSingle();
            unit.TransportOffset.Z = reader.ReadSingle();
            unit.TransportOrientation = reader.ReadSingle();
            unit.TransportLastUpdated = reader.ReadUInt32();
        }
        private static void ReadSwimPitch(BinaryReader reader, WoWUnit unit)
        {
            unit.SwimPitch = reader.ReadSingle();
        }
        private static void ReadJumpData(BinaryReader reader, WoWUnit unit)
        {
            unit.JumpVerticalSpeed = reader.ReadSingle();
            unit.JumpSinAngle = reader.ReadSingle();
            unit.JumpCosAngle = reader.ReadSingle();
            unit.JumpHorizontalSpeed = reader.ReadSingle();
        }
        private static void ReadSpeeds(BinaryReader reader, WoWUnit unit)
        {
            unit.WalkSpeed = reader.ReadSingle();
            unit.RunSpeed = reader.ReadSingle();
            unit.RunBackSpeed = reader.ReadSingle();
            unit.SwimSpeed = reader.ReadSingle();
            unit.SwimBackSpeed = reader.ReadSingle();
            unit.TurnRate = reader.ReadSingle();
        }
        private static void ReadSplineData(BinaryReader reader, WoWUnit unit)
        {
            SplineFlags flags = (SplineFlags)reader.ReadUInt32();
            unit.SplineFlags = flags;

            if (flags.HasFlag(SplineFlags.FinalPoint))
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                unit.SplineFinalPoint = new Position(x, y, z);
            }
            else if (flags.HasFlag(SplineFlags.FinalTarget))
            {
                ulong targetGuid = reader.ReadUInt64();
                unit.SplineTargetGuid = targetGuid;
            }
            else if (flags.HasFlag(SplineFlags.FinalOrientation))
            {
                float angle = reader.ReadSingle();
                unit.SplineFinalOrientation = angle;
            }

            unit.SplineTimePassed = reader.ReadInt32();
            unit.SplineDuration = reader.ReadInt32();
            unit.SplineId = reader.ReadUInt32();

            uint nodeCount = reader.ReadUInt32();
            unit.SplineNodes = [];

            for (int i = 0; i < nodeCount; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                var node = new Position(x, y, z);
                unit.SplineNodes.Add(node);
            }

            float finalX = reader.ReadSingle();
            float finalY = reader.ReadSingle();
            float finalZ = reader.ReadSingle();
            unit.SplineFinalDestination = new Position(finalX, finalY, finalZ);
        }

        internal static byte[] BuildForceMoveAck(WoWLocalPlayer player, uint movementCounter, uint timestamp)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Pack the GUID
            ReaderUtils.WritePackedGuid(writer, player.Guid);
            writer.Write(movementCounter);
            // Timestamp
            writer.Write(BuildMovementInfoBuffer(player, timestamp));

            return ms.ToArray();
        }
    }
}
