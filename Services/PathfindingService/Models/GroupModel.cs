using PathfindingService.Repository;
using System;
using System.Collections.Generic;
using System.IO;

namespace VMAP
{
    /// <summary>
    /// Represents a sub-model (group) of a WorldModel. 
    /// For WMO, each group is a chunk of the overall model (e.g., one interior or exterior section).
    /// For an M2 model, typically there would be one group encompassing the whole mesh.
    /// </summary>
    public class GroupModel
    {
        private AABox iBound;
        private uint iMogpFlags;   // WMO group flags (e.g., outdoor/indoor flags)
        private uint iGroupWMOID;  // Unique identifier for the group (from WMO data)
        private List<Vector3> vertices = new List<Vector3>();
        private List<MeshTriangle> triangles = new List<MeshTriangle>();
        private WmoLiquid? iLiquid; // Liquid data, if any, in this group

        // Properties to expose read-only data
        public AABox Bounds => iBound;
        public uint MogpFlags => iMogpFlags;
        public uint GroupWMOID => iGroupWMOID;

        public GroupModel(uint mogpFlags = 0, uint groupWmoID = 0, AABox? bounds = null)
        {
            iMogpFlags = mogpFlags;
            iGroupWMOID = groupWmoID;
            iBound = bounds ?? AABox.Zero;
        }

        ~GroupModel()
        {
            // Cleanup liquid if allocated (not strictly necessary with GC, but just in case)
            iLiquid = null;
        }

        /// <summary>
        /// Transfers the provided mesh data into this GroupModel and builds a bounding volume hierarchy if needed.
        /// (Here, we simply store the data; the BIH is not explicitly built in C#, relying on linear scan or external structure.)
        /// </summary>
        public void SetMeshData(List<Vector3> verts, List<MeshTriangle> tris)
        {
            vertices = verts;
            triangles = tris;
            // Optionally, we could build an acceleration structure for triangles here for performance.
            // The original C++ builds a BIH (meshTree). For simplicity, we skip or assume an external tree.
        }

        public void SetLiquidData(WmoLiquid? liquid)
        {
            iLiquid = liquid;
        }

        /// <summary>
        /// Ray-triangle intersection test for all triangles in this group.
        /// If StopAtFirstHit is true, returns as soon as a hit is found.
        /// Returns true if any intersection found (and distance is updated to closest hit).
        /// </summary>
        public bool IntersectRay(Ray ray, ref float distance, bool stopAtFirstHit, bool ignoreM2Model = false)
        {
            bool hitAny = false;
            // If ignoring M2 models and this group is part of an M2 (MogpFlags not used for M2, so we require context externally),
            // we might need an external flag. We assume GroupModel for M2 passes ignoreM2 at WorldModel level.
            // We simply iterate through all triangles and check ray intersection.
            for (int i = 0; i < triangles.Count; ++i)
            {
                MeshTriangle tri = triangles[i];
                // Fetch triangle vertices
                Vector3 v0 = vertices[(int)tri.idx0];
                Vector3 v1 = vertices[(int)tri.idx1];
                Vector3 v2 = vertices[(int)tri.idx2];
                if (IntersectTriangle(v0, v1, v2, ray, ref distance))
                {
                    hitAny = true;
                    if (stopAtFirstHit)
                        return true;
                    // continue checking to find if any closer hit exists (distance is updated to nearest so far)
                }
            }
            return hitAny;
        }

        /// <summary>
        /// Check if a point is inside this group's geometry by casting a ray upward.
        /// If an intersection is found above the point, returns true and outputs the vertical distance.
        /// </summary>
        public bool IsInsideObject(Vector3 pos, Vector3 up, out float zDist)
        {
            zDist = 0f;
            if (triangles.Count == 0 || !iBound.Contains(pos))
                return false;
            // Cast a ray upward from pos
            Ray rayUp = new Ray(pos, up);
            float maxDist = float.PositiveInfinity;
            bool hit = IntersectRay(rayUp, ref maxDist, stopAtFirstHit: true);
            if (hit)
            {
                zDist = maxDist;
            }
            return hit;
        }

        /// <summary>
        /// Determines if a point is under this object by checking oriented ray intersections.
        /// This differentiates between entering (outDist) and exiting (inDist) collisions to detect if the point is enclosed.
        /// </summary>
        public bool IsUnderObject(Vector3 pos, Vector3 up, bool isM2, out float outDist, out float inDist)
        {
            outDist = -1f;
            inDist = -1f;
            if (triangles.Count == 0)
                return false;
            // We cast a ray upwards through the model and measure intersections.
            Ray rayUp = new Ray(pos, up);
            float inf = float.PositiveInfinity;
            // We'll record the nearest intersection going out of the object and the nearest intersection going into the object.
            // For an M2, the orientation might be reversed (the triangle winding).
            float minOut = -1f;
            float minIn = -1f;
            // Check all triangle intersections along the ray (we don't break early, we want both entry and exit distances)
            for (int i = 0; i < triangles.Count; ++i)
            {
                MeshTriangle tri = triangles[i];
                Vector3 v0 = vertices[(int)tri.idx0];
                Vector3 v1 = vertices[(int)tri.idx1];
                Vector3 v2 = vertices[(int)tri.idx2];
                float t = inf;
                if (IntersectTriangle(v0, v1, v2, rayUp, ref t))
                {
                    // Determine orientation: we need to check if this intersection is the ray entering or leaving the object.
                    // We can use the triangle normal dot ray direction to see if we are hitting frontface or backface.
                    Vector3 e1 = v1 - v0;
                    Vector3 e2 = v2 - v0;
                    Vector3 normal = e1.Cross(e2);
                    // For M2, the orientation is reversed in terms of what is "inside".
                    // We'll use the isM2 flag to flip logic if needed. 
                    bool frontFace = normal.Dot(rayUp.Direction) < 0f;
                    bool inToOutCollision = isM2 ? frontFace : !frontFace;
                    if (inToOutCollision)
                    {
                        if (minOut < 0f || t < minOut)
                            minOut = t;
                    }
                    else
                    {
                        if (minIn < 0f || t < minIn)
                            minIn = t;
                    }
                }
            }
            if (minOut >= 0f) outDist = minOut;
            if (minIn >= 0f) inDist = minIn;
            // Under the model if ray from inside doesn't hit a top surface before a bottom surface, i.e., either:
            // - No "out" hit but an "in" hit, or
            // - The first hit is an "in" (meaning we started under something).
            bool isUnder = ((minOut < 0f) && (minIn >= 0f)) || (minIn >= 0f && (minOut < 0f || minIn < minOut));
            return isUnder;
        }

        /// <summary>
        /// If this group has liquid, get the liquid height at the given position (model local coords).
        /// </summary>
        public bool GetLiquidLevel(Vector3 pos, out float liqHeight)
        {
            liqHeight = 0f;
            if (iLiquid != null)
            {
                return iLiquid.GetLiquidHeight(pos, out liqHeight);
            }
            return false;
        }

        public uint GetLiquidType()
        {
            return iLiquid?.GetType() ?? 0;
        }

        /// <summary>
        /// Reads this GroupModel from a binary stream (following the VMAP v7 format).
        /// Expects the stream to be positioned at the start of a group definition.
        /// </summary>
        // GroupModel.cs
        public bool ReadFromFile(BinaryReader br)
        {
            try
            {
                Console.WriteLine("GroupModel: Reading AABox...");
                var lo = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var hi = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                iBound = new AABox(lo, hi);
                Console.WriteLine($"GroupModel: bounds={lo}→{hi}");

                Console.WriteLine("GroupModel: Reading flags and group ID...");
                iMogpFlags = br.ReadUInt32();
                iGroupWMOID = br.ReadUInt32();
                Console.WriteLine($"GroupModel: flags=0x{iMogpFlags:X}, groupID={iGroupWMOID}");

                Console.WriteLine("GroupModel: Reading VERT chunk...");
                if (!VMapDefinitions.ReadChunk(br, "VERT", 4))
                {
                    Console.WriteLine("GroupModel: Missing VERT chunk");
                    return false;
                }
                uint vertChunkSize = br.ReadUInt32();    // skip the VERT chunk size
                uint vertCount = br.ReadUInt32();    // now read the real vertex count
                vertices.Clear();
                for (uint i = 0; i < vertCount; ++i)
                {
                    var v = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    vertices.Add(v);
                }

                if (vertCount == 0)
                {
                    Console.WriteLine("GroupModel: No vertices, returning early");
                    return true;
                }

                Console.WriteLine("GroupModel: Reading TRIM chunk...");
                if (!VMapDefinitions.ReadChunk(br, "TRIM", 4))
                {
                    Console.WriteLine("GroupModel: Missing TRIM chunk");
                    return false;
                }
                uint triChunkSize = br.ReadUInt32();
                uint triCount = br.ReadUInt32();
                Console.WriteLine($"GroupModel: triCount={triCount}");
                triangles.Clear();
                for (uint i = 0; i < triCount; ++i)
                    triangles.Add(new MeshTriangle(br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32()));

                Console.WriteLine("GroupModel: Reading MBIH chunk...");
                if (!VMapDefinitions.ReadChunk(br, "MBIH", 4))
                {
                    Console.WriteLine("GroupModel: Missing MBIH chunk");
                    return false;
                }
                var meshBih = new BIH();
                if (!meshBih.readFromFile(br))
                {
                    Console.WriteLine("GroupModel: mesh BIH read failure");
                    return false;
                }

                Console.WriteLine("GroupModel: Reading LIQU chunk...");
                if (!VMapDefinitions.ReadChunk(br, "LIQU", 4))
                {
                    Console.WriteLine("GroupModel: Missing LIQU chunk");
                    return false;
                }
                uint liquSize = br.ReadUInt32();
                Console.WriteLine($"GroupModel: liquSize={liquSize}");
                if (liquSize > 0)
                {
                    WmoLiquid.ReadFromFile(br, out iLiquid);
                    Console.WriteLine("GroupModel: Liquid loaded");
                }

                SetMeshData(vertices, triangles);
                Console.WriteLine("GroupModel: ReadFromFile complete");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GroupModel: Exception: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Internal helper: ray-triangle intersection (Möller–Trumbore algorithm).
        /// If an intersection is found closer than current 'distance', updates 'distance' and returns true.
        /// </summary>
        private static bool IntersectTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Ray ray, ref float distance)
        {
            // Möller-Trumbore intersection algorithm
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            Vector3 p = ray.Direction.Cross(e2);
            float a = e1.Dot(p);
            const float EPSILON = 1e-5f;
            if (MathF.Abs(a) < EPSILON)
                return false; // Ray is parallel to triangle plane
            float f = 1.0f / a;
            Vector3 s = ray.Origin - v0;
            float u = f * s.Dot(p);
            if (u < 0f || u > 1f)
                return false;
            Vector3 q = s.Cross(e1);
            float v = f * ray.Direction.Dot(q);
            if (v < 0f || u + v > 1f)
                return false;
            float t = f * e2.Dot(q);
            if (t > EPSILON && t < distance)
            {
                // Intersection within current closest distance
                distance = t;
                return true;
            }
            return false;
        }
    }
}
