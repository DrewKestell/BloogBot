namespace WoWClientBot.Models
{
    public class CharacterConfig
    {
        public int Level { get; set; }
        public bool IsMainTank { get; set; }
        public bool IsOffTank { get; set; }
        public bool IsMainHealer { get; set; }
        public bool IsOffHealer { get; set; }
        public bool ShouldCleanse { get; set; }
        public bool ShouldRebuff { get; set; }
        public bool IsRole1 { get; set; }
        public bool IsRole2 { get; set; }
        public bool IsRole3 { get; set; }
        public bool IsRole4 { get; set; }
        public bool IsRole5 { get; set; }
        public bool IsRole6 { get; set; }
        public List<int> Spells { get; set; } = [];
        public List<int> Skills { get; set; } = [];
        public List<int> Talents { get; set; } = [];
        public List<int> PetSpells { get; set; } = [];
        public int HeadItem { get; set; }
        public int NeckItem { get; set; }
        public int ShoulderItem { get; set; }
        public int ChestItem { get; set; }
        public int BackItem { get; set; }
        public int ShirtItem { get; set; }
        public int TabardItem { get; set; }
        public int WristsItem { get; set; }
        public int HandsItem { get; set; }
        public int WaistItem { get; set; }
        public int LegsItem { get; set; }
        public int FeetItem { get; set; }
        public int Finger1Item { get; set; }
        public int Finger2Item { get; set; }
        public int Trinket1Item { get; set; }
        public int Trinket2Item { get; set; }
        public int MainHandItem { get; set; }
        public int OffHandItem { get; set; }
        public int RangedItem { get; set; }
    }
}
