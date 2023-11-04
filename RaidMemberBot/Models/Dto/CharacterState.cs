using System.Numerics;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Models.Dto
{
    public class CharacterState
    {
        public int ProcessId { get; set; }
        public string CurrentActivity { get; set; }
        public string RaidLeader { get; set; }
        public string CurrentTask { get; set; }
        public ClassId Class { get; set; }
        public Race Race { get; set; }
        public int Level { get; set; }
        public int MapId { get; set; }
        public string Zone { get; set; }
        public Vector3 Location { get; set; }
        public Vector3 Waypoint { get; set; }
        public ulong Guid { get; set; }
        public float Facing { get; set; }
        public ulong HostileTargetGuid { get; set; }
        public ulong FriendlyTargetGuid { get; set; }
        public string CharacterName { get; set; }
        public int CurrentHealth { get; set; }
        public int CurrentMana { get; set; }
        public int MaxHealth { get; set; }
        public int MaxMana { get; set; }
        public int Rage { get; set; }
        public int Energy { get; set; }
        public int ComboPoints { get; set; }
        public int Casting { get; set; }
        public int Channeling { get; set; }
        public bool IsConnected { get; set; }
        public bool InParty { get; set; }
        public bool InRaid { get; set; }
        public bool InCombat { get; set; }
        public bool IsDiseased { get; set; }
        public bool IsPoisoned { get; set; }
        public bool IsConfused { get; set; }
        public bool IsStunned { get; set; }
        public bool IsMoving { get; set; }
        public bool IsFalling { get; set; }
        public bool IsMounted { get; set; }
    }
}
