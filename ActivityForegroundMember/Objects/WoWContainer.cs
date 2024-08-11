using ActivityForegroundMember.Mem;
using BotRunner.Interfaces;

namespace ActivityForegroundMember.Objects
{
    public class WoWContainer : WoWItem, IWoWContainer
    {
        internal WoWContainer(
            nint pointer,
            ulong guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public int Slots => MemoryManager.ReadInt(nint.Add(Pointer, MemoryAddresses.WoWItem_ContainerSlotsOffset));

        // slot index starts at 0
        public ulong GetItemGuid(int slot) =>
            MemoryManager.ReadUlong(GetDescriptorPtr() + (MemoryAddresses.WoWItem_ContainerFirstItemOffset + slot * 8));
    }
}
