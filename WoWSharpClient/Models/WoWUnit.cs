using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWUnit(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Unit) : WoWGameObject(highGuid, objectType), IWoWUnit
    {
        public HighGuid TargetHighGuid { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Charm { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Summon { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid CharmedBy { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid SummonedBy { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Persuaded { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid ChannelObject { get; } = new HighGuid(new byte[4], new byte[4]);
        public ulong TargetGuid { get; internal set; }
        public uint Health { get; internal set; }
        public uint MaxHealth { get; internal set; }
        public Dictionary<Powers, uint> Powers { get; internal set; } = [];
        public Dictionary<Powers, uint> MaxPowers { get; internal set; } = [];
        public ulong SummonedByGuid { get; internal set; }
        public uint MountDisplayId { get; internal set; }
        public UnitReaction UnitReaction { get; internal set; }
        public UnitFlags UnitFlags { get; internal set; }
        public NPCFlags NpcFlags { get; internal set; }
        public uint NpcEmoteState { get; internal set; }
        public MovementFlags MovementFlags { get; internal set; }
        public uint MovementFlags2 { get; internal set; }
        public CreatureType CreatureType { get; internal set; }

        public float FallTime { get; set; }
        public float WalkSpeed { get; set; }
        public float RunSpeed { get; set; }
        public float RunBackSpeed { get; set; }
        public float SwimSpeed { get; set; }
        public float SwimBackSpeed { get; set; }
        public float TurnRate { get; set; }
        public uint[] Bytes0 { get; } = new uint[4];
        public uint[] VirtualItemSlotDisplay { get; } = new uint[3];
        public uint[] VirtualItemInfo { get; } = new uint[6];
        public uint[] AuraFields { get; } = new uint[48];
        public uint[] AuraFlags { get; } = new uint[6];
        public uint[] AuraLevels { get; } = new uint[12];
        public uint[] AuraApplications { get; } = new uint[12];
        public uint AuraState { get; set; }
        public uint BaseAttackTime { get; set; }
        public uint BaseAttackTime1 { get; set; }
        public uint OffhandAttackTime { get; set; }
        public uint OffhandAttackTime1 { get; set; }
        public float BoundingRadius { get; set; }
        public float CombatReach { get; set; }
        public uint NativeDisplayId { get; set; }
        public uint MinDamage { get; set; }
        public uint MaxDamage { get; set; }
        public uint MinOffhandDamage { get; set; }
        public uint MaxOffhandDamage { get; set; }
        public uint[] Bytes1 { get; } = new uint[4];
        public uint PetNumber { get; set; }
        public uint PetNameTimestamp { get; set; }
        public uint PetExperience { get; set; }
        public uint PetNextLevelExperience { get; set; }
        public uint ChannelingId { get; set; }
        public float ModCastSpeed { get; set; }
        public uint CreatedBySpell { get; set; }
        public uint NPCEmoteState { get; set; }
        public uint TrainingPoints { get; set; }
        public uint Strength { get; set; }
        public uint Agility { get; set; }
        public uint Stamina { get; set; }
        public uint Intellect { get; set; }
        public uint Spirit { get; set; }
        public uint[] Resistances { get; } = new uint[7];
        public uint BaseMana { get; set; }
        public uint BaseHealth { get; set; }
        public uint[] Bytes2 { get; } = new uint[4];
        public uint AttackPower { get; set; }
        public uint AttackPowerMods { get; set; }
        public uint AttackPowerMultipler { get; set; }
        public uint RangedAttackPower { get; set; }
        public uint RangedAttackPowerMods { get; set; }
        public uint RangedAttackPowerMultipler { get; set; }
        public uint MinRangedDamage { get; set; }
        public uint MaxRangedDamage { get; set; }
        public uint[] PowerCostModifers { get; } = new uint[7];
        public uint[] PowerCostMultipliers { get; } = new uint[7];

        public bool DismissBuff(string buffName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISpellEffect> GetBuffs()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISpellEffect> GetDebuffs()
        {
            throw new NotImplementedException();
        }

        public float GetFacingForPosition(Position targetPos)
        {
            throw new NotImplementedException();
        }

        public List<Spell> Buffs { get; } = [];
        public List<Spell> Debuffs { get; } = [];
        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);
    }
}
