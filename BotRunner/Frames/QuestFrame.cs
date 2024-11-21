using BotRunner.Interfaces;

namespace BotRunner.Frames
{
    public interface IQuestFrame
    {
        bool IsOpen { get; }
        void Close();
        ulong NpcGuid { get; }
        void AcceptQuest();
        void DeclineQuest();
        void CompleteQuest();
        QuestFrameState State { get; }
        int QuestFrameId { get; }
        int RewardCount { get; }
        bool Proceed();
        void CompleteQuest(int? parReward = null);
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

    /// <summary>
    ///     Object representing an item we are able to choose when completing a quest
    /// </summary>
    public abstract class QuestChoiceReward
    {
        /// <summary>
        ///     The count
        /// </summary>
        public readonly int Count;

        /// <summary>
        ///     CacheEntry of the Item
        /// </summary>
        public readonly ItemCacheEntry Info;

        /// <summary>
        ///     The item ID
        /// </summary>
        public readonly int ItemId;
    }

    /// <summary>
    ///     Representing Gossip Options related to a quest
    /// </summary>
    public abstract class QuestOption
    {
        /// <summary>
        ///     The state of the Gossip Quest Option (Accept, Complete etc.)
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        public QuestGossipState State { get; }

        /// <summary>
        ///     The text of the Gossip Quest Option
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; } = string.Empty;

        /// <summary>
        ///     The ID of the Quest related to the Gossip Dialog
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public int Id { get; }
    }
}