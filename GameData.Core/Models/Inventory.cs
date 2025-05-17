using GameData.Core.Enums;
using GameData.Core.Interfaces;

namespace GameData.Core.Models
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
                    slots = bag.NumOfSlots;
                }

                for (var k = 0; k <= slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Name == parItemName) totalCount += (int)item.StackCount;
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
                    slots = bag.NumOfSlots;
                }

                for (var k = 0; k <= slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.ItemId == itemId) totalCount += (int)item.StackCount;
                }
            }
            return totalCount;
        }

        static public IList<IWoWItem> GetAllItems()
        {
            var items = new List<IWoWItem>();
            for (int bag = 0; bag < 5; bag++)
            {
                var container = GetExtraBag(bag - 1);
                if (bag != 0 && container == null)
                {
                    continue;
                }

                for (int slot = 0; slot < (bag == 0 ? 16 : container.NumOfSlots); slot++)
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

            return freeSlots;
        }

        static public int EmptyBagSlots
        {
            get
            {
                var bagGuids = new List<ulong>();


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
                    slots = bag.NumOfSlots;
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
                    slots = bag.NumOfSlots;
                }

                for (var k = 0; k < slots; k++)
                {
                    var item = GetItem(i, k);
                    if (item?.Guid == itemGuid) return k + 1;
                }
            }
            return totalCount;
        }

        static public IWoWItem GetEquippedItem(EquipSlot slot)
        {
            return null;
        }
        static public List<IWoWItem> GetEquippedItems()
        {
            IWoWItem headItem = GetEquippedItem(EquipSlot.Head);
            IWoWItem neckItem = GetEquippedItem(EquipSlot.Neck);
            IWoWItem shoulderItem = GetEquippedItem(EquipSlot.Shoulders);
            IWoWItem backItem = GetEquippedItem(EquipSlot.Back);
            IWoWItem chestItem = GetEquippedItem(EquipSlot.Chest);
            IWoWItem shirtItem = GetEquippedItem(EquipSlot.Shirt);
            IWoWItem tabardItem = GetEquippedItem(EquipSlot.Tabard);
            IWoWItem wristItem = GetEquippedItem(EquipSlot.Wrist);
            IWoWItem handsItem = GetEquippedItem(EquipSlot.Hands);
            IWoWItem waistItem = GetEquippedItem(EquipSlot.Waist);
            IWoWItem legsItem = GetEquippedItem(EquipSlot.Legs);
            IWoWItem feetItem = GetEquippedItem(EquipSlot.Feet);
            IWoWItem finger1Item = GetEquippedItem(EquipSlot.Finger1);
            IWoWItem finger2Item = GetEquippedItem(EquipSlot.Finger2);
            IWoWItem trinket1Item = GetEquippedItem(EquipSlot.Trinket1);
            IWoWItem trinket2Item = GetEquippedItem(EquipSlot.Trinket2);
            IWoWItem mainHandItem = GetEquippedItem(EquipSlot.MainHand);
            IWoWItem offHandItem = GetEquippedItem(EquipSlot.OffHand);
            IWoWItem rangedItem = GetEquippedItem(EquipSlot.Ranged);

            List<IWoWItem> list =
            [
                .. headItem != null ? new List<IWoWItem> { headItem } : [],
                .. neckItem != null ? new List<IWoWItem> { neckItem } : [],
                .. shoulderItem != null ? new List<IWoWItem> { shoulderItem } : [],
                .. backItem != null ? new List<IWoWItem> { backItem } : [],
                .. chestItem != null ? new List<IWoWItem> { chestItem } : [],
                .. shirtItem != null ? new List<IWoWItem> { shirtItem } : [],
                .. tabardItem != null ? new List<IWoWItem> { tabardItem } : [],
                .. wristItem != null ? new List<IWoWItem> { wristItem } : [],
                .. handsItem != null ? new List<IWoWItem> { handsItem } : [],
                .. waistItem != null ? new List<IWoWItem> { waistItem } : [],
                .. legsItem != null ? new List<IWoWItem> { legsItem } : [],
                .. feetItem != null ? new List<IWoWItem> { feetItem } : [],
                .. finger1Item != null ? new List<IWoWItem> { finger1Item } : [],
                .. finger2Item != null ? new List<IWoWItem> { finger2Item } : [],
                .. trinket1Item != null ? new List<IWoWItem> { trinket1Item } : [],
                .. trinket2Item != null ? new List<IWoWItem> { trinket2Item } : [],
                .. mainHandItem != null ? new List<IWoWItem> { mainHandItem } : [],
                .. offHandItem != null ? new List<IWoWItem> { offHandItem } : [],
                .. rangedItem != null ? new List<IWoWItem> { rangedItem } : [],
            ];
            return list;
        }

        private static IWoWContainer GetExtraBag(int parSlot)
        {
            return null;
        }

        static public IWoWItem GetItem(int parBag, int parSlot)
        {
            return null;
        }
    }
}
