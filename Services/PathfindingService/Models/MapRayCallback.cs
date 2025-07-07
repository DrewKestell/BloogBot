using G3D;
using VMAP;
using static PathfindingService.Repository.BIH;

namespace PathfindingService.Repository
{
    // ────────────────────────── MapRayCallback ──────────────────────────────
    /// <summary>
    ///  BIH callback that filters model instances and records statistics while tracing a ray
    ///  through the static-map acceleration structure.
    /// </summary>
    public sealed class MapRayCallback : IRayIntersectionCallback
    {
        private readonly ModelInstance?[] _prims;
        private readonly bool _m2Only;

        // ── public telemetry ────────────────────────────────────────────────
        public int Tested { get; private set; }   // primitive AABBs tested
        public int HitCount { get; private set; }   // primitive geometry hits
        public bool DidHit { get; private set; }   // true when <HitCount> > 0

        public MapRayCallback(ModelInstance?[] prims, bool m2Only = false)
        {
            _prims = prims;
            _m2Only = m2Only;
        }

        /// <inheritdoc/>
        public bool OnRayIntersect(Ray ray, uint entry, ref float distance,
                           bool stopAtFirstHit, bool ignoreM2Model)
        {
            ++Tested;
            var inst = _prims[entry];

            // broad‑phase reject: null or filtered
            if (inst == null) return false;
            if (_m2Only && !inst.flags.HasAny((uint)ModelFlags.MOD_M2)) return false;

            // *after* AABB test – extremely cheap and very informative
            Console.WriteLine($"[TEST] id={entry,-6} name={inst.name,-32} max={distance:0.###}");

            bool hit = inst.IntersectRay(ray, ref distance, stopAtFirstHit, ignoreM2Model);
            if (hit)
            {
                ++HitCount; DidHit = true;
                Console.WriteLine($"[HIT ] id={entry,-6} d={distance:0.###}");
                return stopAtFirstHit;              // early‑out when caller asks
            }
            return false;
        }
    }

    // ────────────────── MapIntersectionFinderCallback ───────────────────────
    public class MapIntersectionFinderCallback : IRayIntersectionCallback
    {
        private readonly ModelInstance[] _v;
        public ModelInstance? result;
        public MapIntersectionFinderCallback(ModelInstance[] v) => _v = v;

        public bool OnRayIntersect(Ray ray, uint entry, ref float dist, bool stopAtFirstHit, bool ignoreM2)
        {
            var inst = _v[entry]; if (inst == null) return false;
            bool hit = inst.IntersectRay(ray, ref dist, stopAtFirstHit, ignoreM2);
            if (hit && (result == null || (((uint)result.flags & (uint)ModelFlags.MOD_NO_BREAK_LOS) != 0)))
                result = inst;
            return hit;
        }
    }

    // ─────────────────── Point‑callback helpers ─────────────────────────────
    public class AreaInfoCallback : IPointIntersectionCallback
    {
        private readonly ModelInstance[] _v; 
        public AreaInfo aInfo = new();
        public AreaInfoCallback(ModelInstance[] v) => _v = v;
        public void OnPointIntersect(Vector3 p, uint entry) => _v[entry]?.IntersectPoint(p, ref aInfo);
    }

    public class UnderModelCallback : IPointIntersectionCallback
    {
        private readonly ModelInstance[] _v; public float outDist = -1f, inDist = -1f;
        public UnderModelCallback(ModelInstance[] v) => _v = v;
        public void OnPointIntersect(Vector3 p, uint entry)
        {
            var inst = _v[entry]; if (inst == null) return;
            inst.IsUnderModel(p, out float o, out float i);
            if (o >= 0 && (outDist < 0 || o < outDist)) outDist = o;
            if (i >= 0 && (inDist < 0 || i < inDist)) inDist = i;
        }
        public bool UnderModel() => (outDist < 0 && inDist >= 0) || (inDist >= 0 && (outDist < 0 || inDist < outDist));
    }

    public class LocationInfoCallback : IPointIntersectionCallback
    {
        private readonly ModelInstance[] _v; 
        private LocationInfo _info; 
        public bool result;
        public LocationInfoCallback(ModelInstance[] v, LocationInfo info) { _v = v; _info = info; }
        public void OnPointIntersect(Vector3 p, uint entry)
        {
            var inst = _v[entry]; if (inst == null) return;
            if (inst.GetLocationInfo(p, out _info)) result = true;
        }
    }
}