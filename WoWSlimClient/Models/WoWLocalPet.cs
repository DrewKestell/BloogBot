namespace WoWSlimClient.Models
{
    public class WoWLocalPet(byte[] lowGuid, byte[] highGuid) : WoWUnit(lowGuid, highGuid)
    {
        public void Attack()
        {

        }

        public void FollowPlayer()
        {

        }

        public bool IsHappy()
        {
            return true;
        }

        public bool CanUse(string parPetSpell)
        {
            return true;
        }

        public void Cast(string parPetSpell)
        {
            
        }
        enum PetType
        {
            SUMMON_PET = 0,
            HUNTER_PET = 1,
            GUARDIAN_PET = 2,
            MINI_PET = 3,
            MAX_PET_TYPE = 4
        };

        // stored in character_pet.slot
        enum PetSaveMode
        {
            PET_SAVE_AS_DELETED = -1,                        // not saved in fact
            PET_SAVE_AS_CURRENT = 0,                        // in current slot (with player)
            PET_SAVE_FIRST_STABLE_SLOT = 1,
            PET_SAVE_LAST_STABLE_SLOT = 2,          // last in DB stable slot index (including), all higher have same meaning as PET_SAVE_NOT_IN_SLOT
            PET_SAVE_NOT_IN_SLOT = 100,                      // for avoid conflict with stable size grow will use 100
            PET_SAVE_REAGENTS = 101                       // PET_SAVE_NOT_IN_SLOT with reagents return
        };

        enum PetDatabaseStatus
        {
            PET_DB_NO_PET = 0,
            PET_DB_DEAD = 1,
            PET_DB_ALIVE = 2,
        }

        // There might be a lot more
        enum PetModeFlags
        {
            PET_MODE_UNKNOWN_0 = 0x0000001,
            PET_MODE_UNKNOWN_2 = 0x0000100,
            PET_MODE_DISABLE_ACTIONS = 0x8000000,

            // autoset in client at summon
            PET_MODE_DEFAULT = PET_MODE_UNKNOWN_0 | PET_MODE_UNKNOWN_2,
        }

        enum HappinessState
        {
            UNHAPPY = 1,
            CONTENT = 2,
            HAPPY = 3
        }

        enum LoyaltyLevel
        {
            REBELLIOUS = 1,
            UNRULY = 2,
            SUBMISSIVE = 3,
            DEPENDABLE = 4,
            FAITHFUL = 5,
            BEST_FRIEND = 6
        }

        enum PetSpellState
        {
            PETSPELL_UNCHANGED = 0,
            PETSPELL_CHANGED = 1,
            PETSPELL_NEW = 2,
            PETSPELL_REMOVED = 3
        }

        enum PetSpellType
        {
            PETSPELL_NORMAL = 0,
            PETSPELL_FAMILY = 1,
        }

        enum ActionFeedback
        {
            FEEDBACK_PET_NONE = 0,   // custom, not to be sent
            FEEDBACK_PET_DEAD = 1,
            FEEDBACK_NOTHING_TO_ATT = 2,
            FEEDBACK_CANT_ATT_TARGET = 3,
            FEEDBACK_NO_PATH_TO = 4
        }

        enum PetTalk
        {
            PET_TALK_SPECIAL_SPELL = 0,
            PET_TALK_ATTACK = 1
        }

        enum PetNameInvalidReason
        {
            // custom, not send
            PET_NAME_SUCCESS = 0,

            PET_NAME_INVALID = 1,
            PET_NAME_NO_NAME = 2,
            PET_NAME_TOO_SHORT = 3,
            PET_NAME_TOO_LONG = 4,
            PET_NAME_MIXED_LANGUAGES = 6,
            PET_NAME_PROFANE = 7,
            PET_NAME_RESERVED = 8,
            PET_NAME_THREE_CONSECUTIVE = 11,
            PET_NAME_INVALID_SPACE = 12,
            PET_NAME_CONSECUTIVE_SPACES = 13,
            PET_NAME_RUSSIAN_CONSECUTIVE_SILENT_CHARACTERS = 14,
            PET_NAME_RUSSIAN_SILENT_CHARACTER_AT_BEGINNING_OR_END = 15,
            PET_NAME_DECLENSION_DOESNT_MATCH_BASE_NAME = 16
        }
    }
}
