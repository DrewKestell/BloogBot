using BotRunner.Interfaces;
using System.Collections.Concurrent;

namespace BotRunner.Frames
{
    public interface ILootFrame
    {
        bool IsOpen { get; }
        void Close();
        IEnumerable<LootItem> LootItems { get; }
        int LootCount { get; }
        ulong LootGuid { get; }
        int Coins { get; }
        ConcurrentDictionary<int, int> MissingIds { get; }
        void ItemCallback(int parItemId);
        void LootSlot(int parSlotIndex);
        void LootAll();
    }

    /// <summary>
    ///     Represents an item lootable from an unit
    /// </summary>
    public abstract class LootItem
    {
        /// <summary>
        ///     Determines if this lootable item is a coin
        /// </summary>
        public bool IsCoin { get; }

        /// <summary>
        ///     The items lootslot index
        /// </summary>
        public int LootSlot { get; }

        /// <summary>
        ///     The ID of the item
        /// </summary>
        /// <value>
        ///     The item identifier.
        /// </value>
        public int ItemId { get; }

        /// <summary>
        ///     Determines if this slot got loot
        /// </summary>
        public bool GotLoot
        {
            get
            {
                if (IsCoin && ItemId == -1)
                    return false;
                return ItemId != 0;
            }
        }

        /// <summary>
        ///     The number of items we will loot
        /// </summary>
        /// <value>
        ///     The quantity.
        /// </value>
        public int Quantity { get; }

        /// <summary>
        ///     Information about the item
        /// </summary>
        /// <value>
        ///     The information.
        /// </value>
        public ItemCacheEntry Info { get; }

        /// <summary>
        /// Loots the given loot item
        /// </summary>
        public abstract void Loot();
    }
}
