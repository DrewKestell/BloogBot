using System.Runtime.InteropServices;

namespace WoWClientBot.Constants
{
    [StructLayout(LayoutKind.Sequential)]
    public struct XYZ
    {
        internal float X;
        internal float Y;
        internal float Z;

        internal XYZ(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XYZXYZ
    {
        internal float X1;
        internal float Y1;
        internal float Z1;
        internal float X2;
        internal float Y2;
        internal float Z2;

        internal XYZXYZ(float x1, float y1, float z1,
            float x2, float y2, float z2)
        {
            X1 = x1;
            Y1 = y1;
            Z1 = z1;
            X2 = x2;
            Y2 = y2;
            Z2 = z2;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Intersection
    {
        internal float X;
        internal float Y;
        internal float Z;
        internal float R;
    }
}
