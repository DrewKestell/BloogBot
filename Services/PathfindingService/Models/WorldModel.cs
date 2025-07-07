using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using G3D;
using PathfindingService.Repository;

namespace VMAP
{
    /// <summary>
    /// Holds an entire model's collision data (could be a multi-group WMO or a single M2 model).
    /// Provides methods to test collisions against its geometry.
    /// </summary>
    public class WorldModel
    {
        private uint RootWMOID;                              // Identifier for the root WMO (0 for M2)
        private List<GroupModel> groupModels = new List<GroupModel>();  // All group sub-models
        private AABox modelBounds;                           // Bounding box encompassing all groups
        private uint modelFlags;                             // Flags (e.g., MOD_M2)
        private BIH? groupTree;                              // Optional group‐level BIH for acceleration

        public uint GetRootWmoID() => RootWMOID;
        public void SetRootWmoID(uint id) => RootWMOID = id;
        public uint GetModelFlags() => modelFlags;
        public void SetModelFlags(uint flags) => modelFlags = flags;

        /// <summary>
        /// Assigns group models, computes global bounds, and builds a BIH over group indices.
        /// </summary>
        public void SetGroupModels(List<GroupModel> groups)
        {
            groupModels = groups;
            if (groups.Count > 0)
            {
                modelBounds = groups[0].Bounds;
                foreach (var gm in groups)
                {
                    var b = gm.Bounds;
                    modelBounds.Min = modelBounds.Min.Min(b.Min);
                    modelBounds.Max = modelBounds.Max.Max(b.Max);
                }
            }
            else
            {
                modelBounds = AABox.Zero;
            }

            // Build group‐level BIH for acceleration
            var indices = new List<uint>();
            for (uint i = 0; i < (uint)groups.Count; ++i)
                indices.Add(i);
            groupTree = new BIH(modelBounds, indices);
        }

        /// <summary>
        /// Ray intersection with this model's geometry.
        /// Uses BIH if available, otherwise brute‐forces over groups.
        /// </summary>
        public bool IntersectRay(Ray ray, ref float pMaxDist, bool stopAtFirstHit, bool ignoreM2Model)
        {
            if (ignoreM2Model && ((ModelFlags)modelFlags).HasFlag(ModelFlags.MOD_M2))
                return false;

            bool hit = false;

            if (groupTree != null)
            {
                groupTree.intersectRay(
                    ray,
                    new RayCallback(this, stopAtFirstHit, ignoreM2Model),
                    ref pMaxDist,
                    stopAtFirstHit,
                    ignoreM2Model
                );
                hit = pMaxDist < float.PositiveInfinity;
            }
            else
            {
                if (groupModels.Count == 1)
                {
                    hit = groupModels[0].IntersectRay(ray, ref pMaxDist, stopAtFirstHit, ignoreM2Model);
                }
                else
                {
                    foreach (var gm in groupModels)
                    {
                        float t = ray.IntersectionTime(gm.Bounds);
                        if (float.IsPositiveInfinity(t)) continue;
                        if (gm.IntersectRay(ray, ref pMaxDist, stopAtFirstHit, ignoreM2Model))
                        {
                            hit = true;
                            if (stopAtFirstHit) break;
                        }
                    }
                }
            }

            return hit;
        }

        private class RayCallback : BIH.IRayIntersectionCallback
        {
            private readonly WorldModel owner;
            private readonly bool stopAtFirst;
            private readonly bool ignoreM2;

            public RayCallback(WorldModel owner, bool stopAtFirstHit, bool ignoreM2Model)
            {
                this.owner = owner;
                this.stopAtFirst = stopAtFirstHit;
                this.ignoreM2 = ignoreM2Model;
            }

            public bool OnRayIntersect(Ray r, uint entry, ref float distance, bool stopAtFirstHit, bool ignoreM2Model)
            {
                var gm = owner.groupModels[(int)entry];
                return gm.IntersectRay(r, ref distance, stopAtFirst, ignoreM2);
            }
        }

        /// <summary>
        /// Casts a ray downward from a point to find geometry beneath it.
        /// </summary>
        public bool IntersectPoint(Vector3 point, Vector3 downDirection, out float zDist, out GroupLocationInfo locInfo)
        {
            locInfo = new GroupLocationInfo();
            zDist = 0f;
            bool hit = false;
            float bestDist = float.PositiveInfinity;
            GroupModel? bestGroup = null;

            var downRay = new Ray(point, downDirection);
            foreach (var gm in groupModels)
            {
                if (!gm.Bounds.Contains(point)) continue;
                float localDist = float.PositiveInfinity;
                if (gm.IntersectRay(downRay, ref localDist, true))
                {
                    if (localDist < bestDist)
                    {
                        bestDist = localDist;
                        bestGroup = gm;
                        hit = true;
                    }
                }
            }

            if (hit && bestGroup != null)
            {
                zDist = bestDist;
                locInfo.hitModel = bestGroup;
                locInfo.rootId = (int)RootWMOID;
            }
            return hit;
        }

        /// <summary>
        /// Determines if a point is under any part of this model.
        /// </summary>
        public bool IsUnderObject(Vector3 pos, Vector3 up, bool isM2, float? outDistParam, float? inDistParam)
        {
            bool under = false;
            float nearestOut = -1f, nearestIn = -1f;

            foreach (var gm in groupModels)
            {
                float outDist, inDist;
                if (gm.IsUnderObject(pos, up, isM2, out outDist, out inDist))
                    under = true;

                if (outDist >= 0f && (nearestOut < 0f || outDist < nearestOut))
                    nearestOut = outDist;
                if (inDist >= 0f && (nearestIn < 0f || inDist < nearestIn))
                    nearestIn = inDist;
            }

            if (outDistParam.HasValue) outDistParam = nearestOut;
            if (inDistParam.HasValue) inDistParam = nearestIn;
            return under;
        }

        /// <summary>
        /// Retrieves ground‐collision info for a point.
        /// </summary>
        public bool GetLocationInfo(Vector3 pos, Vector3 downDirection, out float zDist, out GroupLocationInfo info)
        {
            info = new GroupLocationInfo();
            zDist = 0f;
            bool hit = false;
            float bestDist = float.PositiveInfinity;
            GroupModel? hitGroup = null;

            var ray = new Ray(pos, downDirection);
            foreach (var gm in groupModels)
            {
                if (!gm.Bounds.Contains(pos)) continue;
                float localDist = float.PositiveInfinity;
                if (gm.IntersectRay(ray, ref localDist, true))
                {
                    if (localDist < bestDist)
                    {
                        bestDist = localDist;
                        hitGroup = gm;
                        hit = true;
                    }
                }
            }

            if (hit && hitGroup != null)
            {
                zDist = bestDist;
                info.hitModel = hitGroup;
                info.rootId = (int)RootWMOID;
            }
            return hit;
        }

        /// <summary>
        /// Retrieves liquid surface height at a point, if any.
        /// </summary>
        public bool GetLiquidLevel(Vector3 pos, out float level)
        {
            level = 0f;
            bool found = false;
            float highest = float.MinValue;

            foreach (var gm in groupModels)
            {
                if (!gm.Bounds.Contains(pos)) continue;
                if (gm.GetLiquidLevel(pos, out float liqHeight))
                {
                    if (!found || liqHeight > highest)
                    {
                        highest = liqHeight;
                        found = true;
                    }
                }
            }

            if (found) level = highest;
            return found;
        }

        /// <summary>
        /// Reads a WorldModel from a .vmo file.
        /// Follows VMAP_MAGIC → WMOD → GMOD → [GBIH] sequence. :contentReference[oaicite:0]{index=0}
        /// </summary>
        public bool ReadFile(string filename)
        {
            try
            {
                Console.WriteLine($"WorldModel: Opening '{filename}'");
                using var fs = File.OpenRead(filename);
                using var br = new BinaryReader(fs);

                // 1) VMAP_MAGIC
                Console.WriteLine("WorldModel: Checking VMAP_MAGIC...");
                if (!VMapDefinitions.ReadChunk(br, VMapDefinitions.VMAP_MAGIC, 8))
                {
                    Console.WriteLine("WorldModel: VMAP_MAGIC mismatch");
                    return false;
                }
                Console.WriteLine("WorldModel: VMAP_MAGIC OK");

                // 2) WMOD header
                Console.WriteLine("WorldModel: Reading WMOD header...");
                if (!VMapDefinitions.ReadChunk(br, "WMOD", 4))
                {
                    Console.WriteLine("WorldModel: Missing WMOD chunk");
                    return false;
                }
                uint headerSize = br.ReadUInt32();
                RootWMOID = br.ReadUInt32();
                Console.WriteLine($"WorldModel: WMOD headerSize={headerSize}, RootWMOID={RootWMOID}");

                Console.WriteLine($"WorldModel: modelFlags=0x{modelFlags:X}");
                // M2 path
                if ((modelFlags & (uint)ModelFlags.MOD_M2) != 0)
                {
                    Console.WriteLine("WorldModel: Detected MOD_M2 – loading single-group mesh for AABB");

                    // 3) GMOD chunk (should be exactly 1)
                    Console.WriteLine("WorldModel: Reading GMOD chunk for M2...");
                    if (!VMapDefinitions.ReadChunk(br, "GMOD", 4))
                    {
                        Console.WriteLine("WorldModel: Missing GMOD chunk");
                        return false;
                    }
                    uint groupCount = br.ReadUInt32();
                    Console.WriteLine($"WorldModel:   groupCount = {groupCount}");
                    if (groupCount != 1)
                        Console.WriteLine("WorldModel: Warning – expected 1 group for M2");

                    // Load the single group
                    var gm = new GroupModel();
                    Console.WriteLine("WorldModel:   Reading single GroupModel...");
                    if (!gm.ReadFromFile(br))
                    {
                        Console.WriteLine("WorldModel:   Failed to read M2 GroupModel");
                        return false;
                    }
                    Console.WriteLine($"WorldModel:   Local mesh AABB = Min{gm.Bounds.Min} Max{gm.Bounds.Max}");

                    // Build local modelBounds
                    SetGroupModels(new List<GroupModel> { gm });

                    // Read spawn info immediately afterwards
                    Console.WriteLine("WorldModel: Rewinding to WMOD payload for spawn data");
                    fs.Seek(12, SeekOrigin.Begin);
                    var inst = new ModelInstance();
                    Console.WriteLine("WorldModel: Parsing ModelInstance (spawn data)...");
                    if (!inst.ReadFromFile(br))
                    {
                        Console.WriteLine("WorldModel: ModelInstance.ReadFromFile failed");
                        return false;
                    }

                    // Compute world-space AABB
                    Console.WriteLine("WorldModel: Computing world‐space AABB...");
                    var local = modelBounds;
                    var corners = new[]
                    {
                        new Vector3(local.Min.x, local.Min.y, local.Min.z),
                        new Vector3(local.Min.x, local.Min.y, local.Max.z),
                        new Vector3(local.Min.x, local.Max.y, local.Min.z),
                        new Vector3(local.Min.x, local.Max.y, local.Max.z),
                        new Vector3(local.Max.x, local.Min.y, local.Min.z),
                        new Vector3(local.Max.x, local.Min.y, local.Max.z),
                        new Vector3(local.Max.x, local.Max.y, local.Min.z),
                        new Vector3(local.Max.x, local.Max.y, local.Max.z),
                    };
                    // Build rotation matrix from ZYX Euler (rz, ry, rx)
                    var rotMat = Matrix3.FromEulerAnglesZYX(inst.iRot.z, inst.iRot.y, inst.iRot.x);
                    var worldBox = new AABox(
                        new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
                        new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity)
                    );

                    foreach (var c in corners)
                    {
                        // scale → rotate → translate
                        var v = c * inst.iScale;
                        v = rotMat * v;
                        v += inst.iPos;
                        worldBox.Min = worldBox.Min.Min(v);
                        worldBox.Max = worldBox.Max.Max(v);
                    }
                    Console.WriteLine($"WorldModel: World AABB = Min{worldBox.Min} Max{worldBox.Max}");
                    inst.iBound = worldBox;
                    Console.WriteLine("WorldModel: M2 model loaded successfully");
                    return true;
                }

                // WMO path
                Console.WriteLine("WorldModel: Reading GMOD chunk for WMO...");
                if (!VMapDefinitions.ReadChunk(br, "GMOD", 4))
                {
                    Console.WriteLine("WorldModel: Missing GMOD chunk");
                    return false;
                }
                uint count = br.ReadUInt32();
                Console.WriteLine($"WorldModel: groupCount={count}");
                var groups = new List<GroupModel>((int)count);
                for (uint i = 0; i < count; ++i)
                {
                    Console.WriteLine($"WorldModel: Reading GroupModel #{i}...");
                    var gm = new GroupModel();
                    if (!gm.ReadFromFile(br))
                    {
                        Console.WriteLine($"WorldModel: Failed to read GroupModel #{i}");
                        return false;
                    }
                    Console.WriteLine($"WorldModel: GroupModel #{i} loaded (WMOID={gm.GroupWMOID})");
                    groups.Add(gm);
                }
                SetGroupModels(groups);
                Console.WriteLine("WorldModel: All GroupModels loaded and set");

                Console.WriteLine("WorldModel: Checking for GBIH chunk...");
                if (VMapDefinitions.ReadChunk(br, "GBIH", 4))
                {
                    Console.WriteLine("WorldModel: GBIH chunk present – reading BIH");
                    var t = new BIH();
                    if (!t.readFromFile(br))
                    {
                        Console.WriteLine("WorldModel: Failed to read GBIH BIH");
                        return false;
                    }
                    groupTree = t;
                    Console.WriteLine("WorldModel: GBIH BIH loaded");
                }
                else
                {
                    Console.WriteLine("WorldModel: No GBIH chunk");
                }

                Console.WriteLine("WorldModel: ReadFile completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WorldModel: Exception in ReadFile: {ex.Message}");
                return false;
            }
        }

    }
}
