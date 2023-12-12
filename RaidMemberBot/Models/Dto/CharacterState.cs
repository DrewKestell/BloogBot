using System.Collections.Generic;
using System.Numerics;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Models.Dto
{
    public class CharacterState
    {
        public int ProcessId { get; set; }
        public string CurrentActivity { get; set; }
        public string RaidLeader { get; set; }
        public ulong RaidLeaderGuid { get; set; }
        public string Task { get; set; }
        public Class Class { get; set; } = Class.Warrior;
        public Race Race { get; set; } = Race.Human;
        public string AccountName { get; set; }
        public string BotProfileName { get; set; }
        public int Level { get; set; }
        public int MapId { get; set; }
        public int WaypointMapId { get; set; }
        public string Zone { get; set; }
        public Vector3 Position { get; set; } = new Vector3();
        public Vector3 Waypoint { get; set; } = new Vector3();
        public ulong Guid { get; set; }
        public float Facing { get; set; }
        public ulong HostileTargetGuid { get; set; }
        public ulong FriendlyTargetGuid { get; set; }
        public string CharacterName { get; set; }
        public int CurrentHealth { get; set; }
        public int CurrentMana { get; set; }
        public int MaxHealth { get; set; }
        public int MaxMana { get; set; }
        public int MaxEnergy { get; set; }
        public int Rage { get; set; }
        public int Energy { get; set; }
        public int ComboPoints { get; set; }
        public int Casting { get; set; }
        public int ChannelingId { get; set; }
        public bool IsConnected { get; set; }
        public bool InParty { get; set; }
        public bool InRaid { get; set; }
        public bool InCombat { get; set; }
        public bool IsDiseased { get; set; }
        public bool IsPoisoned { get; set; }
        public bool IsCursed { get; set; }
        public bool IsConfused { get; set; }
        public bool IsStunned { get; set; }
        public bool IsSleeping { get; set; }
        public bool IsPolymorphed { get; set; }
        public bool IsDead { get; set; }
        public bool IsFleeing { get; set; }
        public bool IsPossessed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsFalling { get; set; }
        public bool IsOnMount { get; set; }
        public bool IsOnTaxi { get; set; }
        public bool IsFlying { get; set; }
        public List<int> SpellList { get; set; } = new List<int>();
        public List<int> SkillList { get; set; } = new List<int>();
        public Dictionary<ulong, string> WoWUnits { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<ulong, string> WoWObjects { get; set; } = new Dictionary<ulong, string>();
    }
}
