using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Frames
{
    public interface IMerchantFrame
    {
        bool IsOpen { get; }
        void Close();
        bool CanRepair { get; }
        int TotalRepairCost { get; }

        bool Ready { get; }
        IReadOnlyList<MerchantItem> Items { get; }

        void SellItem(int bagId, int slotId, int quantity);
        void BuyItem(int itemGuid, int itemCount);
        void BuybackItem(int itemGuid, int itemCount);
        int RepairCost(EquipSlot equipSlot);
        void RepairByEquipSlot(EquipSlot parSlot);
        void RepairAll();
        void ItemCallback(int parItemId);
        bool IsItemAvaible(int parItemId);
        void VendorByGuid(ulong guid, uint itemCount = 1);
    }

    public class MerchantItem(int vendorItemNumber, int itemId, ItemCacheEntry tmpInfo)
    {
        /// <summary>
        ///     The index of the item inside the Merchant Frame
        /// </summary>
        /// <value>
        ///     The vendor item number.
        /// </value>
        public int VendorItemNumber { get; } = vendorItemNumber;

        /// <summary>
        ///     The ID of the item
        /// </summary>
        /// <value>
        ///     The item identifier.
        /// </value>
        public int ItemId { get; } = itemId;

        /// <summary>
        ///     The name of the item
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name => tmpInfo.Name;

        /// <summary>
        ///     The price for the item in copper
        /// </summary>
        /// <value>
        ///     The price.
        /// </value>
        public int Price { get; }

        /// <summary>
        ///     The number of item we buy with one (Arrows: 200x, Food: 5x etc.)
        /// </summary>
        /// <value>
        ///     The quantity.
        /// </value>
        public int Quantity { get; }

        /// <summary>
        ///     The number of avaible Items
        /// </summary>
        /// <value>
        ///     The number avaible.
        /// </value>
        public int NumAvaible { get; }

        /// <summary>
        ///     Can our character use the item being sold?
        /// </summary>
        /// <value>
        /// </value>
        public bool CanUse { get; }

        /// <summary>
        ///     Information about the Merchant Item
        /// </summary>
        /// <value>
        ///     The information.
        /// </value>
        public ItemCacheEntry Info { get; } = tmpInfo;
    }
}
