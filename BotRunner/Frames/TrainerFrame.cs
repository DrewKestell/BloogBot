namespace BotRunner.Frames
{
    public interface ITrainerFrame
    {
        bool IsOpen { get; }
        void Close();
        IEnumerable<TrainerSpellItem> Spells { get; }
        void TrainSpell(int spellIndex);
        void Update();
    }
    
    /// <summary>
    ///     Represents a spell we can learn at the trainer
    /// </summary>
    public class TrainerSpellItem
    {
        /// <summary>
        ///     The name of the spell
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; } = string.Empty;

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

        /// <summary>
        ///     The cost of the spell at the trainer interface
        /// </summary>
        /// <value>
        ///     The cost.
        /// </value>
        public int Cost { get; }
    }
}
