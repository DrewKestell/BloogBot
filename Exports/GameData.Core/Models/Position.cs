using System.Numerics;
using System.Runtime.InteropServices;

namespace GameData.Core.Models
{
    public class Position
    {
        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position(XYZ xyz)
        {
            X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
        }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float DistanceTo(Position position)
        {
            var deltaX = X - position.X;
            var deltaY = Y - position.Y;
            var deltaZ = Z - position.Z;

            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public float DistanceTo2D(Position position)
        {
            var deltaX = X - position.X;
            var deltaY = Y - position.Y;

            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public Position GetNormalizedVector()
        {
            var magnitude = Math.Sqrt(X * X + Y * Y + Z * Z);

            return new Position((float)(X / magnitude), (float)(Y / magnitude), (float)(Z / magnitude));
        }

        public static Position operator -(Position a, Position b) =>
            new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Position operator +(Position a, Position b) =>
            new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Position operator *(Position a, int n) =>
            new(a.X * n, a.Y * n, a.Z * n);

        public XYZ ToXYZ() => new(X, Y, Z);

        public override string ToString() => $"X: {Math.Round(X, 2)}, Y: {Math.Round(Y, 2)}, Z: {Math.Round(Z, 2)}";

        public Vector3 ToVector3() => new(X, Y, Z);
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct XYZ(float x, float y, float z)
    {
        public readonly float X = x;
        public readonly float Y = y;
        public readonly float Z = z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleStruct
    {
        public XYZ A;
        public XYZ B;
        public XYZ C;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PolygonDataStruct
    {
        public TriangleStruct Triangle;
        public XYZ Normal;
        public SurfaceTag Tag;
        public float Elevation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RaycastHitStruct
    {
        [MarshalAs(UnmanagedType.I1)] // bool in C++
        public bool Hit;
        public XYZ Point;
        public XYZ Normal;
        public SurfaceTag Tag;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CapsuleSweepRequestStruct
    {
        public XYZ BaseCenter;
        public float Height;
        public float Radius;
        public XYZ MotionVector;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NavPoly
    {
        public ulong RefId;
        public uint Area;
        public uint Flags;
        public uint VertCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public XYZ[] Verts;
    }
    public enum SurfaceTag
    {
        Unknown = 0,
        Walkable,
        Water,
        Lava,
        Slippery,
        Climbable
    }

    public enum PointComparisonResult : byte
    {
        LeftOfLine,
        OnLine,
        RightOfLine
    }
}
