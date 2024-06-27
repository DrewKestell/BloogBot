using Newtonsoft.Json;
using WoWActivityMember.Constants;
using WoWActivityMember.Mem;

namespace WoWActivityMember.Objects
{
    public class Position
    {
        [JsonConstructor]
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

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

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
            if (position.X == X && position.Y == Y && position.Z == Z) return true;
            var i = Functions.Intersect(this, position);
            return i.X == 0 && i.Y == 0 && i.Z == 0;
        }

        public static Position operator -(Position a, Position b) =>
            new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Position operator +(Position a, Position b) =>
            new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Position operator *(Position a, int n) =>
        new(a.X * n, a.Y * n, a.Z * n);

        public XYZ ToXYZ() => new(X, Y, Z);
    }
}
