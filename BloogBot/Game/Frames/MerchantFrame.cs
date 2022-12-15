using BloogBot.Game.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.Game.Frames
{
    public class MerchantFrame
    {
        readonly IList<MerchantItem> items = new List<MerchantItem>();

        public readonly bool Ready;

        public MerchantFrame()
        {
            var canRepairResult = Functions.LuaCallWithResult("{0} = CanMerchantRepair()");
            CanRepair = canRepairResult.Length > 0 && canRepairResult[0] == "1";

            var totalVendorItems = MemoryManager.ReadInt((IntPtr)MemoryAddresses.MerchantFrameItemsBasePtr);
            for (var i = 0; i < totalVendorItems; i++)
            {
                var itemId = MemoryManager.ReadInt((IntPtr)(MemoryAddresses.MerchantFrameItemPtr + i * MemoryAddresses.MerchantFrameItemOffset));
                var address = Functions.GetItemCacheEntry(itemId);
                if (address == IntPtr.Zero) continue;
                var entry = MemoryManager.ReadItemCacheEntry(address);
                var info = new ItemCacheInfo(entry);
                items.Add(new MerchantItem(itemId, info));
            }

            Ready = totalVendorItems > 0;
        }

        public bool CanRepair { get; }

        public void SellItemByGuid(uint itemCount, ulong npcGuid, ulong itemGuid) =>
            Functions.SellItemByGuid(itemCount, npcGuid, itemGuid);

        public void BuyItemByName(ulong vendorGuid, string itemName, int quantity)
        {
            var item = items.Single(i => i.Name == itemName);
            Functions.BuyVendorItem(vendorGuid, item.ItemId, quantity);
        }

        public void RepairAll() => Functions.LuaCall("RepairAllItems()");

        public void CloseMerchantFrame() => Functions.LuaCall("CloseMerchant()");
    }

    public class MerchantItem
    {
        internal MerchantItem(int itemId, ItemCacheInfo info)
        {
            ItemId = itemId;
            Info = info;
        }

        public int ItemId { get; }

        public string Name => Info.Name;

        public ItemCacheInfo Info { get; }
    }
}
