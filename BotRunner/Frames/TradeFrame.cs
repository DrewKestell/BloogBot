using BotRunner.Interfaces;

namespace BotRunner.Frames
{
    public interface ITradeFrame
    {
        bool IsOpen { get; }
        void Close(); 
        List<IWoWItem> OfferedItems { get; }
        List<IWoWItem> OtherPlayerItems { get; }
        void OfferMoney(int copperCount);
        void OfferItem(int bagId, int slotId, int quantity, int tradeWindowSlot);
        void AcceptTrade();
        void DeclineTrade();
        void OfferLockpick();
        void OfferEnchant(int enchantId);
    }
}
