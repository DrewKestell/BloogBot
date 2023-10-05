using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
    /// <summary>
    ///     Represents an item lootable from an unit
    /// </summary>
    public abstract class LootItem
    {
        internal LootItem(int lootSlotNumber, int memoryLootSlotNumber, int parItemId, ref ItemCacheEntry? tmpInfo)
        {
            _memLootSlot = memoryLootSlotNumber;
            var start = 0xB71974 + 0x1c * memoryLootSlotNumber;
            Quantity = start.ReadAs<int>();
            LootSlot = lootSlotNumber;
            // ReSharper disable once PossibleInvalidOperationException
            Info = tmpInfo.Value;
            IsCoin = false;
        }

        internal LootItem()
        {
            LootSlot = 0;
            IsCoin = true;
        }


        /// <summary>
        ///     Determines if this lootable item is a coin
        /// </summary>
        public bool IsCoin { get; }

        /// <summary>
        ///     The items lootslot index
        /// </summary>
        public int LootSlot { get; }

        private int _memLootSlot { get; }

        /// <summary>
        ///     The ID of the item
        /// </summary>
        /// <value>
        ///     The item identifier.
        /// </value>
        public int ItemId => IsCoin ? 0xB71BA0.ReadAs<int>() : (0x00B7196C + _memLootSlot * 0x1c).ReadAs<int>();

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
        public ItemCacheEntry Info { get; private set; }

        /// <summary>
        /// Loots the given loot item
        /// </summary>
        public void Loot() => LootFrame.Instance.LootSlot(LootSlot);
    }
}
