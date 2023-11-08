using RaidMemberBot.Constants;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
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

        internal QuestChoiceReward(int parItemId, int parItemCount, ref ItemCacheEntry parEntry)
        {
            ItemId = parItemId;
            Count = parItemCount;
            Info = parEntry;
        }
    }
}
