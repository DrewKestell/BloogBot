namespace WoWSlimClient.Models
{
    public class WorldStateUpdate
    {
        public int ProcessId { get; set; }
        public ActivityAction ActivityAction { get; set; }
        public string CommandParam1 { get; set; } = string.Empty;
        public string CommandParam2 { get; set; } = string.Empty;
        public string CommandParam3 { get; set; } = string.Empty;
        public string CommandParam4 { get; set; } = string.Empty;
    }

    public enum ActivityAction 
    {
        None,
        AddActivity,
        EditActivity,
        AddActivityMember,
        EditActivityMember,
        SetMinMemberSize,
        SetMaxMemberSize,
        ApplyDesiredState
    }
}
