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
        public static byte[] BuildMovementInfoBuffer(WoWLocalPlayer p, uint ctimeMs)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);

            w.Write((uint)p.MovementFlags);
            w.Write(ctimeMs);

            w.Write(p.Position.X);
            w.Write(p.Position.Y);
            w.Write(p.Position.Z);

            w.Write(p.Facing);

            // Transport block
            if ((p.MovementFlags & MovementFlags.MOVEFLAG_ONTRANSPORT) != 0 && p.Transport != null)
            {
                w.Write(p.Transport.Guid);

                w.Write(p.Transport.Position.X);
                w.Write(p.Transport.Position.Y);
                w.Write(p.Transport.Position.Z);

                w.Write(p.Transport.Facing);
            }

            // Swim pitch
            if ((p.MovementFlags & MovementFlags.MOVEFLAG_SWIMMING) != 0)
            {
                w.Write(p.SwimPitch);
            }

            w.Write(p.FallTime);

            // Jump block
            if ((p.MovementFlags & MovementFlags.MOVEFLAG_JUMPING) != 0)
            {
                w.Write(p.JumpVerticalSpeed);
                w.Write(p.JumpCosAngle);
                w.Write(p.JumpSinAngle);
                w.Write(p.JumpHorizontalSpeed);
            }

            // Spline Elevation
            if ((p.MovementFlags & MovementFlags.MOVEFLAG_SPLINE_ELEVATION) != 0)
            {
                w.Write(p.SplineElevation);
            }

            return ms.ToArray();
        }

        public static MovementInfoUpdate ParseMovementBlock(BinaryReader reader)
        {
            var data = new MovementInfoUpdate()
            {
                MovementFlags = (MovementFlags)reader.ReadUInt32(),
                LastUpdated = reader.ReadUInt32(),

                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),

                Facing = reader.ReadSingle(),

                MovementBlockUpdate = new MovementBlockUpdate()
            };

            if (data.HasTransport)
            {
                data.TransportGuid = BitConverter.ToUInt64(reader.ReadBytes(4));
                float tx = reader.ReadSingle();
                float ty = reader.ReadSingle();
                float tz = reader.ReadSingle();
                data.TransportOffset = new Position(tx, ty, tz);
                data.TransportOrientation = reader.ReadSingle();
                data.TransportLastUpdated = reader.ReadUInt32();
            }

            if (data.IsSwimming)
            {
                data.SwimPitch = reader.ReadSingle();
            }

            if (!data.HasTransport)
                data.FallTime = reader.ReadSingle();

            if (data.IsFalling)
            {
                data.JumpVerticalSpeed = reader.ReadSingle();
                data.JumpSinAngle = reader.ReadSingle();
                data.JumpCosAngle = reader.ReadSingle();
                data.JumpHorizontalSpeed = reader.ReadSingle();
            }

            if (data.HasSplineElevation)
            {
                data.SplineElevation = reader.ReadSingle();
            }

            // Read Speeds
            data.MovementBlockUpdate.WalkSpeed = reader.ReadSingle();
            data.MovementBlockUpdate.RunSpeed = reader.ReadSingle();
            data.MovementBlockUpdate.RunBackSpeed = reader.ReadSingle();
            data.MovementBlockUpdate.SwimSpeed = reader.ReadSingle();
            data.MovementBlockUpdate.SwimBackSpeed = reader.ReadSingle();
            data.MovementBlockUpdate.TurnRate = reader.ReadSingle();

            if (data.HasSpline)
            {
                SplineFlags flags = (SplineFlags)reader.ReadUInt32();
                data.MovementBlockUpdate.SplineFlags = flags;

                if (flags.HasFlag(SplineFlags.FinalPoint))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    data.MovementBlockUpdate.SplineFinalPoint = new Position(x, y, z);
                }
                else if (flags.HasFlag(SplineFlags.FinalTarget))
                {
                    data.MovementBlockUpdate.SplineTargetGuid = reader.ReadUInt64();
                }
                else if (flags.HasFlag(SplineFlags.FinalOrientation))
                {
                    data.MovementBlockUpdate.SplineFinalOrientation = reader.ReadSingle();
                }

                data.MovementBlockUpdate.SplineTimePassed = reader.ReadInt32();
                data.MovementBlockUpdate.SplineDuration = reader.ReadInt32();
                data.MovementBlockUpdate.SplineId = reader.ReadUInt32();

                uint nodeCount = reader.ReadUInt32();
                var splineNodes = new List<Position>();

                for (int i = 0; i < nodeCount; i++)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    splineNodes.Add(new Position(x, y, z));
                }

                data.MovementBlockUpdate.SplineNodes = splineNodes;

                float finalX = reader.ReadSingle();
                float finalY = reader.ReadSingle();
                float finalZ = reader.ReadSingle();
                data.MovementBlockUpdate.SplineFinalDestination = new Position(finalX, finalY, finalZ);
            }

            return data;
        }

        public static MovementInfoUpdate ParseMovementInfo(BinaryReader reader)
        {
            var data = new MovementInfoUpdate()
            {
                MovementFlags = (MovementFlags)reader.ReadUInt32(),
                LastUpdated = reader.ReadUInt32(),

                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),

                Facing = reader.ReadSingle()
            };

            if (data.HasTransport)
            {
                data.TransportGuid = BitConverter.ToUInt64(reader.ReadBytes(4));
                float tx = reader.ReadSingle();
                float ty = reader.ReadSingle();
                float tz = reader.ReadSingle();
                data.TransportOffset = new Position(tx, ty, tz);
                data.TransportOrientation = reader.ReadSingle();
                data.TransportLastUpdated = reader.ReadUInt32();
            }

            if (data.IsSwimming)
            {
                data.SwimPitch = reader.ReadSingle();
            }

            data.FallTime = reader.ReadSingle();

            if (data.IsFalling)
            {
                data.JumpVerticalSpeed = reader.ReadSingle();
                data.JumpCosAngle = reader.ReadSingle();
                data.JumpSinAngle = reader.ReadSingle();
                data.JumpHorizontalSpeed = reader.ReadSingle();
            }

            if (data.HasSplineElevation)
            {
                data.SplineElevation = reader.ReadSingle();
            }

            return data;
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
