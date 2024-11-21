namespace BotRunner.Frames
{
    public interface IGossipFrame
    {
        bool IsOpen { get; }
        void Close();
        ulong NPCGuid { get; }
        void SelectGossipOption(int parOptionIndex);
        void SelectFirstGossipOfType(DialogType type);
        List<GossipOption> Options { get; }
        List<QuestOption> Quests { get; }
    }

    /// <summary>
    ///     The different types of Gossip Options in WoW
    /// </summary>
    public enum GossipTypes
    {
        Gossip = 0,
        Vendor = 1,
        Taxi = 2,
        Trainer = 3,
        Healer = 4,
        Binder = 5,
        Banker = 6,
        Petition = 7,
        Tabard = 8,
        Battlemaster = 9,
        Auctioneer = 10
    }

    public enum DialogType
    {
        gossip = 0,
        vendor = 1,
        taxi = 2,
        trainer = 3,
        healer = 4,
        binder = 5,
        banker = 6,
        petition = 7,
        tabard = 8,
        battlemaster = 9,
        auctioneer = 10
    }

    /// <summary>
    ///     Types of Quest-Frames (Accept, Continue, Complete, None)
    /// </summary>
    public enum QuestFrameState
    {
        Accept = 1,
        Continue = 2,
        Complete = 3,
        Greeting = 0
    }

    /// <summary>
    ///     An object representing a Gossip Option of an ingame Gossip Dialog
    /// </summary>
    public abstract class GossipOption
    {
        /// <summary>
        ///     The type of the Gossip Item
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public GossipTypes Type { get; }

        /// <summary>
        ///     The text of the Gossip Item
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; } = string.Empty;
    }
}
