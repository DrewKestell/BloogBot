namespace WoWSlimClient.Models
{
    public class ActivityMemberState
    {
        public Guid ServiceId { get; set; } = Guid.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BehaviorProfileName { get; set; } = string.Empty;
        public string InitialGearProfileName { get; set; } = string.Empty;
        public string DesiredGearProfileName { get; set; } = string.Empty;
        public string ProgressionProfileName { get; set; } = string.Empty;
        public List<int> Spells { get; set; } = [];
        public List<int> Skills { get; set; } = [];
        public List<int> Talents { get; set; } = [];
        public List<int> PetSpells { get; set; } = [];
        public List<string> ActivityMembers { get; set; } = [];
    }
}
