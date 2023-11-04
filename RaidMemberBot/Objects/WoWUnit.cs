using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;
using static RaidMemberBot.Constants.Offsets;
using Functions = RaidMemberBot.Mem.Functions;
using ObjectManager = RaidMemberBot.Game.Statics.ObjectManager;

namespace RaidMemberBot.Objects
{
    /// <summary>
    ///     Represents a unit (NPC or player)
    /// </summary>
    public class WoWUnit : WoWObject
    {
        static readonly string[] ImmobilizedSpellText = { "Immobilized" };
        /// <summary>
        ///     Constructor taking guid aswell Ptr to object
        /// </summary>
        internal WoWUnit(ulong parGuid, IntPtr parPointer, WoWObjectTypes parType)
            : base(parGuid, parPointer, parType)
        {
        }

        /// <summary>
        /// The quest state of the unit
        /// </summary>
        public NpcQuestOfferState QuestState => ReadRelative<NpcQuestOfferState>(0xCB8);

        /// <summary>
        /// UnitFlags of the NPC (Gossip, Flightmaster, Auctionator etc.)
        /// </summary>
        public NpcFlags NpcFlags => GetDescriptor<NpcFlags>(0x24C);

        /// <summary>
        /// Determines if the unit is on taxi (gryphon)
        /// </summary>
        public bool IsOnTaxi => (Pointer.Add(0x110).ReadAs<IntPtr>().Add(0xA0).ReadAs<int>() >> 0x14) != 0;

        /// <summary>
        ///     Location (will be relative to the transport the unit is on)
        /// </summary>
        public override Location Location
        {
            get
            {
                try
                {
                    float X = ReadRelative<float>(Unit.PosX);
                    float Y = ReadRelative<float>(Unit.PosY);
                    float Z = ReadRelative<float>(Unit.PosZ);
                    return new Location(X, Y, Z);
                }
                catch
                {
                    return new Location(0, 0, 0);
                }

            }
        }
        public bool IsBehind(WoWUnit target)
        {
            double halfPi = Math.PI / 2;
            double twoPi = Math.PI * 2;
            double leftThreshold = target.Facing - halfPi;
            double rightThreshold = target.Facing + halfPi;

            bool condition;
            if (leftThreshold < 0)
                condition = Facing < rightThreshold || Facing > twoPi + leftThreshold;
            else if (rightThreshold > twoPi)
                condition = Facing > leftThreshold || Facing < rightThreshold - twoPi;
            else
                condition = Facing > leftThreshold && Facing < rightThreshold;

            return condition && IsFacing(target.Location);
        }
        public bool IsFacing(Location position) => Math.Abs(GetFacingForPosition(position) - Facing) < 0.05f;
        public float GetFacingForPosition(Location position)
        {
            float f = (float)Math.Atan2(position.Y - Location.Y, position.X - Location.X);
            if (f < 0.0f)
                f += (float)Math.PI * 2.0f;
            else
            {
                if (f > (float)Math.PI * 2)
                    f -= (float)Math.PI * 2.0f;
            }
            return f;
        }
        public bool IsDiseased => GetDebuffs().Any(t => t.Type == EffectType.Disease);

        public bool IsCursed => GetDebuffs().Any(t => t.Type == EffectType.Curse);

        public bool IsPoisoned => GetDebuffs().Any(t => t.Type == EffectType.Poison);

        public bool HasMagicDebuff => GetDebuffs().Any(t => t.Type == EffectType.Magic);

        public IEnumerable<SpellEffect> GetDebuffs()
        {
            List<SpellEffect> debuffs = new List<SpellEffect>();
            string target = string.Empty;

            if (Guid == ObjectManager.Instance.Player.Guid)
            {
                target = "player";
            } else if (Guid == ObjectManager.Instance.Player.TargetGuid)
            {
                target = "target";
            }

            if (!string.IsNullOrEmpty(target))
            {
                for (int i = 1; i <= 16; i++)
                {
                    string[] result = Lua.Instance.ExecuteWithResult("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} = UnitDebuff('" + target + "', " + i + ")");

                    string icon = result[0];
                    string stackCount = result[1];
                    string debuffTypeString = result[2];

                    if (string.IsNullOrEmpty(icon))
                        break;

                    bool success = Enum.TryParse(debuffTypeString, out EffectType type);
                    if (!success)
                        type = EffectType.None;

                    debuffs.Add(new SpellEffect(icon, Convert.ToInt32(stackCount), type));
                }
            }

            return debuffs;
        }

        /// <summary>
        /// Check if the unit is dead (health == 0)
        /// </summary>
        public virtual bool IsDead => Health == 0;

        /// <summary>
        /// The required skinning level calculated from the units level. Attention: This number doesnt mean you can actually skin the unit
        /// </summary>
        public int RequiredSkinningLevel
        {
            get
            {
                int level = Level;
                level = level > 60 ? 60 : level;
                if (level > 10)
                {
                    if (level > 20)
                    {
                        return 5 * level;
                    }
                    return 2 * (5 * level - 50);
                }
                return 1;
            }
        }

        /// <summary>
        /// Check if the unit is silenced
        /// </summary>
        public bool IsSilenced => (Flags & UnitFlags.UNIT_FLAG_SILENCED) == UnitFlags.UNIT_FLAG_SILENCED;

        /// <summary>
        /// Transport Guid the unit is on
        /// </summary>
        public ulong TransportGuid => Pointer.Add(0x118).ReadAs<IntPtr>().Add(0x38).ReadAs<ulong>();

        /// <summary>
        /// Transport the unit is on
        /// </summary>
        public WoWGameObject CurrentTransport
        {
            get
            {
                ulong guid = TransportGuid;
                return guid == 0 ? null : ObjectManager.Instance.GameObjects.FirstOrDefault(x => x.Guid == guid);
            }
        }

        /// <summary>
        ///     Distance to our character
        /// </summary>
        public float DistanceToPlayer => Location.GetDistanceTo(ObjectManager.Instance.Player.Location);

        /// <summary>
        ///     All auras on unit by ID
        /// </summary>
        public List<Spell> Buffs
        {
            get
            {
                List<int> tmpAuras = new List<int>();
                int auraBase = Unit.AuraBase;
                int curCount = 0;
                while (true)
                {
                    int auraId = GetDescriptor<int>(auraBase);
                    if (curCount == 10) break;
                    if (auraId != 0)
                        tmpAuras.Add(auraId);
                    auraBase += 4;
                    curCount++;
                }
                return tmpAuras.Select(x => new Spell(x, 0, Spellbook.Instance.GetName(x), string.Empty, string.Empty)).ToList();
            }
        }

        /// <summary>
        ///     All debuffs on unit by ID
        /// </summary>
        public List<Spell> Debuffs
        {
            get
            {
                List<int> tmpAuras = new List<int>();
                int auraBase = 0x13C;
                int curCount = 0;
                while (true)
                {
                    int auraId = GetDescriptor<int>(auraBase);
                    if (curCount == 16) break;
                    if (auraId != 0)
                        tmpAuras.Add(auraId);
                    auraBase += 4;
                    curCount++;
                }
                return tmpAuras.Select(x => new Spell(x, 0, Spellbook.Instance.GetName(x), string.Empty, string.Empty)).ToList();
            }
        }

        /// <summary>
        ///     Name of NPC / Player
        /// </summary>
        public override string Name
        {
            get
            {
                try
                {
                    switch (WoWType)
                    {
                        case WoWObjectTypes.OT_UNIT:
                            return UnitName;

                        case WoWObjectTypes.OT_PLAYER:
                            return PlayerName;
                    }
                }
                catch
                {
                    // ignored
                }
                return "";
            }
        }

        private string UnitName
        {
            get
            {
                IntPtr ptr1 = ReadRelative<IntPtr>(Unit.NameBase);
                if (ptr1 == IntPtr.Zero) return "";
                IntPtr ptr2 = Memory.Reader.Read<IntPtr>(ptr1);
                if (ptr2 == IntPtr.Zero) return "";
                return ptr2.ReadString();
            }
        }

        private string PlayerName
        {
            get
            {
                IntPtr nameBasePtr = Memory.Reader.Read<IntPtr>(PlayerObject.NameBase);
                while (true)
                {
                    ulong nextGuid =
                        Memory.Reader.Read<ulong>(IntPtr.Add(nameBasePtr, PlayerObject.NameBaseNextGuid));
                    if (nextGuid == 0)
                        return "";
                    if (nextGuid != Guid)
                        nameBasePtr = Memory.Reader.Read<IntPtr>(nameBasePtr);
                    else
                        break;
                }
                return nameBasePtr.Add(0x14).ReadString();
            }
        }

        /// <summary>
        ///     0 or the GUID of the object that summoned the creature
        /// </summary>
        public ulong SummonedBy => GetDescriptor<ulong>(Descriptors.SummonedByGuid);

        /// <summary>
        ///     NPC ID
        /// </summary>
        public int Id => ReadRelative<int>(Descriptors.NpcId);

        /// <summary>
        ///     The faction Id
        /// </summary>
        public int FactionId => GetDescriptor<int>(Descriptors.FactionId);

        //internal int FactionID => GetDescriptor<int>(Offsets.Descriptors.FactionId);

        /// <summary>
        ///     The movement state of the Unit
        /// </summary>
        public virtual MovementFlags MovementState => ReadRelative<MovementFlags>(Descriptors.MovementFlags);

        /// <summary>
        ///     Dynamic Flags of the Unit (Is lootable / Is tapped?)
        /// </summary>
        public int DynamicFlags => GetDescriptor<int>(Descriptors.DynamicFlags);

        /// <summary>
        ///     Units flags
        /// </summary>
        public UnitFlags Flags => GetDescriptor<UnitFlags>(Descriptors.Flags);

        /// <summary>
        ///     Gets a value indicating whether this unit is fleeing
        /// </summary>
        public bool IsFleeing
        {
            get
            {
                UnitFlags flag = UnitFlags.UNIT_FLAG_FLEEING;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is confused (Scatter Shot etc.)
        /// </summary>
        public bool IsConfused
        {
            get
            {
                UnitFlags flag = UnitFlags.UNIT_FLAG_CONFUSED;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is in combat.
        /// </summary>
        public bool IsInCombat
        {
            get
            {
                UnitFlags flag = UnitFlags.UNIT_FLAG_IN_COMBAT;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is skinable.
        /// </summary>
        public bool IsSkinable
        {
            get
            {
                UnitFlags flag = UnitFlags.UNIT_FLAG_SKINNABLE;
                return (Flags & flag) ==
                       flag;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this unit is stunned.
        /// </summary>
        public bool IsStunned
        {
            get
            {
                UnitFlags flag = UnitFlags.UNIT_FLAG_STUNNED;
                return (Flags & flag) ==
                       flag;
            }
        }
        public bool TastyCorpsesNearby =>
            ObjectManager.Instance.Units.Any(u =>
                u.Location.GetDistanceTo(Location) < 5
                && u.CreatureType.HasFlag(CreatureType.Humanoid | CreatureType.Undead)
            );
        public ulong SummonedByGuid => ReadRelative<ulong>(0x30);

        /// <summary>
        ///     Gets a value indicating whether this unit got its movement disabled.
        /// </summary>
        public bool IsMovementDisabled
        {
            get
            {
                UnitFlags flag = UnitFlags.UNIT_FLAG_CLIENT_CONTROL_LOST;
                return (Flags & flag) ==
                       flag;
            }
        }
        public bool IsMoving => MovementState != MovementFlags.None;

        public bool IsFalling => (MovementState & MovementFlags.Falling) == MovementFlags.Falling;

        internal bool IsCrowdControlled => IsStunned | IsFleeing | IsConfused;

        /// <summary>
        ///     Tells if the unit is tapped by me (gives XP)
        /// </summary>
        public bool TappedByMe => DynamicFlags >= PrivateEnums.DynamicFlags.TappedByMe &&
                                  DynamicFlags <= PrivateEnums.DynamicFlags.TappedByMe + 0x2;

        /// <summary>
        ///     Tells if the unit is tapped by someone else (grey portrait)
        /// </summary>
        public bool TappedByOther => DynamicFlags >= PrivateEnums.DynamicFlags.TappedByOther &&
                                     DynamicFlags <= PrivateEnums.DynamicFlags.TappedByOther + 2;

        /// <summary>
        ///     Tells if the unit is tapped by no one
        /// </summary>
        public bool IsUntouched => DynamicFlags == PrivateEnums.DynamicFlags.Untouched;

        /// <summary>
        ///     Tells if the unit is hunter marked
        /// </summary>
        public bool IsMarked => DynamicFlags == PrivateEnums.DynamicFlags.IsMarked;

        public bool HasBuff(string name) => Buffs.Any(a => a.Name == name);

        public bool HasDebuff(string name) => Debuffs.Any(a => a.Name == name);

        public bool IsChanneling => Channeling > 0;

        public bool IsCasting => Casting > 0;
        /// <summary>
        ///     Tells if the unit can be looted
        /// </summary>
        public bool CanBeLooted
        {
            get
            {
                if (Health == 0)
                    return (DynamicFlags & 1) != 0;
                return false;
            }
        }

        /// <summary>
        ///     Health
        /// </summary>
        public int Health
        {
            get
            {
                int ret = int.MaxValue;
                try
                {
                    ret = GetDescriptor<int>(Descriptors.Health);
                }
                catch
                {
                    // ignored
                }
                return ret;
            }
        }

        /// <summary>
        ///     Max health
        /// </summary>
        public int MaxHealth => GetDescriptor<int>(Descriptors.MaxHealth);

        /// <summary>
        ///     health percent.
        /// </summary>
        public int HealthPercent => (int)(Health / (float)MaxHealth * 100);

        /// <summary>
        ///     Mana
        /// </summary>
        public int Mana => GetDescriptor<int>(Descriptors.Mana);

        /// <summary>
        ///     maximum mana.
        /// </summary>
        public int MaxMana => GetDescriptor<int>(Descriptors.MaxMana);

        /// <summary>
        ///     mana percent.
        /// </summary>
        public int ManaPercent => (int)(Mana / (float)MaxMana * 100);

        /// <summary>
        ///     Rage
        /// </summary>
        public int Rage => GetDescriptor<int>(Descriptors.Rage) / 10;

        /// <summary>
        ///     Energy
        /// </summary>
        public int Energy => GetDescriptor<int>(Descriptors.Energy);

        /// <summary>
        ///     Guid of the units target
        /// </summary>
        public ulong TargetGuid
        {
            get { return GetDescriptor<ulong>(Descriptors.TargetGuid); }
            set { SetDescriptor(Descriptors.TargetGuid, value); }
        }

        /// <summary>
        ///     Id of the spell the unit is casting currently
        /// </summary>
        public virtual int Casting => ReadRelative<int>(0xC8C);

        /// <summary>
        ///     Id of the spell the unit is channeling currently
        /// </summary>
        public int Channeling => GetDescriptor<int>(Descriptors.IsChanneling);

        /// <summary>
        ///     Units reaction to the player
        /// </summary>
        public virtual UnitReaction Reaction
            => Functions.UnitReaction(Pointer, ObjectManager.Instance.Player.Pointer);

        /// <summary>
        ///     Is player?
        /// </summary>
        public bool IsPlayer => WoWType == WoWObjectTypes.OT_PLAYER;

        /// <summary>
        ///     Is Npc?
        /// </summary>
        public bool IsMob => WoWType == WoWObjectTypes.OT_UNIT;


        /// <summary>
        ///     Facing of the unit
        /// </summary>
        public new float Facing
        {
            get
            {
                float pi2 = (float)(2 * Math.PI);
                float facing = ReadRelative<float>(0x9C4);
                //if (facing >= pi2)
                //    facing -= pi2;
                //else if (facing < 0)
                //{
                //    facing = facing + pi2;
                //}
                return facing;
            }
        }

        /// <summary>
        ///     Is critter?
        /// </summary>
        public bool IsCritter => CreatureType.Critter == CreatureType;

        /// <summary>
        ///     Is unit a totem?
        /// </summary>
        public bool IsTotem => CreatureType.Totem == CreatureType;


        /// <summary>
        ///     Unit rank (rare, elite, normal etc.)
        /// </summary>
        public CreatureRankTypes CreatureRank => (CreatureRankTypes)Functions.GetCreatureRank(Pointer);

        /// <summary>
        ///     The type of the unit
        /// </summary>
        public CreatureType CreatureType => Functions.GetCreatureType(Pointer);

        /// <summary>
        ///     Units level
        /// </summary>
        public int Level => GetDescriptor<int>(Descriptors.Level);

        /// <summary>
        ///     Is the unit mounted?
        /// </summary>
        public bool IsMounted => GetDescriptor<int>(Descriptors.MountDisplayId) != 0;

        /// <summary>
        ///     Is the unit a pet?
        /// </summary>
        public bool IsPet => SummonedBy != 0;

        /// <summary>
        ///     Is the unit pet of a player?
        /// </summary>
        public bool IsPlayerPet
        {
            get
            {
                ulong tmpGuid = SummonedBy;
                if (tmpGuid == 0) return false;
                WoWUnit obj = ObjectManager.Instance.Players.FirstOrDefault(i => i.Guid == tmpGuid);
                return obj != null;
            }
        }

        /// <summary>
        ///     Is the unit swimming?
        /// </summary>
        public bool IsSwimming
            =>
                (MovementState & MovementFlags.Swimming) ==
                MovementFlags.Swimming;


        /// <summary>
        ///     Interacts with the unit
        /// </summary>
        /// <param name="parAutoLoot">Shift</param>
        public void Interact(bool parAutoLoot)
        {
            Functions.OnRightClickUnit(Pointer, Convert.ToInt32(parAutoLoot));
        }

        /// <summary>
        ///     Facing relative to another object
        /// </summary>
        /// <param name="parObject">The object</param>
        /// <returns></returns>
        public float FacingRelativeTo(WoWObject parObject)
        {
            return FacingRelativeTo(parObject.Location);
        }

        /// <summary>
        ///     Facing relative to another object
        /// </summary>
        /// <param name="parLocation">The position.</param>
        /// <returns></returns>
        public float FacingRelativeTo(Location parLocation)
        {
            return (float)Math.Round(Math.Abs(RequiredFacing(parLocation) - Facing), 2);
        }

        /// <summary>
        ///     Required facing to look at the unit
        /// </summary>
        /// <param name="parObject">The object.</param>
        /// <returns></returns>
        public float RequiredFacing(WoWObject parObject)
        {
            return RequiredFacing(parObject.Location);
        }

        /// <summary>
        ///     Required facing to look at the position
        /// </summary>
        /// <param name="parLocation">The position.</param>
        /// <returns></returns>
        public float RequiredFacing(Location parLocation)
        {
            float f = (float)Math.Atan2(parLocation.Y - Location.Y, parLocation.X - Location.X);
            if (f < 0.0f)
            {
                f = f + (float)Math.PI * 2.0f;
            }
            else
            {
                if (f > (float)Math.PI * 2)
                    f = f - (float)Math.PI * 2.0f;
            }
            return f;
        }


        /// <summary>
        ///     Check whether the unit got an aura or not
        /// </summary>
        /// <param name="parName">Name</param>
        /// <returns></returns>
        public bool GotAura(string parName)
        {
            List<Spell> tmpAuras = Buffs;
            return
                tmpAuras.Select(
                    i =>
                        string.Equals(i.Name, parName,
                            StringComparison.OrdinalIgnoreCase)).Any(tmpBool => tmpBool);
        }

        /// <summary>
        ///     Check whether the unit got an debuff or not
        /// </summary>
        /// <param name="parName">Name</param>
        /// <returns></returns>
        public bool GotDebuff(string parName)
        {
            List<Spell> tmpAuras = Debuffs;
            return
                tmpAuras.Select(
                    i =>
                        string.Equals(i.Name, parName,
                            StringComparison.OrdinalIgnoreCase)).Any(tmpBool => tmpBool);
        }

        /// <summary>
        ///     Tells if using aoe will engage the character with other units that arent fighting right now
        /// </summary>
        /// <param name="parRange">The radius around the unit</param>
        /// <returns>
        ///     Returns <c>true</c> if we can use AoE without engaging other unpulled units
        /// </returns>
        public bool IsAoeSafe(int parRange)
        {
            List<WoWUnit> mobs = ObjectManager.Instance.Npcs.
                FindAll(i => (i.Reaction == UnitReaction.Hostile || i.Reaction == UnitReaction.Neutral) &&
                             i.Location.DistanceToPlayer() < parRange).ToList();

            foreach (WoWUnit mob in mobs)
                if (mob.TargetGuid != ObjectManager.Instance.Player.Guid)
                    return false;
            return true;
        }

        /// <summary>
        /// Checks to see if the unit is in the specified range.
        /// 
        /// If target is passed, checks to see if the unit is in range to the given Container.HostileTarget.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool InRange(float range = 5, WoWUnit target = null)
        {
            return (target == null ? DistanceToPlayer : Location.GetDistanceTo(target.Location)) < range;
        }
        public bool IsImmobilized
        {
            get
            {
                return Debuffs.Any(d => ImmobilizedSpellText.Any(s => d.Description.Contains(s) || d.Tooltip.Contains(s)));
            }
        }
    }
}
