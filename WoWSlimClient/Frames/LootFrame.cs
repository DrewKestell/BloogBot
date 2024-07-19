using WoWSlimClient.Models;

namespace WoWSlimClient.Frames
{
    public class LootFrame
    {
        readonly public IList<LootItem> LootItems = [];

        public LootFrame()
        {
            
        }
    }

    public class LootItem
    {
        internal LootItem(
            ItemCacheInfo info,
            int itemId,
            int lootSlot,
            bool isCoins)
        {
            Info = info;
            ItemId = itemId;
            LootSlot = lootSlot;
            IsCoins = isCoins;
        }

        internal int LootSlot { get; set; }

        public int ItemId { get; set; }

        public ItemCacheInfo Info { get; }

        public bool IsCoins { get; }

        public void Loot() { }
    }
}
