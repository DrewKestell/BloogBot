namespace WoWSlimClient.Models
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
                    if (item?.Name == parItemName) totalCount += item.StackCount;
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
            return null;
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

            List<WoWItem> list =
            [
                .. headItem != null ? new List<WoWItem> { headItem } : [],
                .. neckItem != null ? new List<WoWItem> { neckItem } : [],
                .. shoulderItem != null ? new List<WoWItem> { shoulderItem } : [],
                .. backItem != null ? new List<WoWItem> { backItem } : [],
                .. chestItem != null ? new List<WoWItem> { chestItem } : [],
                .. shirtItem != null ? new List<WoWItem> { shirtItem } : [],
                .. tabardItem != null ? new List<WoWItem> { tabardItem } : [],
                .. wristItem != null ? new List<WoWItem> { wristItem } : [],
                .. handsItem != null ? new List<WoWItem> { handsItem } : [],
                .. waistItem != null ? new List<WoWItem> { waistItem } : [],
                .. legsItem != null ? new List<WoWItem> { legsItem } : [],
                .. feetItem != null ? new List<WoWItem> { feetItem } : [],
                .. finger1Item != null ? new List<WoWItem> { finger1Item } : [],
                .. finger2Item != null ? new List<WoWItem> { finger2Item } : [],
                .. trinket1Item != null ? new List<WoWItem> { trinket1Item } : [],
                .. trinket2Item != null ? new List<WoWItem> { trinket2Item } : [],
                .. mainHandItem != null ? new List<WoWItem> { mainHandItem } : [],
                .. offHandItem != null ? new List<WoWItem> { offHandItem } : [],
                .. rangedItem != null ? new List<WoWItem> { rangedItem } : [],
            ];
            return list;
        }

        private static WoWContainer GetExtraBag(int parSlot)
        {
            return null;
        }

        static public WoWItem GetItem(int parBag, int parSlot)
        {
            return null;
        }
    }
}
