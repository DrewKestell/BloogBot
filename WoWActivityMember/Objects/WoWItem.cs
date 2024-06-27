using WoWActivityMember.Game;
using WoWActivityMember.Mem;
using static WoWActivityMember.Constants.Enums;

namespace WoWActivityMember.Objects
{
    public class WoWItem : WoWObject
    {
        internal WoWItem(
            IntPtr pointer,
            ulong guid,
            WoWObjectTypes objectType)
            : base(pointer, guid, objectType)
        {
            var addr = Functions.GetItemCacheEntry(ItemId);
            if (addr != IntPtr.Zero)
            {
                var itemCacheEntry = MemoryManager.ReadItemCacheEntry(addr);
                Info = new ItemCacheInfo(itemCacheEntry);
            }
        }

        public int ItemId => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_ItemIdOffset);

        public int StackCount => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_StackCountOffset);

        public ItemCacheInfo Info { get; }

        public void Use() => Functions.UseItem(Pointer);

        public ItemQuality Quality => Info.Quality;

        public int Durability => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWItem_DurabilityOffset);

        public int DurabilityPercentage => (int)((double)Durability / Info.MaxDurability * 100);
    }
}
