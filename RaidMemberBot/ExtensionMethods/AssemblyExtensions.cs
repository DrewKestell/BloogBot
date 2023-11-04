using System.IO;
using System.Reflection;

namespace RaidMemberBot.ExtensionMethods
{
    internal static class AssemblyExtensions
    {
        internal static string ExtJumpUp(this Assembly value, int parLevels)
        {
            string tmp = value.Location;
            for (int i = 0; i < parLevels; i++)
                tmp = Path.GetDirectoryName(tmp);
            return tmp;
        }
    }
}
