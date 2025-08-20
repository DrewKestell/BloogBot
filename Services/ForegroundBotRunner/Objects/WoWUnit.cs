using ForegroundBotRunner.Mem;
using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using Functions = ForegroundBotRunner.Mem.Functions;

namespace ForegroundBotRunner.Objects
{
    public class WoWUnit(
        nint pointer,
        HighGuid guid,
        WoWObjectType objectType) : WoWObject(pointer, guid, objectType), IWoWUnit
    {
        private static readonly string[] ImmobilizedSpellText = ["Immobilized"];

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

        public MovementFlags MovementFlags => (MovementFlags)MemoryManager.ReadInt(nint.Add(Pointer, MemoryAddresses.WoWUnit_MovementFlagsOffset));

        public bool IsMoving => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD);

        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_JUMPING);
        public uint MountDisplayId => MemoryManager.ReadUint(GetDescriptorPtr() + MemoryAddresses.WoWUnit_MountDisplayIdOffset);

        public bool IsMounted => MountDisplayId > 0;

        public bool IsPet => SummonedByGuid > 0;

        public CreatureType CreatureType => Functions.GetCreatureType(Pointer);

        public UnitReaction UnitReaction => Functions.GetUnitReaction(Pointer, Pointer);

        public virtual CreatureRank CreatureRank => (CreatureRank)Functions.GetCreatureRank(Pointer);

        public static ISpell GetSpellById(int spellId)
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

        public IEnumerable<ISpell> Buffs
        {
            get
            {
                var buffs = new List<ISpell>();
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

        public IEnumerable<ISpell> Debuffs
        {
            get
            {
                var debuffs = new List<ISpell>();
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

        public static IEnumerable<ISpellEffect> GetDebuffs(LuaTarget target)
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

        public IEnumerable<ISpellEffect> GetDebuffs()
        {
            throw new NotImplementedException();
        }

        public Position GetPointBehindUnit(float distance)
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

        public bool IsImmobilized
        {
            get
            {
                return Debuffs.Any(d => ImmobilizedSpellText.Any(s => d.Description.Contains(s) || d.Tooltip.Contains(s)));
            }
        }

        public IWoWUnit Target => throw new NotImplementedException();

        public Dictionary<Powers, uint> Powers => throw new NotImplementedException();

        public Dictionary<Powers, uint> MaxPowers => throw new NotImplementedException();

        public uint DisplayId => throw new NotImplementedException();

        public GOState GoState => throw new NotImplementedException();

        public uint ArtKit => throw new NotImplementedException();

        public uint AnimProgress => throw new NotImplementedException();

        public uint FactionTemplate => throw new NotImplementedException();

        public uint TypeId => throw new NotImplementedException();

        public NPCFlags NPCFlags => throw new NotImplementedException();

        public uint[] Bytes0 => throw new NotImplementedException();

        public uint[] VirtualItemInfo => throw new NotImplementedException();

        public uint[] VirtualItemSlotDisplay => throw new NotImplementedException();

        public uint[] AuraFields => throw new NotImplementedException();

        public uint[] AuraFlags => throw new NotImplementedException();

        public uint[] AuraLevels => throw new NotImplementedException();

        public uint[] AuraApplications => throw new NotImplementedException();

        public uint AuraState => throw new NotImplementedException();

        public float BaseAttackTime => throw new NotImplementedException();

        public float OffhandAttackTime => throw new NotImplementedException();

        public uint NativeDisplayId => throw new NotImplementedException();

        public uint MinDamage => throw new NotImplementedException();

        public uint MaxDamage => throw new NotImplementedException();

        public uint MinOffhandDamage => throw new NotImplementedException();

        public uint MaxOffhandDamage => throw new NotImplementedException();

        public uint[] Bytes1 => throw new NotImplementedException();

        public uint PetNumber => throw new NotImplementedException();

        public uint PetNameTimestamp => throw new NotImplementedException();

        public uint PetExperience => throw new NotImplementedException();

        public uint PetNextLevelExperience => throw new NotImplementedException();

        public float ModCastSpeed => throw new NotImplementedException();

        public uint CreatedBySpell => throw new NotImplementedException();

        public NPCFlags NpcFlags => throw new NotImplementedException();

        public uint NpcEmoteState => throw new NotImplementedException();

        public uint TrainingPoints => throw new NotImplementedException();

        public uint Strength => throw new NotImplementedException();

        public uint Agility => throw new NotImplementedException();

        public uint Stamina => throw new NotImplementedException();

        public uint Intellect => throw new NotImplementedException();

        public uint Spirit => throw new NotImplementedException();

        public uint[] Resistances => throw new NotImplementedException();

        public uint BaseMana => throw new NotImplementedException();

        public uint BaseHealth => throw new NotImplementedException();

        public uint[] Bytes2 => throw new NotImplementedException();

        public uint AttackPower => throw new NotImplementedException();

        public uint AttackPowerMods => throw new NotImplementedException();

        public uint AttackPowerMultipler => throw new NotImplementedException();

        public uint RangedAttackPower => throw new NotImplementedException();

        public uint RangedAttackPowerMods => throw new NotImplementedException();

        public uint RangedAttackPowerMultipler => throw new NotImplementedException();

        public uint MinRangedDamage => throw new NotImplementedException();

        public uint MaxRangedDamage => throw new NotImplementedException();

        public uint[] PowerCostModifers => throw new NotImplementedException();

        public uint[] PowerCostMultipliers => throw new NotImplementedException();

        public uint FallTime => throw new NotImplementedException();

        public float WalkSpeed => throw new NotImplementedException();

        public float RunSpeed => throw new NotImplementedException();

        public float RunBackSpeed => throw new NotImplementedException();

        public float SwimSpeed => throw new NotImplementedException();

        public float SwimBackSpeed => throw new NotImplementedException();

        public float TurnRate => throw new NotImplementedException();

        public HighGuid Charm => throw new NotImplementedException();

        public HighGuid Summon => throw new NotImplementedException();

        public HighGuid CharmedBy => throw new NotImplementedException();

        public HighGuid SummonedBy => throw new NotImplementedException();

        public HighGuid Persuaded => throw new NotImplementedException();

        public HighGuid ChannelObject => throw new NotImplementedException();

        public HighGuid CreatedBy => throw new NotImplementedException();

        public uint Flags => throw new NotImplementedException();

        public float[] Rotation => throw new NotImplementedException();

        public ulong TransportGuid => throw new NotImplementedException();

        public Position TransportOffset => throw new NotImplementedException();

        public float SwimPitch => throw new NotImplementedException();

        public float JumpVerticalSpeed => throw new NotImplementedException();

        public float JumpSinAngle => throw new NotImplementedException();

        public float JumpCosAngle => throw new NotImplementedException();

        public float JumpHorizontalSpeed => throw new NotImplementedException();

        public float SplineElevation => throw new NotImplementedException();

        public uint MovementFlags2 => throw new NotImplementedException();

        public IWoWGameObject Transport { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
