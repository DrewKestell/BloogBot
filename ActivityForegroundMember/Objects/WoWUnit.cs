using ActivityForegroundMember.Mem;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using PathfindingService.Models;
using Functions = ActivityForegroundMember.Mem.Functions;
using ObjectManager = ActivityForegroundMember.Game.Statics.ObjectManager;

namespace ActivityForegroundMember.Objects
{
    public class WoWUnit : WoWObject, IWoWUnit
    {
        private static readonly string[] ImmobilizedSpellText = ["Immobilized"];

        public WoWUnit() { }

        public WoWUnit(
            nint pointer,
            ulong guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public int CreatureId => int.Parse(Guid.ToString("X").Substring(10, 6), System.Globalization.NumberStyles.HexNumber);

        public ulong TargetGuid => MemoryManager.ReadUlong(GetDescriptorPtr() + MemoryAddresses.WoWUnit_TargetGuidOffset);

        public uint Health => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_HealthOffset);

        public uint MaxHealth => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_MaxHealthOffset);

        public uint Mana => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_ManaOffset);

        public uint MaxMana => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_MaxManaOffset);

        public uint Rage => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_RageOffset) / 10;

        public uint Energy => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_EnergyOffset);

        public float BoundingRadius => MemoryManager.ReadFloat(nint.Add(GetDescriptorPtr(), MemoryAddresses.WoWUnit_BoundingRadiusOffset));

        public float CombatReach => MemoryManager.ReadFloat(nint.Add(GetDescriptorPtr(), MemoryAddresses.WoWUnit_CombatReachOffset));

        public uint ChannelingId => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_CurrentChannelingOffset);

        public bool IsChanneling => ChannelingId > 0;

        public uint SpellcastId => MemoryManager.ReadUint(Pointer + MemoryAddresses.WoWUnit_CurrentSpellcastOffset);

        public bool IsCasting => SpellcastId > 0;

        public uint Level => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_LevelOffset);

        public DynamicFlags DynamicFlags => (DynamicFlags)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_DynamicFlagsOffset);

        public bool CanBeLooted => Health == 0 && DynamicFlags.HasFlag(DynamicFlags.CanBeLooted);

        public bool TappedByOther => DynamicFlags.HasFlag(DynamicFlags.Tapped) && !DynamicFlags.HasFlag(DynamicFlags.TappedByMe);

        public UnitFlags UnitFlags => (UnitFlags)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_UnitFlagsOffset);

        public bool IsInCombat => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);

        public bool IsStunned => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_STUNNED);

        public bool IsConfused => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_CONFUSED);

        public bool IsFleeing => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_FLEEING);

        public ulong SummonedByGuid => MemoryManager.ReadUlong(GetDescriptorPtr() + MemoryAddresses.WoWUnit_SummonedByGuidOffset);

        public int FactionId => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_FactionIdOffset);

        public bool NotAttackable => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_NON_ATTACKABLE);

        public bool IsFacing(Position position) => Math.Abs(GetFacingForPosition(position) - Facing) < 0.05f;

        // in radians
        public float GetFacingForPosition(Position position)
        {
            var f = (float)Math.Atan2(position.Y - Position.Y, position.X - Position.X);
            if (f < 0.0f)
                f += (float)Math.PI * 2.0f;
            else
            {
                if (f > (float)Math.PI * 2)
                    f -= (float)Math.PI * 2.0f;
            }
            return f;
        }

        public bool IsBehind(WoWUnit target)
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
        public bool InLosWith(WoWUnit otherUnit)
        {
            Position newThisPosition = new(Position.X, Position.Y, Position.Z + Height);
            Position newOtherPosition = new(otherUnit.Position.X, otherUnit.Position.Y, otherUnit.Position.Z + otherUnit.Height);

            return newThisPosition.InLosWith(otherUnit.Position)
                && Position.InLosWith(newOtherPosition)
                && newThisPosition.InLosWith(newOtherPosition)
                && Position.InLosWith(otherUnit.Position);
        }

        public MovementFlags MovementFlags => (MovementFlags)MemoryManager.ReadInt(nint.Add(Pointer, MemoryAddresses.WoWUnit_MovementFlagsOffset));

        public bool IsMoving => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD);

        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING);
        public uint MountDisplayId => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_MountDisplayIdOffset);

        public bool IsMounted => MountDisplayId > 0;

        public bool IsPet => SummonedByGuid > 0;

        public CreatureType CreatureType => Functions.GetCreatureType(Pointer);

        public UnitReaction UnitReaction => Functions.GetUnitReaction(Pointer, ObjectManager.Instance.Player.Pointer);

        public virtual CreatureRank CreatureRank => (CreatureRank)Functions.GetCreatureRank(Pointer);

        public Spell GetSpellById(int spellId)
        {
            var spellsBasePtr = MemoryManager.ReadIntPtr(0x00C0D788);
            var spellPtr = MemoryManager.ReadIntPtr(spellsBasePtr + spellId * 4);

            var spellCost = MemoryManager.ReadInt(spellPtr + 0x0080);

            var spellNamePtr = MemoryManager.ReadIntPtr(spellPtr + 0x1E0);
            var spellName = MemoryManager.ReadString(spellNamePtr);

            var spellDescriptionPtr = MemoryManager.ReadIntPtr(spellPtr + 0x228);
            var spellDescription = MemoryManager.ReadString(spellDescriptionPtr);

            var spellTooltipPtr = MemoryManager.ReadIntPtr(spellPtr + 0x24C);
            var spellTooltip = MemoryManager.ReadString(spellTooltipPtr);

            return new Spell((uint)spellId, (uint)spellCost, spellName, spellDescription, spellTooltip);
        }

        public IEnumerable<Spell> Buffs
        {
            get
            {
                var buffs = new List<Spell>();
                var currentBuffOffset = MemoryAddresses.WoWUnit_BuffsBaseOffset;
                for (var i = 0; i < 10; i++)
                {
                    var buffId = MemoryManager.ReadInt(GetDescriptorPtr() + currentBuffOffset);
                    if (buffId != 0)
                        buffs.Add(GetSpellById(buffId));
                    currentBuffOffset += 4;
                }
                return buffs;
            }
        }

        public IEnumerable<Spell> Debuffs
        {
            get
            {
                var debuffs = new List<Spell>();
                var currentDebuffOffset = MemoryAddresses.WoWUnit_DebuffsBaseOffset;
                for (var i = 0; i < 16; i++)
                {
                    var debuffId = MemoryManager.ReadInt(GetDescriptorPtr() + currentDebuffOffset);
                    if (debuffId != 0)
                        debuffs.Add(GetSpellById(debuffId));
                    currentDebuffOffset += 4;
                }
                return debuffs;
            }
        }

        public IEnumerable<ISpellEffect> GetDebuffs(LuaTarget target)
        {
            var debuffs = new List<SpellEffect>();

            for (var i = 1; i <= 16; i++)
            {
                var result = Functions.LuaCallWithResult("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} = UnitDebuff('" + target.ToString().ToLower() + "', " + i + ")");
                var icon = result[0];
                var stackCount = result[1];
                var debuffTypeString = result[2];

                if (string.IsNullOrEmpty(icon))
                    break;

                var success = Enum.TryParse(debuffTypeString, out EffectType type);
                if (!success)
                    type = EffectType.None;

                debuffs.Add(new SpellEffect(icon, Convert.ToUInt32(stackCount), type));
            }

            return debuffs;
        }

        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);

        public bool IsFacing(IWoWUnit position)
        {
            return true;
        }

        public bool InLosWith(Position position)
        {
            return true;
        }

        public bool InLosWith(IWoWUnit position)
        {
            return true;
        }

        public IEnumerable<ISpellEffect> GetDebuffs()
        {
            throw new NotImplementedException();
        }

        public bool IsImmobilized
        {
            get
            {
                return Debuffs.Any(d => ImmobilizedSpellText.Any(s => d.Description.Contains(s) || d.Tooltip.Contains(s)));
            }
        }

        public IWoWUnit Target => throw new NotImplementedException();

        public Dictionary<Powers, uint> Power => throw new NotImplementedException();

        public Dictionary<Powers, uint> MaxPower => throw new NotImplementedException();
    }
}
