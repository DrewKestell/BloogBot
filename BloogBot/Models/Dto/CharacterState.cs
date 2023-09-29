using BloogBot.Game;
using BloogBot.Game.Enums;

namespace BloogBot.Models.Dto
{
    public class CharacterState
    {
        public int ProcessId { get; set; }
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; }
        public string CharacterName { get; set; }
        public string CurrentActivity { get; set; }
        public string BotProfileName { get; set; }
        public bool ShouldRun { get; set; }
        public bool IsConnected { get; set; }
        public bool IsRunning { get; set; }
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
        public Position Position { get; set; }
        public string TargetGuid { get; set; }
        public Position TargetPosition { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public Class TargetClass { get; set; }
        public CreatureType TargetCreatureType { get; set; }
        public int TargetHealth { get; set; }
        public int TargetMana { get; set; }
        public int TargetRage { get; set; }
        public int TargetEnergy { get; set; }
        public string TargetFactionId { get; set; }
        public bool TargetIsCasting { get; set; }
        public bool TargetIsChanneling { get; set; }
        public CharacterAction CurrentAction { get; set; }
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
