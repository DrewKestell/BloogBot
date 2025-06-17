using System.Diagnostics;
namespace WoWSharpClient.Utils
{
    public class WorldTimeTracker
    {
        private readonly Stopwatch _realTime = Stopwatch.StartNew();

        /// <summary>
        /// Returns the current server-aligned monotonic time in ms.
        /// Equivalent to WorldTimer::getMSTime().
        /// </summary>
        public TimeSpan NowMS => TimeSpan.FromMilliseconds(_realTime.ElapsedMilliseconds);
    }
}

