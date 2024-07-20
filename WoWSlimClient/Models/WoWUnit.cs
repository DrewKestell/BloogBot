using static WoWSlimClient.Models.Enums;

namespace WoWSlimClient.Models
{
    public class WoWUnit : WoWObject
    {
        public int Level { get; set; }
        public int CreatureId { get; set; }
        public Race Race { get; set; }
        public Class Class { get; set; }
        public byte Gender { get; set; }

        public ulong TargetGuid { get; set; }

        public int Health { get; set; }

        public int MaxHealth { get; set; }

        public int HealthPercent => (int)(Health / (float)MaxHealth * 100);

        public int Mana { get; set; }

        public int MaxMana { get; set; }

        public int ManaPercent => (int)(Mana / (float)MaxMana * 100);

        public int Rage { get; set; }

        public int Energy { get; set; }

        public float Height { get; set; }

        public float BoundingRadius { get; set; }

        public float CombatReach { get; set; }

        public int ChannelingId { get; set; }

        public bool IsChanneling { get; set; }

        public int SpellcastId { get; set; }

        public bool IsCasting { get; set; }

        public DynamicFlags DynamicFlags { get; set; }

        public bool CanBeLooted => Health == 0 && DynamicFlags.HasFlag(DynamicFlags.CanBeLooted);

        public bool TappedByOther => DynamicFlags.HasFlag(DynamicFlags.Tapped) && !DynamicFlags.HasFlag(DynamicFlags.TappedByMe);

        public UnitFlags UnitFlags { get; set; }

        public bool IsInCombat => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);

        public bool IsStunned => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_STUNNED);

        public bool IsConfused => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_CONFUSED);

        public bool IsFleeing => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_FLEEING);

        public ulong SummonedByGuid { get; set; }

        public int FactionId { get; set; }

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

        public MovementFlags MovementFlags { get; set; }

        public bool IsMoving => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FORWARD);

        public bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);

        public bool IsFalling => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_FALLING);

        public bool IsMounted => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_MOUNT);

        public bool IsPet => SummonedByGuid > 0;

        public CreatureType CreatureType { get; set; }

        public UnitReaction UnitReaction { get; set; }

        public CreatureRank CreatureRank { get; set; }

        public Spell GetSpellById(int spellId)
        {
            return null;
        }

        public IEnumerable<Spell> Buffs
        {
            get
            {
                var buffs = new List<Spell>();

                return buffs;
            }
        }

        public IEnumerable<Spell> Debuffs
        {
            get
            {
                var debuffs = new List<Spell>();

                return debuffs;
            }
        }

        public IEnumerable<SpellEffect> GetDebuffs(LuaTarget target)
        {
            return new List<SpellEffect>();
        }

        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);
    }
}
