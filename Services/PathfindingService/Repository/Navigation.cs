// Navigation.cs  – revised to match the new VMapManager2 API
using GameData.Core.Models;
using Pathfinding;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using VMAP;

namespace PathfindingService.Repository
{
    public unsafe class Navigation
    {
        /* ─────────────── native path-finder DLL (unchanged) ─────────────── */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XYZ* CalculatePathDelegate(uint mapId, XYZ start, XYZ end,
                                                    bool straightPath, out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArrDelegate(XYZ* pathArr);

        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;

        /* ─────────────── VMAP manager instance ─────────────── */
        private readonly VMapManager2 vmapMgr = new();
        private readonly string vmapDataPath;
        private readonly ConcurrentDictionary<(uint map, int tx, int ty), byte> loadedTiles = new();

        /* ─────────────── constructor ─────────────── */
        public Navigation()
        {
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            var dllPath = Path.Combine(binFolder, "Navigation.dll");
            var mod = WinProcessImports.LoadLibrary(dllPath);
            if (mod == IntPtr.Zero)
                throw new FileNotFoundException("Failed to load Navigation.dll", dllPath);

            calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(
                                WinProcessImports.GetProcAddress(mod, "CalculatePath"));
            freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArrDelegate>(
                                WinProcessImports.GetProcAddress(mod, "FreePathArr"));

            AdtGroundZLoader.SetMPQPaths([Path.Combine(binFolder, @"Data\terrain.MPQ")]);

            vmapDataPath = Path.Combine(binFolder, "vmaps");
            if (!Directory.Exists(vmapDataPath))
                throw new DirectoryNotFoundException($"VMap data folder not found: {vmapDataPath}");

            Console.WriteLine($"[Navigation] Preloading VMAP tiles in: {vmapDataPath}");

            foreach (var vmtreeFile in Directory.GetFiles(vmapDataPath, "*.vmtree"))
            {
                if (!uint.TryParse(Path.GetFileNameWithoutExtension(vmtreeFile), out uint mapId))
                {
                    Console.WriteLine($"[Navigation] Skipping invalid VMAP file: {vmtreeFile}");
                    continue;
                }

                foreach (var tileFile in Directory.GetFiles(vmapDataPath, $"{mapId:D3}_*_*.vmtile"))
                {
                    var parts = Path.GetFileNameWithoutExtension(tileFile).Split('_');
                    if (parts.Length != 3 ||
                        !int.TryParse(parts[1], out int tileX) ||
                        !int.TryParse(parts[2], out int tileY))
                    {
                        Console.WriteLine($"[Navigation] Skipping malformed tile file: {tileFile}");
                        continue;
                    }

                    vmapMgr.LoadMap(vmapDataPath, mapId, tileX, tileY);
                    loadedTiles.TryAdd((mapId, tileX, tileY), 0);
                }
            }

            Console.WriteLine("[Navigation] VMAP preloading complete.");
        }

        /* ─────────────── helpers (unchanged) ─────────────── */
        public static bool IsValidZ(float z) =>
            !float.IsNaN(z) && z > -200_000f && z < 200_000f;

        public static void WorldToTile(float x, float y, out int tx, out int ty)
        {
            const float tileSize = 533.3333333f;
            tx = 32 - (int)Math.Floor(y / tileSize);
            ty = 32 - (int)Math.Floor(x / tileSize);
            tx = Math.Clamp(tx, 0, 63);
            ty = Math.Clamp(ty, 0, 63);
        }

        private void EnsureTileLoaded(uint mapId, int tx, int ty)
        {
            if (loadedTiles.TryAdd((mapId, tx, ty), 0))
                vmapMgr.LoadMap(vmapDataPath, mapId, tx, ty);
        }

        /* ─────────────── public API exposed to callers ─────────────── */
        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            var ptr = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int len);
            var path = new Position[len];
            for (int i = 0; i < len; ++i) path[i] = new Position(ptr[i]);
            freePathArr(ptr);
            return path;
        }

        public bool IsInLineOfSight(uint mapId, Position a, Position b, bool ignoreM2 = false)
        {
            WorldToTile(a.X, a.Y, out var tx, out var ty);
            EnsureTileLoaded(mapId, tx, ty);
            return vmapMgr.IsInLineOfSight(mapId, a.X, a.Y, a.Z, b.X, b.Y, b.Z, ignoreM2);
        }

        public float GetHeight(uint mapId, Position pos, float maxDist = 100f)
        {
            WorldToTile(pos.X, pos.Y, out var tx, out var ty);
            EnsureTileLoaded(mapId, tx, ty);
            Console.WriteLine($"[Navigation] tileX={tx} tileY={ty} pos={pos}");
            return vmapMgr.GetHeight(mapId, pos.X, pos.Y, pos.Z + 5, maxDist);
        }

        public ZQueryResult QueryZ(uint mapId, Position pos)
        {
            float vmapZ = GetHeight(mapId, pos);
            AdtGroundZLoader.TryGetZ((int)mapId, pos.X, pos.Y, out float terrainZ);

            const float maxDiff = 5f;
            float finalZ = float.NaN;

            if (IsValidZ(vmapZ) && Math.Abs(pos.Z - vmapZ) < maxDiff)
                finalZ = vmapZ;
            else if (IsValidZ(terrainZ))
                finalZ = terrainZ;

            return new ZQueryResult
            {
                FloorZ = float.IsNaN(finalZ) ? pos.Z : finalZ + 0.05f,
                TerrainZ = terrainZ,
                AdtZ = vmapZ,
                RaycastZ = float.NaN,
                LocationZ = float.NaN
            };
        }

        public bool TryGetAreaInfo(uint mapId, Position pos,
                                   out uint flags, out int adtId, out int rootId, out int groupId)
        {
            flags = (uint)(adtId = rootId = groupId = 0);
            WorldToTile(pos.X, pos.Y, out var tx, out var ty);
            EnsureTileLoaded(mapId, tx, ty);

            float z = pos.Z;
            return vmapMgr.GetAreaInfo(mapId, pos.X, pos.Y, ref z,
                                       out flags, out adtId, out rootId, out groupId);
        }

        public bool TryGetLiquidLevel(uint mapId, Position pos, byte reqLiquid,
                                      out float level, out float floor, out uint type)
        {
            level = floor = 0; type = 0;
            WorldToTile(pos.X, pos.Y, out var tx, out var ty);
            EnsureTileLoaded(mapId, tx, ty);

            return vmapMgr.GetLiquidLevel(mapId, pos.X, pos.Y, pos.Z, reqLiquid,
                                          out level, out floor, out type);
        }
    }
}
