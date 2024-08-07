using BotRunner.Constants;
using static BotRunner.Constants.Spellbook;

namespace BotRunner.Interfaces
{
    public interface IWoWUnit : IWoWObject
    {
        string Name { get; }
        ulong TargetGuid { get; }
        IWoWUnit Target { get; }
        uint Health { get; }
        uint MaxHealth { get; }
        uint HealthPercent => (uint)(Health / (float)MaxHealth * 100);
        Dictionary<Powers, uint> Power { get; }
        Dictionary<Powers, uint> MaxPower { get; }
        uint Mana => Power.TryGetValue(Powers.MANA, out uint value) ? value : 0;
        uint MaxMana => MaxPower.TryGetValue(Powers.MANA, out uint value) ? value : 0;
        uint ManaPercent => (uint)(Mana / (float)MaxMana * 100);
        uint Rage => Power.TryGetValue(Powers.RAGE, out uint value) ? value : 0;
        uint MaxRage => MaxPower.TryGetValue(Powers.RAGE, out uint value) ? value : 0;
        uint RagePercent => (uint)(Rage / (float)MaxRage * 100);
        uint Energy => Power.TryGetValue(Powers.ENERGY, out uint value) ? value : 0;
        uint MaxEnergy => MaxPower.TryGetValue(Powers.ENERGY, out uint value) ? value : 0;
        uint EnergyPercent => (uint)(Energy / (float)MaxEnergy * 100);
        uint Focus => Power.TryGetValue(Powers.FOCUS, out uint value) ? value : 0;
        uint MaxFocus => MaxPower.TryGetValue(Powers.FOCUS, out uint value) ? value : 0;
        uint FocusPercent => (uint)(Focus / (float)MaxFocus * 100);
        bool IsPet => SummonedByGuid > 0;
        ulong SummonedByGuid { get; }
        uint MountDisplayId { get; }
        UnitReaction UnitReaction { get; }
        DynamicFlags DynamicFlags { get; }
        UnitFlags UnitFlags { get; }
        MovementFlags MovementFlags { get; }
        bool CanBeLooted => Health == 0 && DynamicFlags.HasFlag(DynamicFlags.CanBeLooted);
        bool TappedByOther => DynamicFlags.HasFlag(DynamicFlags.Tapped) && !DynamicFlags.HasFlag(DynamicFlags.TappedByMe);

        bool IsCasting => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);
        bool IsChanneling => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);
        bool IsInCombat => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_IN_COMBAT);
        bool IsStunned => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_STUNNED);
        bool IsConfused => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_CONFUSED);
        bool IsFleeing => UnitFlags.HasFlag(UnitFlags.UNIT_FLAG_FLEEING);

        bool IsMoving => MovementFlags != MovementFlags.MOVEFLAG_NONE;
        bool IsSwimming => MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING);
        
        uint Level { get; }

        bool HasBuff(string buffName);
        bool HasDebuff(string debuffName);
        IEnumerable<ISpellEffect> GetDebuffs();

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

    public enum UnitReaction
    {
        Hated,
        Hostile,
        Unfriendly,
        Neutral,
        Friendly,
        Honored,
        Revered,
        Exalted
    }
    public enum Powers
    {
        MANA,
        RAGE,
        FOCUS,
        ENERGY,
        HAPPINESS
    }

    [Flags]
    public enum DynamicFlags
    {
        None = 0x00,
        CanBeLooted = 0x01,
        Tapped = 0x02,
        TappedByMe = 0x04
    }

    [Flags]
    public enum MovementFlags
    {
        // Byte 1 (Resets on Movement Key Press)
        MOVEFLAG_NONE = 0x00000000,
        MOVEFLAG_FORWARD = 0x00000001,
        MOVEFLAG_BACKWARD = 0x00000002,
        MOVEFLAG_STRAFE_LEFT = 0x00000004,
        MOVEFLAG_STRAFE_RIGHT = 0x00000008,
        MOVEFLAG_TURN_LEFT = 0x00000010,
        MOVEFLAG_TURN_RIGHT = 0x00000020,
        MOVEFLAG_PITCH_UP = 0x00000040,
        MOVEFLAG_PITCH_DOWN = 0x00000080,

        // Byte 2 (Resets on Situation Change)
        MOVEFLAG_WALK_MODE = 0x00000100,               // Walking

        MOVEFLAG_LEVITATING = 0x00000400,
        MOVEFLAG_FLYING = 0x00000800,               // [-ZERO] is it really need and correct value
        MOVEFLAG_FALLING = 0x00002000,
        MOVEFLAG_FALLINGFAR = 0x00004000,
        MOVEFLAG_SWIMMING = 0x00200000,               // appears with fly flag also
        MOVEFLAG_SPLINE_ENABLED = 0x00400000,
        MOVEFLAG_CAN_FLY = 0x00800000,               // [-ZERO] is it really need and correct value
        MOVEFLAG_FLYING_OLD = 0x01000000,               // [-ZERO] is it really need and correct value

        MOVEFLAG_ONTRANSPORT = 0x02000000,               // Used for flying on some creatures
        MOVEFLAG_SPLINE_ELEVATION = 0x04000000,               // used for flight paths
        MOVEFLAG_ROOT = 0x08000000,               // used for flight paths
        MOVEFLAG_WATERWALKING = 0x10000000,               // prevent unit from falling through water
        MOVEFLAG_SAFE_FALL = 0x20000000,               // active rogue safe fall spell (passive)
        MOVEFLAG_HOVER = 0x40000000
    }

    [Flags]
    public enum SpellInterruptFlags
    {
        SPELL_INTERRUPT_FLAG_MOVEMENT = 0x01,
        SPELL_INTERRUPT_FLAG_DAMAGE = 0x02,
        SPELL_INTERRUPT_FLAG_INTERRUPT = 0x04,
        SPELL_INTERRUPT_FLAG_AUTOATTACK = 0x08,
        SPELL_INTERRUPT_FLAG_ABORT_ON_DMG = 0x10               // _complete_ interrupt on direct damage
                                                               // SPELL_INTERRUPT_UNK               = 0x20               // unk, 564 of 727 spells having this spell start with "Glyph"
    }
    [Flags]
    public enum SpellChannelInterruptFlags
    {
        CHANNEL_FLAG_DAMAGE = 0x0002,
        CHANNEL_FLAG_MOVEMENT = 0x0008,
        CHANNEL_FLAG_TURNING = 0x0010,
        CHANNEL_FLAG_DAMAGE2 = 0x0080,
        CHANNEL_FLAG_DELAY = 0x4000
    }
    [Flags]
    public enum SpellAuraInterruptFlags
    {
        AURA_INTERRUPT_FLAG_UNK0 = 0x00000001,   // 0    removed when getting hit by a negative spell?
        AURA_INTERRUPT_FLAG_DAMAGE = 0x00000002,   // 1    removed by any damage
        AURA_INTERRUPT_FLAG_UNK2 = 0x00000004,   // 2
        AURA_INTERRUPT_FLAG_MOVE = 0x00000008,   // 3    removed by any movement
        AURA_INTERRUPT_FLAG_TURNING = 0x00000010,   // 4    removed by any turning
        AURA_INTERRUPT_FLAG_ENTER_COMBAT = 0x00000020,   // 5    removed by entering combat
        AURA_INTERRUPT_FLAG_NOT_MOUNTED = 0x00000040,   // 6    removed by unmounting
        AURA_INTERRUPT_FLAG_NOT_ABOVEWATER = 0x00000080,   // 7    removed by entering water
        AURA_INTERRUPT_FLAG_NOT_UNDERWATER = 0x00000100,   // 8    removed by leaving water
        AURA_INTERRUPT_FLAG_NOT_SHEATHED = 0x00000200,   // 9    removed by unsheathing
        AURA_INTERRUPT_FLAG_UNK10 = 0x00000400,   // 10
        AURA_INTERRUPT_FLAG_UNK11 = 0x00000800,   // 11
        AURA_INTERRUPT_FLAG_MELEE_ATTACK = 0x00001000,   // 12   removed by melee attacks
        AURA_INTERRUPT_FLAG_UNK13 = 0x00002000,   // 13
        AURA_INTERRUPT_FLAG_UNK14 = 0x00004000,   // 14
        AURA_INTERRUPT_FLAG_UNK15 = 0x00008000,   // 15   removed by casting a spell?
        AURA_INTERRUPT_FLAG_UNK16 = 0x00010000,   // 16
        AURA_INTERRUPT_FLAG_MOUNTING = 0x00020000,   // 17   removed by mounting
        AURA_INTERRUPT_FLAG_NOT_SEATED = 0x00040000,   // 18   removed by standing up (used by food and drink mostly and sleep/Fake Death like)
        AURA_INTERRUPT_FLAG_CHANGE_MAP = 0x00080000,   // 19   leaving map/getting teleported
        AURA_INTERRUPT_FLAG_IMMUNE_OR_LOST_SELECTION = 0x00100000,   // 20   removed by auras that make you invulnerable, or make other to loose selection on you
        AURA_INTERRUPT_FLAG_UNK21 = 0x00200000,   // 21
        AURA_INTERRUPT_FLAG_UNK22 = 0x00400000,   // 22
        AURA_INTERRUPT_FLAG_ENTER_PVP_COMBAT = 0x00800000,   // 23   removed by entering pvp combat
        AURA_INTERRUPT_FLAG_DIRECT_DAMAGE = 0x01000000    // 24   removed by any direct damage
    };

    public enum SpellModOp
    {
        SPELLMOD_DAMAGE = 0,
        SPELLMOD_DURATION = 1,
        SPELLMOD_THREAT = 2,
        SPELLMOD_ATTACK_POWER = 3,
        SPELLMOD_CHARGES = 4,
        SPELLMOD_RANGE = 5,
        SPELLMOD_RADIUS = 6,
        SPELLMOD_CRITICAL_CHANCE = 7,
        SPELLMOD_ALL_EFFECTS = 8,
        SPELLMOD_NOT_LOSE_CASTING_TIME = 9,
        SPELLMOD_CASTING_TIME = 10,
        SPELLMOD_COOLDOWN = 11,
        SPELLMOD_SPEED = 12,
        // spellmod 13 unused
        SPELLMOD_COST = 14,
        SPELLMOD_CRIT_DAMAGE_BONUS = 15,
        SPELLMOD_RESIST_MISS_CHANCE = 16,
        SPELLMOD_JUMP_TARGETS = 17,
        SPELLMOD_CHANCE_OF_SUCCESS = 18,                   // Only used with SPELL_AURA_ADD_FLAT_MODIFIER and affects proc spells
        SPELLMOD_ACTIVATION_TIME = 19,
        SPELLMOD_EFFECT_PAST_FIRST = 20,
        SPELLMOD_CASTING_TIME_OLD = 21,
        SPELLMOD_DOT = 22,
        SPELLMOD_HASTE = 23,
        SPELLMOD_SPELL_BONUS_DAMAGE = 24,
        // spellmod 25 unused
        // SPELLMOD_FREQUENCY_OF_SUCCESS   = 26,
        SPELLMOD_MULTIPLE_VALUE = 27,
        SPELLMOD_RESIST_DISPEL_CHANCE = 28
    }
    public enum SpellFacingFlags
    {
        SPELL_FACING_FLAG_INFRONT = 0x0001
    }
    public enum UnitStandStateType
    {
        UNIT_STAND_STATE_STAND = 0,
        UNIT_STAND_STATE_SIT = 1,
        UNIT_STAND_STATE_SIT_CHAIR = 2,
        UNIT_STAND_STATE_SLEEP = 3,
        UNIT_STAND_STATE_SIT_LOW_CHAIR = 4,
        UNIT_STAND_STATE_SIT_MEDIUM_CHAIR = 5,
        UNIT_STAND_STATE_SIT_HIGH_CHAIR = 6,
        UNIT_STAND_STATE_DEAD = 7,
        UNIT_STAND_STATE_KNEEL = 8,
    }
    [Flags]
    public enum UnitBytes1_Flags
    {
        UNIT_BYTE1_FLAG_ALWAYS_STAND = 0x01,
        UNIT_BYTE1_FLAGS_CREEP = 0x02,
        UNIT_BYTE1_FLAG_UNTRACKABLE = 0x04,
        UNIT_BYTE1_FLAG_ALL = 0xFF
    }
    public enum SheathState
    {
        /// non prepared weapon
        SHEATH_STATE_UNARMED = 0,
        /// prepared melee weapon
        SHEATH_STATE_MELEE = 1,
        /// prepared ranged weapon
        SHEATH_STATE_RANGED = 2
    }
    [Flags]
    public enum UnitBytes2_Flags
    {
        UNIT_BYTE2_FLAG_UNK0 = 0x01,
        UNIT_BYTE2_FLAG_UNK1 = 0x02,
        UNIT_BYTE2_FLAG_UNK2 = 0x04,
        UNIT_BYTE2_FLAG_UNK3 = 0x08,
        UNIT_BYTE2_FLAG_AURAS = 0x10,                     // show positive auras as positive, and allow its dispel
        UNIT_BYTE2_FLAG_UNK5 = 0x20,
        UNIT_BYTE2_FLAG_UNK6 = 0x40,
        UNIT_BYTE2_FLAG_UNK7 = 0x80
    }
    public enum Swing
    {
        NOSWING = 0,
        SINGLEHANDEDSWING = 1,
        TWOHANDEDSWING = 2
    }
    public enum VictimState
    {
        VICTIMSTATE_UNAFFECTED = 0,                         // seen in relation with HITINFO_MISS
        VICTIMSTATE_NORMAL = 1,
        VICTIMSTATE_DODGE = 2,
        VICTIMSTATE_PARRY = 3,
        VICTIMSTATE_INTERRUPT = 4,
        VICTIMSTATE_BLOCKS = 5,
        VICTIMSTATE_EVADES = 6,
        VICTIMSTATE_IS_IMMUNE = 7,
        VICTIMSTATE_DEFLECTS = 8
    }
    [Flags]
    public enum HitInfo
    {
        HITINFO_NORMALSWING = 0x00000000,
        HITINFO_UNK0 = 0x00000001,               // req correct packet structure
        HITINFO_NORMALSWING2 = 0x00000002,
        HITINFO_LEFTSWING = 0x00000004,
        HITINFO_UNK3 = 0x00000008,
        HITINFO_MISS = 0x00000010,
        HITINFO_ABSORB = 0x00000020,               // plays absorb sound
        HITINFO_RESIST = 0x00000040,               // resisted atleast some damage
        HITINFO_CRITICALHIT = 0x00000080,
        HITINFO_UNK8 = 0x00000100,               // wotlk?
        HITINFO_BLOCK = 0x00000800,               // [ZERO]
        HITINFO_UNK9 = 0x00002000,               // wotlk?
        HITINFO_GLANCING = 0x00004000,
        HITINFO_CRUSHING = 0x00008000,
        HITINFO_NOACTION = 0x00010000,
        HITINFO_SWINGNOHITSOUND = 0x00080000
    }
    public enum InventorySlot
    {
        NULL_BAG = 0,
        NULL_SLOT = 255
    }
    public enum UnitModifierType
    {
        BASE_VALUE = 0,
        BASE_PCT = 1,
        TOTAL_VALUE = 2,
        TOTAL_PCT = 3,
        MODIFIER_TYPE_END = 4
    }
    public enum WeaponDamageRange
    {
        MINDAMAGE,
        MAXDAMAGE
    }
    public enum DamageTypeToSchool
    {
        RESISTANCE,
        DAMAGE_DEALT,
        DAMAGE_TAKEN
    }
    public enum AuraRemoveMode
    {
        AURA_REMOVE_BY_DEFAULT,
        AURA_REMOVE_BY_STACK,           ///< at replace by similar aura
        AURA_REMOVE_BY_CANCEL,          ///< It was cancelled by the user (needs confirmation)
        AURA_REMOVE_BY_DISPEL,          ///< It was dispelled by ie Remove Magic
        AURA_REMOVE_BY_DEATH,           ///< The \ref Unit died and there for it was removed
        AURA_REMOVE_BY_DELETE,          ///< use for speedup and prevent unexpected effects at player logout/pet unsummon (must be used _only_ after save), delete.
        AURA_REMOVE_BY_SHIELD_BREAK,    ///< when absorb shield is removed by damage
        AURA_REMOVE_BY_EXPIRE,          ///< at duration end
        AURA_REMOVE_BY_TRACKING         ///< aura is removed because of a conflicting tracked aura
    };

    public enum UnitMods
    {
        UNIT_MOD_STAT_STRENGTH,                                 // UNIT_MOD_STAT_STRENGTH..UNIT_MOD_STAT_SPIRIT must be in existing order, it's accessed by index values of Stats enum.
        UNIT_MOD_STAT_AGILITY,
        UNIT_MOD_STAT_STAMINA,
        UNIT_MOD_STAT_INTELLECT,
        UNIT_MOD_STAT_SPIRIT,
        UNIT_MOD_HEALTH,
        UNIT_MOD_MANA,                                          // UNIT_MOD_MANA..UNIT_MOD_HAPPINESS must be in existing order, it's accessed by index values of Powers enum.
        UNIT_MOD_RAGE,
        UNIT_MOD_FOCUS,
        UNIT_MOD_ENERGY,
        UNIT_MOD_HAPPINESS,
        UNIT_MOD_ARMOR,                                         // UNIT_MOD_ARMOR..UNIT_MOD_RESISTANCE_ARCANE must be in existing order, it's accessed by index values of SpellSchools enum.
        UNIT_MOD_RESISTANCE_HOLY,
        UNIT_MOD_RESISTANCE_FIRE,
        UNIT_MOD_RESISTANCE_NATURE,
        UNIT_MOD_RESISTANCE_FROST,
        UNIT_MOD_RESISTANCE_SHADOW,
        UNIT_MOD_RESISTANCE_ARCANE,
        UNIT_MOD_ATTACK_POWER,
        UNIT_MOD_ATTACK_POWER_RANGED,
        UNIT_MOD_DAMAGE_MAINHAND,
        UNIT_MOD_DAMAGE_OFFHAND,
        UNIT_MOD_DAMAGE_RANGED,
        UNIT_MOD_END,
        // synonyms
        UNIT_MOD_STAT_START = UNIT_MOD_STAT_STRENGTH,
        UNIT_MOD_STAT_END = UNIT_MOD_STAT_SPIRIT + 1,
        UNIT_MOD_RESISTANCE_START = UNIT_MOD_ARMOR,
        UNIT_MOD_RESISTANCE_END = UNIT_MOD_RESISTANCE_ARCANE + 1,
        UNIT_MOD_POWER_START = UNIT_MOD_MANA,
        UNIT_MOD_POWER_END = UNIT_MOD_HAPPINESS + 1
    };

    public enum BaseModGroup
    {
        CRIT_PERCENTAGE,
        RANGED_CRIT_PERCENTAGE,
        OFFHAND_CRIT_PERCENTAGE,
        SHIELD_BLOCK_VALUE,
        BASEMOD_END
    };

    public enum BaseModType
    {
        FLAT_MOD,
        PCT_MOD,
        MOD_END,
    };

    public enum DeathState
    {
        ALIVE = 0,     ///< show as alive
        JUST_DIED = 1,     ///< temporary state at die, for creature auto converted to CORPSE, for player at next update call
        CORPSE = 2,     ///< corpse state, for player this also meaning that player not leave corpse
        DEAD = 3,     ///< for creature despawned state (corpse despawned), for player CORPSE/DEAD not clear way switches (FIXME), and use m_deathtimer > 0 check for real corpse state
        JUST_ALIVED = 4      ///< temporary state at resurrection, for creature auto converted to ALIVE, for player at next update call
    };

    /**
     * internal state flags for some auras and movement generators, other. (Taken from comment)
     */
    [Flags]
    public enum UnitState : uint
    {
        // persistent state (applied by aura/etc until expire)
        UNIT_STAT_MELEE_ATTACKING = 0x00000001,                 // unit is melee attacking someone Unit::Attack
        UNIT_STAT_ATTACK_PLAYER = 0x00000002,                 // unit attack player or player's controlled unit and have contested pvpv timer setup, until timer expire, combat end and etc
        UNIT_STAT_DIED = 0x00000004,                 // Unit::SetFeignDeath
        UNIT_STAT_STUNNED = 0x00000008,                 // Aura::HandleAuraModStun
        UNIT_STAT_ROOT = 0x00000010,                 // Aura::HandleAuraModRoot
        UNIT_STAT_ISOLATED = 0x00000020,                 // area auras do not affect other players, Aura::HandleAuraModSchoolImmunity
        UNIT_STAT_CONTROLLED = 0x00000040,                 // Aura::HandleAuraModPossess

        // persistent movement generator state (all time while movement generator applied to unit (independent from top state of movegen)
        UNIT_STAT_TAXI_FLIGHT = 0x00000080,                 // player is in flight mode (in fact interrupted at far teleport until next map telport landing)
        UNIT_STAT_DISTRACTED = 0x00000100,                 // DistractedMovementGenerator active

        // persistent movement generator state with non-persistent mirror states for stop support
        // (can be removed temporary by stop command or another movement generator apply)
        // not use _MOVE versions for generic movegen state, it can be removed temporary for unit stop and etc
        UNIT_STAT_CONFUSED = 0x00000200,                 // ConfusedMovementGenerator active/onstack
        UNIT_STAT_CONFUSED_MOVE = 0x00000400,
        UNIT_STAT_ROAMING = 0x00000800,                 // RandomMovementGenerator/PointMovementGenerator/WaypointMovementGenerator active (now always set)
        UNIT_STAT_ROAMING_MOVE = 0x00001000,
        UNIT_STAT_CHASE = 0x00002000,                 // ChaseMovementGenerator active
        UNIT_STAT_CHASE_MOVE = 0x00004000,
        UNIT_STAT_FOLLOW = 0x00008000,                 // FollowMovementGenerator active
        UNIT_STAT_FOLLOW_MOVE = 0x00010000,
        UNIT_STAT_FLEEING = 0x00020000,                 // FleeMovementGenerator/TimedFleeingMovementGenerator active/onstack
        UNIT_STAT_FLEEING_MOVE = 0x00040000,
        // More room for other MMGens

        // High-Level states (usually only with Creatures)
        UNIT_STAT_NO_COMBAT_MOVEMENT = 0x01000000,           // Combat Movement for MoveChase stopped
        UNIT_STAT_RUNNING = 0x02000000,           // SetRun for waypoints and such
        UNIT_STAT_WAYPOINT_PAUSED = 0x04000000,           // Waypoint-Movement paused genericly (ie by script)

        UNIT_STAT_IGNORE_PATHFINDING = 0x10000000,           // do not use pathfinding in any MovementGenerator

        // masks (only for check)

        // can't move currently
        UNIT_STAT_CAN_NOT_MOVE = UNIT_STAT_ROOT | UNIT_STAT_STUNNED | UNIT_STAT_DIED,

        // stay by different reasons
        UNIT_STAT_NOT_MOVE = UNIT_STAT_ROOT | UNIT_STAT_STUNNED | UNIT_STAT_DIED |
        UNIT_STAT_DISTRACTED,

        // stay or scripted movement for effect( = in player case you can't move by client command)
        UNIT_STAT_NO_FREE_MOVE = UNIT_STAT_ROOT | UNIT_STAT_STUNNED | UNIT_STAT_DIED |
        UNIT_STAT_TAXI_FLIGHT |
        UNIT_STAT_CONFUSED | UNIT_STAT_FLEEING,

        // not react at move in sight or other
        UNIT_STAT_CAN_NOT_REACT = UNIT_STAT_STUNNED | UNIT_STAT_DIED |
        UNIT_STAT_CONFUSED | UNIT_STAT_FLEEING,

        // AI disabled by some reason
        UNIT_STAT_LOST_CONTROL = UNIT_STAT_CONFUSED | UNIT_STAT_FLEEING | UNIT_STAT_CONTROLLED,

        // above 2 state cases
        UNIT_STAT_CAN_NOT_REACT_OR_LOST_CONTROL = UNIT_STAT_CAN_NOT_REACT | UNIT_STAT_LOST_CONTROL,

        // masks (for check or reset)

        // for real move using movegen check and stop (except unstoppable flight)
        UNIT_STAT_MOVING = UNIT_STAT_ROAMING_MOVE | UNIT_STAT_CHASE_MOVE | UNIT_STAT_FOLLOW_MOVE | UNIT_STAT_FLEEING_MOVE,

        UNIT_STAT_RUNNING_STATE = UNIT_STAT_CHASE_MOVE | UNIT_STAT_FLEEING_MOVE | UNIT_STAT_RUNNING,

        UNIT_STAT_ALL_STATE = 0xFFFFFFFF,
        UNIT_STAT_ALL_DYN_STATES = UNIT_STAT_ALL_STATE & ~(UNIT_STAT_NO_COMBAT_MOVEMENT | UNIT_STAT_RUNNING | UNIT_STAT_WAYPOINT_PAUSED | UNIT_STAT_IGNORE_PATHFINDING)
    };

    public enum UnitMoveType
    {
        MOVE_WALK = 0,
        MOVE_RUN = 1,
        MOVE_RUN_BACK = 2,
        MOVE_SWIM = 3,
        MOVE_SWIM_BACK = 4,
        MOVE_TURN_RATE = 5
    };

    public enum UnitAuraFlags
    {
        UNIT_AURAFLAG_ALIVE_INVISIBLE = 0x1,                  // aura which makes unit invisible for alive
    };

    public enum UnitVisibility
    {
        VISIBILITY_OFF = 0,                      // absolute, not detectable, GM-like, can see all other
        VISIBILITY_ON = 1,
        VISIBILITY_GROUP_STEALTH = 2,                      // detect chance, seen and can see group members
        VISIBILITY_GROUP_INVISIBILITY = 3,                      // invisibility, can see and can be seen only another invisible unit or invisible detection unit, set only if not stealthed, and in checks not used (mask used instead)
        VISIBILITY_GROUP_NO_DETECT = 4,                      // state just at stealth apply for update Grid state. Don't remove, otherwise stealth spells will break
        VISIBILITY_REMOVE_CORPSE = 5                       // special totally not detectable visibility for force delete object while removing a corpse
    };

    /**
     * [-ZERO] Need recheck values
     * Value masks for UNIT_FIELD_FLAGS (Taken from source)
     * \todo Document all the flags, not just the ones already commented
     */
    [Flags]
    public enum UnitFlags
    {
        UNIT_FLAG_NONE = 0x00000000,
        UNIT_FLAG_UNK_0 = 0x00000001,
        UNIT_FLAG_NON_ATTACKABLE = 0x00000002,           ///< not attackable
        UNIT_FLAG_CLIENT_CONTROL_LOST = 0x00000004,           // Generic unspecified loss of control initiated by server script, movement checks disabled, paired with loss of client control packet.
        UNIT_FLAG_PVP_ATTACKABLE = 0x00000008,           ///< allow apply pvp rules to attackable state in addition to faction dependent state, UNIT_FLAG_UNKNOWN1 in pre-bc mangos
        UNIT_FLAG_RENAME = 0x00000010,           ///< rename creature
        UNIT_FLAG_RESTING = 0x00000020,
        UNIT_FLAG_UNK_6 = 0x00000040,
        UNIT_FLAG_OOC_NOT_ATTACKABLE = 0x00000100,           ///< (OOC Out Of Combat) Can not be attacked when not in combat. Removed if unit for some reason enter combat (flag probably removed for the attacked and it's party/group only) \todo Needs more documentation
        UNIT_FLAG_PASSIVE = 0x00000200,           ///< makes you unable to attack everything. Almost identical to our "civilian"-term. Will ignore it's surroundings and not engage in combat unless "called upon" or engaged by another unit.
        UNIT_FLAG_PVP = 0x00001000,
        UNIT_FLAG_SILENCED = 0x00002000,           ///< silenced, 2.1.1
        UNIT_FLAG_UNK_14 = 0x00004000,           ///< 2.0.8
        UNIT_FLAG_UNK_15 = 0x00008000,           ///< related to jerky movement in water?
        UNIT_FLAG_UNK_16 = 0x00010000,           ///< removes attackable icon
        UNIT_FLAG_PACIFIED = 0x00020000,
        UNIT_FLAG_DISABLE_ROTATE = 0x00040000,
        UNIT_FLAG_IN_COMBAT = 0x00080000,
        UNIT_FLAG_NOT_SELECTABLE = 0x02000000,
        UNIT_FLAG_SKINNABLE = 0x04000000,
        UNIT_FLAG_AURAS_VISIBLE = 0x08000000,           ///< magic detect
        UNIT_FLAG_SHEATHE = 0x40000000,
        // UNIT_FLAG_UNK_31              = 0x80000000           // no affect in 1.12.1

        UNIT_FLAG_NOT_ATTACKABLE_1 = 0x00000080,           ///< ?? (UNIT_FLAG_PVP_ATTACKABLE | UNIT_FLAG_NOT_ATTACKABLE_1) is NON_PVP_ATTACKABLE
        UNIT_FLAG_LOOTING = 0x00000400,           ///< loot animation
        UNIT_FLAG_PET_IN_COMBAT = 0x00000800,           ///< in combat?, 2.0.8
        UNIT_FLAG_STUNNED = 0x00040000,           ///< stunned, 2.1.1
        UNIT_FLAG_TAXI_FLIGHT = 0x00100000,           ///< disable casting at client side spell not allowed by taxi flight (mounted?), probably used with 0x4 flag
        UNIT_FLAG_DISARMED = 0x00200000,           ///< disable melee spells casting..., "Required melee weapon" added to melee spells tooltip.
        UNIT_FLAG_CONFUSED = 0x00400000,
        UNIT_FLAG_FLEEING = 0x00800000,
        UNIT_FLAG_POSSESSED = 0x01000000,           ///< used in spell Eyes of the Beast for pet... let attack by controlled creature |// Unit is under remote control by another unit, movement checks disabled, paired with loss of client control packet. New master is allowed to use melee attack and can't select this unit via mouse in the world (as if it was own character).
        UNIT_FLAG_UNK_28 = 0x10000000,
        UNIT_FLAG_UNK_29 = 0x20000000            ///< used in Feign Death spell
    }
    [Flags]
    public enum NPCFlags
    {
        UNIT_NPC_FLAG_NONE = 0x00000000,
        UNIT_NPC_FLAG_GOSSIP = 0x00000001,       ///< 100%
        UNIT_NPC_FLAG_QUESTGIVER = 0x00000002,       ///< 100%
        UNIT_NPC_FLAG_VENDOR = 0x00000004,       ///< 100%
        UNIT_NPC_FLAG_FLIGHTMASTER = 0x00000008,       ///< 100%
        UNIT_NPC_FLAG_TRAINER = 0x00000010,       ///< 100%
        UNIT_NPC_FLAG_SPIRITHEALER = 0x00000020,       ///< guessed
        UNIT_NPC_FLAG_SPIRITGUIDE = 0x00000040,       ///< guessed
        UNIT_NPC_FLAG_INNKEEPER = 0x00000080,       ///< 100%
        UNIT_NPC_FLAG_BANKER = 0x00000100,       ///< 100%
        UNIT_NPC_FLAG_PETITIONER = 0x00000200,       ///< 100% 0xC0000 = guild petitions
        UNIT_NPC_FLAG_TABARDDESIGNER = 0x00000400,       ///< 100%
        UNIT_NPC_FLAG_BATTLEMASTER = 0x00000800,       ///< 100%
        UNIT_NPC_FLAG_AUCTIONEER = 0x00001000,       ///< 100%
        UNIT_NPC_FLAG_STABLEMASTER = 0x00002000,       ///< 100%
        UNIT_NPC_FLAG_REPAIR = 0x00004000,       ///< 100%
        UNIT_NPC_FLAG_SPELLCLICK = 0x01000000,       // cause client to send 1015 opcode (spell click), dynamic, set at loading and don't must be set in DB
        UNIT_NPC_FLAG_OUTDOORPVP = 0x20000000        ///< custom flag for outdoor pvp creatures || Custom flag
    }
    public enum DiminishingLevels
    {
        DIMINISHING_LEVEL_1 = 0,         ///<Won't make a difference to stun duration
        DIMINISHING_LEVEL_2 = 1,         ///<Reduces stun time by 50%
        DIMINISHING_LEVEL_3 = 2,         ///<Reduces stun time by 75%
        DIMINISHING_LEVEL_IMMUNE = 3          ///<The target is immune to the DiminishingGrouop
    }
    public enum MeleeHitOutcome
    {
        MELEE_HIT_EVADE = 0,
        MELEE_HIT_MISS = 1,
        MELEE_HIT_DODGE = 2,     ///< used as misc in SPELL_AURA_IGNORE_COMBAT_RESULT
        MELEE_HIT_BLOCK = 3,     ///< used as misc in SPELL_AURA_IGNORE_COMBAT_RESULT
        MELEE_HIT_PARRY = 4,     ///< used as misc in SPELL_AURA_IGNORE_COMBAT_RESULT
        MELEE_HIT_GLANCING = 5,
        MELEE_HIT_CRIT = 6,
        MELEE_HIT_CRUSHING = 7,
        MELEE_HIT_NORMAL = 8,
        MELEE_HIT_BLOCK_CRIT = 9,
    }

    public enum SpellAuraProcResult
    {
        SPELL_AURA_PROC_OK = 0,                    // proc was processed, will remove charges
        SPELL_AURA_PROC_FAILED = 1,                    // proc failed - if at least one aura failed the proc, charges won't be taken
        SPELL_AURA_PROC_CANT_TRIGGER = 2                     // aura can't trigger - skip charges taking, move to next aura if exists
    }

    public enum CurrentSpellTypes
    {
        CURRENT_MELEE_SPELL = 0,
        CURRENT_GENERIC_SPELL = 1,
        CURRENT_AUTOREPEAT_SPELL = 2,
        CURRENT_CHANNELED_SPELL = 3
    }
    public enum ActiveStates
    {
        ACT_PASSIVE = 0x01,                                    // 0x01 - passive
        ACT_DISABLED = 0x81,                                    // 0x80 - castable
        ACT_ENABLED = 0xC1,                                    // 0x40 | 0x80 - auto cast + castable
        ACT_COMMAND = 0x07,                                    // 0x01 | 0x02 | 0x04
        ACT_REACTION = 0x06,                                    // 0x02 | 0x04
        ACT_DECIDE = 0x00                                     // custom
    }
    public enum ReactStates
    {
        REACT_PASSIVE = 0,
        REACT_DEFENSIVE = 1,
        REACT_AGGRESSIVE = 2
    }
    public enum CommandStates
    {
        COMMAND_STAY = 0,
        COMMAND_FOLLOW = 1,
        COMMAND_ATTACK = 2,
        COMMAND_ABANDON = 3
    }
    public enum ControlledUnitMask
    {
        CONTROLLED_PET = 0x01,
        CONTROLLED_MINIPET = 0x02,
        CONTROLLED_GUARDIANS = 0x04,                            // including PROTECTOR_PET
        CONTROLLED_CHARM = 0x08,
        CONTROLLED_TOTEMS = 0x10,
    }
    public enum ReactiveType
    {
        REACTIVE_DEFENSE = 1,
        REACTIVE_HUNTER_PARRY = 2,
        REACTIVE_OVERPOWER = 5
    }
    public enum PowerDefaults
    {
        POWER_RAGE_DEFAULT = 1000,
        POWER_FOCUS_DEFAULT = 100,
        POWER_ENERGY_DEFAULT = 100,
        POWER_HAPPINESS_DEFAULT = 1000000,
    }
}
