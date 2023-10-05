using RaidMemberBot.Game.Statics;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
    /// <summary>
    ///     Represents a spell we can learn at the trainer
    /// </summary>
    public abstract class TrainerSpellItem
    {
        internal TrainerSpellItem(int parSpellIndex)
        {
            var result =
                Lua.Instance.ExecuteWithResult("{0}, {2}, {1}, _ = GetTrainerServiceInfo(" + parSpellIndex + ");");
            Name = result[0];

            CanLearn = result[1] == "available";

            var tmpRank = result[2];
            var tmpArr = tmpRank.Split(' ');
            if (tmpArr.Length == 2)
            {
                int intRank;
                if (int.TryParse(tmpArr[1], out intRank))
                    Rank = intRank;
            }
            else
            {
                Rank = 0;
            }
            Index = parSpellIndex;
        }

        /// <summary>
        ///     The name of the spell
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        ///     The rank of the spell we learn
        /// </summary>
        /// <value>
        ///     The rank.
        /// </value>
        public int Rank { get; }

        /// <summary>
        ///     The index of the spell at the trainer interface
        /// </summary>
        /// <value>
        ///     The index.
        /// </value>
        public int Index { get; }

        /// <summary>
        ///     Can we learn the spell?
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can learn; otherwise, <c>false</c>.
        /// </value>
        public bool CanLearn { get; }
    }
}
