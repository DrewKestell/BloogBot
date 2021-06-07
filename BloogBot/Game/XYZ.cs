using System.Runtime.InteropServices;

namespace BloogBot.Game
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
}
