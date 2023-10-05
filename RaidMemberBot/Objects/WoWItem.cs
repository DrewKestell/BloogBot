using RaidMemberBot.Constants;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using System;

namespace RaidMemberBot.Objects
{
    /// <summary>
    ///     Represents WoW items (equipped and in the bag)
    /// </summary>
    public class WoWItem : WoWObject
    {
        private ItemCacheEntry _Info;
        private bool GotCache;

        internal WoWItem(ulong parGuid, IntPtr parPointer, Enums.WoWObjectTypes parType)
            : base(parGuid, parPointer, parType)
        {
        }

        /// <summary>
        ///     ItemCacheEntry for the current object
        /// </summary>
        public ItemCacheEntry Info
        {
            get
            {
                if (GotCache) return _Info;
                _Info = ObjectManager.Instance.LookupItemCacheEntry(ItemId, PrivateEnums.ItemCacheLookupType.None) ??
                        default(ItemCacheEntry);
                GotCache = true;
                return _Info;
            }
        }

        /// <summary>
        ///     ID of the object
        /// </summary>
        public int Id => ReadRelative<int>(0x354);

        /// <summary>
        ///     GUID of the object owning the item
        /// </summary>
        public ulong OwnerGuid => GetDescriptor<ulong>(0x18);

        /// <summary>
        ///     Pointer to WDB cache
        /// </summary>
        private IntPtr ItemCachePointer => Functions.ItemCacheGetRow(ItemId, PrivateEnums.ItemCacheLookupType.None);

        private int ItemId => GetDescriptor<int>(Offsets.Descriptors.ItemId);

        /// <summary>
        ///     Durability of the item
        /// </summary>
        public int Durability => GetDescriptor<int>(Offsets.Descriptors.ItemDurability);

        /// <summary>
        ///     Current durability of the item
        /// </summary>
        public int MaxDurability => GetDescriptor<int>(Offsets.Descriptors.ItemMaxDurability);

        /// <summary>
        ///     Current durability in percent
        /// </summary>
        public int DurabilityPercent => (int)(Durability / (float)MaxDurability * 100);

        /// <summary>
        ///     The stack count of the item
        /// </summary>
        public int StackCount => GetDescriptor<int>(Offsets.Descriptors.ItemStackCount);


        /// <summary>
        ///     The quality of the item
        /// </summary>
        public Enums.ItemQuality Quality => Memory.Reader.Read<Enums.ItemQuality>(IntPtr.Add(ItemCachePointer, Offsets.Item.ItemCachePtrQuality));

        /// <summary>
        ///     Name of the item
        /// </summary>
        public override string Name => Info.Name;

        /// <summary>
        ///     Always 0,0,0 -> Items dont have a position
        /// </summary>
        public override Location Location => new Location(0, 0, 0);

        /// <inheritdoc />
        public override float Facing => 0;

        /// <summary>
        ///     Slots avaible (0 if no bag)
        /// </summary>
        public int Slots => ReadRelative<int>(Offsets.Item.ItemSlots);

        /// <summary>
        ///     Uses the item
        /// </summary>
        public void Use()
        {
            Functions.UseItem(Pointer);
        }

        public void UseOn(WoWItem otherItem)
        {
            Functions.UseItem(Pointer, otherItem.Guid);
        }

        /// <summary>
        ///     Uses the item at a position
        /// </summary>
        /// <param name="parPos">The position.</param>
        public void UseAtPos(Location parPos)
        {
            Functions.UseItemAtPos(Pointer, parPos);
        }

        /// <summary>
        ///     Determines if we can use the item
        /// </summary>
        /// <returns></returns>
        public bool CanUse()
        {
            return Functions.CanUseItem(Id, PrivateEnums.ItemCacheLookupType.None);
        }

        public override string ToString()
        {
            return Name + "-> Stackcount: " + StackCount + " Durability: " + Durability + " Quality: " + Quality;
        }
    }
}
