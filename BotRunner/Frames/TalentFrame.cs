namespace BotRunner.Frames
{
    public interface ITalentFrame
    {
        bool IsOpen { get; }
        void Close();
        IEnumerable<TalentTab> Tabs { get; }
        int TalentPointsAll { get; }
        int TalentPointsAvailable { get; }
        int TalentPointsSpent { get; }
        void LearnTalent(int spellId);
    }

    /// <summary>
    ///     Representing a particular talent tree tab
    /// </summary>
    public class TalentTab
    {
        /// <summary>
        ///     The name of the talent tree
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; } = string.Empty;
        public TalentRow[] Rows { get; } = new TalentRow[7];

    }

    public class TalentRow
    {
        public TalentEntry[] Talents { get; } = new TalentEntry[4];
    }

    public class TalentEntry
    {
        public string Name { get; } = string.Empty;
        public int Ranks { get; }
        public int TotalRanksAvailable { get; }
        public int NextRankSpellId { get; }
    }
}
