using BotRunner.Interfaces;

namespace ActivityForegroundMember.Objects
{
    public class ItemCacheInfo(ItemCacheEntry itemCacheEntry)
    {
        public ItemCacheEntry Info { get; private set; } = itemCacheEntry;
    }
}
