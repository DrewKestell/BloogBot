using System.Diagnostics;
namespace WoWSharpClient.Utils
{
    public class WorldTimeTracker
    {
        private readonly Stopwatch _realTime = Stopwatch.StartNew();
        private uint _startTimeMs;

        /// <summary>
        /// Syncs to the server's getMSTime() value.
        /// Should be called on login or time sync packet.
        /// </summary>
        public void OnServerTimeSync(uint serverTimeMs)
        {
            _startTimeMs = serverTimeMs;
            _realTime.Restart();
        }

        /// <summary>
        /// Returns the current server-aligned monotonic time in ms.
        /// Equivalent to WorldTimer::getMSTime().
        /// </summary>
        public uint NowMS => _startTimeMs + (uint)_realTime.ElapsedMilliseconds;

        public static uint GetTimeDiff(uint oldMs, uint newMs)
        {
            if (oldMs > newMs)
            {
                uint diff1 = (0xFFFFFFFFu - oldMs) + newMs;
                uint diff2 = oldMs - newMs;
                return Math.Min(diff1, diff2);
            }
            return newMs - oldMs;
        }

        public uint GetTimeDiffToNow(uint oldMs) => GetTimeDiff(oldMs, NowMS);
    }
}

