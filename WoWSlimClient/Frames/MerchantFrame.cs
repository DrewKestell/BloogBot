using WoWSlimClient.Models;

namespace WoWSlimClient.Frames
{
    public class MerchantFrame
    {
        private readonly IList<MerchantItem> items = [];

        public readonly bool Ready;

        public MerchantFrame()
        {
            Ready = true;
        }

        public bool CanRepair { get; }

        public void SellItemByGuid(uint itemCount, ulong npcGuid, ulong itemGuid)
        {

        }

        public void BuyItemByName(ulong vendorGuid, string itemName, int quantity)
        {

        }

        public void RepairAll()
        {

        }

        public void CloseMerchantFrame()
        {

        }
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
