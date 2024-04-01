using Newtonsoft.Json;
using RaidMemberBot.Constants;
using RaidMemberBot.Objects;
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
        public string Action { get; set; }
        public Class Class { get; set; } = Class.Warrior;
        public Race Race { get; set; } = Race.Human;
        public string AccountName { get; set; }
        public string BotProfileName { get; set; }
        public int Level { get; set; }
        public int MapId { get; set; }
        public string Zone { get; set; }
        public ulong Guid { get; set; }
        public Vector3 Position { get; set; }
        public float Facing { get; set; }
        public Vector3 TankPosition { get; set; }
        public float TankFacing { get; set; }
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
        public bool IsReadyToStart { get; set; }
        public bool IsReset { get; set; }
        public bool IsDone { get; set; }
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
        public bool IsMainTank { get; set; }
        public bool IsOffTank { get; set; }
        public bool IsMainHealer { get; set; }
        public bool IsOffHealer { get; set; }
        public bool ShouldCleanse { get; set; }
        public bool ShouldRebuff { get; set; }
        public bool IsRole1 { get; set; }
        public bool IsRole2 { get; set; }
        public bool IsRole3 { get; set; }
        public bool IsRole4 { get; set; }
        public bool IsRole5 { get; set; }
        public bool IsRole6 { get; set; }
        public int HeadItem { get; set; }
        public int NeckItem { get; set; }
        public int ShoulderItem { get; set; }
        public int ChestItem { get; set; }
        public int BackItem { get; set; }
        public int ShirtItem { get; set; }
        public int Tabardtem { get; set; }
        public int WristsItem { get; set; }
        public int HandsItem { get; set; }
        public int WaistItem { get; set; }
        public int LegsItem { get; set; }
        public int FeetItem { get; set; }
        public int Finger1Item { get; set; }
        public int Finger2Item { get; set; }
        public int Trinket1Item { get; set; }
        public int Trinket2Item { get; set; }
        public int MainHandItem { get; set; }
        public int OffHandItem { get; set; }
        public int RangedItem { get; set; }
        public List<int> Spells { get; set; } = new List<int>();
        public List<int> Skills { get; set; } = new List<int>();
        public List<int> Talents { get; set; } = new List<int>();
        public List<int> PetSpells { get; set; } = new List<int>();
        public Dictionary<ulong, string> WoWUnits { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<ulong, string> WoWObjects { get; set; } = new Dictionary<ulong, string>();
        [JsonIgnore]
        public List<Position> VisitedWaypoints { get; } = new List<Position>();
        [JsonIgnore]
        public Position DungeonStart { get; set; }
    }
}
