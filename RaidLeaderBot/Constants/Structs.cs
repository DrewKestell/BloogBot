using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Objects;
using System.Runtime.InteropServices;
using static RaidMemberBot.Constants.Enums;

namespace RaidLeaderBot.Constants
{

    /// <summary>
    ///     Coordinate struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct _XYZ
    {
        internal float X;
        internal float Y;
        internal float Z;

        internal _XYZ(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
