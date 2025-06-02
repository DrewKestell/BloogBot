using ForegroundBotRunner.Mem;
using GameData.Core.Enums;
using GameData.Core.Models;

namespace ForegroundBotRunner.Objects
{
    public class WoWItem : WoWObject
    {
        internal WoWItem(
            nint pointer,
            HighGuid guid,
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
        public ItemCacheInfo Info => CacheInfo;

        public void Use() => Functions.UseItem(Pointer);

        public static void Loot()
        {

        }

        public static ItemQuality Quality => ItemQuality.Common;

        public uint Durability => (uint)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_DurabilityOffset);

        public uint DurabilityPercentage => (uint)((double)Durability / MaxDurability * 100);

        public static uint MaxDurability => 0;

        public static uint RequiredLevel => 1;

        public static bool IsCoins => false;

        public ItemDynFlags Flags { get; set; }

        public uint Quantity => throw new NotImplementedException();
    }
}
