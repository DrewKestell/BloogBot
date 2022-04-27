using BloogBot.Game.Enums;
using System;
using System.Diagnostics;

namespace BloogBot
{
    public static class ClientHelper
    {
        public static readonly ClientVersion ClientVersion;

        static ClientHelper()
        {
            var clientVersion = Process.GetProcessesByName("WoW")[0].MainModule.FileVersionInfo.FileVersion;

            if (clientVersion == "3, 3, 5, 12340")
            {
                ClientVersion = ClientVersion.WotLK;
            }
            else if (clientVersion == "2, 4, 3, 8606")
            {
                ClientVersion = ClientVersion.TBC;
            }
            else if (clientVersion == "1, 12, 1, 5875")
            {
                ClientVersion = ClientVersion.Vanilla;
            }
            else
                throw new InvalidOperationException("Unknown client version.");
        }
    }
}
