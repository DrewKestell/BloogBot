namespace RaidMemberBot.Models.Dto
{
    public class CharacterState
    {
        public int ProcessId { get; set; }
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; }
        public string CurrentActivity { get; set; }
        public string BotProfileName { get; set; }
        public string RaidLeader { get; set; }
        public bool IsConnected { get; set; }
        public bool InParty { get; set; }
        public string CurrentTask { get; set; }
        public ulong Guid { get; set; }
        public string CharacterName { get; set; }
        public int MapId { get; set; }
        public string Zone { get; set; }
        public int HealthPercent { get; set; }
        public int ManaPercent { get; set; }
        public int Rage { get; set; }
        public int Energy { get; set; }
        public bool InCombat { get; set; }
        public bool IsCasting { get; set; }
        public bool IsChanneling { get; set; }
        public string Location { get; set; }
    }
}
