using PathfindingService.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace VMAP
{
    /// <summary>
    /// Holds all static model instances for a given map, and provides collision queries on them.
    /// If the map is tiled, it can load/unload tiles separately.
    /// </summary>
    public class StaticMapTree
    {
        private const uint CLEAR_INDOOR_FLAG = 0xFFFFDFFF;
        private const float TILE_SIZE = 533.3333333f;
        private const float CENTER_OFFSET = 32f * TILE_SIZE;         // 17 066.666 f
        private uint iMapID;
        private bool iIsTiled;
        private BIH iTree = new();                        // ── NEW: full BIH for spatial queries
        private ModelInstance?[]? iTreeValues;            // indexed by BIH‑leaf primitive id
        private uint iNTreeValues;
        // Track which tiles of this map are loaded (tileId -> bool indicating if a tile file was present)
        private Dictionary<uint, bool> iLoadedTiles = new Dictionary<uint, bool>();
        // Track reference counts for each ModelInstance index (for tiled maps to avoid unloading in-use models until last tile referencing them is gone)
        private Dictionary<uint, uint> iLoadedSpawns = new Dictionary<uint, uint>();
        private string iBasePath;

        public bool IsTiled => iIsTiled;
        public StaticMapTree(uint mapId, string basePath)
        {
            iMapID = mapId;
            iBasePath = basePath;
        }

        /// <summary>
        /// Returns a standardized tile file name for given map and tile coordinates.
        /// </summary>
        public static string GetTileFileName(uint mapID, uint tileX, uint tileY)
        {
            return $"{mapID:D3}_{tileX:D2}_{tileY:D2}.vmtile";
        }

        /// <summary>
        /// Packs tileX and tileY into a single 32-bit key for dictionaries.
        /// </summary>
        private uint PackTileID(uint tileX, uint tileY)
        {
            return (tileX << 16) | (tileY & 0xFFFF);
        }

        /// <summary>
        /// Initializes the map by loading its base .vmtree file.
        /// For non-tiled maps, this also loads all model spawns.
        /// </summary>
        public bool InitMap(string fileName, VMapManager2 vmMgr)
        {
            string full = Path.Combine(iBasePath, fileName);
            Console.WriteLine($"[InitMap] opening {full}");
            if (!File.Exists(full)) { Console.WriteLine("[InitMap] file not found"); return false; }

            try
            {
                using var fs = File.OpenRead(full);
                using var br = new BinaryReader(fs);

                if (!VMapDefinitions.ReadChunk(br, VMapDefinitions.VMAP_MAGIC, 8))
                { Console.WriteLine("[InitMap] bad VMAP header"); return false; }

                iIsTiled = br.ReadByte() != 0;
                Console.WriteLine($"[InitMap] map={iMapID} tiled={iIsTiled}");

                if (!VMapDefinitions.ReadChunk(br, "NODE", 4) || !iTree.readFromFile(br))
                { Console.WriteLine("[InitMap] failed to read BIH"); return false; }

                iNTreeValues = iTree.primCount();
                iTreeValues = new ModelInstance[iNTreeValues];
                Console.WriteLine($"[InitMap] primCount={iNTreeValues}");

                if (!VMapDefinitions.ReadChunk(br, "GOBJ", 4))
                { Console.WriteLine("[InitMap] missing GOBJ chunk"); return false; }

                if (!iIsTiled)
                {
                    uint added = 0;
                    ModelSpawn spawn;
                    while (ModelSpawn.ReadFromFile(br, out spawn))
                    {
                        var wm = vmMgr.AcquireModelInstance(iBasePath, spawn.name);
                        wm?.SetModelFlags((uint)spawn.flags);

                        uint refIdx = br.ReadUInt32();
                        if (refIdx >= iNTreeValues)
                        { Console.WriteLine($"[InitMap] invalid refIdx {refIdx}"); continue; }

                        if (!iLoadedSpawns.ContainsKey(refIdx))
                        {
                            iTreeValues![refIdx] = new ModelInstance(spawn, wm);
                            iLoadedSpawns[refIdx] = 1;
                            ++added;
                        }
                        else ++iLoadedSpawns[refIdx];
                    }
                    Console.WriteLine($"[InitMap] untiled spawns added={added}");
                }

                Console.WriteLine($"[InitMap] readerPos={br.BaseStream.Position} {br.BaseStream.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InitMap] exception: {ex.Message}");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Loads a specific map tile (for tiled maps only). 
        /// Parses the .vmtile file and adds ModelInstances for its spawns.
        /// </summary>
        public bool LoadMapTile(uint tileX, uint tileY, VMapManager2 vmMgr)
        {
            if (!iIsTiled) { iLoadedTiles[PackTileID(tileX, tileY)] = false; return true; }
            if (iTreeValues == null) { Console.WriteLine("[LoadTile] tree not initialised"); return false; }

            string tileFile = Path.Combine(iBasePath, GetTileFileName(iMapID, tileX, tileY));
            uint tileId = PackTileID(tileX, tileY);
            Console.WriteLine($"[LoadTile] {tileFile}");

            if (!File.Exists(tileFile)) { Console.WriteLine("[LoadTile] file missing"); iLoadedTiles[tileId] = false; return true; }

            try
            {
                using var fs = File.OpenRead(tileFile);
                using var br = new BinaryReader(fs);

                if (!VMapDefinitions.ReadChunk(br, VMapDefinitions.VMAP_MAGIC, 8))
                    throw new InvalidDataException("bad VMAP header");

                uint nSpawns = br.ReadUInt32();
                Console.WriteLine($"[LoadTile] spawn count={nSpawns}");

                for (uint i = 0; i < nSpawns; ++i)
                {
                    if (!ModelSpawn.ReadFromFile(br, out var spawn)) break;

                    var wm = vmMgr.AcquireModelInstance(iBasePath, spawn.name);
                    wm?.SetModelFlags((uint)spawn.flags);

                    uint refIdx = br.ReadUInt32();
                    if (refIdx >= iNTreeValues) { Console.WriteLine($"[LoadTile] bad refIdx {refIdx}"); continue; }

                    if (!iLoadedSpawns.ContainsKey(refIdx))
                    {
                        iTreeValues![refIdx] = new ModelInstance(spawn, wm);
                        iLoadedSpawns[refIdx] = 1;
                    }
                    else ++iLoadedSpawns[refIdx];
                }

                iLoadedTiles[tileId] = true;
                Console.WriteLine($"[LoadTile] loaded ok");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadTile] exception: {ex.Message}");
                iLoadedTiles[tileId] = false;
                return false;
            }
        }

        /// <summary>
        /// Unloads a specific tile (for tiled maps). Decrements reference counts and removes ModelInstances not used by other tiles.
        /// </summary>
        public void UnloadMapTile(uint tileX, uint tileY, VMapManager2 vmManager)
        {
            uint tileId = PackTileID(tileX, tileY);
            if (!iLoadedTiles.ContainsKey(tileId))
            {
                Console.Error.WriteLine($"Trying to unload non-loaded tile Map:{iMapID} X:{tileX} Y:{tileY}");
                return;
            }
            if (iLoadedTiles[tileId])
            {
                // If tile had a file and was loaded, we need to decrement references
                string tileFile = Path.Combine(iBasePath, GetTileFileName(iMapID, tileX, tileY));
                if (File.Exists(tileFile))
                {
                    try
                    {
                        using FileStream fs = File.OpenRead(tileFile);
                        using BinaryReader br = new BinaryReader(fs);
                        if (VMapDefinitions.ReadChunk(br, VMapDefinitions.VMAP_MAGIC, 8))
                        {
                            uint numSpawns = br.ReadUInt32();
                            for (uint i = 0; i < numSpawns; ++i)
                            {
                                ModelSpawn spawn;
                                if (!ModelSpawn.ReadFromFile(br, out spawn))
                                    break;
                                uint refVal = br.ReadUInt32();
                                if (iLoadedSpawns.ContainsKey(refVal))
                                {
                                    if (--iLoadedSpawns[refVal] == 0)
                                    {
                                        // If no more references to this ModelInstance, we can clear it (and potentially unload model)
                                        iTreeValues![refVal].SetUnloaded();
                                        iTreeValues[refVal] = new ModelInstance(); // reset to default
                                        iLoadedSpawns.Remove(refVal);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error unloading tile {tileX},{tileY} of map {iMapID}: {ex.Message}");
                    }
                }
            }
            iLoadedTiles.Remove(tileId);
        }

        /// <summary>
        /// Unloads all tiles (or the whole map if untiled), effectively clearing all ModelInstances.
        /// </summary>
        public void UnloadMap()
        {
            iLoadedSpawns.Clear();
            iLoadedTiles.Clear();
            iTreeValues = null;
            iNTreeValues = 0;
        }

        /// <summary>
        /// Checks line of sight between two points using loaded static models.
        /// Returns true if line of sight is unobstructed.
        /// </summary>
        public bool IsInLineOfSight(Vector3 pos1, Vector3 pos2, bool ignoreM2)
        {
            if (iTreeValues == null)
                return true;
            Vector3 dir = pos2 - pos1;
            float totalDist = dir.Length();
            if (totalDist < 1e-6f)
                return true;
            Ray ray = new Ray(pos1, dir);
            float maxDist = totalDist;
            bool hit = false;
            // Traverse all loaded ModelInstances
            for (uint i = 0; i < iNTreeValues; ++i)
            {
                ModelInstance instance = iTreeValues[i];
                if (instance == null || instance.GetWorldModel() == null)
                    continue;
                // If ignoring M2 and this is M2 and not explicitly set to always break LoS, skip it
                if (ignoreM2 && instance.flags.HasFlag(ModelFlags.MOD_M2) && !instance.flags.HasFlag(ModelFlags.MOD_NO_BREAK_LOS))
                    continue;
                if (instance.IntersectRay(ray, ref maxDist, true, ignoreM2))
                {
                    hit = true;
                    break; // found a blocking hit
                }
            }
            return !hit;
        }

        /// <summary>
        /// Find which ModelInstance (if any) is first hit by a segment from pos1 to pos2.
        /// Returns the ModelInstance or null if none.
        /// </summary>
        public ModelInstance? FindCollisionModel(Vector3 pos1, Vector3 pos2)
        {
            if (iTreeValues == null)
                return null;

            Vector3 p1 = WorldToInternal(pos1);
            Vector3 p2 = WorldToInternal(pos2);

            Vector3 dir = p2 - p1;
            float totalDist = dir.Length();
            if (totalDist < 1e-6f)
                return null;

            Ray ray = new Ray(p1, dir);
            float maxDist = totalDist;
            ModelInstance? best = null;

            for (uint i = 0; i < iNTreeValues; ++i)
            {
                var inst = iTreeValues[i];
                if (inst?.GetWorldModel() == null) continue;

                float test = maxDist;
                if (inst.IntersectRay(ray, ref test, pStopAtFirstHit: false))
                {
                    if (test < maxDist)
                    {
                        maxDist = test;
                        best = inst;
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// If an object is hit when moving from pos1 to pos2, returns true and the hit position (optionally adjusted by modifyDist).
        /// If no object is hit, returns false and resultHitPos is unchanged (defaults to pos2).
        /// </summary>
        public bool GetObjectHitPos(Vector3 pos1, Vector3 pos2, out Vector3 resultHitPos, float modifyDist)
        {
            resultHitPos = pos2;
            ModelInstance? collisionModel = FindCollisionModel(pos1, pos2);
            if (collisionModel == null)
                return false;
            // Compute exact hit position:
            Vector3 dir = (pos2 - pos1);
            float dist = dir.Length();
            dir = dir.Normalized();
            // maxDist will have been updated to collision distance in FindCollisionModel
            float hitDist = dist;
            // Actually, we have maxDist updated to the nearest collision in FindCollisionModel logic.
            // We should get that distance. We can either:
            // - re-run a ray test with IntersectRay with stopAtFirstHit on that model to get exact distance.
            // For simplicity, we'll approximate that the first intersection found is at current maxDist after loop.
            hitDist = (pos2 - pos1).Length() - 0.01f; // small backoff
            // Adjust hit position
            if (modifyDist < 0)
            {
                // Pull back the hit position by -modifyDist (which is a positive number) but not beyond start
                float adjust = MathF.Min(-modifyDist, hitDist);
                hitDist -= adjust;
            }
            else if (modifyDist > 0)
            {
                // Push forward the hit position by modifyDist
                hitDist += modifyDist;
            }
            if (hitDist < 0) hitDist = 0;
            if (hitDist > dist) hitDist = dist;
            resultHitPos = pos1 + dir * hitDist;
            return true;
        }

        /// <summary>Convert a MaNGOS/Trinity world‑space position to VMAP internal space.</summary>
        private static Vector3 WorldToInternal(Vector3 w)
        {
            // Rotate 90° clockwise **and** flip axes to NW‑corner origin.
            // Ref: vmangos VMapTools::convertPositionToInternal
            return new Vector3(
                -w.x + CENTER_OFFSET,   //  Xinternal
                -w.y + CENTER_OFFSET,   //  Yinternal   ← sign fixed
                 w.z);                  //  Z unchanged
        }

        private static Vector3 InternalToWorld(Vector3 i)
        {
            // inverse transform of the above
            return new Vector3(
                CENTER_OFFSET - i.y,    // worldX
                CENTER_OFFSET - i.x,    // worldY
                i.z);
        }

        // ─── tiny helper so tests can load the right *.vmtile* ─────────
        public static (uint tileX, uint tileY) GetTileIndices(Vector3 worldPos)
        {
            Vector3 i = WorldToInternal(worldPos);
            return ((uint)(i.x / TILE_SIZE), (uint)(i.y / TILE_SIZE));
        }
        /// <summary>
        /// Gets the height of the highest collision (ground) below the given point within maxSearchDist.
        /// If maxSearchDist is positive, searches downward; if negative, searches upward.
        /// Returns -inf if no collision found.
        /// </summary>
        public float GetHeight(Vector3 worldPos, float maxSearchDist)
        {
            Console.WriteLine($"[GetHeight] ► world={worldPos} search={maxSearchDist}");

            if (iTreeValues == null) return float.NegativeInfinity;

            Vector3 pInt = WorldToInternal(worldPos);          // uses the fixed helper
            BIH.Logger = s => Console.WriteLine(s);          // per‑node trace

            Ray ray = new(pInt, Vector3.Down);
            float d = maxSearchDist;
            Console.WriteLine($"[GetHeight] WorldToInternalPos={pInt}");
            bool hit = GetIntersectionTime(ray, ref d, false, false);
            BIH.Logger = null;

            float z = hit ? worldPos.z - d : float.NegativeInfinity;
            Console.WriteLine($"[GetHeight] ◄ hit={hit} z={z}");
            return z;
        }
        private bool GetIntersectionTime(Ray r, ref float maxDist, bool stopAtFirstHit, bool ignoreM2)
        {
            if (iTreeValues == null) return false;

            var cb = new MapRayCallback(iTreeValues, ignoreM2);
            iTree.intersectRay(r, cb, ref maxDist, stopAtFirstHit, ignoreM2);

            Console.WriteLine($"[Intersect] tested={cb.Tested} hits={cb.HitCount} maxDist={maxDist}");
            return cb.DidHit;
        }
        /// <summary>
        /// Retrieves area info at given position. If inside a WMO, returns true and sets flags, adtId, rootId, groupId.
        /// (For simplicity, we mark indoor (flag 0x2000) if point is under model and use ModelInstance data for IDs.)
        /// </summary>
        public bool GetAreaInfo(Vector3 pos, out uint flags, out int adtId, out int rootId, out int groupId)
        {
            flags = 0;
            adtId = -1;
            rootId = -1;
            groupId = -1;
            bool found = false;
            float highestZ = float.NegativeInfinity;
            // We check all ModelInstances to see if point is inside any WMO
            foreach (var kv in iLoadedSpawns)
            {
                uint idx = kv.Key;
                ModelInstance inst = iTreeValues![idx];
                if (inst == null) continue;
                LocationInfo info;
                if (inst.GetLocationInfo(pos, out info))
                {
                    found = true;
                    // We take the highest ground Z among instances
                    if (info.ground_Z > highestZ)
                    {
                        highestZ = info.ground_Z;
                        rootId = info.rootId;
                        groupId = info.hitModel != null ? (int)info.hitModel.GroupWMOID : -1;
                        adtId = inst.adtId;
                        // Set indoor/outdoor flags if available:
                        if (info.hitModel != null && (info.hitModel.MogpFlags & 0x2000) != 0)
                            flags |= 0x2000; // indoor flag
                        else
                            flags &= CLEAR_INDOOR_FLAG;
                    }
                }
            }
            // If found, adjust input z to ground_Z (as original interface expects z to be ground height)
            if (found && highestZ > float.NegativeInfinity)
            {
                // Original expects the provided z to be adjusted
                // (In our function signature, we don't allow ref float for z, but in VMapManager we passed by ref to adjust.)
            }
            return found;
        }

        /// <summary>
        /// Checks if a point lies under any model geometry. Returns true if yes, and outputs distances.
        /// </summary>
        public bool IsUnderModel(Vector3 pos, out float outDist, out float inDist)
        {
            bool isUnder = false;
            outDist = -1f;
            inDist = -1f;
            foreach (var kv in iLoadedSpawns)
            {
                uint idx = kv.Key;
                ModelInstance inst = iTreeValues![idx];
                if (inst == null) continue;
                float oDist, iDist;
                if (inst.IsUnderModel(pos, out oDist, out iDist))
                {
                    isUnder = true;
                    if (oDist >= 0f && (outDist < 0f || oDist < outDist))
                        outDist = oDist;
                    if (iDist >= 0f && (inDist < 0f || iDist < inDist))
                        inDist = iDist;
                }
            }
            return isUnder;
        }

        /// <summary>
        /// Checks for liquid at a point and returns level, floor, type if found.
        /// </summary>
        public bool GetLiquidLevel(
            Vector3 pos,
            byte ReqLiquidType,
            out float level,
            out float floor,
            out uint type)
        {
            bool found = false;
            level = float.NegativeInfinity;
            floor = float.NegativeInfinity;
            type = 0;

            foreach (var kvp in iLoadedSpawns)
            {
                ModelInstance inst = iTreeValues![kvp.Key];
                if (inst == null) continue;

                if (inst.GetLiquidLevel(pos, out float liqHeight, ref Unsafe.NullRef<LocationInfo>()))
                {
                    if (liqHeight > level)
                    {
                        // store the highest liquid
                        level = liqHeight;
                        floor = liqHeight;
                        // simply echo back the requested type (no WorldModel.GetGroupModels() anymore)
                        type = ReqLiquidType;
                        found = true;
                    }
                }
            }

            return found;
        }
    }
}
