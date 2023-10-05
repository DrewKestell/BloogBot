using System;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Class to deal with little hacks (All warden-proof)
    /// </summary>
    public sealed class Hacks
    {
        private static readonly Lazy<Hacks> _instance = new Lazy<Hacks>(() => new Hacks());

        private Hacks()
        {
        }

        /// <summary>
        ///     Access to the instance
        /// </summary>
        public static Hacks Instance => _instance.Value;

    }
}
