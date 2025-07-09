namespace VMAP
{
    /// <summary>
    /// Ray with origin and direction (direction is normalized).
    /// </summary>
    public struct Ray
    {
        private Vector3 origin;
        private Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction, bool normalizeDir = true)
        {
            this.origin = origin;
            if (normalizeDir)
                this.direction = direction.Normalized();
            else
                this.direction = direction;
        }

        public Vector3 Origin => origin;
        public Vector3 Direction => direction;
        /// <summary>Component-wise reciprocal of the direction (with Inf for 0).</summary>
        public readonly Vector3 invDirection()
        {
            static float inv(float v) => MathF.Abs(v) < 1e-8f ? float.PositiveInfinity : 1f / v;
            return new Vector3(inv(Direction.x), inv(Direction.y), inv(Direction.z));
        }
        /// <summary>
        /// Computes the intersection distance of this ray with an axis-aligned bounding box.
        /// Returns positive infinity if no intersection within [0, +inf) range.
        /// </summary>
        public float IntersectionTime(AABox box)
        {
            // Use slabs method for Ray-AABB intersection
            float tMin = 0.0f;
            float tMax = float.PositiveInfinity;

            // For each axis, compute intersection t range
            // Direction components could be 0 (parallel to axis).
            if (MathF.Abs(direction.x) < 1e-8f)
            {
                if (origin.x < box.Min.x || origin.x > box.Max.x)
                    return float.PositiveInfinity; // no hit (ray is parallel and outside slab)
            }
            else
            {
                float invDir = 1.0f / direction.x;
                float t1 = (box.Min.x - origin.x) * invDir;
                float t2 = (box.Max.x - origin.x) * invDir;
                if (t1 > t2) (t1, t2) = (t2, t1); // swap
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax) return float.PositiveInfinity;
            }
            if (MathF.Abs(direction.y) < 1e-8f)
            {
                if (origin.y < box.Min.y || origin.y > box.Max.y)
                    return float.PositiveInfinity;
            }
            else
            {
                float invDir = 1.0f / direction.y;
                float t1 = (box.Min.y - origin.y) * invDir;
                float t2 = (box.Max.y - origin.y) * invDir;
                if (t1 > t2) (t1, t2) = (t2, t1);
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax) return float.PositiveInfinity;
            }
            if (MathF.Abs(direction.z) < 1e-8f)
            {
                if (origin.z < box.Min.z || origin.z > box.Max.z)
                    return float.PositiveInfinity;
            }
            else
            {
                float invDir = 1.0f / direction.z;
                float t1 = (box.Min.z - origin.z) * invDir;
                float t2 = (box.Max.z - origin.z) * invDir;
                if (t1 > t2) (t1, t2) = (t2, t1);
                tMin = MathF.Max(tMin, t1);
                tMax = MathF.Min(tMax, t2);
                if (tMin > tMax) return float.PositiveInfinity;
            }
            // If we reach here, the ray intersects the box between tMin and tMax.
            if (tMin < 0f)
            {
                // Ray origin is inside the box or just at boundary; treat as hit at t=0
                tMin = 0f;
            }
            return tMin;
        }
    }
}
