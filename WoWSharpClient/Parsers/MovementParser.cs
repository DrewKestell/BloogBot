using GameData.Core.Enums;
using GameData.Core.Models;
using WoWSharpClient.Models;

namespace WoWSharpClient.Parsers
{
    public static class MovementParser
    {
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
            //Console.WriteLine($"[MovementHeader] Flags: {unit.MovementFlags}, LastUpdated: {unit.LastUpdated}");
        }
        private static void ReadPositionAndFacing(BinaryReader reader, WoWUnit unit)
        {
            unit.Position.X = reader.ReadSingle();
            unit.Position.Y = reader.ReadSingle();
            unit.Position.Z = reader.ReadSingle();
            unit.Facing = reader.ReadSingle();

            //Console.WriteLine($"[Position] X:{unit.Position.X}, Y:{unit.Position.Y}, Z:{unit.Position.Z}, Facing:{unit.Facing}");
        }
        private static void ReadTransportData(BinaryReader reader, WoWUnit unit)
        {
            unit.TransportGuid = BitConverter.ToUInt64(reader.ReadBytes(4));
            unit.TransportOffset.X = reader.ReadSingle();
            unit.TransportOffset.Y = reader.ReadSingle();
            unit.TransportOffset.Z = reader.ReadSingle();
            unit.TransportOrientation = reader.ReadSingle();
            unit.TransportLastUpdated = reader.ReadUInt32();

            //Console.WriteLine($"[Transport] Guid:{unit.TransportGuid}, Offset:{unit.TransportOffset}, Orientation:{unit.TransportOrientation}, Updated:{unit.TransportLastUpdated}");
        }
        private static void ReadSwimPitch(BinaryReader reader, WoWUnit unit)
        {
            unit.SwimPitch = reader.ReadSingle();
            //Console.WriteLine($"[SwimPitch] {unit.SwimPitch}");
        }
        private static void ReadJumpData(BinaryReader reader, WoWUnit unit)
        {
            unit.JumpVerticalSpeed = reader.ReadSingle();
            unit.JumpSinAngle = reader.ReadSingle();
            unit.JumpCosAngle = reader.ReadSingle();
            unit.JumpHorizontalSpeed = reader.ReadSingle();

            //Console.WriteLine($"[JumpData] VSpeed:{unit.JumpVerticalSpeed}, Sin:{unit.JumpSinAngle}, Cos:{unit.JumpCosAngle}, HSpeed:{unit.JumpHorizontalSpeed}");
        }
        private static void ReadSpeeds(BinaryReader reader, WoWUnit unit)
        {
            unit.WalkSpeed = reader.ReadSingle();
            unit.RunSpeed = reader.ReadSingle();
            unit.RunBackSpeed = reader.ReadSingle();
            unit.SwimSpeed = reader.ReadSingle();
            unit.SwimBackSpeed = reader.ReadSingle();
            unit.TurnRate = reader.ReadSingle();

            //Console.WriteLine($"[Speeds] Walk:{unit.WalkSpeed}, Run:{unit.RunSpeed}, RunBack:{unit.RunBackSpeed}, Swim:{unit.SwimSpeed}, SwimBack:{unit.SwimBackSpeed}, Turn:{unit.TurnRate}");
        }
        private static void ReadSplineData(BinaryReader reader, WoWUnit unit)
        {
            SplineFlags flags = (SplineFlags)reader.ReadUInt32();
            unit.SplineFlags = flags;
            //Console.WriteLine($"[Spline] Flags: {flags}");

            if (flags.HasFlag(SplineFlags.FinalPoint))
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                unit.SplineFinalPoint = new Position(x, y, z);
                //Console.WriteLine($"[Spline] FinalPoint: X:{x}, Y:{y}, Z:{z}");
            }
            else if (flags.HasFlag(SplineFlags.FinalTarget))
            {
                ulong targetGuid = reader.ReadUInt64();
                unit.SplineTargetGuid = targetGuid;
                //Console.WriteLine($"[Spline] FinalTarget: {targetGuid:X}");
            }
            else if (flags.HasFlag(SplineFlags.FinalOrientation))
            {
                float angle = reader.ReadSingle();
                unit.SplineFinalOrientation = angle;
                //Console.WriteLine($"[Spline] FinalOrientation: {angle}");
            }

            unit.SplineTimePassed = reader.ReadInt32();
            unit.SplineDuration = reader.ReadInt32();
            unit.SplineId = reader.ReadUInt32();

            //Console.WriteLine($"[Spline] TimePassed:{unit.SplineTimePassed}, Duration:{unit.SplineDuration}, Id:{unit.SplineId}");

            uint nodeCount = reader.ReadUInt32();
            unit.SplineNodes = new List<Position>((int)nodeCount);

            //Console.WriteLine($"[Spline] NodeCount: {nodeCount}");

            for (int i = 0; i < nodeCount; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                var node = new Position(x, y, z);
                unit.SplineNodes.Add(node);
                //Console.WriteLine($"[Spline] Node[{i}]: X:{x}, Y:{y}, Z:{z}");
            }

            float finalX = reader.ReadSingle();
            float finalY = reader.ReadSingle();
            float finalZ = reader.ReadSingle();
            unit.SplineFinalDestination = new Position(finalX, finalY, finalZ);

            //Console.WriteLine($"[Spline] FinalDestination: X:{finalX}, Y:{finalY}, Z:{finalZ}");
        }
    }
}
