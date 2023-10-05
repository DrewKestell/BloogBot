using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Game.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.Game.Frames
{
    /// <summary>
    ///     Represents a Trainer-Frame
    /// </summary>
    public sealed class TrainerFrame
    {
        private static volatile TrainerFrame _instance;
        private static readonly object lockObject = new object();
        private static volatile bool _isOpen;
        private static volatile bool _abort;


        private readonly List<TrainerSpellItem> _Spells = new List<TrainerSpellItem>();

        private TrainerFrame()
        {
            Update();
        }

        /// <summary>
        ///     Access to the currently open Trainer-Frame
        /// </summary>
        /// <value>
        ///     The frame.
        /// </value>
        public static TrainerFrame Instance => _instance;

        /// <summary>
        ///     Tells whether a Trainer-Frame is open or not
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public static bool IsOpen => _isOpen;

        /// <summary>
        ///     Number of avaible spells to learn
        /// </summary>
        /// <value>
        ///     The avaible spells.
        /// </value>
        public int AvaibleSpells { get; private set; }

        /// <summary>
        ///     A list of all spells we can learn from the current Trainer-Frame
        /// </summary>
        /// <value>
        ///     The spells.
        /// </value>
        public IReadOnlyList<TrainerSpellItem> Spells => _Spells;

        internal static void Create()
        {
            lock (lockObject)
            {
                _isOpen = false;
                _abort = false;

                var tmp = new TrainerFrame();
                if (_abort) return;

                _instance = tmp;
                _isOpen = true;
            }
        }

        internal static void Destroy()
        {
            _abort = true;
            lock (lockObject)
            {
                _isOpen = false;
            }
        }

        /// <summary>
        ///     Learn an avaible spell by name
        /// </summary>
        /// <param name="parSpell">The par spell.</param>
        public void LearnSpellByName(string parSpell)
        {
            foreach (var x in Spells)
                if (x.Name == parSpell)
                {
                    Lua.Instance.Execute("BuyTrainerService(" + x.Index + ")");
                    Update();
                    return;
                }
        }

        /// <summary>
        ///     Determines if we can learn a certain spell from the currently open Trainer-Frame
        /// </summary>
        /// <param name="parSpell">The spell name.</param>
        /// <param name="parRank">The spell rank.</param>
        /// <returns></returns>
        internal bool CanLearnSpell(string parSpell, int parRank)
        {
            return Spells.Any(x => x.Name == parSpell && x.Rank == parRank);
        }

        /// <summary>
        ///     /// Determines if we can learn a certain spell from the currently open Trainer-Frame
        /// </summary>
        /// <param name="parSpell">The spell name.</param>
        /// <returns></returns>
        public bool CanLearnSpell(string parSpell)
        {
            return Spells.Any(x => x.Name == parSpell);
        }

        private void Update()
        {
            var result = Lua.Instance.ExecuteWithResult(
                "SetTrainerServiceTypeFilter('available', 1) SetTrainerServiceTypeFilter('unavailable', 0) SetTrainerServiceTypeFilter('used', 0) " +
                "{0} = GetNumTrainerServices()");

            AvaibleSpells = Convert.ToInt32(result[0]);

            for (var i = 0; i < AvaibleSpells; i++)
            {
                TrainerSpellItem tsi = new TrainerSpellItemInterface(i + 1);
                if (tsi.CanLearn)
                    _Spells.Add(tsi);
            }
            AvaibleSpells = _Spells.Count;
        }

        private class TrainerSpellItemInterface : TrainerSpellItem
        {
            internal TrainerSpellItemInterface(int id) : base(id)
            {
            }
        }
    }
}
