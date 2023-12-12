using RaidMemberBot.Constants;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game
{
    static public class Inventory
    {
        static public int GetItemCount(string parItemName)
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
                    if (item?.Info.Name == parItemName) totalCount += item.StackCount;
                }
            }
            return totalCount;
        }

        static public int GetItemCount(int itemId)
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
                    if (item?.ItemId == itemId) totalCount += item.StackCount;
                }
            }
            return totalCount;
        }

        static public IList<WoWItem> GetAllItems()
        {
            var items = new List<WoWItem>();
            for (int bag = 0; bag < 5; bag++)
            {
                var container = GetExtraBag(bag - 1);
                if (bag != 0 && container == null)
                {
                    continue;
                }

                for (int slot = 0; slot < (bag == 0 ? 16 : container.Slots); slot++)
                {
                    var item = GetItem(bag, slot);
                    if (item == null)
                    {
                        continue;
                    }

                    items.Add(item);
                }
            }

            return items;
        }

        static public int CountFreeSlots(bool parCountSpecialSlots)
        {
            var freeSlots = 0;
            for (var i = 0; i < 16; i++)
            {
                var tmpSlotGuid = ObjectManager.Player.GetBackpackItemGuid(i);
                if (tmpSlotGuid == 0) freeSlots++;
            }
            var bagGuids = new List<ulong>();
            for (var i = 0; i < 4; i++)
                bagGuids.Add(MemoryManager.ReadUlong(IntPtr.Add((IntPtr)MemoryAddresses.LocalPlayerFirstExtraBag, i * 8)));

            var tmpItems = ObjectManager
                .Containers
                .Where(i => i.Slots != 0 && bagGuids.Contains(i.Guid)).ToList();

            foreach (var bag in tmpItems)
            {
                if ((bag.Info.Name.Contains("Quiver") || bag.Info.Name.Contains("Ammo") || bag.Info.Name.Contains("Shot") ||
                     bag.Info.Name.Contains("Herb") || bag.Info.Name.Contains("Soul")) && !parCountSpecialSlots) continue;

                for (var i = 1; i < bag.Slots; i++)
                {
                    var tmpSlotGuid = bag.GetItemGuid(i);
                    if (tmpSlotGuid == 0) freeSlots++;
                }
            }
            return freeSlots;
        }

        static public int EmptyBagSlots
        {
            get
            {
                var bagGuids = new List<ulong>();
                for (var i = 0; i < 4; i++)
                    bagGuids.Add(MemoryManager.ReadUlong(IntPtr.Add((IntPtr)MemoryAddresses.LocalPlayerFirstExtraBag, i * 8)));

                return bagGuids.Count(b => b == 0);
            }
        }

        static public int GetBagId(ulong itemGuid)
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
                    if (item?.Guid == itemGuid) return i;
                }
            }
            return totalCount;
        }

        static public int GetSlotId(ulong itemGuid)
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
                    if (item?.Guid == itemGuid) return k + 1;
                }
            }
            return totalCount;
        }

        static public WoWItem GetEquippedItem(EquipSlot slot)
        {
            var guid = ObjectManager.Player.GetEquippedItemGuid(slot);
            if (guid == 0) return null;
            return ObjectManager.Items.FirstOrDefault(i => i.Guid == guid);
        }
        static public List<WoWItem> GetEquippedItems()
        {
            WoWItem headItem = GetEquippedItem(EquipSlot.Head);
            WoWItem neckItem = GetEquippedItem(EquipSlot.Neck);
            WoWItem shoulderItem = GetEquippedItem(EquipSlot.Shoulders);
            WoWItem backItem = GetEquippedItem(EquipSlot.Back);
            WoWItem chestItem = GetEquippedItem(EquipSlot.Chest);
            WoWItem shirtItem = GetEquippedItem(EquipSlot.Shirt);
            WoWItem tabardItem = GetEquippedItem(EquipSlot.Tabard);
            WoWItem wristItem = GetEquippedItem(EquipSlot.Wrist);
            WoWItem handsItem = GetEquippedItem(EquipSlot.Hands);
            WoWItem waistItem = GetEquippedItem(EquipSlot.Waist);
            WoWItem legsItem = GetEquippedItem(EquipSlot.Legs);
            WoWItem feetItem = GetEquippedItem(EquipSlot.Feet);
            WoWItem finger1Item = GetEquippedItem(EquipSlot.Finger1);
            WoWItem finger2Item = GetEquippedItem(EquipSlot.Finger2);
            WoWItem trinket1Item = GetEquippedItem(EquipSlot.Trinket1);
            WoWItem trinket2Item = GetEquippedItem(EquipSlot.Trinket2);
            WoWItem mainHandItem = GetEquippedItem(EquipSlot.MainHand);
            WoWItem offHandItem = GetEquippedItem(EquipSlot.OffHand);
            WoWItem rangedItem = GetEquippedItem(EquipSlot.Ranged);

            List<WoWItem> list = new List<WoWItem>();
            if (headItem != null)
            {
               list.Add(headItem);
            }
            if (neckItem != null)
            {
                list.Add(neckItem);
            }
            if (shoulderItem != null)
            {
                list.Add(shoulderItem);
            }
            if (backItem != null)
            {
                list.Add(backItem);
            }
            if (chestItem != null)
            {
                list.Add(chestItem);
            }
            if (shirtItem != null)
            {
                list.Add(shirtItem);
            }
            if (tabardItem != null)
            {
                list.Add(tabardItem);
            }
            if (wristItem != null)
            {
                list.Add(wristItem);
            }
            if (handsItem != null)
            {
                list.Add(handsItem);
            }
            if (waistItem != null)
            {
                list.Add(waistItem);
            }
            if (legsItem != null)
            {
                list.Add(legsItem);
            }
            if (feetItem != null)
            {
                list.Add(feetItem);
            }
            if (finger1Item != null)
            {
                list.Add(finger1Item);
            }
            if (finger2Item != null)
            {
                list.Add(finger2Item);
            }
            if (trinket1Item != null)
            {
                list.Add(trinket1Item);
            }
            if (trinket2Item != null)
            {
                list.Add(trinket2Item);
            }
            if (mainHandItem != null)
            {
                list.Add(mainHandItem);
            }
            if (offHandItem != null)
            {
                list.Add(offHandItem);
            }
            if (rangedItem != null)
            {
                list.Add(rangedItem);
            }
            return list;
        }

        static WoWContainer GetExtraBag(int parSlot)
        {
            if (parSlot > 3 || parSlot < 0) return null;
            var bagGuid = MemoryManager.ReadUlong(IntPtr.Add((IntPtr)MemoryAddresses.LocalPlayerFirstExtraBag, parSlot * 8));
            return bagGuid == 0 ? null : ObjectManager.Containers.FirstOrDefault(i => i.Guid == bagGuid);
        }

        static public WoWItem GetItem(int parBag, int parSlot)
        {
            parBag += 1;
            switch (parBag)
            {
                case 1:
                    ulong itemGuid = 0;
                    if (parSlot < 16 && parSlot >= 0)
                        itemGuid = ObjectManager.Player.GetBackpackItemGuid(parSlot);
                    return itemGuid == 0 ? null : ObjectManager.Items.FirstOrDefault(i => i.Guid == itemGuid);

                case 2:
                case 3:
                case 4:
                case 5:
                    var tmpBag = GetExtraBag(parBag - 2);
                    if (tmpBag == null) return null;
                    var tmpItemGuid = tmpBag.GetItemGuid(parSlot);
                    if (tmpItemGuid == 0) return null;
                    return ObjectManager.Items.FirstOrDefault(i => i.Guid == tmpItemGuid);

                default:
                    return null;
            }
        }
    }
}
