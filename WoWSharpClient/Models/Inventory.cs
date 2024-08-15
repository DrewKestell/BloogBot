using BotRunner.Constants;

namespace WoWSharpClient.Models
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

        static public IList<Item> GetAllItems()
        {
            var items = new List<Item>();
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

        static public Item GetEquippedItem(EquipSlot slot)
        {
            return null;
        }
        static public List<Item> GetEquippedItems()
        {
            Item headItem = GetEquippedItem(EquipSlot.Head);
            Item neckItem = GetEquippedItem(EquipSlot.Neck);
            Item shoulderItem = GetEquippedItem(EquipSlot.Shoulders);
            Item backItem = GetEquippedItem(EquipSlot.Back);
            Item chestItem = GetEquippedItem(EquipSlot.Chest);
            Item shirtItem = GetEquippedItem(EquipSlot.Shirt);
            Item tabardItem = GetEquippedItem(EquipSlot.Tabard);
            Item wristItem = GetEquippedItem(EquipSlot.Wrist);
            Item handsItem = GetEquippedItem(EquipSlot.Hands);
            Item waistItem = GetEquippedItem(EquipSlot.Waist);
            Item legsItem = GetEquippedItem(EquipSlot.Legs);
            Item feetItem = GetEquippedItem(EquipSlot.Feet);
            Item finger1Item = GetEquippedItem(EquipSlot.Finger1);
            Item finger2Item = GetEquippedItem(EquipSlot.Finger2);
            Item trinket1Item = GetEquippedItem(EquipSlot.Trinket1);
            Item trinket2Item = GetEquippedItem(EquipSlot.Trinket2);
            Item mainHandItem = GetEquippedItem(EquipSlot.MainHand);
            Item offHandItem = GetEquippedItem(EquipSlot.OffHand);
            Item rangedItem = GetEquippedItem(EquipSlot.Ranged);

            List<Item> list =
            [
                .. headItem != null ? new List<Item> { headItem } : [],
                .. neckItem != null ? new List<Item> { neckItem } : [],
                .. shoulderItem != null ? new List<Item> { shoulderItem } : [],
                .. backItem != null ? new List<Item> { backItem } : [],
                .. chestItem != null ? new List<Item> { chestItem } : [],
                .. shirtItem != null ? new List<Item> { shirtItem } : [],
                .. tabardItem != null ? new List<Item> { tabardItem } : [],
                .. wristItem != null ? new List<Item> { wristItem } : [],
                .. handsItem != null ? new List<Item> { handsItem } : [],
                .. waistItem != null ? new List<Item> { waistItem } : [],
                .. legsItem != null ? new List<Item> { legsItem } : [],
                .. feetItem != null ? new List<Item> { feetItem } : [],
                .. finger1Item != null ? new List<Item> { finger1Item } : [],
                .. finger2Item != null ? new List<Item> { finger2Item } : [],
                .. trinket1Item != null ? new List<Item> { trinket1Item } : [],
                .. trinket2Item != null ? new List<Item> { trinket2Item } : [],
                .. mainHandItem != null ? new List<Item> { mainHandItem } : [],
                .. offHandItem != null ? new List<Item> { offHandItem } : [],
                .. rangedItem != null ? new List<Item> { rangedItem } : [],
            ];
            return list;
        }

        private static Container GetExtraBag(int parSlot)
        {
            return null;
        }

        static public Item GetItem(int parBag, int parSlot)
        {
            return null;
        }
    }
}
