using Ionic.Zlib;
using System.Text;
using WoWSlimClient.Constants;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;
using static WoWSlimClient.Models.Enums;

namespace WoWSlimClient.Client
{
    internal class OpCodeDispatcher
    {
        public static OpCodeDispatcher Instance { get; } = new OpCodeDispatcher();
        private readonly Dictionary<uint, Action<byte[]>> _handlers = [];

        private OpCodeDispatcher()
        {
            _handlers[(uint)Opcodes.MSG_MOVE_FALL_LAND] = HandleMoveFallLand;
            _handlers[(uint)Opcodes.MSG_MOVE_FEATHER_FALL] = HandleMoveFeatherFall;
            _handlers[(uint)Opcodes.MSG_MOVE_HEARTBEAT] = HandleMoveHeartbeat;
            _handlers[(uint)Opcodes.MSG_MOVE_JUMP] = HandleMoveJump;
            _handlers[(uint)Opcodes.MSG_MOVE_KNOCK_BACK] = HandleMoveKnockBack;
            _handlers[(uint)Opcodes.MSG_MOVE_ROOT] = HandleMoveRoot;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_ALL_SPEED_CHEAT] = HandleMoveSetAllSpeedCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_FACING] = HandleMoveSetFacing;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_PITCH] = HandleMoveSetPitch;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_RUN_BACK_SPEED] = HandleMoveSetRunBackSpeed;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_RUN_BACK_SPEED_CHEAT] = HandleMoveSetRunBackSpeedCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_RUN_MODE] = HandleMoveSetRunMode;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_RUN_SPEED] = HandleMoveSetRunSpeed;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_RUN_SPEED_CHEAT] = HandleMoveSetRunSpeedCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_SWIM_BACK_SPEED] = HandleMoveSetSwimBackSpeed;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_SWIM_BACK_SPEED_CHEAT] = HandleMoveSetSwimBackSpeedCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_SWIM_SPEED] = HandleMoveSetSwimSpeed;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_SWIM_SPEED_CHEAT] = HandleMoveSetSwimSpeedCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_TURN_RATE] = HandleMoveSetTurnRate;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_TURN_RATE_CHEAT] = HandleMoveSetTurnRateCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_WALK_MODE] = HandleMoveSetWalkMode;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_WALK_SPEED] = HandleMoveSetWalkSpeed;
            _handlers[(uint)Opcodes.MSG_MOVE_SET_WALK_SPEED_CHEAT] = HandleMoveSetWalkSpeedCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_START_BACKWARD] = HandleMoveStartBackward;
            _handlers[(uint)Opcodes.MSG_MOVE_START_FORWARD] = HandleMoveStartForward;
            _handlers[(uint)Opcodes.MSG_MOVE_START_PITCH_DOWN] = HandleMoveStartPitchDown;
            _handlers[(uint)Opcodes.MSG_MOVE_START_PITCH_UP] = HandleMoveStartPitchUp;
            _handlers[(uint)Opcodes.MSG_MOVE_START_STRAFE_LEFT] = HandleMoveStartStrafeLeft;
            _handlers[(uint)Opcodes.MSG_MOVE_START_STRAFE_RIGHT] = HandleMoveStartStrafeRight;
            _handlers[(uint)Opcodes.MSG_MOVE_START_SWIM] = HandleMoveStartSwim;
            _handlers[(uint)Opcodes.MSG_MOVE_START_TURN_LEFT] = HandleMoveStartTurnLeft;
            _handlers[(uint)Opcodes.MSG_MOVE_START_TURN_RIGHT] = HandleMoveStartTurnRight;
            _handlers[(uint)Opcodes.MSG_MOVE_STOP] = HandleMoveStop;
            _handlers[(uint)Opcodes.MSG_MOVE_STOP_PITCH] = HandleMoveStopPitch;
            _handlers[(uint)Opcodes.MSG_MOVE_STOP_STRAFE] = HandleMoveStopStrafe;
            _handlers[(uint)Opcodes.MSG_MOVE_STOP_SWIM] = HandleMoveStopSwim;
            _handlers[(uint)Opcodes.MSG_MOVE_STOP_TURN] = HandleMoveStopTurn;
            _handlers[(uint)Opcodes.MSG_MOVE_TELEPORT] = HandleMoveTeleport;
            _handlers[(uint)Opcodes.MSG_MOVE_TELEPORT_ACK] = HandleMoveTeleportAck;
            _handlers[(uint)Opcodes.MSG_MOVE_TELEPORT_CHEAT] = HandleMoveTeleportCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_TOGGLE_COLLISION_CHEAT] = HandleMoveToggleCollisionCheat;
            _handlers[(uint)Opcodes.MSG_MOVE_TOGGLE_FALL_LOGGING] = HandleMoveToggleFallLogging;
            _handlers[(uint)Opcodes.MSG_MOVE_TOGGLE_LOGGING] = HandleMoveToggleLogging;
            _handlers[(uint)Opcodes.MSG_MOVE_WORLDPORT_ACK] = HandleMoveWorldportAck;
            _handlers[(uint)Opcodes.SMSG_ACCOUNT_DATA_TIMES] = HandleAccountDataTimes;
            _handlers[(uint)Opcodes.SMSG_ACTION_BUTTONS] = HandleActionButtons;
            _handlers[(uint)Opcodes.SMSG_ADDON_INFO] = HandleAddonInfo;
            _handlers[(uint)Opcodes.SMSG_AI_REACTION] = HandleAIReaction;
            _handlers[(uint)Opcodes.SMSG_ATTACKSTART] = HandleAttackStart;
            _handlers[(uint)Opcodes.SMSG_ATTACKSTOP] = HandleAttackStop;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_BADFACING] = HandleAttackSwingBadFacing;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_CANT_ATTACK] = HandleAttackSwingCantAttack;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_DEADTARGET] = HandleAttackSwingDeadTarget;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_NOTINRANGE] = HandleAttackSwingNotInRange;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_NOTSTANDING] = HandleAttackSwingNotStanding;
            _handlers[(uint)Opcodes.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[(uint)Opcodes.SMSG_BINDER_CONFIRM] = HandleBinderConfirm;
            _handlers[(uint)Opcodes.SMSG_BINDPOINTUPDATE] = HandleBindPointUpdate;
            _handlers[(uint)Opcodes.SMSG_BUY_BANK_SLOT_RESULT] = HandleBuyBankSlotResult;
            _handlers[(uint)Opcodes.SMSG_CANCEL_COMBAT] = HandleCancelCombat;
            _handlers[(uint)Opcodes.SMSG_CHAR_CREATE] = HandleCharCreate;
            _handlers[(uint)Opcodes.SMSG_CHAR_DELETE] = HandleCharDelete;
            _handlers[(uint)Opcodes.SMSG_CHAR_ENUM] = HandleCharEnum;
            _handlers[(uint)Opcodes.SMSG_CHAR_RENAME] = HandleCharRename;
            _handlers[(uint)Opcodes.SMSG_CHARACTER_LOGIN_FAILED] = HandleCharacterLoginFailed;
            _handlers[(uint)Opcodes.SMSG_CHAT_PLAYER_NOT_FOUND] = HandleChatPlayerNotFound;
            _handlers[(uint)Opcodes.SMSG_CHAT_RESTRICTED] = HandleChatRestricted;
            _handlers[(uint)Opcodes.SMSG_CHAT_WRONG_FACTION] = HandleChatWrongFaction;
            _handlers[(uint)Opcodes.SMSG_CHECK_FOR_BOTS] = HandleCheckForBots;
            _handlers[(uint)Opcodes.SMSG_CLIENT_CONTROL_UPDATE] = HandleClientControlUpdate;
            _handlers[(uint)Opcodes.SMSG_CLEAR_COOLDOWN] = HandleClearCooldown;
            _handlers[(uint)Opcodes.SMSG_COMPRESSED_MOVES] = HandleCompressedMoves;
            _handlers[(uint)Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT] = HandleCompressedUpdateObject;
            _handlers[(uint)Opcodes.SMSG_COOLDOWN_EVENT] = HandleCooldownEvent;
            _handlers[(uint)Opcodes.SMSG_CREATURE_QUERY_RESPONSE] = HandleCreatureQueryResponse;
            _handlers[(uint)Opcodes.SMSG_DBLOOKUP] = HandleDBLookup;
            _handlers[(uint)Opcodes.SMSG_DEBUG_AISTATE] = HandleDebugAIState;
            _handlers[(uint)Opcodes.SMSG_DEBUGINFOSPELLMISS_OBSOLETE] = HandleDebugInfoSpellMissObsolete;
            _handlers[(uint)Opcodes.SMSG_DESTROY_OBJECT] = HandleDestroyObject;
            _handlers[(uint)Opcodes.SMSG_DISMOUNTRESULT] = HandleDismountResult;
            _handlers[(uint)Opcodes.SMSG_DUEL_COMPLETE] = HandleDuelComplete;
            _handlers[(uint)Opcodes.SMSG_DUEL_COUNTDOWN] = HandleDuelCountdown;
            _handlers[(uint)Opcodes.SMSG_DUEL_INBOUNDS] = HandleDuelInBounds;
            _handlers[(uint)Opcodes.SMSG_DUEL_OUTOFBOUNDS] = HandleDuelOutOfBounds;
            _handlers[(uint)Opcodes.SMSG_DUEL_REQUESTED] = HandleDuelRequested;
            _handlers[(uint)Opcodes.SMSG_DUEL_WINNER] = HandleDuelWinner;
            _handlers[(uint)Opcodes.SMSG_EMOTE] = HandleEmote;
            _handlers[(uint)Opcodes.SMSG_ENCHANTMENTLOG] = HandleEnchantmentLog;
            _handlers[(uint)Opcodes.SMSG_ENVIRONMENTALDAMAGELOG] = HandleEnvironmentalDamageLog;
            _handlers[(uint)Opcodes.SMSG_EXPECTED_SPAM_RECORDS] = HandleExpectedSpamRecords;
            _handlers[(uint)Opcodes.SMSG_EXPLORATION_EXPERIENCE] = HandleExplorationExperience;
            _handlers[(uint)Opcodes.SMSG_FEIGN_DEATH_RESISTED] = HandleFeignDeathResisted;
            _handlers[(uint)Opcodes.SMSG_FISH_ESCAPED] = HandleFishEscaped;
            _handlers[(uint)Opcodes.SMSG_FISH_NOT_HOOKED] = HandleFishNotHooked;
            _handlers[(uint)Opcodes.SMSG_FORCE_RUN_BACK_SPEED_CHANGE] = HandleForceRunBackSpeedChange;
            _handlers[(uint)Opcodes.SMSG_FORCE_RUN_SPEED_CHANGE] = HandleForceRunSpeedChange;
            _handlers[(uint)Opcodes.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE] = HandleForceSwimBackSpeedChange;
            _handlers[(uint)Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE] = HandleForceSwimSpeedChange;
            _handlers[(uint)Opcodes.SMSG_FORCE_TURN_RATE_CHANGE] = HandleForceTurnRateChange;
            _handlers[(uint)Opcodes.SMSG_FORCE_WALK_SPEED_CHANGE] = HandleForceWalkSpeedChange;
            _handlers[(uint)Opcodes.SMSG_FORCE_MOVE_ROOT] = HandleForceMoveRoot;
            _handlers[(uint)Opcodes.SMSG_FORCE_MOVE_UNROOT] = HandleForceMoveUnroot;
            _handlers[(uint)Opcodes.SMSG_FORCEACTIONSHOW] = HandleForceActionShow;
            _handlers[(uint)Opcodes.SMSG_FRIEND_LIST] = HandleFriendList;
            _handlers[(uint)Opcodes.SMSG_FRIEND_STATUS] = HandleFriendStatus;
            _handlers[(uint)Opcodes.SMSG_GAMEOBJECT_CUSTOM_ANIM] = HandleGameObjectCustomAnim;
            _handlers[(uint)Opcodes.SMSG_GAMEOBJECT_QUERY_RESPONSE] = HandleGameObjectQueryResponse;
            _handlers[(uint)Opcodes.SMSG_GAMESPEED_SET] = HandleGameSpeedSet;
            _handlers[(uint)Opcodes.SMSG_GAMETIME_SET] = HandleGameTimeSet;
            _handlers[(uint)Opcodes.SMSG_GAMETIME_UPDATE] = HandleGameTimeUpdate;
            _handlers[(uint)Opcodes.SMSG_GOSSIP_COMPLETE] = HandleGossipComplete;
            _handlers[(uint)Opcodes.SMSG_GOSSIP_MESSAGE] = HandleGossipMessage;
            _handlers[(uint)Opcodes.SMSG_GROUP_DESTROYED] = HandleGroupDestroyed;
            _handlers[(uint)Opcodes.SMSG_GROUP_INVITE] = HandleGroupInvite;
            _handlers[(uint)Opcodes.SMSG_GROUP_LIST] = HandleGroupList;
            _handlers[(uint)Opcodes.SMSG_GROUP_SET_LEADER] = HandleGroupSetLeader;
            _handlers[(uint)Opcodes.SMSG_GROUP_UNINVITE] = HandleGroupUninvite;
            _handlers[(uint)Opcodes.SMSG_GUILD_COMMAND_RESULT] = HandleGuildCommandResult;
            _handlers[(uint)Opcodes.SMSG_GUILD_EVENT] = HandleGuildEvent;
            _handlers[(uint)Opcodes.SMSG_GUILD_INFO] = HandleGuildInfo;
            _handlers[(uint)Opcodes.SMSG_GUILD_QUERY_RESPONSE] = HandleGuildQueryResponse;
            _handlers[(uint)Opcodes.SMSG_GUILD_ROSTER] = HandleGuildRoster;
            _handlers[(uint)Opcodes.SMSG_IGNORE_LIST] = HandleIgnoreList;
            _handlers[(uint)Opcodes.SMSG_INIT_WORLD_STATES] = HandleInitWorldStates;
            _handlers[(uint)Opcodes.SMSG_INITIAL_SPELLS] = HandleInitialSpells;
            _handlers[(uint)Opcodes.SMSG_INITIALIZE_FACTIONS] = HandleInitializeFactions;
            _handlers[(uint)Opcodes.SMSG_INSPECT] = HandleInspect;
            _handlers[(uint)Opcodes.SMSG_INSTANCE_SAVE_CREATED] = HandleInstanceSaveCreated;
            _handlers[(uint)Opcodes.SMSG_ITEM_COOLDOWN] = HandleItemCooldown;
            _handlers[(uint)Opcodes.SMSG_ITEM_NAME_QUERY_RESPONSE] = HandleItemNameQueryResponse;
            _handlers[(uint)Opcodes.SMSG_ITEM_PUSH_RESULT] = HandleItemPushResult;
            _handlers[(uint)Opcodes.SMSG_ITEM_QUERY_MULTIPLE_RESPONSE] = HandleItemQueryMultipleResponse;
            _handlers[(uint)Opcodes.SMSG_ITEM_QUERY_SINGLE_RESPONSE] = HandleItemQuerySingleResponse;
            _handlers[(uint)Opcodes.SMSG_LEARNED_SPELL] = HandleLearnedSpell;
            _handlers[(uint)Opcodes.SMSG_LEVELUP_INFO] = HandleLevelUpInfo;
            _handlers[(uint)Opcodes.SMSG_LOGIN_SETTIMESPEED] = HandleLoginSetTimeSpeed;
            _handlers[(uint)Opcodes.SMSG_LOGIN_VERIFY_WORLD] = HandleLoginVerifyWorld;
            _handlers[(uint)Opcodes.SMSG_LOGOUT_CANCEL_ACK] = HandleLogoutCancelAck;
            _handlers[(uint)Opcodes.SMSG_LOGOUT_COMPLETE] = HandleLogoutComplete;
            _handlers[(uint)Opcodes.SMSG_LOGOUT_RESPONSE] = HandleLogoutResponse;
            _handlers[(uint)Opcodes.SMSG_LOG_XPGAIN] = HandleLogXpGain;
            _handlers[(uint)Opcodes.SMSG_LOOT_CLEAR_MONEY] = HandleLootClearMoney;
            _handlers[(uint)Opcodes.SMSG_LOOT_MONEY_NOTIFY] = HandleLootMoneyNotify;
            _handlers[(uint)Opcodes.SMSG_LOOT_RELEASE_RESPONSE] = HandleLootReleaseResponse;
            _handlers[(uint)Opcodes.SMSG_LOOT_REMOVED] = HandleLootRemoved;
            _handlers[(uint)Opcodes.SMSG_LOOT_RESPONSE] = HandleLootResponse;
            _handlers[(uint)Opcodes.SMSG_MESSAGECHAT] = HandleMessageChat;
            _handlers[(uint)Opcodes.SMSG_MINIGAME_MOVE_FAILED] = HandleMinigameMoveFailed;
            _handlers[(uint)Opcodes.SMSG_MINIGAME_SETUP] = HandleMinigameSetup;
            _handlers[(uint)Opcodes.SMSG_MINIGAME_STATE] = HandleMinigameState;
            _handlers[(uint)Opcodes.SMSG_MOUNTRESULT] = HandleMountResult;
            _handlers[(uint)Opcodes.SMSG_MOUNTSPECIAL_ANIM] = HandleMountSpecialAnim;
            _handlers[(uint)Opcodes.SMSG_MOVE_FEATHER_FALL] = HandleMoveFeatherFall;
            _handlers[(uint)Opcodes.SMSG_MOVE_KNOCK_BACK] = HandleMoveKnockBack;
            _handlers[(uint)Opcodes.SMSG_MOVE_NORMAL_FALL] = HandleMoveNormalFall;
            _handlers[(uint)Opcodes.SMSG_MOVE_SET_FLIGHT] = HandleMoveSetFlight;
            _handlers[(uint)Opcodes.SMSG_MOVE_SET_HOVER] = HandleMoveSetHover;
            _handlers[(uint)Opcodes.SMSG_MOVE_UNSET_FLIGHT] = HandleMoveUnsetFlight;
            _handlers[(uint)Opcodes.SMSG_MOVE_UNSET_HOVER] = HandleMoveUnsetHover;
            _handlers[(uint)Opcodes.SMSG_NAME_QUERY_RESPONSE] = HandleNameQueryResponse;
            _handlers[(uint)Opcodes.SMSG_NEW_TAXI_PATH] = HandleNewTaxiPath;
            _handlers[(uint)Opcodes.SMSG_NOTIFICATION] = HandleNotification;
            _handlers[(uint)Opcodes.SMSG_NPC_TEXT_UPDATE] = HandleNpcTextUpdate;
            _handlers[(uint)Opcodes.SMSG_OPEN_CONTAINER] = HandleOpenContainer;
            _handlers[(uint)Opcodes.SMSG_OVERRIDE_LIGHT] = HandleOverrideLight;
            _handlers[(uint)Opcodes.SMSG_PAGE_TEXT_QUERY_RESPONSE] = HandlePageTextQueryResponse;
            _handlers[(uint)Opcodes.SMSG_PARTY_MEMBER_STATS] = HandlePartyMemberStats;
            _handlers[(uint)Opcodes.SMSG_PARTYKILLLOG] = HandlePartyKillLog;
            _handlers[(uint)Opcodes.SMSG_PET_ACTION_FEEDBACK] = HandlePetActionFeedback;
            _handlers[(uint)Opcodes.SMSG_PET_CAST_FAILED] = HandlePetCastFailed;
            _handlers[(uint)Opcodes.SMSG_PET_MODE] = HandlePetMode;
            _handlers[(uint)Opcodes.SMSG_PET_NAME_INVALID] = HandlePetNameInvalid;
            _handlers[(uint)Opcodes.SMSG_PET_NAME_QUERY_RESPONSE] = HandlePetNameQueryResponse;
            _handlers[(uint)Opcodes.SMSG_PET_SPELLS] = HandlePetSpells;
            _handlers[(uint)Opcodes.SMSG_PET_TAME_FAILURE] = HandlePetTameFailure;
            _handlers[(uint)Opcodes.SMSG_PETITION_QUERY_RESPONSE] = HandlePetitionQueryResponse;
            _handlers[(uint)Opcodes.SMSG_PETITION_SHOW_SIGNATURES] = HandlePetitionShowSignatures;
            _handlers[(uint)Opcodes.SMSG_PETITION_SIGN_RESULTS] = HandlePetitionSignResults;
            _handlers[(uint)Opcodes.SMSG_PLAY_MUSIC] = HandlePlayMusic;
            _handlers[(uint)Opcodes.SMSG_PLAY_OBJECT_SOUND] = HandlePlayObjectSound;
            _handlers[(uint)Opcodes.SMSG_PLAY_SOUND] = HandlePlaySound;
            _handlers[(uint)Opcodes.SMSG_PLAY_TIME_WARNING] = HandlePlayTimeWarning;
            _handlers[(uint)Opcodes.SMSG_PLAYERBOUND] = HandlePlayerBound;
            _handlers[(uint)Opcodes.SMSG_PLAYER_COMBAT_XP_GAIN_OBSOLETE] = HandlePlayerCombatXpGainObsolete;
            _handlers[(uint)Opcodes.SMSG_PONG] = HandlePong;
            _handlers[(uint)Opcodes.SMSG_QUERY_OBJECT_POSITION] = HandleQueryObjectPosition;
            _handlers[(uint)Opcodes.SMSG_QUERY_OBJECT_ROTATION] = HandleQueryObjectRotation;
            _handlers[(uint)Opcodes.SMSG_QUERY_TIME_RESPONSE] = HandleQueryTimeResponse;
            _handlers[(uint)Opcodes.SMSG_QUEST_CONFIRM_ACCEPT] = HandleQuestConfirmAccept;
            _handlers[(uint)Opcodes.SMSG_QUEST_QUERY_RESPONSE] = HandleQuestQueryResponse;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_OFFER_REWARD] = HandleQuestGiverOfferReward;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_COMPLETE] = HandleQuestGiverQuestComplete;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_DETAILS] = HandleQuestGiverQuestDetails;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_FAILED] = HandleQuestGiverQuestFailed;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_INVALID] = HandleQuestGiverQuestInvalid;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_LIST] = HandleQuestGiverQuestList;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_REQUEST_ITEMS] = HandleQuestGiverRequestItems;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_STATUS] = HandleQuestGiverStatus;
            _handlers[(uint)Opcodes.SMSG_QUESTLOG_FULL] = HandleQuestLogFull;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_ADD_ITEM] = HandleQuestUpdateAddItem;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_ADD_KILL] = HandleQuestUpdateAddKill;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_COMPLETE] = HandleQuestUpdateComplete;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_FAILED] = HandleQuestUpdateFailed;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_FAILEDTIMER] = HandleQuestUpdateFailedTimer;
            _handlers[(uint)Opcodes.SMSG_RAID_GROUP_ONLY] = HandleRaidGroupOnly;
            _handlers[(uint)Opcodes.SMSG_RAID_INSTANCE_INFO] = HandleRaidInstanceInfo;
            _handlers[(uint)Opcodes.SMSG_RAID_INSTANCE_MESSAGE] = HandleRaidInstanceMessage;
            _handlers[(uint)Opcodes.SMSG_READ_ITEM_FAILED] = HandleReadItemFailed;
            _handlers[(uint)Opcodes.SMSG_READ_ITEM_OK] = HandleReadItemOk;
            _handlers[(uint)Opcodes.SMSG_RESISTLOG] = HandleResistLog;
            _handlers[(uint)Opcodes.SMSG_RESURRECT_REQUEST] = HandleResurrectRequest;
            _handlers[(uint)Opcodes.SMSG_SCRIPT_MESSAGE] = HandleScriptMessage;
            _handlers[(uint)Opcodes.SMSG_SERVER_MESSAGE] = HandleServerMessage;
            _handlers[(uint)Opcodes.SMSG_SET_EXTRA_AURA_INFO] = HandleSetExtraAuraInfo;
            _handlers[(uint)Opcodes.SMSG_SET_EXTRA_AURA_INFO_NEED_UPDATE] = HandleSetExtraAuraInfoNeedUpdate;
            _handlers[(uint)Opcodes.SMSG_SET_FACTION_ATWAR] = HandleSetFactionAtWar;
            _handlers[(uint)Opcodes.SMSG_SET_FACTION_STANDING] = HandleSetFactionStanding;
            _handlers[(uint)Opcodes.SMSG_SET_FACTION_VISIBLE] = HandleSetFactionVisible;
            _handlers[(uint)Opcodes.SMSG_SET_PROFICIENCY] = HandleSetProficiency;
            _handlers[(uint)Opcodes.SMSG_SET_REST_START] = HandleSetRestStart;
            _handlers[(uint)Opcodes.SMSG_SHOW_BANK] = HandleShowBank;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_FEATHER_FALL] = HandleSplineMoveFeatherFall;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_LAND_WALK] = HandleSplineMoveLandWalk;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_NORMAL_FALL] = HandleSplineMoveNormalFall;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_SET_HOVER] = HandleSplineMoveSetHover;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_SET_RUN_MODE] = HandleSplineMoveSetRunMode;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_SET_WALK_MODE] = HandleSplineMoveSetWalkMode;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_START_SWIM] = HandleSplineMoveStartSwim;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_STOP_SWIM] = HandleSplineMoveStopSwim;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_UNROOT] = HandleSplineMoveUnroot;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_UNSET_HOVER] = HandleSplineMoveUnsetHover;
            _handlers[(uint)Opcodes.SMSG_SPLINE_MOVE_WATER_WALK] = HandleSplineMoveWaterWalk;
            _handlers[(uint)Opcodes.SMSG_SPLINE_SET_RUN_BACK_SPEED] = HandleSplineSetRunBackSpeed;
            _handlers[(uint)Opcodes.SMSG_SPLINE_SET_RUN_SPEED] = HandleSplineSetRunSpeed;
            _handlers[(uint)Opcodes.SMSG_SPLINE_SET_SWIM_BACK_SPEED] = HandleSplineSetSwimBackSpeed;
            _handlers[(uint)Opcodes.SMSG_SPLINE_SET_SWIM_SPEED] = HandleSplineSetSwimSpeed;
            _handlers[(uint)Opcodes.SMSG_SPLINE_SET_TURN_RATE] = HandleSplineSetTurnRate;
            _handlers[(uint)Opcodes.SMSG_SPLINE_SET_WALK_SPEED] = HandleSplineSetWalkSpeed;
            _handlers[(uint)Opcodes.SMSG_SPELL_COOLDOWN] = HandleSpellCooldown;
            _handlers[(uint)Opcodes.SMSG_SPELL_FAILURE] = HandleSpellFailure;
            _handlers[(uint)Opcodes.SMSG_SPELL_GO] = HandleSpellGo;
            _handlers[(uint)Opcodes.SMSG_SPELL_START] = HandleSpellStart;
            _handlers[(uint)Opcodes.SMSG_SPELLENERGIZELOG] = HandleSpellEnergizeLog;
            _handlers[(uint)Opcodes.SMSG_SPELLHEALLOG] = HandleSpellHealLog;
            _handlers[(uint)Opcodes.SMSG_SPELLLOGMISS] = HandleSpellLogMiss;
            _handlers[(uint)Opcodes.SMSG_START_MIRROR_TIMER] = HandleStartMirrorTimer;
            _handlers[(uint)Opcodes.SMSG_STANDSTATE_UPDATE] = HandleStandStateUpdate;
            _handlers[(uint)Opcodes.SMSG_STOP_MIRROR_TIMER] = HandleStopMirrorTimer;
            _handlers[(uint)Opcodes.SMSG_SUPERCEDED_SPELL] = HandleSupersededSpell;
            _handlers[(uint)Opcodes.SMSG_TOTEM_CREATED] = HandleTotemCreated;
            _handlers[(uint)Opcodes.SMSG_TRADE_STATUS] = HandleTradeStatus;
            _handlers[(uint)Opcodes.SMSG_TRADE_STATUS_EXTENDED] = HandleTradeStatusExtended;
            _handlers[(uint)Opcodes.SMSG_TRAINER_BUY_FAILED] = HandleTrainerBuyFailed;
            _handlers[(uint)Opcodes.SMSG_TRAINER_BUY_SUCCEEDED] = HandleTrainerBuySucceeded;
            _handlers[(uint)Opcodes.SMSG_TRAINER_LIST] = HandleTrainerList;
            _handlers[(uint)Opcodes.SMSG_TUTORIAL_FLAGS] = HandleTutorialFlags;
            _handlers[(uint)Opcodes.SMSG_TURN_IN_PETITION_RESULTS] = HandleTurnInPetitionResults;
            _handlers[(uint)Opcodes.SMSG_UPDATE_AURA_DURATION] = HandleUpdateAuraDuration;
            _handlers[(uint)Opcodes.SMSG_UPDATE_OBJECT] = HandleUpdateObject;
            _handlers[(uint)Opcodes.SMSG_UPDATE_WORLD_STATE] = HandleUpdateWorldState;
            _handlers[(uint)Opcodes.SMSG_WEATHER] = HandleWeather;
            _handlers[(uint)Opcodes.SMSG_WHO] = HandleWho;
            _handlers[(uint)Opcodes.SMSG_WHOIS] = HandleWhois;
            _handlers[(uint)Opcodes.SMSG_ZONE_UNDER_ATTACK] = HandleZoneUnderAttack;

        }

        private void HandleStandStateUpdate(byte[] obj)
        {
            
        }

        private void HandleSetRestStart(byte[] obj)
        {

        }

        private void HandleMoveToggleFallLogging(byte[] obj)
        {

        }

        private void HandleDBLookup(byte[] obj)
        {

        }

        private void HandleDebugInfoSpellMissObsolete(byte[] obj)
        {

        }

        private void HandleQueryObjectRotation(byte[] obj)
        {

        }

        private void HandleQueryObjectPosition(byte[] obj)
        {

        }

        private void HandlePong(byte[] obj)
        {

        }

        private void HandleServerMessage(byte[] obj)
        {

        }

        private void HandleSpellLogMiss(byte[] obj)
        {

        }

        private void HandleMoveRoot(byte[] obj)
        {

        }

        private void HandleMoveHeartbeat(byte[] obj)
        {

        }

        private void HandleMoveWorldportAck(byte[] obj)
        {

        }

        private void HandleMoveToggleLogging(byte[] obj)
        {

        }

        private void HandleMoveToggleCollisionCheat(byte[] obj)
        {

        }

        private void HandleMoveTeleportCheat(byte[] obj)
        {

        }

        private void HandleMoveTeleportAck(byte[] obj)
        {

        }

        private void HandleMoveStopTurn(byte[] obj)
        {

        }

        private void HandleMoveStopSwim(byte[] obj)
        {

        }

        private void HandleMoveStopStrafe(byte[] obj)
        {

        }

        private void HandleMoveStopPitch(byte[] obj)
        {

        }

        private void HandleMoveStop(byte[] obj)
        {

        }

        private void HandleMoveStartTurnRight(byte[] obj)
        {

        }

        private void HandleMoveStartTurnLeft(byte[] obj)
        {

        }

        private void HandleMoveStartSwim(byte[] obj)
        {

        }

        private void HandleMoveStartStrafeRight(byte[] obj)
        {

        }

        private void HandleMoveStartStrafeLeft(byte[] obj)
        {

        }

        private void HandleMoveStartPitchUp(byte[] obj)
        {

        }

        private void HandleMoveStartPitchDown(byte[] obj)
        {

        }

        private void HandleMoveStartBackward(byte[] obj)
        {

        }

        private void HandleMoveSetWalkSpeedCheat(byte[] obj)
        {

        }

        private void HandleMoveSetWalkSpeed(byte[] obj)
        {

        }

        private void HandleMoveSetWalkMode(byte[] obj)
        {

        }

        private void HandleMoveSetTurnRateCheat(byte[] obj)
        {

        }

        private void HandleMoveSetTurnRate(byte[] obj)
        {

        }

        private void HandleMoveSetSwimSpeedCheat(byte[] obj)
        {

        }

        private void HandleMoveSetSwimSpeed(byte[] obj)
        {

        }

        private void HandleMoveSetSwimBackSpeedCheat(byte[] obj)
        {

        }

        private void HandleMoveSetSwimBackSpeed(byte[] obj)
        {

        }

        private void HandleMoveSetRunSpeedCheat(byte[] obj)
        {

        }

        private void HandleMoveSetRunSpeed(byte[] obj)
        {

        }

        private void HandleMoveSetRunMode(byte[] obj)
        {

        }

        private void HandleMoveSetRunBackSpeedCheat(byte[] obj)
        {

        }

        private void HandleMoveSetRunBackSpeed(byte[] obj)
        {

        }

        private void HandleMoveSetPitch(byte[] obj)
        {

        }

        private void HandleMoveSetFacing(byte[] obj)
        {

        }

        private void HandleMoveSetAllSpeedCheat(byte[] obj)
        {

        }

        private void HandleMoveJump(byte[] obj)
        {

        }

        private void HandleMoveStartForward(byte[] obj)
        {

        }

        private void HandleDestroyObject(byte[] obj)
        {

        }

        private void HandleInitWorldStates(byte[] obj)
        {

        }

        private void HandleGroupList(byte[] obj)
        {

        }

        private void HandleWho(byte[] obj)
        {

        }

        private void HandleWeather(byte[] obj)
        {

        }

        private void HandleUpdateWorldState(byte[] obj)
        {

        }

        private void HandleTradeStatusExtended(byte[] obj)
        {

        }

        private void HandleTotemCreated(byte[] obj)
        {

        }

        private void HandleSplineSetWalkSpeed(byte[] obj)
        {

        }

        private void HandleSplineSetTurnRate(byte[] obj)
        {

        }

        private void HandleSplineSetSwimSpeed(byte[] obj)
        {

        }

        private void HandleSplineSetSwimBackSpeed(byte[] obj)
        {

        }

        private void HandleSplineSetRunSpeed(byte[] obj)
        {

        }

        private void HandleSplineSetRunBackSpeed(byte[] obj)
        {

        }

        private void HandleSplineMoveWaterWalk(byte[] obj)
        {

        }

        private void HandleSplineMoveUnsetHover(byte[] obj)
        {

        }

        private void HandleSplineMoveUnroot(byte[] obj)
        {

        }

        private void HandleSplineMoveStopSwim(byte[] obj)
        {

        }

        private void HandleSplineMoveStartSwim(byte[] obj)
        {

        }

        private void HandleSplineMoveSetWalkMode(byte[] obj)
        {

        }

        private void HandleSplineMoveSetRunMode(byte[] obj)
        {

        }

        private void HandleSplineMoveSetHover(byte[] obj)
        {

        }

        private void HandleSplineMoveNormalFall(byte[] obj)
        {

        }

        private void HandleSplineMoveLandWalk(byte[] obj)
        {

        }

        private void HandleSplineMoveFeatherFall(byte[] obj)
        {

        }

        private void HandleSetFactionVisible(byte[] obj)
        {

        }

        private void HandleSetFactionStanding(byte[] obj)
        {

        }

        private void HandleSetFactionAtWar(byte[] obj)
        {

        }

        private void HandleSetExtraAuraInfoNeedUpdate(byte[] obj)
        {

        }

        private void HandleSetExtraAuraInfo(byte[] obj)
        {

        }

        private void HandleServerTime(byte[] obj)
        {

        }

        private void HandleScriptMessage(byte[] obj)
        {

        }

        private void HandleReadItemOk(byte[] obj)
        {

        }

        private void HandleReadItemFailed(byte[] obj)
        {

        }

        private void HandleRaidInstanceMessage(byte[] obj)
        {

        }

        private void HandleRaidInstanceInfo(byte[] obj)
        {

        }

        private void HandleRaidGroupOnly(byte[] obj)
        {

        }

        private void HandlePlayerCombatXpGainObsolete(byte[] obj)
        {

        }

        private void HandlePlayTimeWarning(byte[] obj)
        {

        }

        private void HandlePlaySound(byte[] obj)
        {

        }

        private void HandlePlayObjectSound(byte[] obj)
        {

        }

        private void HandlePlayMusic(byte[] obj)
        {

        }

        private void HandlePetNameQueryResponse(byte[] obj)
        {

        }

        private void HandlePetActionFeedback(byte[] obj)
        {

        }

        private void HandlePartyKillLog(byte[] obj)
        {

        }

        private void HandleOverrideLight(byte[] obj)
        {

        }

        private void HandleOpenContainer(byte[] obj)
        {

        }

        private void HandleMoveUnsetHover(byte[] obj)
        {

        }

        private void HandleMoveUnsetFlight(byte[] obj)
        {

        }

        private void HandleMoveSetHover(byte[] obj)
        {

        }

        private void HandleMoveSetFlight(byte[] obj)
        {

        }

        private void HandleMoveNormalFall(byte[] obj)
        {

        }

        private void HandleMoveKnockBack(byte[] obj)
        {

        }

        private void HandleMoveFeatherFall(byte[] obj)
        {

        }

        private void HandleMinigameState(byte[] obj)
        {

        }

        private void HandleMinigameSetup(byte[] obj)
        {

        }

        private void HandleMinigameMoveFailed(byte[] obj)
        {

        }

        private void HandleLogoutCancelAck(byte[] obj)
        {

        }

        private void HandleLoginSetTimeSpeed(byte[] obj)
        {

        }

        private void HandleItemQueryMultipleResponse(byte[] obj)
        {

        }

        private void HandleItemNameQueryResponse(byte[] obj)
        {

        }

        private void HandleItemCooldown(byte[] obj)
        {

        }

        private void HandleInstanceSaveCreated(byte[] obj)
        {

        }

        private void HandleInspect(byte[] obj)
        {

        }

        private void HandleInitializeFactions(byte[] obj)
        {

        }

        private void HandleGuildRoster(byte[] obj)
        {

        }

        private void HandleGuildInfo(byte[] obj)
        {

        }

        private void HandleGameTimeUpdate(byte[] obj)
        {

        }

        private void HandleGameTimeSet(byte[] obj)
        {

        }

        private void HandleGameSpeedSet(byte[] obj)
        {

        }

        private void HandleForceActionShow(byte[] obj)
        {

        }

        private void HandleForceMoveUnroot(byte[] obj)
        {

        }

        private void HandleForceMoveRoot(byte[] obj)
        {

        }

        private void HandleForceWalkSpeedChange(byte[] obj)
        {

        }

        private void HandleForceTurnRateChange(byte[] obj)
        {

        }

        private void HandleForceSwimSpeedChange(byte[] obj)
        {

        }

        private void HandleForceSwimBackSpeedChange(byte[] obj)
        {

        }

        private void HandleForceRunSpeedChange(byte[] obj)
        {

        }

        private void HandleForceRunBackSpeedChange(byte[] obj)
        {

        }

        private void HandleFeignDeathResisted(byte[] obj)
        {

        }

        private void HandleExplorationExperience(byte[] obj)
        {

        }

        private void HandleExpectedSpamRecords(byte[] obj)
        {

        }

        private void HandleEnvironmentalDamageLog(byte[] obj)
        {

        }

        private void HandleEmote(byte[] obj)
        {

        }

        private void HandleDuelCountdown(byte[] obj)
        {

        }

        private void HandleDebugAIState(byte[] obj)
        {

        }

        private void HandleClientControlUpdate(byte[] obj)
        {

        }

        private void HandleCheckForBots(byte[] obj)
        {

        }

        private void HandleChatRestricted(byte[] obj)
        {

        }

        private void HandleBinderConfirm(byte[] obj)
        {

        }

        private void HandleMessageChat(byte[] obj)
        {

        }

        private void HandleSpellLogMissed(byte[] obj)
        {

        }

        public void Dispatch(uint opcode, byte[] body)
        {
            if (_handlers.TryGetValue(opcode, out var handler))
            {
                handler(body);
            }
            else
            {
                HandleUnknownOpcode(opcode, body);
            }
        }
        private void HandleAuthResponse(byte[] body)
        {
            if (body.Length < 4)
            {
                Console.WriteLine("[WorldClient] Incomplete SMSG_AUTH_RESPONSE packet.");
                return;
            }

            uint result = BitConverter.ToUInt32(body.Take(4).ToArray(), 0);
            if (result == (uint)ResponseCodes.AUTH_OK) // AUTH_OK
            {
                OnWorldSessionStart?.Invoke(this, new EventArgs());
            }
            else
            {
                OnWorldSessionEnd?.Invoke(this, new EventArgs());
            }
        }

        private void HandleCharEnum(byte[] body)
        {
            ObjectManager.Instance.CharacterSelects.Clear();
            try
            {
                using var memoryStream = new MemoryStream(body);
                using var reader = new BinaryReader(memoryStream);
                byte numOfCharacters = reader.ReadByte();
                ulong guid = 0;
                for (int i = 0; i < numOfCharacters; i++)
                {
                    ObjectManager.Instance.CharacterSelects.Add(new CharacterSelect()
                    {
                        Guid = reader.ReadUInt64(),
                        Name = PacketManager.ReadCString(reader),
                        Race = reader.ReadByte(),
                        CharacterClass = reader.ReadByte(),

                        Gender = reader.ReadByte(),
                        Skin = reader.ReadByte(),
                        Face = reader.ReadByte(),
                        HairStyle = reader.ReadByte(),
                        HairColor = reader.ReadByte(),
                        FacialHair = reader.ReadByte(),

                        Level = reader.ReadByte(),

                        ZoneId = reader.ReadUInt32(),
                        MapId = reader.ReadUInt32(),
                        X = reader.ReadSingle(),
                        Y = reader.ReadSingle(),
                        Z = reader.ReadSingle(),

                        Equipment = reader.ReadBytes(20 * 2)
                    });
                }
                OnCharacterListLoaded.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void HandleCompressedUpdateObject(byte[] body)
        {
            //Console.WriteLine("[WorldClient][HandleCompressedUpdateObject] Received compressed data.");

            using (var memoryStream = new MemoryStream(body))
            using (var reader = new BinaryReader(memoryStream))
            {
                uint compressSize = reader.ReadUInt32();

                byte[] compressedData = reader.ReadBytes((int)(memoryStream.Length - 4));
                byte[] decompressedData = DecompressData(compressedData);

                if (decompressedData.Length != compressSize)
                {
                    Console.WriteLine($" *********** UNCOMPRESSED SIZE [{compressSize}] does not equal INFLATED SIZE [{decompressedData.Length}]");
                }

                if (decompressedData.Length > 0)
                {
                    // Handle the decompressed data as a new packet
                    HandleUpdateObject(decompressedData);
                }
                else if (compressSize > 0)
                {
                    Console.WriteLine("[WorldClient][HandleCompressedUpdateObject] Decompressed data is empty.");
                }
            }
        }

        private byte[] DecompressData(byte[] data)
        {
            try
            {
                using (var inputStream = new MemoryStream(data))
                using (var outputStream = new MemoryStream())
                {
                    using (var decompressionStream = new ZlibStream(inputStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient][DecompressData] Unexpected error: {ex}");
                return Array.Empty<byte>();
            }
        }

        private void HandleUpdateObject(byte[] data)
        {
            try
            {
                using (var memoryStream = new MemoryStream(data))
                using (var reader = new BinaryReader(memoryStream))
                {
                    uint count = reader.ReadUInt32();
                    for (uint i = 0; i < count; i++)
                    {
                        byte updateType = reader.ReadByte();
                        if (!Enum.IsDefined(typeof(OpType), updateType))
                        {
                            Console.WriteLine($"[WorldClient][HandleUpdateObject] Unknown update type: {updateType:X}");
                            continue;
                        }

                        switch ((OpType)updateType)
                        {
                            case OpType.UPDATE_PARTIAL:
                                HandlePartialUpdate(reader);
                                break;
                            case OpType.UPDATE_MOVEMENT:
                                HandleMovementUpdate(reader);
                                break;
                            case OpType.UPDATE_FULL:
                                HandleCreateObject(reader);
                                break;
                            case OpType.UPDATE_OUT_OF_RANGE:
                                HandleFarObjects(reader);
                                break;
                            case OpType.UPDATE_IN_RANGE:
                                HandleNearObjects(reader);
                                break;
                            default:
                                Console.WriteLine($"[WorldClient][HandleUpdateObject] Unhandled update type: {(OpType)updateType}");
                                break;
                        }
                    }
                }
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"[WorldClient][HandleUpdateObject] EndOfStreamException: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WorldClient][HandleUpdateObject] Exception: {e}");
            }
        }

        private void HandlePartialUpdate(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            ExtractUpdateBlock(reader, guid);
        }

        private void HandleMovementUpdate(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            ExtractMovementBlock(reader, guid);
        }

        private void HandleCreateObject(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            byte objectType = reader.ReadByte();
            ExtractMovementBlock(reader, guid);
            ExtractUpdateBlock(reader, guid);
        }

        private void HandleFarObjects(BinaryReader reader)
        {
            try
            {
                uint numGuids = reader.ReadUInt32();
                for (uint i = 0; i < numGuids; i++)
                {
                    ulong guid = ReadPackedGuid(reader);
                    ObjectManager.Instance.RemoveObject(guid);
                }
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"[WorldClient][HandleFarObjects] EndOfStreamException: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WorldClient][HandleFarObjects] Exception: {e}");
            }
        }

        private void HandleNearObjects(BinaryReader reader)
        {
            try
            {
                uint numGuids = reader.ReadUInt32();
                for (uint i = 0; i < numGuids; i++)
                {
                    ulong guid = ReadPackedGuid(reader);
                    // Add in-range object handling if necessary
                }
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"[WorldClient][HandleNearObjects] EndOfStreamException: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WorldClient][HandleNearObjects] Exception: {e}");
            }
        }

        private void ExtractUpdateBlock(BinaryReader reader, ulong guid)
        {
            try
            {
                byte numMaskBlocks = reader.ReadByte();
                if (numMaskBlocks > 0x1C) // Adjust this limit based on your knowledge of valid ranges
                {
                    Console.WriteLine($"[ExtractUpdateBlock] NumMaskBlocks is unusually high: {numMaskBlocks}. Aborting.");
                    return;
                }

                uint[] maskBlocks = new uint[numMaskBlocks];
                for (int i = 0; i < numMaskBlocks; i++)
                {
                    maskBlocks[i] = reader.ReadUInt32();
                }

                uint[] updateBlocks = new uint[maskBlocks.Sum(mask => CountSetBits(mask))];
                for (int i = 0; i < updateBlocks.Length; i++)
                {
                    updateBlocks[i] = reader.ReadUInt32();
                }

                // Process update blocks according to object type and fields
                // This is where you would handle specific fields for different object types
                WoWObject obj = new WoWObject
                {
                    Guid = guid,
                    // Populate additional fields as needed
                };
                ObjectManager.Instance.AddOrUpdateObject(obj);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"[ExtractUpdateBlock] EndOfStreamException: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ExtractUpdateBlock] Exception: {e}");
            }
        }

        private void ExtractMovementBlock(BinaryReader reader, ulong guid)
        {
            try
            {
                uint flags = reader.ReadUInt32();
                int unk = reader.ReadInt32();
                float[] position = new float[4];
                for (int i = 0; i < 4; i++)
                {
                    position[i] = reader.ReadSingle();
                }

                // Additional movement fields based on flags
                if ((flags & (uint)MovementFlags.MOVEFLAG_ONTRANSPORT) != 0)
                {
                    ulong transportGuid = ReadPackedGuid(reader);
                    float[] transportPosition = new float[4];
                    for (int i = 0; i < 4; i++)
                    {
                        transportPosition[i] = reader.ReadSingle();
                    }
                }
                if ((flags & (uint)MovementFlags.MOVEFLAG_SWIMMING) != 0)
                {
                    uint swimPitch = reader.ReadUInt32();
                }
                if ((flags & (uint)MovementFlags.MOVEFLAG_FALLING) != 0)
                {
                    uint time = reader.ReadUInt32();
                    float velocity = reader.ReadSingle();
                    float sin = reader.ReadSingle();
                    float cos = reader.ReadSingle();
                    float xySpeed = reader.ReadSingle();
                }
                if ((flags & (uint)MovementFlags.MOVEFLAG_SPLINE_ELEVATION) != 0)
                {
                    float unkFloat = reader.ReadSingle();
                }

                float[] speeds = new float[6];
                for (int i = 0; i < 6; i++)
                {
                    speeds[i] = reader.ReadSingle();
                }

                // Update movement fields for the object
                var obj = ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid);
                if (obj != null)
                {
                    obj.Position = new Position(position[0], position[1], position[2]);
                    //obj.Speeds = speeds;
                    ObjectManager.Instance.AddOrUpdateObject(obj);
                }
                else
                {
                    Console.WriteLine($"[ExtractMovementBlock] Object with GUID {guid} not found.");
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"[ExtractMovementBlock] InvalidOperationException: {e}");
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"[ExtractMovementBlock] EndOfStreamException: {e}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ExtractMovementBlock] Exception: {e}");
            }
        }

        private ulong ReadPackedGuid(BinaryReader reader)
        {
            ulong guid = 0;
            byte mask = reader.ReadByte();
            for (int i = 0; i < 8; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    guid |= (ulong)reader.ReadByte() << (i * 8);
                }
            }
            return guid;
        }

        private int CountSetBits(uint mask)
        {
            int count = 0;
            while (mask != 0)
            {
                mask &= (mask - 1);
                count++;
            }
            return count;
        }

        private void HandleLoginVerifyWorld(byte[] body)
        {
            using (var reader = new BinaryReader(new MemoryStream(body)))
            {
                int mapId = reader.ReadInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                float orientation = reader.ReadSingle();

                OnPlayerInit.Invoke(this, new EventArgs());

                Console.WriteLine($"[WorldClient][LoginVerifyWorld]Map ID: {mapId}, Position: ({x}, {y}, {z}), Orientation: {orientation}");
            }
        }

        private void HandleTutorialFlags(byte[] body)
        {
            using (var reader = new BinaryReader(new MemoryStream(body)))
            {
                byte[] tutorialFlags = reader.ReadBytes(32); // Assuming the packet contains 32 bytes

                bool allDisabled = tutorialFlags.All(b => b == 0xFF);
                Console.WriteLine($"[WorldClient][TutorialFlags]Tutorials disabled: {allDisabled}");
            }
        }

        private void HandleQuestGiverQuestList(byte[] body) { }

        private void HandleQuestGiverQuestDetails(byte[] body) { }

        private void HandleQuestGiverRequestItems(byte[] body) { }

        private void HandleQuestGiverOfferReward(byte[] body) { }

        private void HandleQuestGiverQuestInvalid(byte[] body) { }

        private void HandleQuestGiverQuestComplete(byte[] body) { }

        private void HandleQuestGiverQuestFailed(byte[] body) { }

        private void HandleQuestLogFull(byte[] body) { }

        private void HandleQuestUpdateFailed(byte[] body) { }

        private void HandleQuestUpdateFailedTimer(byte[] body) { }

        private void HandleQuestUpdateComplete(byte[] body) { }

        private void HandleQuestUpdateAddKill(byte[] body) { }

        private void HandleQuestUpdateAddItem(byte[] body) { }

        private void HandleQuestConfirmAccept(byte[] body) { }

        private void HandleNewTaxiPath(byte[] body) { }

        private void HandleTrainerList(byte[] body) { }

        private void HandleTrainerBuySucceeded(byte[] body) { }

        private void HandleTrainerBuyFailed(byte[] body) { }

        private void HandleShowBank(byte[] body) { }

        private void HandleBuyBankSlotResult(byte[] body) { }

        private void HandlePetitionSignResults(byte[] body) { }

        private void HandleTurnInPetitionResults(byte[] body) { }

        private void HandlePetitionQueryResponse(byte[] body) { }

        private void HandleFishNotHooked(byte[] body) { }

        private void HandleFishEscaped(byte[] body) { }

        private void HandleNotification(byte[] body) { }

        private void HandleQueryTimeResponse(byte[] body) { }

        private void HandleLogXpGain(byte[] body) { }

        private void HandleLevelUpInfo(byte[] body) { }

        private void HandleResistLog(byte[] body) { }

        private void HandleEnchantmentLog(byte[] body) { }

        private void HandleStartMirrorTimer(byte[] body) { }

        private void HandlePauseMirrorTimer(byte[] body) { }

        private void HandleStopMirrorTimer(byte[] body) { }

        private void HandleClearCooldown(byte[] body) { }

        private void HandleGameObjectCustomAnim(byte[] body) { }

        private void HandleQuestGiverStatus(byte[] body) { }

        private void HandleNpcTextUpdate(byte[] body) { }

        private void HandleGossipComplete(byte[] body) { }

        private void HandleGossipMessage(byte[] body) { }

        private void HandlePetMode(byte[] body) { }

        private void HandlePetSpells(byte[] body) { }

        private void HandlePetNameInvalid(byte[] body) { }

        private void HandlePetTameFailure(byte[] body) { }

        private void HandleMountSpecialAnim(byte[] body) { }

        private void HandleDismountResult(byte[] body) { }

        private void HandleMountResult(byte[] body) { }

        private void HandleDuelWinner(byte[] body) { }

        private void HandleDuelComplete(byte[] body) { }

        private void HandleDuelInBounds(byte[] body) { }

        private void HandleDuelOutOfBounds(byte[] body) { }

        private void HandleDuelRequested(byte[] body) { }

        private void HandleItemPushResult(byte[] body) { }

        private void HandleLootClearMoney(byte[] body) { }

        private void HandleLootMoneyNotify(byte[] body) { }

        private void HandleLootRemoved(byte[] body) { }

        private void HandleLootReleaseResponse(byte[] body) { }

        private void HandleLootResponse(byte[] body) { }

        private void HandleResurrectRequest(byte[] body) { }

        private void HandlePlayerBound(byte[] body) { }

        private void HandleBindPointUpdate(byte[] body) { }

        private void HandleSpellEnergizeLog(byte[] body) { }

        private void HandleSpellHealLog(byte[] body) { }

        private void HandleCancelCombat(byte[] body) { }

        private void HandleAttackSwingCantAttack(byte[] body) { }

        private void HandleAttackSwingDeadTarget(byte[] body) { }

        private void HandleAttackSwingNotStanding(byte[] body) { }

        private void HandleAttackSwingBadFacing(byte[] body) { }

        private void HandleAttackSwingNotInRange(byte[] body) { }

        private void HandleAttackStop(byte[] body) { }

        private void HandleAttackStart(byte[] body) { }

        private void HandleAIReaction(byte[] body) { }

        private void HandlePetCastFailed(byte[] body) { }

        private void HandleUpdateAuraDuration(byte[] body) { }

        private void HandleCooldownEvent(byte[] body) { }

        private void HandleSpellCooldown(byte[] body) { }

        private void HandleSpellFailure(byte[] body) { }

        private void HandleSpellGo(byte[] body) { }

        private void HandleSpellStart(byte[] body) { }

        private void HandleSupersededSpell(byte[] body) { }

        private void HandleLearnedSpell(byte[] body) { }

        private void HandleInitialSpells(byte[] body) { }

        private void HandleActionButtons(byte[] body) { }

        private void HandleSetProficiency(byte[] body) { }

        private void HandleChannelList(byte[] body) { }

        private void HandleGuildCommandResult(byte[] body) { }

        private void HandleGuildEvent(byte[] body) { }

        private void HandleFriendStatus(byte[] body) { }

        private void HandlePlayedTime(byte[] body) { }

        private void HandleTradeStatus(byte[] body) { }

        private void HandlePetitionShowSignatures(byte[] body) { }

        private void HandleLogoutComplete(byte[] body) { }

        private void HandleLogoutResponse(byte[] body) { }

        private void HandleGroupUninvite(byte[] body) { }

        private void HandleGroupDestroyed(byte[] body) { }

        private void HandleChatWrongFaction(byte[] body) { }

        private void HandleChatPlayerNotFound(byte[] body) { }

        private void HandleAccountDataTimes(byte[] body) { }

        private void HandleChannelNotify(byte[] body) { }

        private void Handle_Pong(byte[] body) { }

        private void HandleCharRename(byte[] body) { }

        private void HandleCharDelete(byte[] body) { }

        private void HandleCharCreate(byte[] body) { }

        private void HandleZoneUnderAttack(byte[] body) { }

        private void HandlePartyMemberStats(byte[] body) { }

        private void HandleGuildRank(byte[] body) { }

        private void HandleGroupSetLeader(byte[] body) { }

        private void HandleGroupInvite(byte[] body) { }

        private void HandleIgnoreList(byte[] body) { }

        private void HandleFriendList(byte[] body) { }

        private void HandleWhois(byte[] body) { }

        private void HandleCreatureQueryResponse(byte[] body) { }

        private void HandleGameObjectQueryResponse(byte[] body) { }

        private void HandleQuestQueryResponse(byte[] body) { }

        private void HandlePageTextQueryResponse(byte[] body) { }

        private void HandleItemQuerySingleResponse(byte[] body) { }

        private void HandleGuildQueryResponse(byte[] body) { }

        private void HandleNameQueryResponse(byte[] body) { }

        private void HandleCharacterLoginFailed(byte[] body) { }

        private void HandleAddonInfo(byte[] body)
        {
            using (var reader = new BinaryReader(new MemoryStream(body)))
            {
                try
                {
                    // Read the size of the addon information block
                    int addonInfoSize = reader.ReadInt32();

                    // Read the compressed addon information
                    byte[] compressedAddonInfo = reader.ReadBytes(addonInfoSize);

                    // Check if the data is compressed
                    if (compressedAddonInfo.Length > 2 && compressedAddonInfo[0] == 0x78 && compressedAddonInfo[1] == 0x9C)
                    {
                        // Decompress the addon information using zlib
                        byte[] addonInfo = DecompressAddonInfo(compressedAddonInfo);
                        ProcessAddonInfo(addonInfo);
                    }
                    else
                    {
                        // Data is not compressed
                        ProcessAddonInfo(compressedAddonInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WorldClient][HandleAddonInfo] Error: {ex.Message}");
                }
            }
        }

        private byte[] DecompressAddonInfo(byte[] compressedData)
        {
            using (var compressedStream = new MemoryStream(compressedData))
            using (var zlibStream = new ZlibStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zlibStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        private void ProcessAddonInfo(byte[] addonInfo)
        {
            using (var reader = new BinaryReader(new MemoryStream(addonInfo)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Read each addon entry
                    uint size = reader.ReadUInt32();
                    if (size == 0)
                        break;

                    string addonName = Encoding.UTF8.GetString(reader.ReadBytes((int)size));
                    byte enabled = reader.ReadByte();

                    Console.WriteLine($"Addon: {addonName}, Enabled: {enabled}");
                }
            }
        }

        private async Task<byte[]> ReadAsync(BinaryReader reader, int count)
        {
            byte[] buffer = new byte[count];
            int read = await reader.BaseStream.ReadAsync(buffer, 0, count);
            if (read < count)
            {
                byte[] result = new byte[read];
                Array.Copy(buffer, result, read);
                return result;
            }
            return buffer;
        }

        private static void HandleCompressedMoves(byte[] body)
        {
            // Read the length of the compressed data
            int compressedLength = BitConverter.ToInt32(body, 0);
            byte[] compressedData = body.Skip(4).Take(compressedLength).ToArray();

            // Decompress the data
            byte[] decompressedData;
            try
            {
                using var inputStream = new MemoryStream(compressedData);
                using var outputStream = new MemoryStream();
                using (var decompressionStream = new ZlibStream(inputStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(outputStream);
                }
                decompressedData = outputStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient][HandleNetworkMessages] Error during decompression: {ex}");
                return;
            }

            // Check if decompressed data is empty
            if (decompressedData.Length == 0)
            {
                Console.WriteLine("[WorldClient][HandleNetworkMessages] Decompressed data is empty.");
                return;
            }

            // Process the decompressed data
            using var memoryStream = new MemoryStream(decompressedData);
            using var reader = new BinaryReader(memoryStream);
            while (memoryStream.Position < memoryStream.Length)
            {
                // Read the opcode and length of the inner packet
                ushort innerOpcode = reader.ReadUInt16();
                ushort innerPacketLength = reader.ReadUInt16();
                byte[] innerPacketData = reader.ReadBytes(innerPacketLength);

                Instance.Dispatch(innerOpcode, innerPacketData);
            }
        }

        private static void HandleMoveTeleport(byte[] data) => Console.WriteLine("[WorldClient][HandleNetworkMessages] Handling MSG_MOVE_TELEPORT...");// Add logic to handle MSG_MOVE_TELEPORT

        private static void HandleMoveFallLand(byte[] data) => Console.WriteLine("[WorldClient][HandleNetworkMessages] Handling MSG_MOVE_FALL_LAND...");// Add logic to handle MSG_MOVE_FALL_LAND

        private static void HandleUnknownOpcode(uint opcode, byte[] body)
        {
            Console.WriteLine($"[OpCodeDispatcher] Unhandled opcode: {opcode:X}");
            //Console.WriteLine($"[OpCodeDispatcher] Body: {BitConverter.ToString(body)}");
        }
        /// <summary>
        ///     Occurs on level up
        /// </summary>
        public event EventHandler LevelUp;

        /// <summary>
        ///     Occurs when looting a new item
        /// </summary>
        public event EventHandler<OnLootArgs> OnLoot;

        /// <summary>
        ///     Occurs when the login is rejected by the authentication server
        /// </summary>
        public event EventHandler OnHandshakeBegin;

        /// <summary>
        ///     Occurs when the login is rejected by the authentication server
        /// </summary>
        public event EventHandler OnWorldSessionStart;

        /// <summary>
        ///     Occurs when the login is rejected by the authentication server
        /// </summary>
        public event EventHandler OnWorldSessionEnd;

        /// <summary>
        ///     Occurs when the login is rejected by the authentication server
        /// </summary>
        public event EventHandler OnWrongLogin;

        /// <summary>
        ///     Occurs when the character list is loaded completely and we arrived at character selection
        /// </summary>
        public event EventHandler OnCharacterListLoaded;

        /// <summary>
        ///     Is fired multiple times a second while we are in a server queue
        /// </summary>
        public event EventHandler InServerQueue;

        /// <summary>
        ///     Occurs when WoW prompts us to choose a realm
        /// </summary>
        public event EventHandler OnChooseRealm;

        /// <summary>
        ///     Occurs when we disconnect
        /// </summary>
        public event EventHandler OnDisconnect;

        /// <summary>
        ///     Occurs when a new error message pops up (Out of range, must be standing etc.)
        /// </summary>
        public event EventHandler<OnUiMessageArgs> OnErrorMessage;

        /// <summary>
        ///     Occurs when a new ui message pops up
        /// </summary>
        public event EventHandler<OnUiMessageArgs> OnUiMessage;

        /// <summary>
        ///     Occurs when a new system message pops up (afk cleared etc.)
        /// </summary>
        public event EventHandler<OnUiMessageArgs> OnSystemMessage;

        /// <summary>
        ///     Occurs when a skill leveled up
        /// </summary>
        public event EventHandler<OnUiMessageArgs> OnSkillMessage;

        public event EventHandler<EventArgs> OnBlockParryDodge;

        public event EventHandler<EventArgs> OnParry;

        public event EventHandler<EventArgs> OnSlamReady;

        /// <summary>
        ///     Occurs when we are not able to drink/eat anymore
        /// </summary>
        public event EventHandler OnFightStart;

        /// <summary>
        ///     Occurs when we can use food / drinks again
        /// </summary>
        public event EventHandler OnFightStop;

        /// <summary>
        ///     Occurs when we kill a unit
        /// </summary>
        public event EventHandler OnUnitKilled;

        /// <summary>
        ///     Occurs on a new party invite
        /// </summary>
        public event EventHandler<OnRequestArgs> OnPartyInvite;

        /// <summary>
        ///     Occurs when our character dies
        /// </summary>
        public event EventHandler OnDeath;

        /// <summary>
        ///     Occurs when we resurrect
        /// </summary>
        public event EventHandler OnResurrect;

        /// <summary>
        ///     Occurs when walk into corpse resurrect range
        /// </summary>
        public event EventHandler OnCorpseInRange;

        /// <summary>
        ///     Occurs when we walk out of corpse resurrect range
        /// </summary>
        public event EventHandler OnCorpseOutOfRange;

        /// <summary>
        ///     Occurs when the loot window is opened
        /// </summary>
        public event EventHandler OnLootOpened;

        /// <summary>
        ///     Occurs when the loot window is closed
        /// </summary>
        public event EventHandler OnLootClosed;

        /// <summary>
        ///     Occurs when a gossip menu shows up
        /// </summary>
        public event EventHandler OnGossipShow;

        /// <summary>
        ///     Occurs when a gossip menu closed
        /// </summary>
        public event EventHandler OnGossipClosed;

        /// <summary>
        ///     Occurs when a merchant frame shows up
        /// </summary>
        public event EventHandler OnMerchantShow;

        /// <summary>
        ///     Occurs when a merchant frame closed
        /// </summary>
        public event EventHandler OnMerchantClosed;

        /// <summary>
        ///     Occurs when a taxi frame opened
        /// </summary>
        public event EventHandler OnTaxiShow;

        /// <summary>
        ///     Occurs when a taxi frame closed
        /// </summary>
        public event EventHandler OnTaxiClosed;

        /// <summary>
        ///     Occurs when a trainer frame shows up
        /// </summary>
        public event EventHandler OnTrainerShow;

        /// <summary>
        ///     Occurs when a trainer frame closes
        /// </summary>
        public event EventHandler OnTrainerClosed;

        /// <summary>
        ///     Occurs whenever the character gains XP
        /// </summary>
        public event EventHandler<OnXpGainArgs> OnXpGain;

        /// <summary>
        ///     Occurs when a aura is removed/added to an unit
        /// </summary>
        public event EventHandler<AuraChangedArgs> AuraChanged;

        /// <summary>
        ///     Occurs when we are asked for a duel
        /// </summary>
        public event EventHandler<OnRequestArgs> OnDuelRequest;

        /// <summary>
        ///     Occurs when we are invited to a guild
        /// </summary>
        public event EventHandler<GuildInviteArgs> OnGuildInvite;

        /// <summary>
        ///     Occurs on a new chat message
        /// </summary>
        public event EventHandler<ChatMessageArgs> OnChatMessage;

        /// <summary>
        ///     Occurs on all kind of events fired by WoW
        /// </summary>
        public event EventHandler<OnEventArgs> OnEvent;

        /// <summary>
        /// Will be fired once the player object is available
        /// </summary>
        public event EventHandler OnPlayerInit;


        /// <summary>
        ///     Occurs on a click to move action
        /// </summary>
        public event EventHandler<OnCtmArgs> OnCtm;

        /// <summary>
        ///     Occurs when the trade window shows
        /// </summary>
        public event EventHandler OnTradeShow;

        /// <summary>
        ///     Occurs when the characters money changes
        /// </summary>
        public event EventHandler OnMoneyChange;

        /// <summary>
        ///     Occurs when the characters target changed
        /// </summary>
        public event EventHandler OnTargetChange;

        /// <summary>
        ///     Occurs when all objectives of a quest are done
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestComplete;

        /// <summary>
        ///     Occurs when all objectives of a quest are done
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestObjectiveComplete;

        /// <summary>
        ///     Occurs when the QuestFrame is opened
        /// </summary>
        public event EventHandler OnQuestFrameOpen;

        /// <summary>
        ///     Occurs when the QuestFrame is closed
        /// </summary>
        public event EventHandler OnQuestFrameClosed;

        /// <summary>
        ///     Occurs when the QuestGreetingFrame is opened
        /// </summary>
        public event EventHandler OnQuestGreetingFrameOpen;

        /// <summary>
        ///     Occurs when the QUestGreetingFrame is closed
        /// </summary>
        public event EventHandler OnQuestGreetingFrameClosed;

        /// <summary>
        ///     Occurs when a quest failed
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestFailed;

        /// <summary>
        ///     Occurs on quest progress (required unit killed, item collected, event completed etc.)
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestProgress;

        /// <summary>
        ///     Occurs on opening of the mailbox
        /// </summary>
        public event EventHandler OnMailboxOpen;

        /// <summary>
        ///     Occurs on closing of the mailbox
        /// </summary>
        public event EventHandler OnMailboxClosed;

        /// <summary>
        ///     Occurs on opening of the bankframe
        /// </summary>
        public event EventHandler OnBankFrameOpen;

        /// <summary>
        ///     Occurs on closing of the bankframe
        /// </summary>
        public event EventHandler OnBankFrameClosed;
    }
    public class OnLootArgs : EventArgs
    {
        internal OnLootArgs(int itemId, string itemName, int count)
        {
            ItemId = itemId;
            ItemName = itemName;
            Count = count;
            Time = DateTime.Now;
        }

        public int ItemId { get; }
        public string ItemName { get; }
        public int Count { get; }
        public DateTime Time { get; }

        public override string ToString()
        {
            return "[" + Time.ToShortTimeString() + "] " + Count + "x " + ItemName + " (" + ItemId + ")";
        }
    }

    /// <summary>
    ///     UI message args
    /// </summary>
    public class OnUiMessageArgs : EventArgs
    {
        public readonly string Message;

        internal OnUiMessageArgs(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    ///     On xp args
    /// </summary>
    public class OnXpGainArgs : EventArgs
    {
        public readonly int Xp;

        internal OnXpGainArgs(int xp)
        {
            Xp = xp;
        }
    }

    /// <summary>
    ///     On aura changed args
    /// </summary>
    public class AuraChangedArgs : EventArgs
    {
        public readonly string AffectedUnit;

        internal AuraChangedArgs(string affectedUnit)
        {
            AffectedUnit = affectedUnit;
        }
    }

    /// <summary>
    ///     On request args
    /// </summary>
    public class OnRequestArgs : EventArgs
    {
        public readonly string Player;

        internal OnRequestArgs(string player)
        {
            Player = player;
        }
    }

    /// <summary>
    ///     chat message args
    /// </summary>
    public class ChatMessageArgs : EventArgs
    {
        internal ChatMessageArgs(ChatSenderType unitType, string chatTag, string unitName, string chatChannel,
            string message)
        {
            UnitType = unitType;
            ChatTag = chatTag;
            UnitName = unitName;
            ChatChannel = chatChannel;
            Message = message;
            Time = DateTime.Now;
        }

        public ChatSenderType UnitType { get; private set; }
        public string ChatTag { get; }
        public string UnitName { get; }
        public string ChatChannel { get; }
        public string Message { get; }
        public DateTime Time { get; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return "[" + Time.ToShortTimeString() + "]" + (ChatTag != "" ? " [" + ChatTag + " " : " [") + UnitName +
                   "] "
                   + "[" + ChatChannel + "]: " + Message;
        }
    }

    /// <summary>
    ///     Guild invite args
    /// </summary>
    public class GuildInviteArgs : EventArgs
    {
        internal readonly string Guild;
        internal readonly string Player;

        internal GuildInviteArgs(string player, string guild)
        {
            Player = player;
            Guild = guild;
        }
    }

    /// <summary>
    ///     On CTM args
    /// </summary>
    public class OnCtmArgs : EventArgs
    {
        /// <summary>
        ///     The type of CTM as int
        /// </summary>
        public readonly int CtmType;

        /// <summary>
        ///     The Position of the Ctm as Position
        /// </summary>
        public readonly Position Position;

        internal OnCtmArgs(Position position, int ctmType)
        {
            Position = position;
            CtmType = ctmType;
        }
    }

    /// <summary>
    ///     On event args
    /// </summary>
    public class OnEventArgs : EventArgs
    {
        /// <summary>
        ///     Name of the WoW event
        /// </summary>
        public readonly string EventName;

        /// <summary>
        ///     Parameters of the event (can be null)
        /// </summary>
        public readonly object[] Parameters;

        internal OnEventArgs(string eventName, object[] parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }
    }

    enum MerchantState
    {
        SHOW = 1,
        CLOSE = 2
    }

    enum LootState
    {
        SHOW = 1,
        CLOSE = 2
    }
}
