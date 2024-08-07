using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class Unit(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.Unit) : GameObject(lowGuid, highGuid, objectType), IWoWUnit
    {
        public int CreatureId { get; internal set; }
        public Race Race { get; internal set; }
        public Class Class { get; internal set; }
        public byte Gender { get; internal set; }

        public IWoWUnit Target { get; internal set; }
        public ulong TargetGuid { get; internal set; }

        public uint Health { get; internal set; }

        public uint MaxHealth { get; internal set; }
        public Dictionary<Powers, uint> Power { get; } = [];
        public Dictionary<Powers, uint> MaxPower { get; } = [];

        public float BaseMeleeRangeOffset { get; internal set; } = 1.33f;

        public float BaseMinDamage { get; internal set; } = 1.0f;

        public float BaseMaxDamage { get; internal set; } = 2.0f;

        public float BaseAttackTime { get; internal set; } = 2000;

        public float BoundingRadius { get; internal set; }

        public float CombatReach { get; internal set; }

        public int ChannelingId { get; internal set; }

        public bool IsChanneling { get; internal set; }

        public int SpellcastId { get; internal set; }

        public bool IsCasting { get; internal set; }

        public DynamicFlags DynamicFlags { get; internal set; }

        public UnitFlags UnitFlags { get; internal set; }

        public ulong SummonedByGuid { get; internal set; }

        public int FactionId { get; internal set; }

        public bool NotAttackable => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_NON_ATTACKABLE);

        public bool IsFacing(Position position) => Math.Abs(GetFacingForPosition(position) - Facing) < 0.05f;

        // in radians
        public float GetFacingForPosition(Position position)
        {
            var f = (float)Math.Atan2(position.Y - Position.Y, position.X - Position.X);
            if (f < 0.0f)
                f += (float)Math.PI * 2.0f;
            else if (f > (float)Math.PI * 2)
                f -= (float)Math.PI * 2.0f;

            return f;
        }

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

        public bool InLosWith(IWoWUnit otherUnit)
        {
            Position newThisPosition = new(Position.X, Position.Y, Position.Z + (Height /2 ));
            Position newOtherPosition = new(otherUnit.Position.X, otherUnit.Position.Y, otherUnit.Position.Z + (otherUnit.Height / 2));

            return newThisPosition.InLosWith(newOtherPosition);
        }

        public bool IsFacing(IWoWUnit position)
        {
            return true;
        }

        public bool InLosWith(Position position)
        {
            return true;
        }

        public MovementFlags MovementFlags { get; internal set; }

        public bool IsMoving => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD);

        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING);

        public bool IsMounted { get; internal set; }

        public CreatureType CreatureType { get; internal set; }

        public UnitReaction UnitReaction { get; internal set; }

        public CreatureRank CreatureRank { get; internal set; }

        public Spell GetSpellById(int spellId)
        {
            // Implementation for getting a spell by its ID
            return null;
        }

        public List<Spell> Buffs { get; internal set; } = [];

        public List<Spell> Debuffs { get; internal set; } = [];

        public ulong SummonGuid { get; internal set; }
        public ulong CharmedByGuid { get; internal set; }
        public ulong ChannelObject { get; internal set; }
        public ulong PersuadedGuid { get; internal set; }
        public ulong CreatedBy { get; internal set; }
        public ulong CharmGuid { get; internal set; }
        public byte[] Bytes0 { get; internal set; }
        public Dictionary<object, object> VirtualItemSlotDisplay { get; internal set; }
        public Dictionary<object, object> VirtualItemInfo { get; internal set; }
        public uint Flags { get; internal set; }
        public Dictionary<object, object> AuraFields { get; internal set; }
        

        public IEnumerable<SpellEffect> GetDebuffs(LuaTarget target)
        {
            return [];
        }

        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);

        public void MoveTo(float x, float y, float z)
        {
            // Implementation for moving the unit to a specific position
        }

        public void StopMoving()
        {
            // Implementation for stopping the unit's movement
        }

        public void CastSpell(Unit target, int spellId, bool triggered)
        {
            // Implementation for casting a spell
        }

        public void InterruptSpell(CurrentSpellTypes spellType, bool withDelayed)
        {
            // Implementation for interrupting a spell
        }

        public void Attack(Unit victim, bool meleeAttack)
        {
            // Implementation for attacking a unit
        }

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
            if (!Power.ContainsKey(powerType))
                Power[powerType] = 0;

            Power[powerType] = value;
            if (Power[powerType] > MaxPower[powerType])
                Power[powerType] = MaxPower[powerType];
        }

        public void SetMaxPower(Powers powerType, uint value)
        {
            if (!MaxPower.ContainsKey(powerType))
                MaxPower[powerType] = 0;

            MaxPower[powerType] = value;
            if (Power[powerType] > MaxPower[powerType])
                Power[powerType] = MaxPower[powerType];
        }

        public IEnumerable<ISpellEffect> GetDebuffs()
        {
            throw new NotImplementedException();
        }

        public bool IsAttacking { get; set; }
        public uint LastUpdated { get; internal set; }

        public uint MountDisplayId => throw new NotImplementedException();
    }
}
