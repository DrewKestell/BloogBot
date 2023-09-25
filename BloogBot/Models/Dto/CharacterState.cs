using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Models.Enums;
using Newtonsoft.Json;
using System;

namespace BloogBot.Models.Dto
{
    public class CharacterState
    {
        public CharacterState()
        {

        }
        public CharacterState(Action killswitch)
        {
            Killswitch = killswitch;
        }

        [JsonIgnore]
        public Action Killswitch { get; }
        public int ProcessId { get; set; }
        public bool IsRunning { get; set; }
        public bool LoginRequested { get; set; }
        public ulong Guid { get; set; }
        public string Zone { get; set; }
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Role Role { get; set; }
        public short Health { get; set; }
        public short Mana { get; set; }
        public byte Rage { get; set; }
        public byte Energy { get; set; }
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
        public short TargetHealth { get; set; }
        public short TargetMana { get; set; }
        public byte TargetRage { get; set; }
        public byte TargetEnergy { get; set; }
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
