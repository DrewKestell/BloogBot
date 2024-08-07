using ActivityForegroundMember.Mem;
using BotRunner.Constants;
using BotRunner.Interfaces;

namespace ActivityForegroundMember.Objects
{
    public class WoWItem : WoWObject, IWoWItem
    {
        internal WoWItem(
            nint pointer,
            ulong guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
            var addr = Functions.GetItemCacheEntry((int)ItemId);
            if (addr != nint.Zero)
            {
                var itemCacheEntry = MemoryManager.ReadItemCacheEntry(addr);
                CacheInfo = new ItemCacheInfo(itemCacheEntry);
            }
        }

        public uint ItemId => (uint)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_ItemIdOffset);

        public uint StackCount => (uint)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_StackCountOffset);

        private readonly ItemCacheInfo CacheInfo;
        public IWoWItemCacheInfo Info => CacheInfo;

        public void Use() => Functions.UseItem(Pointer);

        public void Loot()
        {

        }

        public ItemQuality Quality => ItemQuality.Common;

        public uint Durability => (uint)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_DurabilityOffset);

        public uint DurabilityPercentage => (uint)((double)Durability / MaxDurability * 100);

        public uint MaxDurability => 0;

        public uint RequiredLevel => 1;

        public bool IsCoins => false;
    }
}
