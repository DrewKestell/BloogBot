namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    /// Helps the bot generate paths through the world
    /// </summary>
    //public unsafe class Navigation
    //{
    //    /// <summary>
    //    /// Access to the pathfinder
    //    /// </summary>
    //    public static Navigation Instance = new Navigation();

    //    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //    private delegate _XYZ* CalculatePathDelegate(
    //        uint mapId, _XYZ start, _XYZ end, bool parSmooth,
    //        out int length);

    //    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //    private delegate void FreePathArr(
    //        _XYZ* pathArr);

    //    private static CalculatePathDelegate _calculatePath;
    //    private static FreePathArr _freePathArr;

    //    /// <summary>
    //    /// Generate a path from start to end
    //    /// </summary>
    //    /// <param name="mapId">The map ID the player is on</param>
    //    /// <param name="start">Start</param>
    //    /// <param name="end">End</param>
    //    /// <param name="parSmooth">Smooth path</param>
    //    /// <returns>An array of points</returns>
    //    public Location[] CalculatePath(
    //        uint mapId, Location start, Location end, bool parSmooth)
    //    {
    //        int length;
    //        var ret = _calculatePath(mapId, start.ToStruct, end.ToStruct, parSmooth, out length);
    //        var list = new Location[length];
    //        for (var i = 0; i < length; i++)
    //        {
    //            list[i] = new Location(ret[i]);
    //        }
    //        _freePathArr(ret);
    //        return list;
    //    }

    //    private Navigation()
    //    {
    //        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    //        var mapsPath = $"{currentFolder}\\Navigation.dll";

    //        var navProcPtr = WinImports.LoadLibrary(mapsPath);

    //        var calculatePathPtr = WinImports.GetProcAddress(navProcPtr, "CalculatePath");
    //        _calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(calculatePathPtr);

    //        var freePathPtr = WinImports.GetProcAddress(navProcPtr, "FreePathArr");
    //        _freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArr>(freePathPtr);
    //    }
    //}
}
