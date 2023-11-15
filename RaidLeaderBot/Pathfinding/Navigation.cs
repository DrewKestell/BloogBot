using RaidMemberBot.Constants;
using RaidMemberBot.Objects;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RaidLeaderBot.Pathfinding
{
    /// <summary>
    /// Helps the bot generate paths through the world
    /// </summary>
    public unsafe class Navigation
    {
        private object _lock = new object();
        /// <summary>
        /// Access to the pathfinder
        /// </summary>
        public static Navigation Instance = new Navigation();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XYZ* CalculatePathDelegate(
            uint mapId, XYZ start, XYZ end, bool parSmooth,
            out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArr(
            XYZ* pathArr);

        private static CalculatePathDelegate _calculatePath;
        private static FreePathArr _freePathArr;

        /// <summary>
        /// Generate a path from start to end
        /// </summary>
        /// <param name="mapId">The map ID the player is on</param>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <param name="parSmooth">Smooth path</param>
        /// <returns>An array of points</returns>
        public Position[] CalculatePath(
            uint mapId, Position start, Position end, bool parSmooth)
        {
            lock (_lock)
            {
                int length;
                XYZ* ret = _calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), parSmooth, out length);
                Position[] list = new Position[length];
                for (int i = 0; i < length; i++)
                {
                    list[i] = new Position(ret[i]);
                }
                _freePathArr(ret);
                return list;
            }
        }

        private Navigation()
        {
            string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string mapsPath = $"{currentFolder}\\Navigation.dll";

            System.IntPtr navProcPtr = WinImports.LoadLibrary(mapsPath);

            System.IntPtr calculatePathPtr = WinImports.GetProcAddress(navProcPtr, "CalculatePath");
            _calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(calculatePathPtr);

            System.IntPtr freePathPtr = WinImports.GetProcAddress(navProcPtr, "FreePathArr");
            _freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArr>(freePathPtr);
        }
    }
}
