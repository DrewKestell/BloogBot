using RaidMemberBot.Constants;
using RaidMemberBot.Mem;
using System;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Objects
{
    public class WoWContainer : WoWItem
    {
        internal WoWContainer(
            IntPtr pointer,
            ulong guid,
            WoWObjectTypes objectType)
            : base(pointer, guid, objectType)
        {
        }

        public int Slots => MemoryManager.ReadInt(IntPtr.Add(Pointer, MemoryAddresses.WoWItem_ContainerSlotsOffset));

        // slot index starts at 0
        public ulong GetItemGuid(int slot) =>
            MemoryManager.ReadUlong(GetDescriptorPtr() + (MemoryAddresses.WoWItem_ContainerFirstItemOffset + (slot * 8)));
    }
}
