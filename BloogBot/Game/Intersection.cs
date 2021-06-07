using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Intersection
    {
        internal float X;
        internal float Y;
        internal float Z;
        internal float R;
    }
}
