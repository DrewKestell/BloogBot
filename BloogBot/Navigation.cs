using BloogBot.Game;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BloogBot
{
    public unsafe class Navigation
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate XYZ* CalculatePathDelegate(
            uint mapId,
            XYZ start,
            XYZ end,
            bool straightPath,
            out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void FreePathArr(XYZ* pathArr);

        static CalculatePathDelegate calculatePath;
        static FreePathArr freePathArr;

        static Navigation()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var mapsPath = $"{currentFolder}\\Navigation.dll";

            var navProcPtr = LoadLibrary(mapsPath);

            var calculatePathPtr = GetProcAddress(navProcPtr, "CalculatePath");
            calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(calculatePathPtr);

            var freePathPtr = GetProcAddress(navProcPtr, "FreePathArr");
            freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArr>(freePathPtr);
        }

        static public float DistanceViaPath(uint mapId, Position start, Position end)
        {
            var distance = 0f;
            var path = CalculatePath(mapId, start, end, false);
            for (var i = 0; i < path.Length - 1; i++)
                distance += path[i].DistanceTo(path[i + 1]);
            return distance;
        }

        static public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            var ret = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int length);
            var list = new Position[length];
            for (var i = 0; i < length; i++)
            {
                list[i] = new Position(ret[i]);
            }
            freePathArr(ret);
            return list;
        }

        static public Position GetNextWaypoint(uint mapId, Position start, Position end, bool straightPath)
        {
            var path = CalculatePath(mapId, start, end, straightPath);
            if (path.Length <= 1)
            {
                Logger.Log($"Problem building path for mapId \"{mapId}\". Make sure the \"mmaps\" directory contains the required mmap and tile-files. Returning destination as next waypoint...");
                return end;
            }

            return path[1];
        }

        // if p0 and p1 make a line, this method calculates whether point p2 is leftOf, on, or rightOf that line
        static PointComparisonResult IsLeft(Position p0, Position p1, Position p2)
        {
            var result = (p1.X - p0.Y) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y);

            if (result < 0)
                return PointComparisonResult.RightOfLine;
            else if (result > 0)
                return PointComparisonResult.LeftOfLine;
            else
                return PointComparisonResult.OnLine;
        }

        static public bool IsPositionInsidePolygon(Position point, Position[] polygon)
        {
            var cn = 0;

            for (var i = 0; i < polygon.Length - 1; i++)
            {
                if (((polygon[i].Y <= point.Y) && (polygon[i + 1].Y > point.Y)) || ((polygon[i].Y > point.Y) && (polygon[i + 1].Y <= point.Y)))
                {
                    var vt = (float)(point.Y - polygon[i].Y) / (polygon[i + 1].Y - polygon[i].Y);
                    if (point.X < polygon[i].X + vt * (polygon[i + 1].X - polygon[i].X))
                        ++cn;
                }
            }

            return cn == 1;
        }
    }

    enum PointComparisonResult : byte
    {
        LeftOfLine,
        OnLine,
        RightOfLine
    }
}