using MaNGOSDBDomain.Models;
using Newtonsoft.Json;
using System.Numerics;
using WoWClientBot.Objects;
using static WoWClientBot.Constants.Enums;

namespace WoWClientBot.Models
{
    public class CharacterState
    {
        public int ProcessId { get; set; }
        public bool ShouldRun { get; set; }
        public string ActivityAddress { get; set; } = "127.0.0.1";
        public string DatabaseAddress { get; set; } = "127.0.0.1";
        public int ActivityPort { get; set; }
        public int DatabasePort { get; set; }
        public string CurrentActivity { get; set; } = string.Empty;
        public string RaidLeader { get; set; } = string.Empty;
        public ulong RaidLeaderGuid { get; set; }
        public string Action { get; set; } = string.Empty;
        public Class Class { get; set; } = Class.Warrior;
        public Race Race { get; set; } = Race.Human;
        public string AccountName { get; set; } = string.Empty;
        public string BotProfileName { get; set; } = string.Empty;
        public bool IsAlliance => Race == Race.Human || Race == Race.Dwarf || Race == Race.Gnome || Race == Race.NightElf;
        public int Level { get; set; }
        public int MapId { get; set; }
        public string Zone { get; set; } = string.Empty;
        public ulong Guid { get; set; }
        public Vector3 Position { get; set; }
        public float Facing { get; set; }
        public Vector3 TankPosition { get; set; }
        public float TankFacing { get; set; }
        public string CharacterName { get; set; } = string.Empty;
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
        public bool IsReadyToStart { get; set; }
        public bool HasStarted { get; set; }
        public bool TankInPosition { get; set; }
        public bool IsReset { get; set; }
        public bool IsDone { get; set; }
        public bool InParty { get; set; }
        public bool InRaid { get; set; }
        public bool InBGQueue { get; set; }
        public bool InCombat { get; set; }
        public ulong TargetGuid { get; set; }
        public string TargetPointer { get; set; } = string.Empty;
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
        public CharacterConfig CharacterConfig { get; set; } = new();
        public Dictionary<ulong, string> WoWUnits { get; set; } = [];
        public Dictionary<ulong, string> WoWObjects { get; set; } = [];
        [JsonIgnore]
        public List<Position> VisitedWaypoints { get; } = [];
        [JsonIgnore]
        public Position DungeonStart { get; set; } = new Position(0, 0, 0);
        [JsonIgnore]
        public List<List<Creature>> Encounters { get; } = [];
    }
}
