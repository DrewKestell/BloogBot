using GameData.Core.Models;
using Pathfinding;
using System.Reflection;
using System.Runtime.InteropServices;
using Position = GameData.Core.Models.Position;

namespace PathfindingService.Repository
{
    public unsafe class Navigation
    {
        /* ─────────────── Structs ─────────────── */

        [StructLayout(LayoutKind.Sequential)]
        public struct NavPoly
        {
            public ulong RefId;
            public uint Area;
            public uint Flags;
            public uint VertCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public XYZ[] Verts;
        }

        /* ─────────────── Native delegates ─────────────── */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XYZ* CalculatePathDelegate(uint mapId, XYZ start, XYZ end,
                                                    bool straightPath, out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArrDelegate(XYZ* pathArr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LineOfSightDelegate(uint mapId, XYZ from, XYZ to);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CapsuleOverlapDelegate(uint mapId, XYZ position,
                                                       float radius, float height, out int count);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreeNavPolyArrDelegate(IntPtr ptr);

        /* ─────────────── Function pointers ─────────────── */

        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;
        private readonly LineOfSightDelegate lineOfSight;
        private readonly CapsuleOverlapDelegate capsuleOverlap;
        private readonly FreeNavPolyArrDelegate freeNavPolyArr;

        /* ─────────────── Constructor: bind all exports ─────────────── */

        private readonly AdtGroundZLoader _adtGroundZLoader;

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
            lineOfSight = Marshal.GetDelegateForFunctionPointer<LineOfSightDelegate>(
                WinProcessImports.GetProcAddress(mod, "LineOfSight"));
            capsuleOverlap = Marshal.GetDelegateForFunctionPointer<CapsuleOverlapDelegate>(
                WinProcessImports.GetProcAddress(mod, "CapsuleOverlap"));
            freeNavPolyArr = Marshal.GetDelegateForFunctionPointer<FreeNavPolyArrDelegate>(
                WinProcessImports.GetProcAddress(mod, "FreeNavPolyArr"));

            _adtGroundZLoader = new AdtGroundZLoader([Path.Combine(binFolder, @"Data\terrain.MPQ")]);
        }

        /* ─────────────── High-level API ─────────────── */

        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            var ptr = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int len);
            var path = new Position[len];
            for (int i = 0; i < len; ++i)
                path[i] = new Position(ptr[i]);
            freePathArr(ptr);
            return path;
        }

        public bool IsLineOfSight(uint mapId, Position from, Position to)
        {
            return lineOfSight(mapId, from.ToXYZ(), to.ToXYZ());
        }

        public TerrainProbeResponse GetTerrainProbe(uint mapId, Game.Position pos,
                                            float radius, float height)
        {
            TerrainProbeResponse response = new();

            // ADT height-field sample
            _adtGroundZLoader.TryGetZ((int)mapId, pos.X, pos.Y, out float z, out float liqZ);
            response.GroundZ = z;
            response.LiquidZ = liqZ;

            // polygon overlap
            IntPtr ptr = capsuleOverlap(mapId, pos.ToXYZ(), radius, height, out int count);
            if (ptr == IntPtr.Zero || count == 0) return response;

            int size = Marshal.SizeOf<NavPoly>();
            for (int i = 0; i < count; i++)
            {
                IntPtr curr = IntPtr.Add(ptr, i * size);
                var poly = Marshal.PtrToStructure<NavPoly>(curr);

                NavPolyHit hit = new()
                {
                    RefId = poly.RefId,
                    Area = (NavTerrain)poly.Area,
                    Flags = (NavPolyFlag)poly.Flags
                };

                foreach (var vert in poly.Verts)
                    hit.Verts.Add(vert.ToProto());

                response.Overlaps.Add(hit);
            }

            freeNavPolyArr(ptr);
            return response;
        }
    }
}
