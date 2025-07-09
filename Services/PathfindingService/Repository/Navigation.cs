// Navigation.cs  – revised to match the new VMapManager2 API
using GameData.Core.Models;
using Pathfinding;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using VMAP;
using System;

namespace PathfindingService.Repository
{
    public unsafe class Navigation
    {
            /* ─────────────── Native path-finder DLL delegates ─────────────── */
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate XYZ* CalculatePathDelegate(uint mapId, XYZ start, XYZ end,
                                                        bool straightPath, out int length);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void FreePathArrDelegate(XYZ* pathArr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate bool GetFloorHeightDelegate(uint mapId,
                                                         float posX,
                                                         float posY,
                                                         float zGuess,
                                                         out float outHeight);

        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;
        private readonly GetFloorHeightDelegate getFloorHeight;

        /* ─────────────── Constructor: load and bind all exports ─────────────── */
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

            // Ensure the ADT terrain loader can find the .MPQ containing ADT heights:
            AdtGroundZLoader.SetMPQPaths(new[] { Path.Combine(binFolder, @"Data\terrain.MPQ") });

            calculatePath(0, new Position(0, 0, 0).ToXYZ(), new Position(0, 0, 0).ToXYZ(), true, out int length0);
            calculatePath(1, new Position(0, 0, 0).ToXYZ(), new Position(0, 0, 0).ToXYZ(), true, out int length1);
        }

        /* ─────────────── Public API ─────────────── */

        /// <summary>
        /// Calculates a path via native CalculatePath, marshals into managed Positions.
        /// </summary>
        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            var ptr = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int len);
            var path = new Position[len];
            for (int i = 0; i < len; ++i)
                path[i] = new Position(ptr[i]);
            freePathArr(ptr);
            return path;
        }

        /// <summary>
        /// Queries both native VMap GetFloorHeight and ADT terrain loader, returning the higher Z.
        /// If the native call fails, its result is treated as –∞ so ADT always wins if valid.
        /// </summary>
        public float GetFloorHeight(uint mapId, float x, float y, float z)
        {
            Console.WriteLine($"[GetFloorHeight] Enter: mapId={mapId} pos=({x:0.000},{y:0.000},{z:0.000})");

            // 1) ADT terrain height
            bool adtOk = AdtGroundZLoader.TryGetZ((int)mapId, x, y, out float adtZ);
            if (adtOk)
                Console.WriteLine($"[GetFloorHeight] ADT Z={adtZ:0.000}");
            else
                Console.WriteLine($"[GetFloorHeight] ADT lookup failed");

            // 2) Form raycast guess = ADT Z + 0.5f (server does exactly this)
            const float raySpan = 100.0f;
            float startZ = z + 0.5f;
            float endZ = startZ - raySpan;
            Console.WriteLine($"[GetFloorHeight] Ray startZ={startZ:0.000} endZ={endZ:0.000}");

            // 3) VMAP raycast
            bool vmapOk = getFloorHeight(mapId, x, y, startZ, out float vmapZ);
            if (vmapOk)
                Console.WriteLine($"[GetFloorHeight] VMAP Z={vmapZ:0.000}");
            else
                Console.WriteLine($"[GetFloorHeight] VMAP lookup failed");

            // 4) Return whichever is higher (closest underfoot)
            float useAdt = adtOk ? adtZ : float.NegativeInfinity;
            float useVmp = vmapOk ? vmapZ : float.NegativeInfinity;
            float result = Math.Max(useAdt, useVmp);

            Console.WriteLine($"[GetFloorHeight] Return Z={result:0.000}");
            return result;
        }
    }
}
