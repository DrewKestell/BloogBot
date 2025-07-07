using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using PathfindingService.Repository;

namespace VMAP
{
    /// <summary>
    /// The main manager for VMAP data. Manages loading/unloading of static map collision (VMaps),
    /// as well as references to loaded model files.
    /// </summary>
    public class VMapManager2 : IVMapManager
    {
        // Map of loaded WorldModel files by filename.
        // We use a ManagedModel wrapper to allow weak referencing when managed pointers are enabled.
        private readonly Dictionary<string, ManagedModel> iLoadedModelFiles = new Dictionary<string, ManagedModel>();
        // Map of mapId to StaticMapTree (each map holds its static geometry tree).
        private readonly Dictionary<uint, StaticMapTree> iInstanceMapTrees = new Dictionary<uint, StaticMapTree>();

        // ReaderWriterLock to protect model file map (for thread-safe loading/unloading).
        private readonly ReaderWriterLockSlim modelLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Helper class to wrap loaded WorldModels, supporting optional persistent storage.
        /// </summary>
        private class ManagedModel
        {
            private WeakReference<WorldModel> weakRef;
            private WorldModel? strongRef;
            public ManagedModel(WorldModel model, bool allowUnload)
            {
                if (allowUnload)
                {
                    weakRef = new WeakReference<WorldModel>(model);
                    strongRef = null;
                }
                else
                {
                    // Keep a strong reference to avoid unloading
                    weakRef = new WeakReference<WorldModel>(model);
                    strongRef = model;
                }
            }
            public WorldModel? GetModel()
            {
                // If a strong reference exists, return it (it ensures model stays loaded).
                if (strongRef != null)
                    return strongRef;
                // Otherwise, try to get from weak reference
                if (weakRef.TryGetTarget(out WorldModel? model))
                    return model;
                return null;
            }
        }

        public VMapManager2() { /* Optionally, preload anything if needed */ }

        ~VMapManager2()
        {
            // Clean up all static map trees and model references
            foreach (var tree in iInstanceMapTrees.Values)
            {
                tree.UnloadMap(); // free any structures
            }
            iInstanceMapTrees.Clear();
            iLoadedModelFiles.Clear();
        }

        /// <summary>
        /// Converts a mapId to the base filename for its VMAP directory file.
        /// e.g., map 123 -> "123.vmtree"
        /// </summary>
        public static string GetMapFileName(uint mapId)
        {
            return $"{mapId:D3}.vmtree"; // zero-padded 3-digit (assuming map IDs use 3 digits, adjust if needed)
        }

        /// <summary>
        /// Acquire (load if necessary) a WorldModel for a given model filename.
        /// Uses caching to avoid reloading the same model file multiple times.
        /// </summary>
        public WorldModel? AcquireModelInstance(string basePath, string filename)
        {
            // Ensure basePath ends with separator
            if (!basePath.EndsWith(Path.DirectorySeparatorChar) && !basePath.EndsWith(Path.AltDirectorySeparatorChar))
            {
                basePath += Path.DirectorySeparatorChar;
            }
            string filePath = basePath + filename;
            // If the file lacks a known extension, assume .vmo (the extracted model format)
            if (!File.Exists(filePath))
            {
                // Try adding .vmo extension
                if (!filename.EndsWith(".vmo", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = basePath + filename + ".vmo";
                }
            }
            WorldModel? model = null;
            modelLock.EnterUpgradeableReadLock();
            try
            {
                if (iLoadedModelFiles.TryGetValue(filePath, out ManagedModel managed))
                {
                    model = managed.GetModel();
                }
                if (model == null)
                {
                    if (!File.Exists(filePath))
                    {
                        //Console.WriteLine($"[ModelLoad] ✘  MISSING  {filePath}");
                        model = null;
                    }
                    else
                    {
                        Console.WriteLine($"[ModelLoad] ↺ loading {Path.GetFileName(filePath)}");
                        model = new WorldModel();

                        if (filename.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                            model.SetModelFlags((uint)ModelFlags.MOD_M2);

                        if (!model.ReadFile(filePath))
                            Console.WriteLine($"[ModelLoad] !! FAILED to parse {filename}");
                    }

                    // remember (even if null) so we don’t try again next time
                    iLoadedModelFiles[filePath] = new ManagedModel(model!, useManagedModelStorage);
                }
            }
            finally
            {
                modelLock.ExitUpgradeableReadLock();
            }
            return model;
        }

        public override VMAPLoadResult LoadMap(string basePath, uint mapId, int tileX, int tileY)
        {
            if (!IsMapLoadingEnabled())
                return VMAPLoadResult.Ignored;
            // Only load if not already loaded
            StaticMapTree tree;
            if (!iInstanceMapTrees.TryGetValue(mapId, out tree))
            {
                tree = new StaticMapTree(mapId, basePath);
                iInstanceMapTrees[mapId] = tree;
            }
            // If the map uses tiles, load the specific tile:
            bool success;
            if (!tree.IsTiled)
            {
                // Load entire map file (only once for maps without tiling)
                success = tree.InitMap(GetMapFileName(mapId), this);
            }
            else
            {
                // For tiled maps, load the particular tile
                success = tree.LoadMapTile((uint)tileX, (uint)tileY, this);
            }
            return success ? VMAPLoadResult.Success : VMAPLoadResult.Error;
        }

        public override void UnloadMap(uint mapId, int tileX, int tileY)
        {
            if (iInstanceMapTrees.TryGetValue(mapId, out var tree))
            {
                if (!tree.IsTiled)
                {
                    // For untiled maps, unload only when all tiles (grids) are removed, which we'll handle in UnloadMap(mapId).
                    tree.UnloadMap();
                }
                else
                {
                    tree.UnloadMapTile((uint)tileX, (uint)tileY, this);
                    // If no tiles remain loaded in this map, we could potentially unload entire map.
                }
            }
        }

        public override void UnloadMap(uint mapId)
        {
            if (iInstanceMapTrees.TryGetValue(mapId, out var tree))
            {
                tree.UnloadMap();
                iInstanceMapTrees.Remove(mapId);
            }
        }

        public override bool IsInLineOfSight(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2)
        {
            if (!enableLineOfSightCalc)
                return true; // if LoS calculation is disabled, always consider line of sight available
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return true; // no collision data for this map, assume clear line of sight
            return tree.IsInLineOfSight(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2), ignoreM2);
        }

        public override ModelInstance? FindCollisionModel(uint mapId, float x0, float y0, float z0, float x1, float y1, float z1)
        {
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return null;
            return tree.FindCollisionModel(new Vector3(x0, y0, z0), new Vector3(x1, y1, z1));
        }

        public override bool GetObjectHitPos(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2,
                                             out float rx, out float ry, out float rz, float modifyDist)
        {
            rx = x2; ry = y2; rz = z2;
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return false;
            Vector3 p1 = new Vector3(x1, y1, z1);
            Vector3 p2 = new Vector3(x2, y2, z2);
            if (!tree.GetObjectHitPos(p1, p2, out Vector3 hitPos, modifyDist))
            {
                return false;
            }
            rx = hitPos.x;
            ry = hitPos.y;
            rz = hitPos.z;
            return true;
        }

        public override float GetHeight(uint mapId, float x, float y, float z, float maxSearchDist)
        {
            if (!enableHeightCalc)
                return VMapDefinitions.INVALID_HEIGHT; // if disabled, return a sentinel
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return VMapDefinitions.INVALID_HEIGHT;
            float height = tree.GetHeight(new Vector3(x, y, z), maxSearchDist);
            if (float.IsNegativeInfinity(height))
                return VMapDefinitions.INVALID_HEIGHT_VALUE; // unknown height
            return height;
        }

        public override bool GetAreaInfo(uint mapId, float x, float y, ref float z, out uint flags, out int adtId, out int rootId, out int groupId)
        {
            flags = 0;
            adtId = -1;
            rootId = -1;
            groupId = -1;
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return false;
            return tree.GetAreaInfo(new Vector3(x, y, z), out flags, out adtId, out rootId, out groupId);
        }

        public override bool IsUnderModel(uint mapId, float x, float y, float z, out float outDist, out float inDist)
        {
            outDist = inDist = 0f;
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return false;
            return tree.IsUnderModel(new Vector3(x, y, z), out outDist, out inDist);
        }

        public override bool GetLiquidLevel(uint mapId, float x, float y, float z, byte reqLiquidType, out float level, out float floor, out uint type)
        {
            level = 0f;
            floor = 0f;
            type = 0;
            if (!iInstanceMapTrees.TryGetValue(mapId, out var tree))
                return false;
            return tree.GetLiquidLevel(new Vector3(x, y, z), reqLiquidType, out level, out floor, out type);
        }

        public override string GetDirFileName(uint mapId, int tileX, int tileY)
        {
            // The "dir file name" is basically the combined filename for the map's directory file.
            // For our implementation, we use the map's .vmtree file as the directory.
            return GetMapFileName(mapId);
        }

        public override bool ExistsMap(string basePath, uint mapId, int tileX, int tileY)
        {
            // Check if the main map file exists (and if tiled, also check tile file)
            string mapFile = Path.Combine(basePath, GetMapFileName(mapId));
            if (!File.Exists(mapFile))
                return false;
            bool isTiled = false;
            // Check if map is marked tiled by reading its header (without fully loading).
            try
            {
                using FileStream fs = File.OpenRead(mapFile);
                using BinaryReader br = new BinaryReader(fs);
                if (!VMapDefinitions.ReadChunk(br, VMapDefinitions.VMAP_MAGIC, 8))
                    return false;
                // After magic, next byte in file indicates whether tiled (in VMap v7 format, it's a char for 'tiled')
                byte tiledFlag = br.ReadByte();
                isTiled = (tiledFlag != 0);
            }
            catch { /* ignore errors, assume not tiled if fail */ }
            if (isTiled)
            {
                string tileName = StaticMapTree.GetTileFileName(mapId, (uint)tileX, (uint)tileY);
                string tilePath = Path.Combine(basePath, tileName);
                return File.Exists(tilePath);
            }
            return true;
        }
    }
}
