using GameData.Core.Models;
using Pathfinding;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PathfindingService.Repository
{
    public unsafe class Navigation
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XYZ* CalculatePathDelegate(uint mapId, XYZ start, XYZ end, bool straightPath, out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArrDelegate(XYZ* pathArr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_IsInLineOfSightDelegate(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float VMAP_GetHeightDelegate(uint mapId, float x, float y, float z, float maxSearchDist);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_GetAreaInfoDelegate(uint mapId, float x, float y, float z, out uint flags, out int adtId, out int rootId, out int groupId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_GetLiquidLevelDelegate(uint mapId, float x, float y, float z, byte reqLiquidType, out float level, out float floor, out uint type);

        readonly object _vMapLock = new();

        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;
        private readonly VMAP_IsInLineOfSightDelegate isInLineOfSight;
        private readonly VMAP_GetHeightDelegate getHeight;
        private readonly VMAP_GetAreaInfoDelegate getAreaInfo;
        private readonly VMAP_GetLiquidLevelDelegate getLiquidLevel;

        public Navigation()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WinProcessImports.SetDllDirectory(currentFolder!);

            //log the current folder for debugging
            Console.WriteLine($"Current folder: {currentFolder}");

            var mpqPath = Path.Combine(currentFolder!, "Data\\terrain.MPQ");
            
            AdtGroundZLoader.SetMPQPaths([mpqPath]);

            var dllPath = Path.Combine(currentFolder!, "Navigation.dll");

            var navProcPtr = WinProcessImports.LoadLibrary(dllPath);

            if (navProcPtr == IntPtr.Zero)
                throw new Exception($"Failed to load {dllPath} (Win32 error {Marshal.GetLastWin32Error()})");

            calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "CalculatePath"));
            freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArrDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "FreePathArr"));
            isInLineOfSight = Marshal.GetDelegateForFunctionPointer<VMAP_IsInLineOfSightDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_IsInLineOfSight"));
            getHeight = Marshal.GetDelegateForFunctionPointer<VMAP_GetHeightDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetHeight"));
            getAreaInfo = Marshal.GetDelegateForFunctionPointer<VMAP_GetAreaInfoDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetAreaInfo"));
            getLiquidLevel = Marshal.GetDelegateForFunctionPointer<VMAP_GetLiquidLevelDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetLiquidLevel"));
        }

        private static bool IsValidZ(float z) => !float.IsNaN(z) && z > -200000.0f && z < 200000.0f;

        public ZQueryResult QueryZ(uint mapId, Position pos)
        {
            float serverZ = GetHeight(mapId, pos, 100.0f);
            AdtGroundZLoader.TryGetZ((int)mapId, pos.X, pos.Y, out float terrainZ);

            float finalZ = float.NaN;
            const float maxAcceptableDiff = 5.0f;

            if (IsValidZ(serverZ) && Math.Abs(pos.Z - serverZ) < maxAcceptableDiff)
                finalZ = serverZ;
            else if (IsValidZ(terrainZ))
                finalZ = terrainZ;

            return new ZQueryResult
            {
                FloorZ = float.IsNaN(finalZ) ? pos.Z : finalZ + 0.05f,
                TerrainZ = terrainZ,
                AdtZ = serverZ,
                RaycastZ = float.NaN,
                LocationZ = float.NaN
            };
        }

        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            lock (_vMapLock)
            {
                var ret = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int length);
                var list = new Position[length];
                for (var i = 0; i < length; i++)
                    list[i] = new Position(ret[i]);
                freePathArr(ret);
                return list;
            }
        }

        public bool IsInLineOfSight(uint mapId, Position a, Position b)
        {
            lock (_vMapLock)
            {
                return isInLineOfSight(mapId, a.X, a.Y, a.Z, b.X, b.Y, b.Z);
            }
        }

        public float GetHeight(uint mapId, Position pos, float maxDist)
        {
            return getHeight(mapId, pos.X, pos.Y, pos.Z, maxDist);
        }

        public bool TryGetAreaInfo(uint mapId, Position pos, out uint flags, out int adtId, out int rootId, out int groupId)
        {
            lock (_vMapLock)
            {
                return getAreaInfo(mapId, pos.X, pos.Y, pos.Z, out flags, out adtId, out rootId, out groupId);
            }
        }

        public bool TryGetLiquidLevel(uint mapId, Position pos, byte reqLiquid, out float level, out float floor, out uint type)
        {
            lock (_vMapLock)
            {
                return getLiquidLevel(mapId, pos.X, pos.Y, pos.Z, reqLiquid, out level, out floor, out type);
            }
        }
    }
}
