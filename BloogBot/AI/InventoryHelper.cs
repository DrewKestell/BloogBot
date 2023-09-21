using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Models;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public class InventoryHelper
    {
        public static float GetGearScoreWithItem(Item item)
        {
            CharacterEquip characterEquip = new CharacterEquip();

            switch (item.InventoryType)
            {
                case (int)InventoryType.Head:
                    characterEquip.HeadItem = item;
                    break;
                case (int)InventoryType.Neck:
                    characterEquip.NeckItem = item;
                    break;
                case (int)InventoryType.Cloak:
                    characterEquip.BackItem = item;
                    break;
                case (int)InventoryType.Shoulders:
                    characterEquip.ShoulderItem = item;
                    break;
                case (int)InventoryType.Chest:
                    characterEquip.ChestItem = item;
                    break;
                case (int)InventoryType.Waist:
                    characterEquip.WaistItem = item;
                    break;
                case (int)InventoryType.Legs:
                    characterEquip.LegItem = item;
                    break;
                case (int)InventoryType.Feet:
                    characterEquip.FeetItem = item;
                    break;
                case (int)InventoryType.Wrists:
                    characterEquip.WristItem = item;
                    break;
                case (int)InventoryType.Hands:
                    characterEquip.HandItem = item;
                    break;
                case (int)InventoryType.Finger:
                    characterEquip.Finger1Item = item;
                    break;
                case (int)InventoryType.Trinket:
                    characterEquip.Trinket1Item = item;
                    break;
                case (int)InventoryType.Weapon:
                case (int)InventoryType.MainHand:
                case (int)InventoryType.TwoHander:
                    characterEquip.MainHandItem = item;
                    break;
                case (int)InventoryType.Offhand:
                case (int)InventoryType.Shield:
                case (int)InventoryType.Holdable:
                    if (characterEquip.MainHandItem.ItemClass != ItemClass.AxeTwoHand
                        && characterEquip.MainHandItem.ItemClass != ItemClass.MaceTwoHand
                        && characterEquip.MainHandItem.ItemClass != ItemClass.SwordTwoHand
                        && characterEquip.MainHandItem.ItemClass != ItemClass.Polearm
                        && characterEquip.MainHandItem.ItemClass != ItemClass.Spear
                        && characterEquip.MainHandItem.ItemClass != ItemClass.Staff)
                    {
                        characterEquip.OffHandItem = item;
                    }
                    break;
                case (int)InventoryType.RangedRight:
                case (int)InventoryType.Ranged:
                case (int)InventoryType.Relic:
                    characterEquip.RangedItem = item;
                    break;
                case (int)InventoryType.Thrown:
                case (int)InventoryType.Ammo:
                    characterEquip.AmmoItem = item;
                    break;
            }

            return characterEquip.GetGearScore();
        }
        public static void UpgradeToLatestEquipment()
        {
            List<Item> allItems = new List<Item>();

            foreach(EquipSlot equipSlot in Enum.GetValues(typeof(EquipSlot)))
            {
                ulong itemGuid = ObjectManager.Player.GetEquippedItemGuid(equipSlot);
                allItems.Add(SqliteRepository.GetItemById(itemGuid));
            }

            CharacterEquip bestEquip = FindBestGearSetup(allItems);

            EquipItemsInGearSetup(bestEquip);
        }

        private static CharacterEquip FindBestGearSetup(List<Item> allItems)
        {
            List<Item> headItems = new List<Item>();
            List<Item> neckItems = new List<Item>();
            List<Item> shoulderItems = new List<Item>();
            List<Item> cloakItems = new List<Item>();
            List<Item> chestItems = new List<Item>();
            List<Item> wristItems = new List<Item>();
            List<Item> handsItems = new List<Item>();
            List<Item> waistItems = new List<Item>();
            List<Item> legsItems = new List<Item>();
            List<Item> feetItems = new List<Item>();
            List<Item> fingerItems = new List<Item>();
            List<Item> trinketItems = new List<Item>();
            List<Item> relicItems = new List<Item>();
            List<Item> oneHandedItems = new List<Item>();
            List<Item> mainHandItems = new List<Item>();
            List<Item> twoHandedItems = new List<Item>();
            List<Item> shieldItems = new List<Item>();
            List<Item> offHandItems = new List<Item>();
            List<Item> holdableItems = new List<Item>();
            List<Item> rangedItems = new List<Item>();
            List<Item> thrownItems = new List<Item>();
            List<Item> ammoItems = new List<Item>();

            foreach (Item item in allItems)
            {
                if (item.RequiredLevel <= ObjectManager.Player.Level && item.CanEquip)
                {
                    switch (item.InventoryType)
                    {
                        case (int)InventoryType.Head:
                            headItems.Add(item);
                            break;
                        case (int)InventoryType.Neck:
                            neckItems.Add(item);
                            break;
                        case (int)InventoryType.Shoulders:
                            shoulderItems.Add(item);
                            break;
                        case (int)InventoryType.Chest:
                            chestItems.Add(item);
                            break;
                        case (int)InventoryType.Waist:
                            waistItems.Add(item);
                            break;
                        case (int)InventoryType.Legs:
                            legsItems.Add(item);
                            break;
                        case (int)InventoryType.Feet:
                            feetItems.Add(item);
                            break;
                        case (int)InventoryType.Wrists:
                            wristItems.Add(item);
                            break;
                        case (int)InventoryType.Hands:
                            handsItems.Add(item);
                            break;
                        case (int)InventoryType.Finger:
                            fingerItems.Add(item);
                            break;
                        case (int)InventoryType.Trinket:
                            trinketItems.Add(item);
                            break;
                        case (int)InventoryType.Weapon:
                            oneHandedItems.Add(item);
                            break;
                        case (int)InventoryType.Shield:
                            shieldItems.Add(item);
                            break;
                        case (int)InventoryType.Ranged:
                            rangedItems.Add(item);
                            break;
                        case (int)InventoryType.Cloak:
                            cloakItems.Add(item);
                            break;
                        case (int)InventoryType.TwoHander:
                            twoHandedItems.Add(item);
                            break;
                        case (int)InventoryType.MainHand:
                            mainHandItems.Add(item);
                            break;
                        case (int)InventoryType.Offhand:
                            offHandItems.Add(item);
                            break;
                        case (int)InventoryType.Holdable:
                            holdableItems.Add(item);
                            break;
                        case (int)InventoryType.Ammo:
                            ammoItems.Add(item);
                            break;
                        case (int)InventoryType.Thrown:
                            thrownItems.Add(item);
                            break;
                        case (int)InventoryType.RangedRight:
                            rangedItems.Add(item);
                            break;
                        case (int)InventoryType.Relic:
                            relicItems.Add(item);
                            break;
                    }
                }
            }

            List<CharacterEquip> characterEquips = PermutateEquips(new List<CharacterEquip>(), headItems);
            characterEquips = PermutateEquips(characterEquips, neckItems);
            characterEquips = PermutateEquips(characterEquips, shoulderItems);
            characterEquips = PermutateEquips(characterEquips, chestItems);
            characterEquips = PermutateEquips(characterEquips, wristItems);
            characterEquips = PermutateEquips(characterEquips, handsItems);
            characterEquips = PermutateEquips(characterEquips, waistItems);
            characterEquips = PermutateEquips(characterEquips, legsItems);
            characterEquips = PermutateEquips(characterEquips, feetItems);
            characterEquips = PermutateEquips(characterEquips, fingerItems);
            characterEquips = PermutateEquips(characterEquips, trinketItems);
            characterEquips = PermutateEquips(characterEquips, twoHandedItems);
            characterEquips = PermutateEquips(characterEquips, mainHandItems);
            characterEquips = PermutateEquips(characterEquips, oneHandedItems);
            characterEquips = PermutateEquips(characterEquips, shieldItems);
            characterEquips = PermutateEquips(characterEquips, offHandItems);
            characterEquips = PermutateEquips(characterEquips, holdableItems);
            characterEquips = PermutateEquips(characterEquips, rangedItems);
            characterEquips = PermutateEquips(characterEquips, relicItems);
            characterEquips = PermutateEquips(characterEquips, thrownItems);
            characterEquips = PermutateEquips(characterEquips, ammoItems);

            CharacterEquip bestEquip = new CharacterEquip();

            if (characterEquips.Count > 0)
            {
                bestEquip = characterEquips[0];

                foreach (CharacterEquip characterEquip in characterEquips)
                {
                    if (characterEquip.GetGearScore() > bestEquip.GetGearScore())
                    {
                        bestEquip = characterEquip;
                    }
                }
            }

            return bestEquip;
        }

        private static void EquipItemsInGearSetup(CharacterEquip bestEquip)
        {
            if (bestEquip.HeadItem != null && (int)(int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Head) != bestEquip.HeadItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.HeadItem.Name);
            }
            if (bestEquip.NeckItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Neck) != bestEquip.NeckItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.NeckItem.Name);
            }
            if (bestEquip.ShoulderItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Shoulders) != bestEquip.ShoulderItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.ShoulderItem.Name);
            }
            if (bestEquip.BackItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Back) != bestEquip.BackItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.BackItem.Name);
            }
            if (bestEquip.ChestItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Chest) != bestEquip.ChestItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.ChestItem.Name);
            }
            if (bestEquip.WristItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Head) != bestEquip.WristItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.WristItem.Name);
            }
            if (bestEquip.HandItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Head) != bestEquip.HandItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.HandItem.Name);
            }
            if (bestEquip.WaistItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Head) != bestEquip.WaistItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.WaistItem.Name);
            }
            if (bestEquip.LegItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Head) != bestEquip.LegItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.LegItem.Name);
            }
            if (bestEquip.FeetItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Head) != bestEquip.FeetItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.FeetItem.Name);
            }
            if (bestEquip.Finger1Item != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Finger1) != bestEquip.Finger1Item.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.Finger1Item.Name);
            }
            if (bestEquip.Finger2Item != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Finger2) != bestEquip.Finger2Item.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.Finger2Item.Name);
            }
            if (bestEquip.Trinket1Item != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Trinket1) != bestEquip.Trinket1Item.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.Trinket1Item.Name);
            }
            if (bestEquip.Trinket2Item != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Trinket2) != bestEquip.Trinket2Item.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.Trinket2Item.Name);
            }
            if (bestEquip.MainHandItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.MainHand) != bestEquip.MainHandItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.MainHandItem.Name);
            }
            if (bestEquip.OffHandItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.OffHand) != bestEquip.OffHandItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.OffHandItem.Name);
            }
            if (bestEquip.RangedItem != null && (int)ObjectManager.Player.GetEquippedItemGuid(EquipSlot.Ranged) != bestEquip.RangedItem.ItemId)
            {
                ObjectManager.Player.EquipItemByName(bestEquip.RangedItem.Name);
            }
        }

        public CharacterEquip GetBestEquipmentSetup()
        {
            CharacterEquip bestEquip = new CharacterEquip();

            return bestEquip;
        }

        private static List<CharacterEquip> PermutateEquips(List<CharacterEquip> characterEquips, List<Item> slotItems)
        {
            if (slotItems.Count == 0)
            {
                return characterEquips;
            }

            List<CharacterEquip> newCharacterEquips = new List<CharacterEquip>();

            if (characterEquips.Count > 0)
            {
                foreach (CharacterEquip characterEquip in characterEquips)
                {
                    foreach (Item item in slotItems)
                    {
                        CharacterEquip newCharacterEquip = ApplyItemToEquipmentBuild(characterEquip, item);

                        newCharacterEquips.Add(newCharacterEquip);
                    }
                }
            }
            else
            {
                foreach (Item item in slotItems)
                {
                    CharacterEquip newCharacterEquip = ApplyItemToEquipmentBuild(new CharacterEquip(), item);

                    newCharacterEquips.Add(newCharacterEquip);
                }
            }

            return newCharacterEquips;
        }

        private static CharacterEquip ApplyItemToEquipmentBuild(CharacterEquip characterEquip, Item item)
        {
            CharacterEquip newCharacterEquip = (CharacterEquip)characterEquip.Clone();

            switch (item.InventoryType)
            {
                case (int)InventoryType.Head:
                    newCharacterEquip.HeadItem = item;
                    break;
                case (int)InventoryType.Neck:
                    newCharacterEquip.NeckItem = item;
                    break;
                case (int)InventoryType.Shoulders:
                    newCharacterEquip.ShoulderItem = item;
                    break;
                case (int)InventoryType.Cloak:
                    newCharacterEquip.BackItem = item;
                    break;
                case (int)InventoryType.Chest:
                    newCharacterEquip.ChestItem = item;
                    break;
                case (int)InventoryType.Waist:
                    newCharacterEquip.WaistItem = item;
                    break;
                case (int)InventoryType.Legs:
                    newCharacterEquip.LegItem = item;
                    break;
                case (int)InventoryType.Feet:
                    newCharacterEquip.FeetItem = item;
                    break;
                case (int)InventoryType.Wrists:
                    newCharacterEquip.WristItem = item;
                    break;
                case (int)InventoryType.Hands:
                    newCharacterEquip.HandItem = item;
                    break;
                case (int)InventoryType.Finger:
                    newCharacterEquip.Finger1Item = item;
                    break;
                case (int)InventoryType.Trinket:
                    newCharacterEquip.Trinket1Item = item;
                    break;
                case (int)InventoryType.Shield:
                case (int)InventoryType.Offhand:
                    if (newCharacterEquip.MainHandItem.InventoryType != (int)InventoryType.TwoHander)
                    {
                        newCharacterEquip.OffHandItem = item;
                    }
                    break;
                case (int)InventoryType.Ranged:
                case (int)InventoryType.RangedRight:
                case (int)InventoryType.Relic:
                    newCharacterEquip.RangedItem = item;
                    break;
                case (int)InventoryType.TwoHander:
                    newCharacterEquip.MainHandItem = item;
                    newCharacterEquip.OffHandItem = null;
                    break;
                case (int)InventoryType.Weapon:
                case (int)InventoryType.MainHand:
                    newCharacterEquip.MainHandItem = item;
                    break;
                case (int)InventoryType.Holdable:
                    newCharacterEquip.OffHandItem = item;
                    break;
                case (int)InventoryType.Thrown:
                case (int)InventoryType.Ammo:
                    newCharacterEquip.AmmoItem = item;
                    break;
            }

            return newCharacterEquip;
        }

        public static int GetBestQuestReward(QuestTask task)
        {
            int rewardChoice = 1;
            CharacterEquip characterEquip = new CharacterEquip();

            if (task.RewardItem2 != null)
            {
                if (task.RewardItem2.CanEquip && GetGearScoreWithItem(task.RewardItem2) > characterEquip.GetGearScore())
                {
                    rewardChoice = 2;
                }
                if (task.RewardItem3 != null)
                {
                    if (task.RewardItem3.CanEquip && GetGearScoreWithItem(task.RewardItem3) > characterEquip.GetGearScore())
                    {
                        rewardChoice = 3;
                    }
                    if (task.RewardItem4 != null)
                    {
                        if (task.RewardItem4.CanEquip && GetGearScoreWithItem(task.RewardItem4) > characterEquip.GetGearScore())
                        {
                            rewardChoice = 4;
                        }
                        if (task.RewardItem5 != null)
                        {
                            if (task.RewardItem5.CanEquip && GetGearScoreWithItem(task.RewardItem5) > characterEquip.GetGearScore())
                            {
                                rewardChoice = 5;
                            }
                            if (task.RewardItem6 != null)
                            {
                                if (task.RewardItem6.CanEquip && GetGearScoreWithItem(task.RewardItem6) > characterEquip.GetGearScore())
                                {
                                    rewardChoice = 6;
                                }
                            }
                        }
                    }
                }
            }

            return rewardChoice;
        }
    }
}
