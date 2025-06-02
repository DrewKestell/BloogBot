using GameData.Core.Models;

namespace ForegroundBotRunner.Objects
{
    public class ItemCacheInfo(ItemCacheEntry itemCacheEntry)
    {
        public ItemCacheEntry Info { get; private set; } = itemCacheEntry;
    }
}
