using G3D;
using VMAP;

namespace PathfindingService.Repository
{
    public sealed class BIH
    {
        public interface IRayIntersectionCallback
        {
            bool OnRayIntersect(Ray ray, uint entry, ref float distance,
                                bool stopAtFirstHit, bool ignoreM2Model);
        }
        public interface IPointIntersectionCallback { void OnPointIntersect(Vector3 p, uint entry); }

        public const int MAX_STACK_SIZE = 64;

        private readonly List<uint> _objects = new();
        private AABox _bounds;

        public BIH() => _bounds = new AABox(Vector3.Zero, Vector3.Zero);
        internal BIH(AABox bounds, IList<uint> objs)
        {
            _bounds = bounds; _objects = new List<uint>(objs);
        }

        public AABox Bounds => _bounds;
        public uint primCount() => (uint)_objects.Count;
        private uint[] _nodes = Array.Empty<uint>();

        private struct StackEntry
        {
            public int node;
            public float tnear;
            public float tfar;
        }
        // BIH.cs
        public bool readFromFile(BinaryReader br)
        {
            try
            {
                Console.WriteLine("BIH: Reading bounds...");
                Vector3 lo = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Vector3 hi = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                _bounds = new AABox(lo, hi);
                Console.WriteLine($"BIH: bounds = {lo} → {hi}");

                Console.WriteLine("BIH: Reading node count...");
                uint nodeCnt = br.ReadUInt32();
                Console.WriteLine($"BIH: nodeCnt = {nodeCnt}");
                _nodes = new uint[nodeCnt];
                for (uint i = 0; i < nodeCnt; ++i)
                {
                    _nodes[i] = br.ReadUInt32();
                    //Console.WriteLine($"BIH: node[{i}] = {_nodes[i]}");
                }

                Console.WriteLine("BIH: Reading object index count...");
                uint objCnt = br.ReadUInt32();
                Console.WriteLine($"BIH: objCnt = {objCnt}");
                _objects.Clear();
                for (uint i = 0; i < objCnt; ++i)
                {
                    uint o = br.ReadUInt32();
                    _objects.Add(o);
                    //Console.WriteLine($"BIH: object[{i}] = {o}");
                }

                Console.WriteLine("BIH: readFromFile succeeded");
                return true;
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"BIH: EndOfStreamException: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"BIH: Exception: {e.Message}");
                return false;
            }
        }

        // ───────────────────────────── Logging support ────────────────────────
        /// <summary>
        /// When non‑null, receives trace strings that describe how <see cref="intersectRay"/>
        /// traverses the BIH.  Assign e.g. <c>BIH.Logger = Console.WriteLine;</c> to enable.
        /// </summary>
        public static Action<string>? Logger { get; set; }

        /// <inheritdoc cref="BIH.intersectRay"/>
        public void intersectRay(
            Ray r,
            IRayIntersectionCallback cb,
            ref float maxDist,
            bool stopAtFirstHit,
            bool ignoreM2Model)
        {
            const float EPS = 1e-8f;

            Vector3 org = r.Origin;
            Vector3 dir = r.Direction;
            Vector3 invDir = r.invDirection();

            Logger?.Invoke($"[BIH] START origin={org} dir={dir} max={maxDist:0.###}");

            // ── 1. Root‑AABB clip ───────────────────────────────────────────
            float tMin = 0f, tMax = maxDist;
            if (!_bounds.Intersects(r, ref tMin, ref tMax)) { Logger?.Invoke("[BIH] ROOT miss – abort"); return; }
            if (tMax <= 0f) { Logger?.Invoke("[BIH] behind origin"); return; }
            tMin = MathF.Max(tMin, 0f);
            tMax = MathF.Min(tMax, maxDist);

            // ── 2. Degenerate tree (no nodes) ───────────────────────────────
            if (_nodes.Length == 0)
            {
                for (int i = 0; i < _objects.Count; ++i)
                    if (cb.OnRayIntersect(r, _objects[i], ref maxDist, stopAtFirstHit, ignoreM2Model) && stopAtFirstHit)
                        return;
                return;
            }

            // ── 3. Direction‑dependent child offset tables ──────────────────
            int[] offF = new int[3];
            int[] offB = new int[3];
            int[] offF3 = new int[3];
            int[] offB3 = new int[3];
            for (int ax = 0; ax < 3; ++ax)
            {
                int s = (BitConverter.SingleToInt32Bits(dir[ax]) >> 31) & 1;
                offF[ax] = s;          // front child idx offset (0/1)
                offB[ax] = s ^ 1;      // back  child idx offset
                offF3[ax] = (s) * 3;   // front child array offset (+0 or +3)
                offB3[ax] = (s ^ 1) * 3;
                ++offF[ax];            // node layout compensation
                ++offB[ax];
            }

            // ── 4. Traversal stack ------------------------------------------------
            Span<StackEntry> stack = stackalloc StackEntry[MAX_STACK_SIZE];
            int sp = 0;   // stack pointer
            int node = 0; // current node index
            float iMin = tMin;
            float iMax = tMax;

            // ── 5. DFS loop -------------------------------------------------------
            while (true)
            {
                // ----- descend -----
                while (true)
                {
                    uint raw = _nodes[node];
                    int axis = (int)((raw >> 30) & 3u);
                    bool isBVH2 = (raw & (1u << 29)) != 0;
                    int offs = (int)(raw & ~(7u << 29));

                    if (!isBVH2)
                    {
                        if (axis < 3) // interior 2‑child node
                        {
                            float tf = (BitConverter.Int32BitsToSingle((int)_nodes[node + offF[axis]]) - org[axis]) * invDir[axis];
                            float tb = (BitConverter.Int32BitsToSingle((int)_nodes[node + offB[axis]]) - org[axis]) * invDir[axis];

                            Logger?.Invoke($"[BIH] node={node} axis={axis} tf={tf:0.###} tb={tb:0.###} imin={iMin:0.###} imax={iMax:0.###}");

                            if (tf < iMin && tb > iMax) break; // prune

                            int farChild = offs + offB3[axis];
                            node = farChild; // assume far

                            if (tf < iMin) { iMin = MathF.Max(tb, iMin); continue; } // only far

                            int nearChild = offs + offF3[axis];
                            node = nearChild; // near first

                            if (tb > iMax) { iMax = MathF.Min(tf, iMax); continue; } // only near

                            // both children: push far
                            if (sp >= MAX_STACK_SIZE)
                                throw new InvalidOperationException("BIH stack overflow");
                            stack[sp].node = farChild;
                            stack[sp].tnear = MathF.Max(tb, iMin);
                            stack[sp].tfar = iMax;
                            ++sp;

                            iMax = MathF.Min(tf, iMax);
                            continue;
                        }
                        else // *leaf* node ------------------------------------------------
                        {
                            int count = unchecked((int)_nodes[node + 1]);
                            int first = offs;
                            Logger?.Invoke($"[BIH] leaf node={node} objs={count} tRange=[{iMin:0.###},{iMax:0.###}]");
                            for (int i = 0; i < count; ++i)
                            {
                                bool hit = cb.OnRayIntersect(r, _objects[first + i], ref maxDist, stopAtFirstHit, ignoreM2Model);
                                if (hit && stopAtFirstHit) { Logger?.Invoke("[BIH]   early‑exit hit"); return; }
                            }
                            break; // finished leaf
                        }
                    }
                    else // BVH2 slab node ------------------------------------------------
                    {
                        float tf = (BitConverter.Int32BitsToSingle((int)_nodes[node + offF[axis]]) - org[axis]) * invDir[axis];
                        float tb = (BitConverter.Int32BitsToSingle((int)_nodes[node + offB[axis]]) - org[axis]) * invDir[axis];
                        Logger?.Invoke($"[BIH] BVH2 node={node} axis={axis} tf={tf:0.###} tb={tb:0.###} imin={iMin:0.###} imax={iMax:0.###}");
                        node = offs; // single child
                        iMin = MathF.Max(tf, iMin);
                        iMax = MathF.Min(tb, iMax);
                        if (iMin > iMax) break; // miss
                        continue;
                    }
                }

                // ----- back‑track -----
                while (true)
                {
                    if (sp == 0) { Logger?.Invoke("[BIH] END traversal"); return; }
                    --sp;
                    iMin = stack[sp].tnear;
                    if (maxDist < iMin) continue; // cell behind best hit
                    node = stack[sp].node;
                    iMax = stack[sp].tfar;
                    Logger?.Invoke($"[BIH] pop→ node={node} tRange=[{iMin:0.###},{iMax:0.###}]");
                    break; // resume descent
                }
            }
        }
        public void intersectPoint(Vector3 p, IPointIntersectionCallback cb)
        {
            if (!_bounds.Contains(p)) return; for (uint i = 0; i < _objects.Count; ++i) cb.OnPointIntersect(p, _objects[(int)i]);
        }
    }
}