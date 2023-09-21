using BloogBot.Game;
using System;
using System.Text;

namespace BloogBot.Models
{
    public class CharacterEquip : ICloneable
    {
        public CharacterEquip()
        {
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Head) != 0)
            {
                HeadItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Head));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Neck) != 0)
            {
                NeckItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Neck));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Shoulders) != 0)
            {
                ShoulderItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Shoulders));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Back) != 0)
            {
                BackItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Back));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Chest) != 0)
            {
                ChestItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Chest));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Wrist) != 0)
            {
                WristItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Wrist));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Hands) != 0)
            {
                HandItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Hands));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Waist) != 0)
            {
                WaistItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Waist));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Legs) != 0)
            {
                LegItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Legs));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Feet) != 0)
            {
                FeetItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Feet));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Finger1) != 0)
            {
                Finger1Item = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Finger1));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Finger2) != 0)
            {
                Finger2Item = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Finger2));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Trinket1) != 0)
            {
                Trinket1Item = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Trinket1));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Trinket2) != 0)
            {
                Trinket2Item = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Trinket2));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.MainHand) != 0)
            {
                MainHandItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.MainHand));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.OffHand) != 0)
            {
                OffHandItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.OffHand));
            }
            if (ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Ranged) != 0)
            {
                RangedItem = SqliteRepository.GetItemById(ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.Ranged));
            }
            //if (ObjectManager.Player.GetEquippedItemGuid(wManager.Wow.Enums.InventorySlot.INVSLOT_AMMO) != 0)
            //{
            //    AmmoItem = SqliteRepository.GetItemById((int)ObjectManager.Player.GetEquippedItemGuid(Game.Enums.EquipSlot.A));
            //}
        }
        public Item HeadItem { get; set; }
        public Item NeckItem { get; set; }
        public Item ShoulderItem { get; set; }
        public Item BackItem { get; set; }
        public Item ChestItem { get; set; }
        public Item WristItem { get; set; }
        public Item HandItem { get; set; }
        public Item WaistItem { get; set; }
        public Item LegItem { get; set; }
        public Item FeetItem { get; set; }
        public Item Finger1Item { get; set; }
        public Item Finger2Item { get; set; }
        public Item Trinket1Item { get; set; }
        public Item Trinket2Item { get; set; }
        public Item MainHandItem { get; set; }
        public Item OffHandItem { get; set; }
        public Item RangedItem { get; set; }
        public Item AmmoItem { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (HeadItem != null)
            {
                sb.Append("HeadItem: " + HeadItem.Name + " ");
            }
            if (NeckItem != null)
            {
                sb.Append("NeckItem: " + NeckItem.Name + " ");
            }
            if (ShoulderItem != null)
            {
                sb.Append("ShoulderItem: " + ShoulderItem.Name + " ");
            }
            if (BackItem != null)
            {
                sb.Append("BackItem: " + BackItem.Name + " ");
            }
            if (ChestItem != null)
            {
                sb.Append("ChestItem: " + ChestItem.Name + " ");
            }
            if (WristItem != null)
            {
                sb.Append("WristItem: " + WristItem.Name + " ");
            }
            if (HandItem != null)
            {
                sb.Append("HandItem: " + HandItem.Name + " ");
            }
            if (WaistItem != null)
            {
                sb.Append("WaistItem: " + WaistItem.Name + " ");
            }
            if (LegItem != null)
            {
                sb.Append("LegItem: " + LegItem.Name + " ");
            }
            if (FeetItem != null)
            {
                sb.Append("FeetItem: " + FeetItem.Name + " ");
            }
            if (Finger1Item != null)
            {
                sb.Append("Finger1Item: " + Finger1Item.Name + " ");
            }
            if (Finger2Item != null)
            {
                sb.Append("Finger2Item: " + Finger2Item.Name + " ");
            }
            if (Trinket1Item != null)
            {
                sb.Append("Trinket1Item: " + Trinket1Item.Name + " ");
            }
            if (Trinket2Item != null)
            {
                sb.Append("Trinket2Item: " + Trinket2Item.Name + " ");
            }
            if (MainHandItem != null)
            {
                sb.Append("MainHandItem: " + MainHandItem.Name + " ");
            }
            if (OffHandItem != null)
            {
                sb.Append("OffHandItem: " + OffHandItem.Name + " ");
            }
            if (RangedItem != null)
            {
                sb.Append("RangedItem: " + RangedItem.Name + " ");
            }
            if (AmmoItem != null)
            {
                sb.Append("AmmoItem: " + AmmoItem.Name + " ");
            }
            return sb.ToString();
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public float GetGearScore()
        {
            float gearScore = 0;

            if (HeadItem != null)
            {
                gearScore += HeadItem.GetArmorGearScore();
            }
            if (NeckItem != null)
            {
                gearScore += NeckItem.GetArmorGearScore();
            }
            if (ShoulderItem != null)
            {
                gearScore += ShoulderItem.GetArmorGearScore();
            }
            if (BackItem != null)
            {
                gearScore += BackItem.GetArmorGearScore();
            }
            if (ChestItem != null)
            {
                gearScore += ChestItem.GetArmorGearScore();
            }
            if (WristItem != null)
            {
                gearScore += WristItem.GetArmorGearScore();
            }
            if (HandItem != null)
            {
                gearScore += HandItem.GetArmorGearScore();
            }
            if (WaistItem != null)
            {
                gearScore += WaistItem.GetArmorGearScore();
            }
            if (LegItem != null)
            {
                gearScore += LegItem.GetArmorGearScore();
            }
            if (FeetItem != null)
            {
                gearScore += FeetItem.GetArmorGearScore();
            }
            if (Finger1Item != null)
            {
                gearScore += Finger1Item.GetArmorGearScore();
            }
            if (Finger2Item != null)
            {
                gearScore += Finger2Item.GetArmorGearScore();
            }
            if (Trinket1Item != null)
            {
                gearScore += Trinket1Item.GetArmorGearScore();
            }
            if (Trinket2Item != null)
            {
                gearScore += Trinket2Item.GetArmorGearScore();
            }
            if (MainHandItem != null)
            {
                gearScore += MainHandItem.GetWeaponGearScore();
            }
            if (OffHandItem != null)
            {
                // TODO Differentiate based on item type
                gearScore += OffHandItem.GetArmorGearScore();
            }
            if (RangedItem != null)
            {
                gearScore += RangedItem.GetWeaponGearScore();
            }
            if (AmmoItem != null)
            {
                gearScore += AmmoItem.GetWeaponGearScore();
            }

            return gearScore;
        }
    }
}
