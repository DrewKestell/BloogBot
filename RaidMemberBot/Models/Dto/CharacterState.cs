namespace RaidMemberBot.Models.Dto
{
    public class CharacterState
    {
        public int ProcessId { get; set; }
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; }
        public string CharacterName { get; set; }
        public string CurrentActivity { get; set; }
        public string BotProfileName { get; set; }
        public string RaidLeader { get; set; }
        public bool ShouldRun { get; set; }
        public bool IsConnected { get; set; }
        public bool IsRunning { get; set; }
        public bool InParty { get; set; }
        public bool SetAccountInfoRequested { get; set; }
        public bool StartRequested { get; set; }
        public bool StopRequested { get; set; }
        public ulong Guid { get; set; }
        public string Zone { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }
        public int Rage { get; set; }
        public int Energy { get; set; }
        public bool IsCasting { get; set; }
        public bool IsChanneling { get; set; }
        public string CurrentTask { get; set; }
        public string Location { get; set; }
        public ulong TargetGuid { get; set; }
    }
    public enum CharacterAction : byte
    {
        Still,
        Moving,
        Attacking = 4,
        Casting = 8,
        Crafting = 16,
        Interacting = 32,
        UsingItem = 64,
    }
}
