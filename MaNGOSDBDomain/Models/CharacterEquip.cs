using System.Text;

namespace MaNGOSDBDomain.Models
{
    public class CharacterEquip : ICloneable
    {
        public CharacterEquip()
        {
        }
        public ItemTemplate HeadItem { get; set; }
        public ItemTemplate NeckItem { get; set; }
        public ItemTemplate ShoulderItem { get; set; }
        public ItemTemplate BackItem { get; set; }
        public ItemTemplate ChestItem { get; set; }
        public ItemTemplate WristItem { get; set; }
        public ItemTemplate HandItem { get; set; }
        public ItemTemplate WaistItem { get; set; }
        public ItemTemplate LegItem { get; set; }
        public ItemTemplate FeetItem { get; set; }
        public ItemTemplate Finger1Item { get; set; }
        public ItemTemplate Finger2Item { get; set; }
        public ItemTemplate Trinket1Item { get; set; }
        public ItemTemplate Trinket2Item { get; set; }
        public ItemTemplate MainHandItem { get; set; }
        public ItemTemplate OffHandItem { get; set; }
        public ItemTemplate RangedItem { get; set; }
        public ItemTemplate AmmoItem { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new();

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
