namespace WoWActivityMember.Models
{
    public class ActivityMemberPreset
    {
        public int ProcessId { get; set; }
        public string BehaviorProfile { get; set; } = "Protection Warrior";
        public string Account { get; set; } = "HuWr1";
        public string ProgressionConfig { get; set; } = "WarArms1";
        public string InitialStateConfig { get; set; } = "War8";
        public string EndStateConfig { get; set; } = "War8";
    }
}
