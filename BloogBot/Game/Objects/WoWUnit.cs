using BloogBot.Game.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.Game.Objects
{
    public class WoWUnit : WoWObject
    {
        static readonly string[] ImmobilizedSpellText = { "Immobilized" };

        public WoWUnit() { }

        public WoWUnit(
            IntPtr pointer,
            ulong guid,
            ObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public ulong TargetGuid => MemoryManager.ReadUlong(GetDescriptorPtr() + MemoryAddresses.WoWUnit_TargetGuidOffset);

        public int Health => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_HealthOffset);

        public int MaxHealth => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_MaxHealthOffset);

        public int HealthPercent => (int)(Health / (float)MaxHealth * 100);

        public int Mana => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_ManaOffset);

        public int MaxMana => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_MaxManaOffset);

        public int ManaPercent => (int)(Mana / (float)MaxMana * 100);

        public int Rage => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_RageOffset) / 10;

        public int Energy => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_EnergyOffset);

        public int CurrentChannelingId
        {
            get
            {
                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    return MemoryManager.ReadInt(GetDescriptorPtr() + 0x240);
                }
                else
                {
                    return MemoryManager.ReadInt(Pointer + MemoryAddresses.WoWUnit_CurrentChannelingOffset);
                }
            }
        }

        public bool IsChanneling => CurrentChannelingId > 0;

        public int CurrentSpellcastId => MemoryManager.ReadInt(Pointer + MemoryAddresses.WoWUnit_CurrentSpellcastOffset);

        public bool IsCasting => CurrentSpellcastId > 0;

        public virtual int Level => MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_LevelOffset);

        public DynamicFlags DynamicFlags => (DynamicFlags)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_DynamicFlagsOffset);

        public bool CanBeLooted => Health == 0 && DynamicFlags.HasFlag(DynamicFlags.CanBeLooted);

        public bool TappedByOther => DynamicFlags.HasFlag(DynamicFlags.Tapped) && !DynamicFlags.HasFlag(DynamicFlags.TappedByMe);

        public UnitFlags UnitFlags => (UnitFlags)MemoryManager.ReadInt(GetDescriptorPtr() + MemoryAddresses.WoWUnit_UnitFlagsOffset);

        public bool IsInCombat => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);

        public bool IsStunned => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_STUNNED);

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
            var halfPi = Math.PI / 2;
            var twoPi = Math.PI * 2;
            var leftThreshold = target.Facing - halfPi;
            var rightThreshold = target.Facing + halfPi;

            bool condition;
            if (leftThreshold < 0)
                condition = Facing < rightThreshold || Facing > twoPi + leftThreshold;
            else if (rightThreshold > twoPi)
                condition = Facing > leftThreshold || Facing < rightThreshold - twoPi;
            else
                condition = Facing > leftThreshold && Facing < rightThreshold;

            return condition && IsFacing(target.Position);
        }

        public MovementFlags MovementFlags => (MovementFlags)MemoryManager.ReadInt(IntPtr.Add(Pointer, MemoryAddresses.WoWUnit_MovementFlagsOffset));

        public bool IsMoving => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD);

        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING);

        public bool IsMounted => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_MOUNT);

        public bool IsPet => SummonedByGuid > 0;

        public CreatureType CreatureType => Functions.GetCreatureType(Pointer);

        public UnitReaction UnitReaction => Functions.GetUnitReaction(Pointer, ObjectManager.Player.Pointer);

        public virtual CreatureRank CreatureRank => (CreatureRank)Functions.GetCreatureRank(Pointer);

        public Spell GetSpellById(int spellId)
        {
            if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
            {
                var spellsBasePtr = MemoryManager.ReadIntPtr((IntPtr)0x00C0D788);
                var spellPtr =  MemoryManager.ReadIntPtr(spellsBasePtr + spellId * 4);

                var spellCost = MemoryManager.ReadInt(spellPtr + 0x0080);

                var spellNamePtr = MemoryManager.ReadIntPtr(spellPtr + 0x1E0);
                var spellName = MemoryManager.ReadString(spellNamePtr);

                var spellDescriptionPtr = MemoryManager.ReadIntPtr(spellPtr + 0x228);
                var spellDescription = MemoryManager.ReadString(spellDescriptionPtr);

                var spellTooltipPtr = MemoryManager.ReadIntPtr(spellPtr + 0x24C);
                var spellTooltip = MemoryManager.ReadString(spellTooltipPtr);

                return new Spell(spellId, spellCost, spellName, spellDescription, spellTooltip);
            }
            else
            {
                return Functions.GetSpellDBEntry(spellId);
            }
        }

        public void LuaCall(string code) => Functions.LuaCall(code);

        public string[] LuaCallWithResults(string code) => Functions.LuaCallWithResult(code);

        public IEnumerable<Spell> Buffs
        {
            get
            {
                // TODO: figure out what's going on here. WotLK seems to store buffs at a static offset from the Player Pointer,
                // but TBC seems to store them as a Descriptor
                if (ClientHelper.ClientVersion == ClientVersion.WotLK)
                {
                    var count = Functions.GetAuraCount(Pointer);
                    var buffs = new List<Spell>();
                    for (var i = 0; i < count; i++)
                    {
                        var buffPtr = Functions.GetAuraPointer(Pointer, i);

                        var spellId = MemoryManager.ReadInt(buffPtr + 0x8);
                        if (spellId > 0) // some weird invisible auras exist?
                        {
                            var flags = (AuraFlags)MemoryManager.ReadInt(buffPtr + 0x10);
                            if (!flags.HasFlag(AuraFlags.Harmful))
                            {
                                buffs.Add(GetSpellById(spellId));
                            }
                        }
                        
                    }
                    return buffs;
                }
                else
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
        }

        public IEnumerable<Spell> Debuffs
        {
            get
            {
                // TODO: figure out what's going on here. WotLK seems to store buffs at a static offset from the Player Pointer,
                // but TBC seems to store them as a Descriptor
                if (ClientHelper.ClientVersion == ClientVersion.WotLK)
                {
                    var count = Functions.GetAuraCount(Pointer);
                    var buffs = new List<Spell>();
                    for (var i = 0; i < count; i++)
                    {
                        var buffPtr = Functions.GetAuraPointer(Pointer, i);

                        var spellId = MemoryManager.ReadInt(buffPtr + 0x8);
                        if (spellId > 0) // some weird invisible auras exist?
                        {
                            var flags = (AuraFlags)MemoryManager.ReadInt(buffPtr + 0x10);
                            if (flags.HasFlag(AuraFlags.Harmful))
                            {
                                buffs.Add(GetSpellById(spellId));
                            }
                        }
                    }
                    return buffs;
                }
                else if (ClientHelper.ClientVersion == ClientVersion.TBC)
                {
                    var debuffs = new List<Spell>();
                    var currentDebuffOffset = MemoryAddresses.WoWUnit_DebuffsBaseOffset;
                    for (var i = 0; i < 16; i++)
                    {
                        var debuffId = MemoryManager.ReadInt(Pointer + currentDebuffOffset);
                        if (debuffId != 0)
                            debuffs.Add(GetSpellById(debuffId));
                        currentDebuffOffset += 4;
                    }
                    return debuffs;
                }
                else
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
        }

        public IEnumerable<SpellEffect> GetDebuffs(LuaTarget target)
        {
            var debuffs = new List<SpellEffect>();

            for (var i = 1; i <= 16; i++)
            {
                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    var result = LuaCallWithResults("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} = UnitDebuff('" + target.ToString().ToLower() + "', " + i + ")");
                    var icon = result[0];
                    var stackCount = result[1];
                    var debuffTypeString = result[2];

                    if (string.IsNullOrEmpty(icon))
                        break;

                    var success = Enum.TryParse(debuffTypeString, out EffectType type);
                    if (!success)
                        type = EffectType.None;

                    debuffs.Add(new SpellEffect(icon, Convert.ToInt32(stackCount), type));
                }
                else
                {
                    var result = LuaCallWithResults("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} = UnitDebuff('" + target.ToString().ToLower() + "', " + i + ")");
                    var icon = result[2];
                    var stackCount = result[3];
                    var debuffTypeString = result[4];

                    if (string.IsNullOrEmpty(icon))
                        break;

                    var success = Enum.TryParse(debuffTypeString, out EffectType type);
                    if (!success)
                        type = EffectType.None;

                    debuffs.Add(new SpellEffect(icon, Convert.ToInt32(stackCount), type));
                }
            }

            return debuffs;
        }

        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);

        public bool IsImmobilized
        {
            get
            {
                return Debuffs.Any(d => ImmobilizedSpellText.Any(s => d.Description.Contains(s) || d.Tooltip.Contains(s)));
            }
        }
    }
}
