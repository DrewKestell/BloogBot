using GameData.Core.Enums;
using GameData.Core.Models;
using static GameData.Core.Constants.Spellbook;

namespace GameData.Core.Interfaces
{
    public interface IWoWUnit : IWoWGameObject
    {
        ulong TargetGuid { get; }
        uint Health { get; }
        uint MaxHealth { get; }
        uint HealthPercent => (uint)(Health / (float)MaxHealth * 100);
        Dictionary<Powers, uint> Powers { get; }
        Dictionary<Powers, uint> MaxPowers { get; }
        uint Mana => Powers.TryGetValue(Enums.Powers.MANA, out uint value) ? value : 0;
        uint MaxMana => MaxPowers.TryGetValue(Enums.Powers.MANA, out uint value) ? value : 0;
        uint ManaPercent => (uint)(Mana / (float)MaxMana * 100);
        uint Rage => Powers.TryGetValue(Enums.Powers.RAGE, out uint value) ? value : 0;
        uint MaxRage => MaxPowers.TryGetValue(Enums.Powers.RAGE, out uint value) ? value : 0;
        uint RagePercent => (uint)(Rage / (float)MaxRage * 100);
        uint Energy => Powers.TryGetValue(Enums.Powers.ENERGY, out uint value) ? value : 0;
        uint MaxEnergy => MaxPowers.TryGetValue(Enums.Powers.ENERGY, out uint value) ? value : 0;
        uint EnergyPercent => (uint)(Energy / (float)MaxEnergy * 100);
        uint Focus => Powers.TryGetValue(Enums.Powers.FOCUS, out uint value) ? value : 0;
        uint MaxFocus => MaxPowers.TryGetValue(Enums.Powers.FOCUS, out uint value) ? value : 0;
        uint FocusPercent => (uint)(Focus / (float)MaxFocus * 100);
        uint[] Bytes0 { get; }
        uint[] VirtualItemInfo { get; }
        uint[] VirtualItemSlotDisplay { get; }
        uint[] AuraFields { get; }
        uint[] AuraFlags { get; }
        uint[] AuraLevels { get; }
        uint[] AuraApplications { get; }
        uint AuraState { get; }
        float BaseAttackTime { get; }
        float OffhandAttackTime { get; }
        float BoundingRadius { get; }
        float CombatReach { get; }
        uint NativeDisplayId { get; }
        uint MinDamage { get; }
        uint MaxDamage { get; }
        uint MinOffhandDamage { get; }
        uint MaxOffhandDamage { get; }
        uint[] Bytes1 { get; }
        uint PetNumber { get; }
        uint PetNameTimestamp { get; }
        uint PetExperience { get; }
        uint PetNextLevelExperience { get; }
        uint ChannelingId { get; }
        float ModCastSpeed { get; }
        uint CreatedBySpell { get; }
        NPCFlags NpcFlags { get; }
        uint NpcEmoteState { get; }
        uint TrainingPoints { get; }
        uint Strength { get; }
        uint Agility { get; }
        uint Stamina { get; }
        uint Intellect { get; }
        uint Spirit { get; }
        uint[] Resistances { get; }
        uint BaseMana { get; }
        uint BaseHealth { get; }
        uint[] Bytes2 { get; }
        uint AttackPower { get; }
        uint AttackPowerMods { get; }
        uint AttackPowerMultipler { get; }
        uint RangedAttackPower { get; }
        uint RangedAttackPowerMods { get; }
        uint RangedAttackPowerMultipler { get; }
        uint MinRangedDamage { get; }
        uint MaxRangedDamage { get; }
        uint[] PowerCostModifers { get; }
        uint[] PowerCostMultipliers { get; }
        bool IsPet => SummonedByGuid > 0;
        ulong SummonedByGuid { get; }
        uint MountDisplayId { get; }
        UnitReaction UnitReaction { get; }
        UnitFlags UnitFlags { get; }
        MovementFlags MovementFlags { get; }
        uint MovementFlags2 { get; }
        uint FallTime { get; }
        float WalkSpeed { get; }
        float RunSpeed { get; }
        float RunBackSpeed { get; }
        float SwimSpeed { get; }
        float SwimBackSpeed { get; }
        float TurnRate { get; }
        HighGuid Charm { get; }
        HighGuid Summon { get; }
        HighGuid CharmedBy { get; }
        HighGuid SummonedBy { get; }
        HighGuid Persuaded { get; }
        HighGuid ChannelObject { get; }
        ulong TransportGuid { get; }
        float TransportOrientation { get; }
        float SwimPitch { get; }
        float JumpVerticalSpeed { get; }
        float JumpSinAngle { get; }
        float JumpCosAngle { get; }
        float JumpHorizontalSpeed { get; }
        float SplineElevation { get; }
        uint TransportLastUpdated { get; }
        SplineFlags SplineFlags { get; set; }
        Position SplineFinalPoint { get; set; }
        ulong SplineTargetGuid { get; set; }
        float SplineFinalOrientation { get; set; }
        int SplineTimePassed { get; set; }
        int SplineDuration { get; set; }
        uint SplineId { get; set; }
        List<Position> SplineNodes { get; set; }
        Position SplineFinalDestination { get; set; }
        IWoWGameObject Transport { get; set; }
        bool IsCasting => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);
        bool IsChanneling => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);
        bool IsInCombat => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);
        bool IsStunned => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_STUNNED);
        bool IsConfused => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_CONFUSED);
        bool IsFleeing => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_FLEEING);

        bool IsMoving => MovementFlags != MovementFlags.MOVEFLAG_NONE;
        bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        bool DismissBuff(string buffName);
        bool HasBuff(string buffName);
        bool HasDebuff(string debuffName);
        IEnumerable<ISpellEffect> GetDebuffs();
        IEnumerable<ISpellEffect> GetBuffs();
        public string CurrentShapeshiftForm
        {
            get
            {
                if (HasBuff(BearForm))
                    return BearForm;

                if (HasBuff(CatForm))
                    return CatForm;

                return "Human Form";
            }
        }

        CreatureType CreatureType { get; }
    }
}
