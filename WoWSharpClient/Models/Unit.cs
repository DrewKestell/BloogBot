using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class Unit(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Unit) : GameObject(highGuid, objectType), IWoWUnit
    {
        public int CreatureId { get; set; }
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Gender Gender { get; set; }

        public ulong TargetGuid => TargetHighGuid.FullGuid;
        public HighGuid TargetHighGuid { get; set; } = new HighGuid(new byte[4], new byte[4]);

        public uint Health { get; set; }
        public uint MaxHealth { get; set; }
        public Dictionary<Powers, uint> Powers { get; } = [];
        public Dictionary<Powers, uint> MaxPowers { get; } = [];

        public float BaseMeleeRangeOffset { get; set; } = 1.33f;

        public uint MinDamage { get; set; } = 1;
        public uint MaxDamage { get; set; } = 2;
        public uint BaseAttackTime { get; set; } = 2000;
        public uint BaseAttackTime1 { get; set; } = 2000;
        public uint OffhandAttackTime { get; set; } = 2000;
        public uint OffhandAttackTime1 { get; set; } = 2000;
        public uint RangedAttackTime { get; set; } = 2000;
        public float BoundingRadius { get; set; }
        public float CombatReach { get; set; }
        public uint ChannelingId { get; set; }
        public bool IsChanneling { get; set; }
        public int SpellcastId { get; set; }
        public bool IsCasting { get; set; }
        public UnitFlags UnitFlags { get; set; }
        public ulong SummonedByGuid { get; set; }
        public int FactionId { get; set; }
        public bool NotAttackable => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_NON_ATTACKABLE);

        public bool IsBehind(IWoWUnit target)
        {
            if (target == null) return false;

            float facing = GetFacingForPosition(target.Position);

            var halfPi = Math.PI / 2;
            var twoPi = Math.PI * 2;
            var leftThreshold = target.Facing - halfPi;
            var rightThreshold = target.Facing + halfPi;

            bool condition;
            if (leftThreshold < 0)
                condition = facing < rightThreshold || facing > twoPi + leftThreshold;
            else if (rightThreshold > twoPi)
                condition = facing > leftThreshold || facing < rightThreshold - twoPi;
            else
                condition = facing > leftThreshold && facing < rightThreshold;

            return condition;
        }

        public bool IsBehind(Position position, float targetFacing)
        {
            if (position == null) return false;

            float facing = GetFacingForPosition(position);

            var halfPi = Math.PI / 2;
            var twoPi = Math.PI * 2;
            var leftThreshold = targetFacing - halfPi;
            var rightThreshold = targetFacing + halfPi;

            bool condition;
            if (leftThreshold < 0)
                condition = facing < rightThreshold || facing > twoPi + leftThreshold;
            else if (rightThreshold > twoPi)
                condition = facing > leftThreshold || facing < rightThreshold - twoPi;
            else
                condition = facing > leftThreshold && facing < rightThreshold;

            return condition;
        }

        public MovementFlags MovementFlags { get; set; }

        public bool IsMoving => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD);

        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING);

        public bool IsMounted { get; set; }

        public CreatureType CreatureType { get; set; }

        public UnitReaction UnitReaction { get; set; }

        public CreatureRank CreatureRank { get; set; }

        public Spell GetSpellById(int spellId)
        {
            // Implementation for getting a spell by its ID
            return null;
        }

        public List<Spell> Buffs { get; } = [];
        public List<Spell> Debuffs { get; } = [];
        public HighGuid Charm { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Summon { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid CharmedBy { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid SummonedBy { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Persuaded { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid ChannelObject { get; } = new HighGuid(new byte[4], new byte[4]);
        public byte[] Bytes0 { get; } = new byte[4];
        public byte[] Bytes1 { get; } = new byte[4];
        public byte[] Bytes2 { get; } = new byte[4];
        public uint[] VirtualItemSlotDisplay { get; } = new uint[3];
        public uint[] VirtualItemInfo { get; } = new uint[6];
        public uint[] AuraFields { get; } = new uint[48];
        public uint[] AuraFlags { get; } = new uint[6];
        public uint[] AuraLevels { get; } = new uint[12];
        public uint[] AuraApplications { get; } = new uint[12];
        public uint AuraState { get; set; }
        public uint[] Resistances { get; } = new uint[7];
        public uint MountDisplayId { get; set; }
        public uint NativeDisplayId { get; internal set; }
        public uint MinOffhandDamage { get; internal set; }
        public uint MaxOffhandDamage { get; internal set; }
        public uint PetNumber { get; internal set; }
        public uint PetNameTimestamp { get; internal set; }
        public uint PetExperience { get; internal set; }
        public uint PetNextLevelExperience { get; internal set; }
        public float ModCastSpeed { get; internal set; }
        public uint CreatedBySpell { get; internal set; }
        public NPCFlags NpcFlags { get; internal set; }
        public uint NpcEmoteState { get; internal set; }
        public uint TrainingPoints { get; internal set; }
        public uint Strength { get; internal set; }
        public uint Agility { get; internal set; }
        public uint Stamina { get; internal set; }
        public uint Intellect { get; internal set; }
        public uint Spirit { get; internal set; }

        public bool IsAttacking { get; set; }
        public uint LastUpdated { get; set; }
        public uint BaseMana { get; internal set; }
        public uint BaseHealth { get; internal set; }
        public uint[] PowerCostModifers { get; internal set; } = new uint[7];
        public uint[] PowerCostMultipliers { get; internal set; } = new uint[7];
        public uint MaxRangedDamage { get; internal set; }
        public uint MinRangedDamage { get; internal set; }
        public uint AttackPowerMultipler { get; internal set; }
        public uint AttackPowerMods { get; internal set; }
        public uint RangedAttackPowerMultipler { get; internal set; }
        public uint RangedAttackPowerMods { get; internal set; }
        public uint RangedAttackPower { get; internal set; }
        public uint AttackPower { get; internal set; }
        public float FallTime { get; internal set; }
        public float WalkSpeed { get; internal set; }
        public float RunSpeed { get; internal set; }
        public float RunBackSpeed { get; internal set; }
        public float SwimSpeed { get; internal set; }
        public float SwimBackSpeed { get; internal set; }
        public float TurnRate { get; internal set; }

        public NPCFlags NPCFlags => throw new NotImplementedException();

        public IEnumerable<SpellEffect> GetDebuffs(LuaTarget target)
        {
            return [];
        }

        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);

        public void StopAttack()
        {
            // Implementation for stopping an attack
        }

        public void SetFlag(UnitFlags flag)
        {
            UnitFlags |= flag;
        }

        public void RemoveFlag(UnitFlags flag)
        {
            UnitFlags &= ~flag;
        }

        public bool HasFlag(UnitFlags flag) => (UnitFlags & flag) != 0;

        public void SetHealth(uint value)
        {
            Health = value;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public void SetMaxHealth(uint value)
        {
            MaxHealth = value;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public void SetPower(Powers powerType, uint value)
        {
            if (!Powers.ContainsKey(powerType))
                Powers[powerType] = 0;

            Powers[powerType] = value;
            if (Powers[powerType] > MaxPowers[powerType])
                Powers[powerType] = MaxPowers[powerType];
        }

        public void SetMaxPower(Powers powerType, uint value)
        {
            if (!MaxPowers.ContainsKey(powerType))
                MaxPowers[powerType] = 0;

            MaxPowers[powerType] = value;
            if (Powers[powerType] > MaxPowers[powerType])
                Powers[powerType] = MaxPowers[powerType];
        }

        public IEnumerable<ISpellEffect> GetDebuffs()
        {
            throw new NotImplementedException();
        }

        public bool DismissBuff(string buffName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISpellEffect> GetBuffs()
        {
            throw new NotImplementedException();
        }
    }
}
