using BotRunner.Interfaces;

namespace WoWSharpClient.Models
{
    public class Player(byte[] lowGuid, byte[] highGuid) : Unit(lowGuid, highGuid, WoWObjectType.Player)
    {
        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");

        public uint GuildId { get; internal set; }
        enum SpellModType
        {
            SPELLMOD_FLAT = 107,                      // SPELL_AURA_ADD_FLAT_MODIFIER
            SPELLMOD_PCT = 108                       // SPELL_AURA_ADD_PCT_MODIFIER
        }
        enum PlayerUnderwaterState
        {
            UNDERWATER_NONE = 0x00,
            UNDERWATER_INWATER = 0x01,                     // terrain type is water and player is afflicted by it
            UNDERWATER_INLAVA = 0x02,                     // terrain type is lava and player is afflicted by it
            UNDERWATER_INSLIME = 0x04,                     // terrain type is lava and player is afflicted by it
            UNDERWATER_INDARKWATER = 0x08,                     // terrain type is dark water and player is afflicted by it

            UNDERWATER_EXIST_TIMERS = 0x10
        }
        enum BuyBankSlotResult
        {
            ERR_BANKSLOT_FAILED_TOO_MANY = 0,
            ERR_BANKSLOT_INSUFFICIENT_FUNDS = 1,
            ERR_BANKSLOT_NOTBANKER = 2,
            ERR_BANKSLOT_OK = 3
        }
        enum PlayerSpellState
        {
            PLAYERSPELL_UNCHANGED = 0,
            PLAYERSPELL_CHANGED = 1,
            PLAYERSPELL_NEW = 2,
            PLAYERSPELL_REMOVED = 3
        }
        enum RaidGroupError
        {
            ERR_RAID_GROUP_REQUIRED = 1,
            ERR_RAID_GROUP_FULL = 2
        }
        enum DrunkenState
        {
            DRUNKEN_SOBER = 0,
            DRUNKEN_TIPSY = 1,
            DRUNKEN_DRUNK = 2,
            DRUNKEN_SMASHED = 3
        }
        enum TYPE_OF_HONOR
        {
            HONORABLE = 1,
            DISHONORABLE = 2,
        }
        enum HonorKillState
        {
            HK_NEW = 0,
            HK_OLD = 1,
            HK_DELETED = 2,
            HK_UNCHANGED = 3
        }
        enum PlayerFlags
        {
            PLAYER_FLAGS_NONE = 0x00000000,
            PLAYER_FLAGS_GROUP_LEADER = 0x00000001,
            PLAYER_FLAGS_AFK = 0x00000002,
            PLAYER_FLAGS_DND = 0x00000004,
            PLAYER_FLAGS_GM = 0x00000008,
            PLAYER_FLAGS_GHOST = 0x00000010,
            PLAYER_FLAGS_RESTING = 0x00000020,
            PLAYER_FLAGS_UNK7 = 0x00000040,       // admin?
            PLAYER_FLAGS_FFA_PVP = 0x00000080,
            PLAYER_FLAGS_CONTESTED_PVP = 0x00000100,       // Player has been involved in a PvP combat and will be attacked by contested guards
            PLAYER_FLAGS_IN_PVP = 0x00000200,
            PLAYER_FLAGS_HIDE_HELM = 0x00000400,
            PLAYER_FLAGS_HIDE_CLOAK = 0x00000800,
            PLAYER_FLAGS_PARTIAL_PLAY_TIME = 0x00001000,       // played long time
            PLAYER_FLAGS_NO_PLAY_TIME = 0x00002000,       // played too long time
            PLAYER_FLAGS_UNK15 = 0x00004000,
            PLAYER_FLAGS_UNK16 = 0x00008000,       // strange visual effect (2.0.1), looks like PLAYER_FLAGS_GHOST flag
            PLAYER_FLAGS_SANCTUARY = 0x00010000,       // player entered sanctuary
            PLAYER_FLAGS_TAXI_BENCHMARK = 0x00020000,       // taxi benchmark mode (on/off) (2.0.1)
            PLAYER_FLAGS_PVP_TIMER = 0x00040000,       // 3.0.2, pvp timer active (after you disable pvp manually)
            PLAYER_FLAGS_XP_USER_DISABLED = 0x02000000,
        }
        enum PlayerFieldByteFlags
        {
            PLAYER_FIELD_BYTE_TRACK_STEALTHED = 0x02,
            PLAYER_FIELD_BYTE_RELEASE_TIMER = 0x08,             // Display time till auto release spirit
            PLAYER_FIELD_BYTE_NO_RELEASE_WINDOW = 0x10              // Display no "release spirit" window at all
        }
        enum PlayerFieldByte2Flags
        {
            PLAYER_FIELD_BYTE2_NONE = 0x00,
            PLAYER_FIELD_BYTE2_DETECT_AMORE_0 = 0x02,            // SPELL_AURA_DETECT_AMORE, not used as value and maybe not relcted to, but used in code as base for mask apply
            PLAYER_FIELD_BYTE2_DETECT_AMORE_1 = 0x04,            // SPELL_AURA_DETECT_AMORE value 1
            PLAYER_FIELD_BYTE2_DETECT_AMORE_2 = 0x08,            // SPELL_AURA_DETECT_AMORE value 2
            PLAYER_FIELD_BYTE2_DETECT_AMORE_3 = 0x10,            // SPELL_AURA_DETECT_AMORE value 3
            PLAYER_FIELD_BYTE2_STEALTH = 0x20,
            PLAYER_FIELD_BYTE2_INVISIBILITY_GLOW = 0x40
        }
        enum MirrorTimerType
        {
            FATIGUE_TIMER = 0,
            BREATH_TIMER = 1,
            FIRE_TIMER = 2     // probably mistake. More like to FEIGN_DEATH_TIMER
        }
        enum PlayerExtraFlags
        {
            // gm abilities
            PLAYER_EXTRA_GM_ON = 0x0001,
            PLAYER_EXTRA_GM_ACCEPT_TICKETS = 0x0002,
            PLAYER_EXTRA_ACCEPT_WHISPERS = 0x0004,
            PLAYER_EXTRA_TAXICHEAT = 0x0008,
            PLAYER_EXTRA_GM_INVISIBLE = 0x0010,
            PLAYER_EXTRA_GM_CHAT = 0x0020,               // Show GM badge in chat messages
            PLAYER_EXTRA_AUCTION_NEUTRAL = 0x0040,
            PLAYER_EXTRA_AUCTION_ENEMY = 0x0080,               // overwrite PLAYER_EXTRA_AUCTION_NEUTRAL

            // other states
            PLAYER_EXTRA_PVP_DEATH = 0x0100                // store PvP death status until corpse creating.
        }
        enum AtLoginFlags
        {
            AT_LOGIN_NONE = 0x00,
            AT_LOGIN_RENAME = 0x01,
            AT_LOGIN_RESET_SPELLS = 0x02,
            AT_LOGIN_RESET_TALENTS = 0x04,
            // AT_LOGIN_CUSTOMIZE         = 0x08, -- used in post-3.x
            // AT_LOGIN_RESET_PET_TALENTS = 0x10, -- used in post-3.x
            AT_LOGIN_FIRST = 0x20,
        }
        enum QuestSlotOffsets
        {
            QUEST_ID_OFFSET = 0,
            QUEST_COUNT_STATE_OFFSET = 1,                        // including counters 6bits+6bits+6bits+6bits + state 8bits
            QUEST_TIME_OFFSET = 2
        }
        enum QuestSlotStateMask
        {
            QUEST_STATE_NONE = 0x0000,
            QUEST_STATE_COMPLETE = 0x0001,
            QUEST_STATE_FAIL = 0x0002
        }
        enum SkillUpdateState
        {
            SKILL_UNCHANGED = 0,
            SKILL_CHANGED = 1,
            SKILL_NEW = 2,
            SKILL_DELETED = 3
        }
        enum PlayerSlots
        {
            // first slot for item stored (in any way in player m_items data)
            PLAYER_SLOT_START = 0,
            // last+1 slot for item stored (in any way in player m_items data)
            PLAYER_SLOT_END = 118,
            PLAYER_SLOTS_COUNT = (PLAYER_SLOT_END - PLAYER_SLOT_START)
        }
        enum EquipmentSlots                                         // 19 slots
        {
            EQUIPMENT_SLOT_START = 0,
            EQUIPMENT_SLOT_HEAD = 0,
            EQUIPMENT_SLOT_NECK = 1,
            EQUIPMENT_SLOT_SHOULDERS = 2,
            EQUIPMENT_SLOT_BODY = 3,
            EQUIPMENT_SLOT_CHEST = 4,
            EQUIPMENT_SLOT_WAIST = 5,
            EQUIPMENT_SLOT_LEGS = 6,
            EQUIPMENT_SLOT_FEET = 7,
            EQUIPMENT_SLOT_WRISTS = 8,
            EQUIPMENT_SLOT_HANDS = 9,
            EQUIPMENT_SLOT_FINGER1 = 10,
            EQUIPMENT_SLOT_FINGER2 = 11,
            EQUIPMENT_SLOT_TRINKET1 = 12,
            EQUIPMENT_SLOT_TRINKET2 = 13,
            EQUIPMENT_SLOT_BACK = 14,
            EQUIPMENT_SLOT_MAINHAND = 15,
            EQUIPMENT_SLOT_OFFHAND = 16,
            EQUIPMENT_SLOT_RANGED = 17,
            EQUIPMENT_SLOT_TABARD = 18,
            EQUIPMENT_SLOT_END = 19
        }

        enum InventorySlots                                         // 4 slots
        {
            INVENTORY_SLOT_BAG_START = 19,
            INVENTORY_SLOT_BAG_END = 23
        }

        enum InventoryPackSlots                                     // 16 slots
        {
            INVENTORY_SLOT_ITEM_START = 23,
            INVENTORY_SLOT_ITEM_END = 39
        }

        enum BankItemSlots                                          // 28 slots
        {
            BANK_SLOT_ITEM_START = 39,
            BANK_SLOT_ITEM_END = 63
        }

        enum BankBagSlots                                           // 7 slots
        {
            BANK_SLOT_BAG_START = 63,
            BANK_SLOT_BAG_END = 69
        }

        enum BuyBackSlots                                           // 12 slots
        {
            // stored in m_buybackitems
            BUYBACK_SLOT_START = 69,
            BUYBACK_SLOT_END = 81
        }

        enum KeyRingSlots                                           // 32 slots
        {
            KEYRING_SLOT_START = 81,
            KEYRING_SLOT_END = 97
        }
        enum TradeSlots
        {
            TRADE_SLOT_COUNT = 7,
            TRADE_SLOT_TRADED_COUNT = 6,
            TRADE_SLOT_NONTRADED = 6
        }

        enum TransferAbortReason
        {
            TRANSFER_ABORT_MAX_PLAYERS = 0x01,     // Transfer Aborted: instance is full
            TRANSFER_ABORT_NOT_FOUND = 0x02,     // Transfer Aborted: instance not found
            TRANSFER_ABORT_TOO_MANY_INSTANCES = 0x03,     // You have entered too many instances recently.
            TRANSFER_ABORT_SILENTLY = 0x04,     // no message shown the same effect give values above 5
            TRANSFER_ABORT_ZONE_IN_COMBAT = 0x05,     // Unable to zone in while an encounter is in progress.
        }

        enum InstanceResetWarningType
        {
            RAID_INSTANCE_WARNING_HOURS = 1,                    // WARNING! %s is scheduled to reset in %d hour(s).
            RAID_INSTANCE_WARNING_MIN = 2,                    // WARNING! %s is scheduled to reset in %d minute(s)!
            RAID_INSTANCE_WARNING_MIN_SOON = 3,                    // WARNING! %s is scheduled to reset in %d minute(s). Please exit the zone or you will be returned to your bind location!
            RAID_INSTANCE_WELCOME = 4                     // Welcome to %s. This raid instance is scheduled to reset in %s.
        }

        enum RestType
        {
            REST_TYPE_NO = 0,
            REST_TYPE_IN_TAVERN = 1,
            REST_TYPE_IN_CITY = 2
        }

        enum DuelCompleteType
        {
            DUEL_INTERRUPTED = 0,
            DUEL_WON = 1,
            DUEL_FLED = 2
        }

        enum TeleportToOptions
        {
            TELE_TO_GM_MODE = 0x01,
            TELE_TO_NOT_LEAVE_TRANSPORT = 0x02,
            TELE_TO_NOT_LEAVE_COMBAT = 0x04,
            TELE_TO_NOT_UNSUMMON_PET = 0x08,
            TELE_TO_SPELL = 0x10,
        }

        /// Type of environmental damages
        enum EnvironmentalDamageType
        {
            DAMAGE_EXHAUSTED = 0,
            DAMAGE_DROWNING = 1,
            DAMAGE_FALL = 2,
            DAMAGE_LAVA = 3,
            DAMAGE_SLIME = 4,
            DAMAGE_FIRE = 5,
            DAMAGE_FALL_TO_VOID = 6                         // custom case for fall without durability loss
        }

        enum PlayedTimeIndex
        {
            PLAYED_TIME_TOTAL = 0,
            PLAYED_TIME_LEVEL = 1
        }
        enum PlayerLoginQueryIndex
        {
            PLAYER_LOGIN_QUERY_LOADFROM,
            PLAYER_LOGIN_QUERY_LOADGROUP,
            PLAYER_LOGIN_QUERY_LOADBOUNDINSTANCES,
            PLAYER_LOGIN_QUERY_LOADAURAS,
            PLAYER_LOGIN_QUERY_LOADSPELLS,
            PLAYER_LOGIN_QUERY_LOADQUESTSTATUS,
            PLAYER_LOGIN_QUERY_LOADHONORCP,
            PLAYER_LOGIN_QUERY_LOADREPUTATION,
            PLAYER_LOGIN_QUERY_LOADINVENTORY,
            PLAYER_LOGIN_QUERY_LOADITEMLOOT,
            PLAYER_LOGIN_QUERY_LOADACTIONS,
            PLAYER_LOGIN_QUERY_LOADSOCIALLIST,
            PLAYER_LOGIN_QUERY_LOADHOMEBIND,
            PLAYER_LOGIN_QUERY_LOADSPELLCOOLDOWNS,
            PLAYER_LOGIN_QUERY_LOADGUILD,
            PLAYER_LOGIN_QUERY_LOADBGDATA,
            PLAYER_LOGIN_QUERY_LOADSKILLS,
            PLAYER_LOGIN_QUERY_LOADMAILS,
            PLAYER_LOGIN_QUERY_LOADMAILEDITEMS,

            MAX_PLAYER_LOGIN_QUERY
        }

        enum PlayerDelayedOperations
        {
            DELAYED_SAVE_PLAYER = 0x01,
            DELAYED_RESURRECT_PLAYER = 0x02,
            DELAYED_SPELL_CAST_DESERTER = 0x04,
            DELAYED_END
        }

        enum ReputationSource
        {
            REPUTATION_SOURCE_KILL,
            REPUTATION_SOURCE_QUEST,
            REPUTATION_SOURCE_SPELL
        }
        enum PlayerRestState
        {
            REST_STATE_RESTED = 0x01,
            REST_STATE_NORMAL = 0x02,
            REST_STATE_RAF_LINKED = 0x04                      // Exact use unknown
        }

        enum PlayerMountResult
        {
            MOUNTRESULT_INVALIDMOUNTEE = 0,    // You can't mount that unit!
            MOUNTRESULT_TOOFARAWAY = 1,    // That mount is too far away!
            MOUNTRESULT_ALREADYMOUNTED = 2,    // You're already mounted!
            MOUNTRESULT_NOTMOUNTABLE = 3,    // That unit can't be mounted!
            MOUNTRESULT_NOTYOURPET = 4,    // That mount isn't your pet!
            MOUNTRESULT_OTHER = 5,    // internal
            MOUNTRESULT_LOOTING = 6,    // You can't mount while looting!
            MOUNTRESULT_RACECANTMOUNT = 7,    // You can't mount because of your race!
            MOUNTRESULT_SHAPESHIFTED = 8,    // You can't mount while shapeshifted!
            MOUNTRESULT_FORCEDDISMOUNT = 9,    // You dismount before continuing.
            MOUNTRESULT_OK = 10    // no error
        }

        enum PlayerDismountResult
        {
            DISMOUNTRESULT_NOPET = 0,    // internal
            DISMOUNTRESULT_NOTMOUNTED = 1,    // You're not mounted!
            DISMOUNTRESULT_NOTYOURPET = 2,    // internal
            DISMOUNTRESULT_OK = 3     // no error
        }
    }
}
