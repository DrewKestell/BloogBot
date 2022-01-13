using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    [StructLayout(LayoutKind.Sequential)]
    public struct XY
    {
        public int X;
        public int Y;

        public XY(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
