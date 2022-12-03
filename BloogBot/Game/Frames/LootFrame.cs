using BloogBot.Game.Cache;
using System;
using System.Collections.Generic;

namespace BloogBot.Game.Frames
{
    public class LootFrame
    {
        readonly public IList<LootItem> LootItems = new List<LootItem>();

        public LootFrame()
        {
            var hasCoins = MemoryManager.ReadInt((IntPtr)MemoryAddresses.CoinCountPtr) > 0;
            if (hasCoins)
                LootItems.Add(new LootItem(null, 0, 0, true));
            for (var i = 0; i <= 15; i++)
            {
                var itemId = MemoryManager.ReadInt((IntPtr)(MemoryAddresses.LootFrameItemsBasePtr + i * MemoryAddresses.LootFrameItemOffset));
                if (itemId == 0) break;
                var itemCacheEntry = MemoryManager.ReadItemCacheEntry(Functions.GetItemCacheEntry(itemId));
                var itemCacheInfo = new ItemCacheInfo(itemCacheEntry);
                var lootSlot = hasCoins ? i + 1 : i;
                LootItems.Add(new LootItem(itemCacheInfo, itemId, lootSlot, false));
            }
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

        public void Loot() => Functions.LootSlot(LootSlot);
    }
}
