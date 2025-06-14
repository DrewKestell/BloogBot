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

        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;

        public Navigation()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dllPath = Path.Combine(currentFolder!, "Navigation.dll");

            var navProcPtr = WinProcessImports.LoadLibrary(dllPath);
            if (navProcPtr == IntPtr.Zero)
                throw new Exception("Failed to load Navigation.dll");

            calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "CalculatePath"));
            freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArrDelegate>(WinProcessImports.GetProcAddress(navProcPtr, "FreePathArr"));
        }

        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
                var ret = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int length);
                var list = new Position[length];
                for (var i = 0; i < length; i++)
                    list[i] = new Position(ret[i]);
                freePathArr(ret);
                return list;
        }
    }
}
