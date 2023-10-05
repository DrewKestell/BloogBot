using RaidMemberBot.Constants;
using System;

namespace RaidMemberBot.ExtensionMethods
{
    internal static class EnumExtensions
    {
        internal static bool IsFlag(this Enum keys, Enum flag)
        {
            try
            {
                var keysVal = Convert.ToUInt64(keys);
                var flagVal = Convert.ToUInt64(flag);

                return (keysVal | flagVal) == flagVal;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    internal static class MovementFlagsExtensions
    {
        internal static bool HasFlag(this Enums.MovementFlags value, Enums.MovementFlags flag)
        {
            return (value & flag) != 0;
        }
    }

    internal static class ControlBitsExtensions
    {
        internal static bool HasFlag(this Enums.ControlBits value, Enums.ControlBits flag)
        {
            return (value & flag) != 0;
        }
    }
}
