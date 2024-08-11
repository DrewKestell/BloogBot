using BotRunner.Interfaces;

namespace WoWSharpClient.Frames
{
    public class MerchantFrame : IMerchantFrame
    {
        private readonly IList<MerchantItem> items = [];

        public bool Ready { get; private set; }

        public MerchantFrame() => Ready = true;

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
        internal MerchantItem(int itemId, IWoWItemCacheInfo info)
        {
            ItemId = itemId;
            Info = info;
        }

        public int ItemId { get; }

        public string Name => Info.Name;

        public IWoWItemCacheInfo Info { get; }
    }
}
