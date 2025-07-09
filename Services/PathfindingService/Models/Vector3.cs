namespace VMAP
{
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 Zero => new(0f, 0f, 0f);
        public static Vector3 Up => new(0f, 0f, 1f);
        public static Vector3 Down => new(0f, 0f, -1f);
        public float Length() => MathF.Sqrt(x * x + y * y + z * z);
        public Vector3 Normalized() { float len = Length(); return len < 1e-6f ? Zero : new(x / len, y / len, z / len); }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 v) => new(-v.x, -v.y, -v.z);
        public static Vector3 operator *(Vector3 v, float s) => new(v.x * s, v.y * s, v.z * s);
        public static Vector3 operator *(float s, Vector3 v) => new(v.x * s, v.y * s, v.z * s);
        public static Vector3 operator /(Vector3 v, float s) => s != 0 ? new(v.x / s, v.y / s, v.z / s) : Zero;
        public float Dot(Vector3 o) => x * o.x + y * o.y + z * o.z;
        public Vector3 Cross(Vector3 o) => new(y * o.z - z * o.y, z * o.x - x * o.z, x * o.y - y * o.x);
        public Vector3 Min(Vector3 o) => new(MathF.Min(x, o.x), MathF.Min(y, o.y), MathF.Min(z, o.z));
        public Vector3 Max(Vector3 o) => new(MathF.Max(x, o.x), MathF.Max(y, o.y), MathF.Max(z, o.z));
        public override string ToString() => $"({x:F3},{y:F3},{z:F3})";
        public override bool Equals(object? o) => (o is Vector3 v) && x == v.x && y == v.y && z == v.z;
        public override int GetHashCode() => HashCode.Combine(x, y, z);
        public static bool operator ==(Vector3 a, Vector3 b) => a.Equals(b);
        public static bool operator !=(Vector3 a, Vector3 b) => !a.Equals(b);
        public static Vector3 operator *(Vector3 v, Matrix3 m)
        {
            return new Vector3(
                v.x * m.m00 + v.y * m.m10 + v.z * m.m20,
                v.x * m.m01 + v.y * m.m11 + v.z * m.m21,
                v.x * m.m02 + v.y * m.m12 + v.z * m.m22);
        }
        public float this[int i]
        {
            readonly get => i switch
            {
                0 => x,
                1 => y,
                2 => z,
                _ => throw new IndexOutOfRangeException(nameof(i))
            };
            set
            {
                switch (i)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new IndexOutOfRangeException(nameof(i));
                }
            }
        }
    }
}