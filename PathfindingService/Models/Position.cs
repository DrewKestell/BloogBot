using System.Runtime.InteropServices;

namespace PathfindingService.Models
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
        public bool InLosWith(Position position)
        {
            //if (position.X == X && position.Y == Y && position.Z == Z) return true;
            //var i = Functions.Intersect(this, position);
            //return i.X == 0 && i.Y == 0 && i.Z == 0;
            return true;
        }

        public static Position operator -(Position a, Position b) =>
            new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Position operator +(Position a, Position b) =>
            new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Position operator *(Position a, int n) =>
        new(a.X * n, a.Y * n, a.Z * n);

        public XYZ ToXYZ() => new(X, Y, Z);
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct XYZ(float x, float y, float z)
    {
        internal float X = x;
        internal float Y = y;
        internal float Z = z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XYZXYZ(float x1, float y1, float z1,
        float x2, float y2, float z2)
    {
        internal float X1 = x1;
        internal float Y1 = y1;
        internal float Z1 = z1;
        internal float X2 = x2;
        internal float Y2 = y2;
        internal float Z2 = z2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Intersection
    {
        internal float X;
        internal float Y;
        internal float Z;
        internal float R;
    }
}
