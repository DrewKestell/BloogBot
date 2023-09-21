namespace BloogBot.Models
{
    public class NpcVendorEntry
    {
        public int Entry { get; set; }
        public int Item { get; set; }
        public int MaxCount { get; set; }
        public int IncrTime { get; set; }
        public int Slot { get; set; }
        public int ConditionId { get; set; }
        public string Comments { get; set; }
    }
}
