namespace BotRunner.Interfaces
{
    public interface IMerchantFrame
    {
        public bool CanRepair { get; }
        bool Ready { get; }

        public void SellItemByGuid(uint itemCount, ulong npcGuid, ulong itemGuid);
        public void BuyItemByName(ulong vendorGuid, string itemName, int quantity);
        public void RepairAll();
        public void CloseMerchantFrame();
    }
}
