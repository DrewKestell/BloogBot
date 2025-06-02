using ForegroundBotRunner.Mem;
using GameData.Core.Enums;
using GameData.Core.Models;

namespace ForegroundBotRunner.Objects
{
    public class WoWContainer : WoWItem
    {
        internal WoWContainer(
            nint pointer,
            HighGuid guid,
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
