using ActivityForegroundMember.Mem;
using ActivityForegroundMember.Objects;

namespace ActivityForegroundMember.Game.Frames
{
    public class MerchantFrame
    {
        private readonly IList<MerchantItem> items = [];

        public readonly bool Ready;

        public MerchantFrame()
        {
            var canRepairResult = Functions.LuaCallWithResult("{0} = CanMerchantRepair()");
            CanRepair = canRepairResult.Length > 0 && canRepairResult[0] == "1";

            var totalVendorItems = MemoryManager.ReadInt(MemoryAddresses.MerchantFrameItemsBasePtr);
            for (var i = 0; i < totalVendorItems; i++)
            {
                var itemId = MemoryManager.ReadInt(MemoryAddresses.MerchantFrameItemPtr + i * MemoryAddresses.MerchantFrameItemOffset);
                var address = Functions.GetItemCacheEntry(itemId);
                if (address == nint.Zero) continue;
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

        public string Name => string.Empty;

        public ItemCacheInfo Info { get; }
    }
}
