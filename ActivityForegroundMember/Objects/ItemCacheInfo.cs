using BotRunner.Interfaces;

namespace ActivityForegroundMember.Objects
{
    public class ItemCacheInfo(ItemCacheEntry itemCacheEntry) : IWoWItemCacheInfo
    {
        public ItemCacheEntry Info { get; private set; } = itemCacheEntry;
    }
}
