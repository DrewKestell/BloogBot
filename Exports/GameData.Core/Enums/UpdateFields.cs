namespace GameData.Core.Enums
{
    public class UpdateFields
    {
        public enum EObjectFields : uint
        {
            OBJECT_FIELD_GUID = 0x00, // Size:2
            OBJECT_FIELD_TYPE = 0x02, // Size:1
            OBJECT_FIELD_ENTRY = 0x03, // Size:1
            OBJECT_FIELD_SCALE_X = 0x04, // Size:1
            OBJECT_FIELD_PADDING = 0x05, // Size:1
            OBJECT_END = 0x06,
        }

        public enum EItemFields : uint
        {
            ITEM_FIELD_OWNER = EObjectFields.OBJECT_END + 0x00, // Size:2
            ITEM_FIELD_CONTAINED = EObjectFields.OBJECT_END + 0x02, // Size:2
            ITEM_FIELD_CREATOR = EObjectFields.OBJECT_END + 0x04, // Size:2
            ITEM_FIELD_GIFTCREATOR = EObjectFields.OBJECT_END + 0x06, // Size:2
            ITEM_FIELD_STACK_COUNT = EObjectFields.OBJECT_END + 0x08, // Size:1
            ITEM_FIELD_DURATION = EObjectFields.OBJECT_END + 0x09, // Size:1
            ITEM_FIELD_SPELL_CHARGES = EObjectFields.OBJECT_END + 0x0A, // Size:5
            ITEM_FIELD_SPELL_CHARGES_01 = EObjectFields.OBJECT_END + 0x0B,
            ITEM_FIELD_SPELL_CHARGES_02 = EObjectFields.OBJECT_END + 0x0C,
            ITEM_FIELD_SPELL_CHARGES_03 = EObjectFields.OBJECT_END + 0x0D,
            ITEM_FIELD_SPELL_CHARGES_04 = EObjectFields.OBJECT_END + 0x0E,
            ITEM_FIELD_FLAGS = EObjectFields.OBJECT_END + 0x0F, // Size:1
            ITEM_FIELD_ENCHANTMENT = EObjectFields.OBJECT_END + 0x10, // count=21
            ITEM_FIELD_PROPERTY_SEED = EObjectFields.OBJECT_END + 0x25, // Size:1
            ITEM_FIELD_RANDOM_PROPERTIES_ID = EObjectFields.OBJECT_END + 0x26, // Size:1
            ITEM_FIELD_ITEM_TEXT_ID = EObjectFields.OBJECT_END + 0x27, // Size:1
            ITEM_FIELD_DURABILITY = EObjectFields.OBJECT_END + 0x28, // Size:1
            ITEM_FIELD_MAXDURABILITY = EObjectFields.OBJECT_END + 0x29, // Size:1
            ITEM_END = EObjectFields.OBJECT_END + 0x2A,
        }

        public enum EContainerFields : uint
        {
            CONTAINER_FIELD_NUM_SLOTS = EItemFields.ITEM_END + 0x00, // Size:1
            CONTAINER_ALIGN_PAD = EItemFields.ITEM_END + 0x01, // Size:1
            CONTAINER_FIELD_SLOT_1 = EItemFields.ITEM_END + 0x02, // count=56
            CONTAINER_FIELD_SLOT_LAST = EItemFields.ITEM_END + 0x38,
            CONTAINER_END = EItemFields.ITEM_END + 0x3A,
        }

        /**
         * Fields that are available for a \ref Unit ?
         * \see Object::HasFlag
         * \todo Document this properly!
         */
        public enum EUnitFields : uint
        {
            UNIT_FIELD_CHARM = 0x00 + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_SUMMON = 0x02 + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_CHARMEDBY = 0x04 + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_SUMMONEDBY = 0x06 + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_CREATEDBY = 0x08 + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_TARGET = 0x0A + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_PERSUADED = 0x0C + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_CHANNEL_OBJECT = 0x0E + EObjectFields.OBJECT_END, // Size:2
            UNIT_FIELD_HEALTH = 0x10 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_POWER1 = 0x11 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_POWER2 = 0x12 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_POWER3 = 0x13 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_POWER4 = 0x14 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_POWER5 = 0x15 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXHEALTH = 0x16 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXPOWER1 = 0x17 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXPOWER2 = 0x18 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXPOWER3 = 0x19 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXPOWER4 = 0x1A + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXPOWER5 = 0x1B + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_LEVEL = 0x1C + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_FACTIONTEMPLATE = 0x1D + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_BYTES_0 = 0x1E + EObjectFields.OBJECT_END, // Size:1
            UNIT_VIRTUAL_ITEM_SLOT_DISPLAY = 0x1F + EObjectFields.OBJECT_END, // Size:3
            UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_01 = 0x20 + EObjectFields.OBJECT_END,
            UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02 = 0x21 + EObjectFields.OBJECT_END,
            UNIT_VIRTUAL_ITEM_INFO = 0x22 + EObjectFields.OBJECT_END, // Size:6
            UNIT_VIRTUAL_ITEM_INFO_01 = 0x23 + EObjectFields.OBJECT_END,
            UNIT_VIRTUAL_ITEM_INFO_02 = 0x24 + EObjectFields.OBJECT_END,
            UNIT_VIRTUAL_ITEM_INFO_03 = 0x25 + EObjectFields.OBJECT_END,
            UNIT_VIRTUAL_ITEM_INFO_04 = 0x26 + EObjectFields.OBJECT_END,
            UNIT_VIRTUAL_ITEM_INFO_05 = 0x27 + EObjectFields.OBJECT_END,
            UNIT_FIELD_FLAGS = 0x28 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_AURA = 0x29 + EObjectFields.OBJECT_END, // Size:48
            UNIT_FIELD_AURA_LAST = 0x58 + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURAFLAGS = 0x59 + EObjectFields.OBJECT_END, // Size:6
            UNIT_FIELD_AURAFLAGS_01 = 0x5a + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURAFLAGS_02 = 0x5b + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURAFLAGS_03 = 0x5c + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURAFLAGS_04 = 0x5d + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURAFLAGS_05 = 0x5e + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURALEVELS = 0x5f + EObjectFields.OBJECT_END, // Size:12
            UNIT_FIELD_AURALEVELS_LAST = 0x6a + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURAAPPLICATIONS = 0x6b + EObjectFields.OBJECT_END, // Size:12
            UNIT_FIELD_AURAAPPLICATIONS_LAST = 0x76 + EObjectFields.OBJECT_END,
            UNIT_FIELD_AURASTATE = 0x77 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_BASEATTACKTIME = 0x78 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_OFFHANDATTACKTIME = 0x79 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_RANGEDATTACKTIME = 0x7a + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_BOUNDINGRADIUS = 0x7b + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_COMBATREACH = 0x7c + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_DISPLAYID = 0x7d + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_NATIVEDISPLAYID = 0x7e + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MOUNTDISPLAYID = 0x7f + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MINDAMAGE = 0x80 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXDAMAGE = 0x81 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MINOFFHANDDAMAGE = 0x82 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXOFFHANDDAMAGE = 0x83 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_BYTES_1 = 0x84 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_PETNUMBER = 0x85 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_PET_NAME_TIMESTAMP = 0x86 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_PETEXPERIENCE = 0x87 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_PETNEXTLEVELEXP = 0x88 + EObjectFields.OBJECT_END, // Size:1
            UNIT_DYNAMIC_FLAGS = 0x89 + EObjectFields.OBJECT_END, // Size:1
            UNIT_CHANNEL_SPELL = 0x8a + EObjectFields.OBJECT_END, // Size:1
            UNIT_MOD_CAST_SPEED = 0x8b + EObjectFields.OBJECT_END, // Size:1
            UNIT_CREATED_BY_SPELL = 0x8c + EObjectFields.OBJECT_END, // Size:1
            UNIT_NPC_FLAGS = 0x8d + EObjectFields.OBJECT_END, // Size:1
            UNIT_NPC_EMOTESTATE = 0x8e + EObjectFields.OBJECT_END, // Size:1
            UNIT_TRAINING_POINTS = 0x8f + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_STAT0 = 0x90 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_STAT1 = 0x91 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_STAT2 = 0x92 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_STAT3 = 0x93 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_STAT4 = 0x94 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_RESISTANCES = 0x95 + EObjectFields.OBJECT_END, // Size:7
            UNIT_FIELD_RESISTANCES_01 = 0x96 + EObjectFields.OBJECT_END,
            UNIT_FIELD_RESISTANCES_02 = 0x97 + EObjectFields.OBJECT_END,
            UNIT_FIELD_RESISTANCES_03 = 0x98 + EObjectFields.OBJECT_END,
            UNIT_FIELD_RESISTANCES_04 = 0x99 + EObjectFields.OBJECT_END,
            UNIT_FIELD_RESISTANCES_05 = 0x9a + EObjectFields.OBJECT_END,
            UNIT_FIELD_RESISTANCES_06 = 0x9b + EObjectFields.OBJECT_END,
            UNIT_FIELD_BASE_MANA = 0x9c + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_BASE_HEALTH = 0x9d + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_BYTES_2 = 0x9e + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_ATTACK_POWER = 0x9f + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_ATTACK_POWER_MODS = 0xa0 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_ATTACK_POWER_MULTIPLIER = 0xa1 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_RANGED_ATTACK_POWER = 0xa2 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = 0xa3 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = 0xa4 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MINRANGEDDAMAGE = 0xa5 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_MAXRANGEDDAMAGE = 0xa6 + EObjectFields.OBJECT_END, // Size:1
            UNIT_FIELD_POWER_COST_MODIFIER = 0xa7 + EObjectFields.OBJECT_END, // Size:7
            UNIT_FIELD_POWER_COST_MODIFIER_01 = 0xa8 + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MODIFIER_02 = 0xa9 + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MODIFIER_03 = 0xaa + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MODIFIER_04 = 0xab + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MODIFIER_05 = 0xac + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MODIFIER_06 = 0xad + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MULTIPLIER = 0xae + EObjectFields.OBJECT_END, // Size:7
            UNIT_FIELD_POWER_COST_MULTIPLIER_01 = 0xaf + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MULTIPLIER_02 = 0xb0 + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MULTIPLIER_03 = 0xb1 + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MULTIPLIER_04 = 0xb2 + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MULTIPLIER_05 = 0xb3 + EObjectFields.OBJECT_END,
            UNIT_FIELD_POWER_COST_MULTIPLIER_06 = 0xb4 + EObjectFields.OBJECT_END,
            UNIT_FIELD_PADDING = 0xb5 + EObjectFields.OBJECT_END,
            UNIT_END = 0xb6 + EObjectFields.OBJECT_END,
        }

        public enum EPlayerFields : uint
        {
            PLAYER_DUEL_ARBITER = 0x388,
            PLAYER_FLAGS = 0x390,
            PLAYER_GUILDID = 0x394,
            PLAYER_GUILDRANK = 0x398,
            PLAYER_BYTES = 0x39C,
            PLAYER_BYTES_2 = 0x3A0,
            PLAYER_BYTES_3 = 0x3A4,
            PLAYER_DUEL_TEAM = 0x3A8,
            PLAYER_GUILD_TIMESTAMP = 0x3AC,
            PLAYER_QUEST_LOG_1_1 = 0x3B0,
            PLAYER_QUEST_LOG_1_2 = 0x3B4,
            PLAYER_QUEST_LOG_2_1 = 0x3BC,
            PLAYER_QUEST_LOG_2_2 = 0x3C0,
            PLAYER_QUEST_LOG_3_1 = 0x3C8,
            PLAYER_QUEST_LOG_3_2 = 0x3CC,
            PLAYER_QUEST_LOG_4_1 = 0x3D4,
            PLAYER_QUEST_LOG_4_2 = 0x3D8,
            PLAYER_QUEST_LOG_5_1 = 0x3E0,
            PLAYER_QUEST_LOG_5_2 = 0x3E4,
            PLAYER_QUEST_LOG_6_1 = 0x3EC,
            PLAYER_QUEST_LOG_6_2 = 0x3F0,
            PLAYER_QUEST_LOG_7_1 = 0x3F8,
            PLAYER_QUEST_LOG_7_2 = 0x3FC,
            PLAYER_QUEST_LOG_8_1 = 0x404,
            PLAYER_QUEST_LOG_8_2 = 0x408,
            PLAYER_QUEST_LOG_9_1 = 0x410,
            PLAYER_QUEST_LOG_9_2 = 0x414,
            PLAYER_QUEST_LOG_10_1 = 0x41C,
            PLAYER_QUEST_LOG_10_2 = 0x420,
            PLAYER_QUEST_LOG_11_1 = 0x428,
            PLAYER_QUEST_LOG_11_2 = 0x42C,
            PLAYER_QUEST_LOG_12_1 = 0x434,
            PLAYER_QUEST_LOG_12_2 = 0x438,
            PLAYER_QUEST_LOG_13_1 = 0x440,
            PLAYER_QUEST_LOG_13_2 = 0x444,
            PLAYER_QUEST_LOG_14_1 = 0x44C,
            PLAYER_QUEST_LOG_14_2 = 0x450,
            PLAYER_QUEST_LOG_15_1 = 0x458,
            PLAYER_QUEST_LOG_15_2 = 0x45C,
            PLAYER_QUEST_LOG_16_1 = 0x464,
            PLAYER_QUEST_LOG_16_2 = 0x468,
            PLAYER_QUEST_LOG_17_1 = 0x470,
            PLAYER_QUEST_LOG_17_2 = 0x474,
            PLAYER_QUEST_LOG_18_1 = 0x47C,
            PLAYER_QUEST_LOG_18_2 = 0x480,
            PLAYER_QUEST_LOG_19_1 = 0x488,
            PLAYER_QUEST_LOG_19_2 = 0x48C,
            PLAYER_QUEST_LOG_20_1 = 0x494,
            PLAYER_QUEST_LOG_20_2 = 0x498,
            PLAYER_QUEST_LOG_21_1 = 0x4A0,
            PLAYER_QUEST_LOG_21_2 = 0x4A4,
            PLAYER_QUEST_LOG_22_1 = 0x4AC,
            PLAYER_QUEST_LOG_22_2 = 0x4B0,
            PLAYER_QUEST_LOG_23_1 = 0x4B8,
            PLAYER_QUEST_LOG_23_2 = 0x4BC,
            PLAYER_QUEST_LOG_24_1 = 0x4C4,
            PLAYER_QUEST_LOG_24_2 = 0x4C8,
            PLAYER_QUEST_LOG_25_1 = 0x4D0,
            PLAYER_QUEST_LOG_25_2 = 0x4D4,
            PLAYER_VISIBLE_ITEM_1_CREATOR = 0x4DC,
            PLAYER_VISIBLE_ITEM_1_0 = 0x4E4,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = 0x514,
            PLAYER_VISIBLE_ITEM_1_PAD = 0x518,
            PLAYER_VISIBLE_ITEM_2_CREATOR = 0x51C,
            PLAYER_VISIBLE_ITEM_2_0 = 0x524,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = 0x554,
            PLAYER_VISIBLE_ITEM_2_PAD = 0x558,
            PLAYER_VISIBLE_ITEM_3_CREATOR = 0x55C,
            PLAYER_VISIBLE_ITEM_3_0 = 0x564,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = 0x594,
            PLAYER_VISIBLE_ITEM_3_PAD = 0x598,
            PLAYER_VISIBLE_ITEM_4_CREATOR = 0x59C,
            PLAYER_VISIBLE_ITEM_4_0 = 0x5A4,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = 0x5D4,
            PLAYER_VISIBLE_ITEM_4_PAD = 0x5D8,
            PLAYER_VISIBLE_ITEM_5_CREATOR = 0x5DC,
            PLAYER_VISIBLE_ITEM_5_0 = 0x5E4,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = 0x614,
            PLAYER_VISIBLE_ITEM_5_PAD = 0x618,
            PLAYER_VISIBLE_ITEM_6_CREATOR = 0x61C,
            PLAYER_VISIBLE_ITEM_6_0 = 0x624,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = 0x654,
            PLAYER_VISIBLE_ITEM_6_PAD = 0x658,
            PLAYER_VISIBLE_ITEM_7_CREATOR = 0x65C,
            PLAYER_VISIBLE_ITEM_7_0 = 0x664,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = 0x694,
            PLAYER_VISIBLE_ITEM_7_PAD = 0x698,
            PLAYER_VISIBLE_ITEM_8_CREATOR = 0x69C,
            PLAYER_VISIBLE_ITEM_8_0 = 0x6A4,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = 0x6D4,
            PLAYER_VISIBLE_ITEM_8_PAD = 0x6D8,
            PLAYER_VISIBLE_ITEM_9_CREATOR = 0x6DC,
            PLAYER_VISIBLE_ITEM_9_0 = 0x6E4,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = 0x714,
            PLAYER_VISIBLE_ITEM_9_PAD = 0x718,
            PLAYER_VISIBLE_ITEM_10_CREATOR = 0x71C,
            PLAYER_VISIBLE_ITEM_10_0 = 0x724,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = 0x754,
            PLAYER_VISIBLE_ITEM_10_PAD = 0x758,
            PLAYER_VISIBLE_ITEM_11_CREATOR = 0x75C,
            PLAYER_VISIBLE_ITEM_11_0 = 0x764,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = 0x794,
            PLAYER_VISIBLE_ITEM_11_PAD = 0x798,
            PLAYER_VISIBLE_ITEM_12_CREATOR = 0x79C,
            PLAYER_VISIBLE_ITEM_12_0 = 0x7A4,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = 0x7D4,
            PLAYER_VISIBLE_ITEM_12_PAD = 0x7D8,
            PLAYER_VISIBLE_ITEM_13_CREATOR = 0x7DC,
            PLAYER_VISIBLE_ITEM_13_0 = 0x7E4,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = 0x814,
            PLAYER_VISIBLE_ITEM_13_PAD = 0x818,
            PLAYER_VISIBLE_ITEM_14_CREATOR = 0x81C,
            PLAYER_VISIBLE_ITEM_14_0 = 0x824,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = 0x854,
            PLAYER_VISIBLE_ITEM_14_PAD = 0x858,
            PLAYER_VISIBLE_ITEM_15_CREATOR = 0x85C,
            PLAYER_VISIBLE_ITEM_15_0 = 0x864,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = 0x894,
            PLAYER_VISIBLE_ITEM_15_PAD = 0x898,
            PLAYER_VISIBLE_ITEM_16_CREATOR = 0x89C,
            PLAYER_VISIBLE_ITEM_16_0 = 0x8A4,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = 0x8D4,
            PLAYER_VISIBLE_ITEM_16_PAD = 0x8D8,
            PLAYER_VISIBLE_ITEM_17_CREATOR = 0x8DC,
            PLAYER_VISIBLE_ITEM_17_0 = 0x8E4,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = 0x914,
            PLAYER_VISIBLE_ITEM_17_PAD = 0x918,
            PLAYER_VISIBLE_ITEM_18_CREATOR = 0x91C,
            PLAYER_VISIBLE_ITEM_18_0 = 0x924,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = 0x954,
            PLAYER_VISIBLE_ITEM_18_PAD = 0x958,
            PLAYER_VISIBLE_ITEM_19_CREATOR = 0x95C,
            PLAYER_VISIBLE_ITEM_19_0 = 0x964,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = 0x994,
            PLAYER_VISIBLE_ITEM_19_PAD = 0x998,
            PLAYER_CHOSEN_TITLE = 0x99C,
            PLAYER_FIELD_INV_SLOT_HEAD = 0x9A0,
            PLAYER_FIELD_PACK_SLOT_1 = 0xA58,
            PLAYER_FIELD_BANK_SLOT_1 = 0xAD8,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 0xBB8,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = 0xBF0,
            PLAYER_FIELD_KEYRING_SLOT_1 = 0xC50,
            PLAYER_FARSIGHT = 0xD50,
            PLAYER__FIELD_KNOWN_TITLES = 0xD58,
            PLAYER_XP = 0xD60,
            PLAYER_NEXT_LEVEL_XP = 0xD64,
            PLAYER_SKILL_INFO_1_1 = 0xD68,
            PLAYER_CHARACTER_POINTS1 = 0x1368,
            PLAYER_CHARACTER_POINTS2 = 0x136C,
            PLAYER_TRACK_CREATURES = 0x1370,
            PLAYER_TRACK_RESOURCES = 0x1374,
            PLAYER_BLOCK_PERCENTAGE = 0x1378,
            PLAYER_DODGE_PERCENTAGE = 0x137C,
            PLAYER_PARRY_PERCENTAGE = 0x1380,
            PLAYER_CRIT_PERCENTAGE = 0x1384,
            PLAYER_RANGED_CRIT_PERCENTAGE = 0x1388,
            PLAYER_OFFHAND_CRIT_PERCENTAGE = 0x138C,
            PLAYER_SPELL_CRIT_PERCENTAGE1 = 0x1390,
            PLAYER_EXPLORED_ZONES_1 = 0x13AC,
            PLAYER_REST_STATE_EXPERIENCE = 0x14AC,
            PLAYER_FIELD_COINAGE = 0x14B0,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 0x14B4,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 0x14D0,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 0x14EC,
            PLAYER_FIELD_MOD_HEALING_DONE_POS = 0x1508,
            PLAYER_FIELD_MOD_TARGET_RESISTANCE = 0x150C,
            PLAYER_FIELD_BYTES = 0x1510,
            PLAYER_AMMO_ID = 0x1514,
            PLAYER_SELF_RES_SPELL = 0x1518,
            PLAYER_FIELD_PVP_MEDALS = 0x151C,
            PLAYER_FIELD_BUYBACK_PRICE_1 = 0x1520,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = 0x1550,
            PLAYER_FIELD_KILLS = 0x1580,
            PLAYER_FIELD_TODAY_CONTRIBUTION = 0x1584,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = 0x1588,
            PLAYER_FIELD_LIFETIME_HONORABLE_KILLS = 0x158C,
            PLAYER_FIELD_BYTES2 = 0x1590,
            PLAYER_FIELD_WATCHED_FACTION_INDEX = 0x1594,
            PLAYER_FIELD_COMBAT_RATING_1 = 0x1598,
            PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = 0x15F4,
            PLAYER_FIELD_HONOR_CURRENCY = 0x1630,
            PLAYER_FIELD_ARENA_CURRENCY = 0x1634,
            PLAYER_FIELD_MOD_MANA_REGEN = 0x1638,
            PLAYER_FIELD_MOD_MANA_REGEN_INTERRUPT = 0x163C,
            PLAYER_FIELD_MAX_LEVEL = 0x1640,
            PLAYER_FIELD_DAILY_QUESTS_1 = 0x1644,
            PLAYER_FIELD_PADDING = 0x166C,
            PLAYER_END = 0x166C + 1,

            // Additional fields for Player
            PLAYER_QUEST_LOG_LAST_3 = 0x45 + EUnitFields.UNIT_END,
            PLAYER_FIELD_PACK_SLOT_LAST = PLAYER_FIELD_PACK_SLOT_1 + 0x7F, // 32 slots * 4 bytes - 4
            PLAYER_FIELD_BANK_SLOT_LAST = PLAYER_FIELD_BANK_SLOT_1 + 0xDF, // 56 slots * 4 bytes - 4
            PLAYER_FIELD_BANKBAG_SLOT_LAST = PLAYER_FIELD_BANKBAG_SLOT_1 + 0x37, // 14 slots * 4 bytes - 4
            PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST = PLAYER_FIELD_VENDORBUYBACK_SLOT_1 + 0x5F, // 24 slots * 4 bytes - 4
            PLAYER_FIELD_KEYRING_SLOT_LAST = PLAYER_FIELD_KEYRING_SLOT_1 + 0xFF, // 64 slots * 4 bytes - 4
            PLAYER_FIELD_COMBO_TARGET = 0x20e + EUnitFields.UNIT_END, // Size:2
            PLAYER_FIELD_BUYBACK_PRICE_LAST = PLAYER_FIELD_BUYBACK_PRICE_1 + 0x2C, // 12 slots * 4 bytes - 4
            PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST = PLAYER_FIELD_BUYBACK_TIMESTAMP_1 + 0x2C, // 12 slots * 4 bytes - 4
            PLAYER_FIELD_YESTERDAY_KILLS = PLAYER_FIELD_KILLS + 0x04,
            PLAYER_FIELD_LAST_WEEK_KILLS = PLAYER_FIELD_KILLS + 0x08,
            PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS = 0x42c + EUnitFields.UNIT_END, // Size:1
            PLAYER_FIELD_POSSTAT0 = PLAYER_FIELD_MOD_DAMAGE_DONE_NEG - 0x14,
            PLAYER_FIELD_POSSTAT4 = PLAYER_FIELD_POSSTAT0 + 4,
            PLAYER_FIELD_NEGSTAT0 = PLAYER_FIELD_POSSTAT4 + 1,
            PLAYER_FIELD_NEGSTAT4 = PLAYER_FIELD_NEGSTAT0 + 4,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = PLAYER_FIELD_NEGSTAT4 + 1,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE + 7,
            PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = 0x42a + EUnitFields.UNIT_END, // Size:1
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = PLAYER_FIELD_TODAY_CONTRIBUTION + 1,
            PLAYER_FIELD_LAST_WEEK_RANK = PLAYER_FIELD_LAST_WEEK_CONTRIBUTION + 1,
        }

        public enum EGameObjectFields : uint
        {
            OBJECT_FIELD_CREATED_BY = EObjectFields.OBJECT_END + 0x00,
            GAMEOBJECT_DISPLAYID = EObjectFields.OBJECT_END + 0x02,
            GAMEOBJECT_FLAGS = EObjectFields.OBJECT_END + 0x03,
            GAMEOBJECT_ROTATION = EObjectFields.OBJECT_END + 0x04,
            GAMEOBJECT_STATE = EObjectFields.OBJECT_END + 0x08,
            GAMEOBJECT_POS_X = EObjectFields.OBJECT_END + 0x09,
            GAMEOBJECT_POS_Y = EObjectFields.OBJECT_END + 0x0A,
            GAMEOBJECT_POS_Z = EObjectFields.OBJECT_END + 0x0B,
            GAMEOBJECT_FACING = EObjectFields.OBJECT_END + 0x0C,
            GAMEOBJECT_DYN_FLAGS = EObjectFields.OBJECT_END + 0x0D,
            GAMEOBJECT_FACTION = EObjectFields.OBJECT_END + 0x0E,
            GAMEOBJECT_TYPE_ID = EObjectFields.OBJECT_END + 0x0F,
            GAMEOBJECT_LEVEL = EObjectFields.OBJECT_END + 0x10,
            GAMEOBJECT_ARTKIT = EObjectFields.OBJECT_END + 0x11,
            GAMEOBJECT_ANIMPROGRESS = EObjectFields.OBJECT_END + 0x12,
            GAMEOBJECT_PADDING = EObjectFields.OBJECT_END + 0x13,
            GAMEOBJECT_END = EObjectFields.OBJECT_END + 0x14,
        }

        public enum EDynamicObjectFields : uint
        {
            DYNAMICOBJECT_CASTER = EObjectFields.OBJECT_END + 0x00,
            DYNAMICOBJECT_BYTES = EObjectFields.OBJECT_END + 0x02,
            DYNAMICOBJECT_SPELLID = EObjectFields.OBJECT_END + 0x03,
            DYNAMICOBJECT_RADIUS = EObjectFields.OBJECT_END + 0x04,
            DYNAMICOBJECT_POS_X = EObjectFields.OBJECT_END + 0x05,
            DYNAMICOBJECT_POS_Y = EObjectFields.OBJECT_END + 0x06,
            DYNAMICOBJECT_POS_Z = EObjectFields.OBJECT_END + 0x07,
            DYNAMICOBJECT_FACING = EObjectFields.OBJECT_END + 0x08,
            DYNAMICOBJECT_PAD = EObjectFields.OBJECT_END + 0x09,
            DYNAMICOBJECT_END = EObjectFields.OBJECT_END + 0x0A,
        }

        public enum ECorpseFields : uint
        {
            CORPSE_FIELD_OWNER = EObjectFields.OBJECT_END + 0x00,
            CORPSE_FIELD_FACING = EObjectFields.OBJECT_END + 0x02,
            CORPSE_FIELD_POS_X = EObjectFields.OBJECT_END + 0x03,
            CORPSE_FIELD_POS_Y = EObjectFields.OBJECT_END + 0x04,
            CORPSE_FIELD_POS_Z = EObjectFields.OBJECT_END + 0x05,
            CORPSE_FIELD_DISPLAY_ID = EObjectFields.OBJECT_END + 0x06,
            CORPSE_FIELD_ITEM = EObjectFields.OBJECT_END + 0x07, // 19
            CORPSE_FIELD_BYTES_1 = EObjectFields.OBJECT_END + 0x1A,
            CORPSE_FIELD_BYTES_2 = EObjectFields.OBJECT_END + 0x1B,
            CORPSE_FIELD_GUILD = EObjectFields.OBJECT_END + 0x1C,
            CORPSE_FIELD_FLAGS = EObjectFields.OBJECT_END + 0x1D,
            CORPSE_FIELD_DYNAMIC_FLAGS = EObjectFields.OBJECT_END + 0x1E,
            CORPSE_FIELD_PAD = EObjectFields.OBJECT_END + 0x1F,
            CORPSE_END = EObjectFields.OBJECT_END + 0x20,
        }
    }
}
