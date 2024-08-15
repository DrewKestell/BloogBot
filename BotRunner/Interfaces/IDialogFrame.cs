namespace BotRunner.Interfaces
{
    public interface IDialogFrame
    {
        void CloseDialogFrame();

        void SelectFirstGossipOfType(DialogType type);
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
    ///     The state of a quest selectable in a gossip dialog (complete, accept etc.)
    /// </summary>
    public enum QuestGossipState
    {
        Accepted = 3,
        Available = 5,
        Completeable = 4
    }
}
