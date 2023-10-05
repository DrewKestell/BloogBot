using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Represents an inventory
    /// </summary>
    public sealed class Inventory
    {
        private static readonly object lockObject = new object();

        private static readonly Lazy<Inventory> _instance =
            new Lazy<Inventory>(() => new Inventory());

        private Inventory()
        {
        }

        /// <summary>
        ///     Access to the characters inventory
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Inventory Instance
        {
            get
            {
                lock (lockObject)
                {
                    return _instance.Value;
                }
            }
        }

        /// <summary>
        ///     Gets an item of the characters inventory
        /// </summary>
        /// <param name="parBag">The bag (starting at 0)</param>
        /// <param name="parSlot">The bag slot (starting at 0)</param>
        /// <returns>The item represented as WoWItem</returns>
        public WoWItem GetItem(int parBag, int parSlot)
        {
            if (!ObjectManager.Instance.IsIngame) return null;
            parBag += 1;
            switch (parBag)
            {
                case 1:
                    ulong itemGuid = 0;
                    if (parSlot < 16 && parSlot >= 0)
                        itemGuid = ObjectManager.Instance.Player.GetDescriptor<ulong>(0x850 + parSlot * 8);
                    return itemGuid == 0 ? null : ObjectManager.Instance.Items.FirstOrDefault(i => i.Guid == itemGuid);

                case 2:
                case 3:
                case 4:
                case 5:
                    var tmpBag = GetExtraBag(parBag - 2);
                    if (tmpBag == null) return null;
                    var tmpItemGuid = tmpBag.GetDescriptor<ulong>(0xC0 + (parSlot + 1) * 8);
                    if (tmpItemGuid == 0) return null;
                    return ObjectManager.Instance.Items.FirstOrDefault(i => i.Guid == tmpItemGuid);

                default:
                    return null;
            }
        }

        /// <summary>
        ///     Gets an item of the characters inventory by name
        /// </summary>
        /// <param name="parItemName">The item name</param>
        /// <returns>The item represented as WoWItem</returns>
        public WoWItem GetItem(string parItemName)
        {
            return !ObjectManager.Instance.IsIngame
                ? null
                : ObjectManager.Instance.Items.FirstOrDefault(i => i.Name == parItemName);
        }

        /// <summary>
        ///     Get item representing an equipable bag
        /// </summary>
        /// <param name="parSlot">The equipable bag slot (0 to 3)</param>
        /// <returns>null or the equipable bag represented as WoWItem</returns>
        public WoWItem GetExtraBag(int parSlot)
        {
            if (parSlot > 3 || parSlot < 0) return null;
            var bagGuid = Memory.Reader.Read<ulong>(IntPtr.Add(new IntPtr(0xBDD060), parSlot * 8));
            return bagGuid == 0 ? null : ObjectManager.Instance.Items.FirstOrDefault(i => i.Guid == bagGuid);
        }

        /// <summary>
        ///     Get the count of a certain item
        /// </summary>
        /// <param name="parItemId">The ID of the item</param>
        /// <returns>The total count of the item specified</returns>
        public int GetItemCount(int parItemId)
        {
            var totalCount = 0;
            for (var i = 0; i < 5; i++)
            {
                int slots;
                if (i == 0)
                {
                    slots = 16;
                }
                else
                {
                    var iAdjusted = i - 1;
                    var bag = GetExtraBag(iAdjusted);
                    if (bag == null) continue;
                    slots = bag.Slots;
                }

                for (var k = 0; k < slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Id == parItemId) totalCount += item.StackCount;
                }
            }
            return totalCount;
        }

        /// <summary>
        ///     Get the count of a certain item
        /// </summary>
        /// <param name="parItemName">Name of the item</param>
        /// <returns>The total count of the item specified</returns>
        public int GetItemCount(string parItemName)
        {
            var totalCount = 0;
            for (var i = 0; i < 5; i++)
            {
                int slots;
                if (i == 0)
                {
                    slots = 16;
                }
                else
                {
                    var iAdjusted = i - 1;
                    var bag = GetExtraBag(iAdjusted);
                    if (bag == null) continue;
                    slots = bag.Slots;
                }

                for (var k = 0; k <= slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Name == parItemName) totalCount += item.StackCount;
                }
            }
            return totalCount;
        }


        /// <summary>
        ///     Tells if a specific item exists in the characters inventory
        ///     <para>Example: We have a weaker potion X and a stronger one named Y</para>
        ///     We pass a list with X (first item) and Y (second item)
        ///     <para>If Y isnt found it will look for X</para>
        ///     Like this we can find the best item to use in the current situation
        ///     <para>If the stronger potion isnt avaible we will use a weaker one</para>
        /// </summary>
        /// <param name="parListOfNames">The list of items</param>
        /// <returns>
        ///     Returns the name of the most important item found
        ///     <para>If no item of the list is found the string will be empty</para>
        /// </returns>
        public string GetLastItem(string[] parListOfNames)
        {
            return ThreadSynchronizer.Instance.RunOnMainThread(() =>
            {
                try
                {
                    for (var i = parListOfNames.Length - 1; i > 0; i--)
                    {
                        var i1 = i;
                        var tmpList = ObjectManager.Instance.Items.Where(a => a.Name == parListOfNames[i1]).ToList();
                        if (tmpList.Count != 0)
                            return parListOfNames[i];
                    }
                }
                catch
                {
                    // ignored
                }
                return "";
            });
        }

        /// <summary>
        ///     Get an item equipped at a specific equipment slot
        /// </summary>
        /// <param name="parSlot">The slot</param>
        /// <returns>null or the item represented as WoWItem</returns>
        public WoWItem GetEquippedItem(EquipSlot parSlot)
        {
            var slot = (int)parSlot;
            var guid = ObjectManager.Instance.Player.ReadRelative<ulong>(0x2508 + (slot - 1) * 0x8);
            if (guid == 0) return null;
            return ObjectManager.Instance.Items.FirstOrDefault(i => i.Guid == guid);
        }

        /// <summary>
        ///     Return the number of free bag spaces as an int
        /// </summary>
        /// <param name="parCountSpecialSlots">Count specialized slots (Soul shard bags or quivers ect) </param>
        /// <returns></returns>
        public int CountFreeSlots(bool parCountSpecialSlots)
        {
            return ThreadSynchronizer.Instance.RunOnMainThread(() =>
            {
                var freeSlots = 0;
                try
                {
                    // Itera through base bag
                    for (var i = 0; i < 16; i++)
                    {
                        // get guid of the item stored in current slot (i = slot number)
                        var tmpSlotGuid = ObjectManager.Instance.Player.GetDescriptor<ulong>(0x850 + i * 8);
                        // current slot empty? +1 free slot
                        if (tmpSlotGuid == 0) freeSlots++;
                    }
                    // List where we store guids of our equipped bags
                    var BagGuids = new List<ulong>();
                    for (var i = 0; i < 4; i++)
                        BagGuids.Add(Memory.Reader.Read<ulong>(IntPtr.Add(new IntPtr(0xBDD060), i * 8)));
                    // Filter out our bags from the item list maintained
                    // by the object manager
                    var tmpItems = ObjectManager.Instance.Items
                        .Where(i => i.Slots != 0
                                    && BagGuids.Contains(i.Guid)).ToList();

                    // iterate over the bag list
                    foreach (var bag in tmpItems)
                    {
                        if ((bag.Name.Contains("Quiver") || bag.Name.Contains("Ammo") || bag.Name.Contains("Shot") ||
                             bag.Name.Contains("Herb") || bag.Name.Contains("Soul")) && !parCountSpecialSlots) continue;
                        // iterate over the current bag and count free slots
                        // i = current slot
                        for (var i = 1; i < bag.Slots + 1; i++)
                        {
                            var tmpSlotGuid = bag.GetDescriptor<ulong>(0xC0 + i * 8);
                            if (tmpSlotGuid == 0) freeSlots++;
                        }
                    }
                    // return the total free slots
                    return freeSlots;
                }
                catch
                {
                    return 16;
                }
            });
        }

        /// <summary>
        /// Gets all items in the inventory
        /// </summary>
        /// <returns>A List of WoWItems</returns>
        public List<WoWItem> GetAllItems()
        {
            List<WoWItem> items = new List<WoWItem>();
            for (int bag = 0; bag < 5; bag++)
            {
                WoWItem bagItem = GetExtraBag(bag - 1);
                if (bag != 0 && bagItem == null)
                {
                    continue;
                }

                for (int slot = 0; slot < (bag == 0 ? 16 : bagItem.Slots); slot++)
                {
                    WoWItem item = GetItem(bag, slot);
                    if (item == null)
                    {
                        continue;
                    }

                    items.Add(item);
                }
            }

            return items;
        }
    }
}
