using GameData.Core.Models;
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
        private delegate bool VMAP_GetObjectHitPosDelegate(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2, out float hitX, out float hitY, out float hitZ, float modifyDist);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_GetAreaInfoDelegate(uint mapId, float x, float y, float z, out uint flags, out int adtId, out int rootId, out int groupId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_GetLocationInfoDelegate(uint mapId, float x, float y, float z, out float floorZ);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_GetLiquidLevelDelegate(uint mapId, float x, float y, float z, byte reqLiquidType, out float level, out float floor, out uint type);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMAP_DebugTest(uint mapId, float x, float y, float z);

        readonly object _mMapLock = new();
        readonly object _vMapLock = new();

        readonly CalculatePathDelegate calculatePath;
        readonly FreePathArrDelegate freePathArr;
        readonly VMAP_IsInLineOfSightDelegate isInLineOfSight;
        readonly VMAP_GetHeightDelegate getHeight;
        readonly VMAP_GetObjectHitPosDelegate getObjectHitPos;
        readonly VMAP_GetAreaInfoDelegate getAreaInfo;
        readonly VMAP_GetLocationInfoDelegate getLocationInfo;
        readonly VMAP_GetLiquidLevelDelegate getLiquidLevel;

        readonly AdtGroundZLoader _adtGroundZLoader;

        public Navigation()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dllPath = Path.Combine(currentFolder!, "Navigation.dll");

            var navProcPtr = WinProcessImports.LoadLibrary(dllPath);
            if (navProcPtr == IntPtr.Zero)
                throw new Exception("Failed to load Navigation.dll");

            _adtGroundZLoader = new AdtGroundZLoader();

            calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "CalculatePath"));
            freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArrDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "FreePathArr"));
            isInLineOfSight = Marshal.GetDelegateForFunctionPointer<VMAP_IsInLineOfSightDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_IsInLineOfSight"));
            getHeight = Marshal.GetDelegateForFunctionPointer<VMAP_GetHeightDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetHeight"));
            getObjectHitPos = Marshal.GetDelegateForFunctionPointer<VMAP_GetObjectHitPosDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetObjectHitPos"));
            getAreaInfo = Marshal.GetDelegateForFunctionPointer<VMAP_GetAreaInfoDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetAreaInfo"));
            getLocationInfo = Marshal.GetDelegateForFunctionPointer<VMAP_GetLocationInfoDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetLocationInfo"));
            getLiquidLevel = Marshal.GetDelegateForFunctionPointer<VMAP_GetLiquidLevelDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "VMAP_GetLiquidLevel"));
        }

        private bool IsValidZ(float z) => !float.IsNaN(z) && z > -200000.0f;

        public float GetFloorZ(uint mapId, Position pos)
        {
            var result = QueryZ(mapId, pos);
            return result.FloorZ;
        }

        public ZQueryResult QueryZ(uint mapId, Position pos)
        {
            var rayStart = new Position(pos.X, pos.Y, pos.Z + 50.0f);
            var rayEnd = new Position(pos.X, pos.Y, pos.Z - 500.0f);

            float raycastZ = float.NaN;
            float locationZ = float.NaN;
            float terrainZ = float.NaN;
            float adtZ = float.NaN;

            if (TryGetObjectHitPos(mapId, rayStart, rayEnd, out Position hit, 0.0f))
                raycastZ = hit.Z;

            terrainZ = GetHeight(mapId, pos, 100.0f);

            adtZ = _adtGroundZLoader.GetGroundZ((int)mapId, pos.X, pos.Y, pos.Z);

            if (TryGetLocationInfo(mapId, pos, out float locZ))
                locationZ = locZ;

            float finalZ = float.NaN;
            float maxWalkableDiff = 5.0f; // Tweakable threshold for walkable surface proximity

            if (IsValidZ(locationZ) && Math.Abs(pos.Z - locationZ) < maxWalkableDiff)
                finalZ = locationZ;
            else if (IsValidZ(terrainZ) && Math.Abs(pos.Z - terrainZ) < maxWalkableDiff)
                finalZ = terrainZ;
            else if (IsValidZ(raycastZ) && raycastZ < pos.Z && Math.Abs(pos.Z - raycastZ) < 15.0f)
                finalZ = raycastZ;
            else
                finalZ = adtZ;

            return new ZQueryResult
            {
                FloorZ = finalZ + 0.5f,
                RaycastZ = raycastZ,
                TerrainZ = terrainZ,
                AdtZ = adtZ,
                LocationZ = locationZ
            };
        }


        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            lock (_mMapLock)
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
            lock (_vMapLock)
            {
                return getHeight(mapId, pos.X, pos.Y, pos.Z, maxDist);
            }
        }

        public bool TryGetObjectHitPos(uint mapId, Position a, Position b, out Position hit, float modifyDist)
        {
            lock (_vMapLock)
            {
                bool result = getObjectHitPos(mapId, a.X, a.Y, a.Z, b.X, b.Y, b.Z, out float x, out float y, out float z, modifyDist);
                hit = new Position(x, y, z);
                return result;
            }
        }

        public bool TryGetLocationInfo(uint mapId, Position pos, out float z)
        {
            lock (_vMapLock)
            {
                return getLocationInfo(mapId, pos.X, pos.Y, pos.Z, out z);
            }
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

    public struct ZQueryResult
    {
        public float FloorZ;
        public float RaycastZ;
        public float TerrainZ;
        public float AdtZ;
        public float LocationZ;
    }
}
