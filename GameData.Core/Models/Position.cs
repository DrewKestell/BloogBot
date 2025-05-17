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
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XYZ
    {
        internal float X;
        internal float Y;
        internal float Z;

        internal XYZ(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public enum PointComparisonResult : byte
    {
        LeftOfLine,
        OnLine,
        RightOfLine
    }
}
