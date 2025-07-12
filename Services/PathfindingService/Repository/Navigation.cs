using Game;
using GameData.Core.Models;
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
        private delegate bool GetFloorHeightDelegate(uint mapId, float posX, float posY,
                                                     float zGuess, out float outHeight);

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
        private readonly GetFloorHeightDelegate getFloorHeight;
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
            getFloorHeight = Marshal.GetDelegateForFunctionPointer<GetFloorHeightDelegate>(
                WinProcessImports.GetProcAddress(mod, "GetNavmeshFloorHeight"));
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

        public float GetFloorHeight(uint mapId, float x, float y, float z)
        {
            if (getFloorHeight(mapId, x, y, z + 0.5f, out float vmapZ))
                return vmapZ;
            return float.NegativeInfinity;
        }

        public (float, float) GetADTHeight(uint mapId, float x, float y)
        {
            _adtGroundZLoader.TryGetZ((int)mapId, x, y, out float z, out float liqZ);
            return (z, liqZ);
        }

        public bool IsLineOfSight(uint mapId, Position from, Position to)
        {
            return lineOfSight(mapId, from.ToXYZ(), to.ToXYZ());
        }

        public NavPoly[] GetCapsuleOverlaps(uint mapId, Position pos, float radius, float height)
        {
            IntPtr ptr = capsuleOverlap(mapId, pos.ToXYZ(), radius, height, out int count);
            if (ptr == IntPtr.Zero || count == 0)
                return [];

            var result = new NavPoly[count];
            int size = Marshal.SizeOf<NavPoly>();
            for (int i = 0; i < count; i++)
            {
                IntPtr curr = IntPtr.Add(ptr, i * size);
                result[i] = Marshal.PtrToStructure<NavPoly>(curr);
            }

            freeNavPolyArr(ptr);
            return result;
        }
    }
    public static class NavTerrain
    {
        public const uint Empty = 0x00;
        public const uint Ground = 0x01;
        public const uint Magma = 0x02;
        public const uint Slime = 0x04;
        public const uint Water = 0x08;
    }

    public static class RaceDimensions
    {
        /// <summary>
        /// Returns canonical collision capsule dimensions for a given character race.
        /// </summary>
        /// <param name="race">The character race</param>
        /// <returns>Tuple of (radius, height)</returns>
        public static (float radius, float height) GetCapsuleForRace(Race race)
        {
            return race switch
            {
                Race.Human => (0.3889f, 1.95f),
                Race.Orc => (0.5f, 2.25f),
                Race.Dwarf => (0.35f, 1.5f),
                Race.NightElf => (0.5f, 2.2f),
                Race.Undead => (0.4f, 2.0f),
                Race.Tauren => (0.8f, 2.7f),
                Race.Gnome => (0.3f, 1.25f),
                Race.Troll => (0.65f, 2.5f),
                _ => (0.4f, 2.0f), // fallback
            };
        }
    }
}
