namespace VMAP
{
    /// <summary>
    /// Axis-Aligned Bounding Box with minimum and maximum corners.
    /// </summary>
    public struct AABox
    {
        public Vector3 Min;
        public Vector3 Max;

        public AABox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Returns a special AABox representing an empty or zero-size box.</summary>
        public static AABox Zero => new AABox(Vector3.Zero, Vector3.Zero);

        /// <summary>Check if a point lies within this bounding box (inclusive on boundaries).</summary>
        public bool Contains(Vector3 point)
        {
            return (point.x >= Min.x && point.x <= Max.x) &&
                   (point.y >= Min.y && point.y <= Max.y) &&
                   (point.z >= Min.z && point.z <= Max.z);
        }

        /// <summary>Merge this box with a point, expanding it to include the point.</summary>
        public void Merge(Vector3 point)
        {
            Min = Min.Min(point);
            Max = Max.Max(point);
        }

        // Offset the box by a vector (translates both min and max)
        public static AABox operator +(AABox box, Vector3 vec)
        {
            return new AABox(box.Min + vec, box.Max + vec);
        }
        /// <summary>Ray–AABB clip; returns false on miss, true on hit and updates <c>tMin/tMax</c>.</summary>
        public readonly bool Intersects(in Ray r, ref float tMin, ref float tMax)
        {
            Vector3 org = r.Origin, dir = r.Direction;
            Vector3 inv = r.invDirection();

            for (int ax = 0; ax < 3; ++ax)
            {
                float o = org[ax], dInv = inv[ax], mn = Min[ax], mx = Max[ax];
                float t1 = (mn - o) * dInv;
                float t2 = (mx - o) * dInv;
                if (t1 > t2) (t1, t2) = (t2, t1);
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax)
                    return false;
            }
            return true;
        }
        public override string ToString() => $"Box[Min={Min}, Max={Max}]";

        // Equality and hashing to allow comparisons
        public override bool Equals(object? obj)
        {
            if (obj is AABox other)
                return Min == other.Min && Max == other.Max;
            return false;
        }
        public override int GetHashCode() => HashCode.Combine(Min, Max);
        public static bool operator ==(AABox a, AABox b) => a.Equals(b);
        public static bool operator !=(AABox a, AABox b) => !a.Equals(b);
    }
}
