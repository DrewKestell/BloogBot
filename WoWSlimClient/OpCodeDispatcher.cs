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
        private readonly Dictionary<uint, Action<byte[]>> _handlers = new();

        private OpCodeDispatcher()
        {
            _handlers[(uint)Opcodes.SMSG_AUTH_RESPONSE] = HandleAuthResponse;
            _handlers[(uint)Opcodes.SMSG_CHAR_ENUM] = HandleCharEnum;
            _handlers[(uint)Opcodes.SMSG_COMPRESSED_MOVES] = HandleSMSGCompressedMoves;
            _handlers[(uint)Opcodes.SMSG_UPDATE_OBJECT] = HandleUpdateObject;
            _handlers[(uint)Opcodes.SMSG_ADDON_INFO] = HandleAddonInfo;
            _handlers[(uint)Opcodes.SMSG_LOGIN_VERIFY_WORLD] = HandleLoginVerifyWorld;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_LIST] = HandleSMSGQuestGiverQuestList;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_LIST] = HandleSMSGQuestGiverQuestList;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_DETAILS] = HandleSMSGQuestGiverQuestDetails;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_REQUEST_ITEMS] = HandleSMSGQuestGiverRequestItems;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_OFFER_REWARD] = HandleSMSGQuestGiverOfferReward;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_INVALID] = HandleSMSGQuestGiverQuestInvalid;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_COMPLETE] = HandleSMSGQuestGiverQuestComplete;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_QUEST_FAILED] = HandleSMSGQuestGiverQuestFailed;
            _handlers[(uint)Opcodes.SMSG_QUESTLOG_FULL] = HandleSMSGQuestLogFull;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_FAILED] = HandleSMSGQuestUpdateFailed;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_FAILEDTIMER] = HandleSMSGQuestUpdateFailedTimer;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_COMPLETE] = HandleSMSGQuestUpdateComplete;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_ADD_KILL] = HandleSMSGQuestUpdateAddKill;
            _handlers[(uint)Opcodes.SMSG_QUESTUPDATE_ADD_ITEM] = HandleSMSGQuestUpdateAddItem;
            _handlers[(uint)Opcodes.SMSG_QUEST_CONFIRM_ACCEPT] = HandleSMSGQuestConfirmAccept;
            _handlers[(uint)Opcodes.SMSG_NEW_TAXI_PATH] = HandleSMSGNewTaxiPath;
            _handlers[(uint)Opcodes.SMSG_TRAINER_LIST] = HandleSMSGTrainerList;
            _handlers[(uint)Opcodes.SMSG_TRAINER_BUY_SUCCEEDED] = HandleSMSGTrainerBuySucceeded;
            _handlers[(uint)Opcodes.SMSG_TRAINER_BUY_FAILED] = HandleSMSGTrainerBuyFailed;
            _handlers[(uint)Opcodes.SMSG_SHOW_BANK] = HandleSMSGShowBank;
            _handlers[(uint)Opcodes.SMSG_BUY_BANK_SLOT_RESULT] = HandleSMSGBuyBankSlotResult;
            _handlers[(uint)Opcodes.SMSG_PETITION_SIGN_RESULTS] = HandleSMSGPetitionSignResults;
            _handlers[(uint)Opcodes.SMSG_TURN_IN_PETITION_RESULTS] = HandleSMSGTurnInPetitionResults;
            _handlers[(uint)Opcodes.SMSG_PETITION_QUERY_RESPONSE] = HandleSMSGPetitionQueryResponse;
            _handlers[(uint)Opcodes.SMSG_FISH_NOT_HOOKED] = HandleSMSGFishNotHooked;
            _handlers[(uint)Opcodes.SMSG_FISH_ESCAPED] = HandleSMSGFishEscaped;
            _handlers[(uint)Opcodes.SMSG_NOTIFICATION] = HandleSMSGNotification;
            _handlers[(uint)Opcodes.SMSG_QUERY_TIME_RESPONSE] = HandleSMSGQueryTimeResponse;
            _handlers[(uint)Opcodes.SMSG_LOG_XPGAIN] = HandleSMSGLogXpGain;
            _handlers[(uint)Opcodes.SMSG_LEVELUP_INFO] = HandleSMSGLevelUpInfo;
            _handlers[(uint)Opcodes.SMSG_RESISTLOG] = HandleSMSGResistLog;
            _handlers[(uint)Opcodes.SMSG_ENCHANTMENTLOG] = HandleSMSGEnchantmentLog;
            _handlers[(uint)Opcodes.SMSG_START_MIRROR_TIMER] = HandleSMSGStartMirrorTimer;
            _handlers[(uint)Opcodes.SMSG_PAUSE_MIRROR_TIMER] = HandleSMSGPauseMirrorTimer;
            _handlers[(uint)Opcodes.SMSG_STOP_MIRROR_TIMER] = HandleSMSGStopMirrorTimer;
            _handlers[(uint)Opcodes.SMSG_CLEAR_COOLDOWN] = HandleSMSGClearCooldown;
            _handlers[(uint)Opcodes.SMSG_GAMEOBJECT_CUSTOM_ANIM] = HandleSMSGGameObjectCustomAnim;
            _handlers[(uint)Opcodes.SMSG_QUESTGIVER_STATUS] = HandleSMSGQuestGiverStatus;
            _handlers[(uint)Opcodes.SMSG_NPC_TEXT_UPDATE] = HandleSMSGNpcTextUpdate;
            _handlers[(uint)Opcodes.SMSG_GOSSIP_COMPLETE] = HandleSMSGGossipComplete;
            _handlers[(uint)Opcodes.SMSG_GOSSIP_MESSAGE] = HandleSMSGGossipMessage;
            _handlers[(uint)Opcodes.SMSG_PET_MODE] = HandleSMSGPetMode;
            _handlers[(uint)Opcodes.SMSG_PET_SPELLS] = HandleSMSGPetSpells;
            _handlers[(uint)Opcodes.SMSG_PET_NAME_INVALID] = HandleSMSGPetNameInvalid;
            _handlers[(uint)Opcodes.SMSG_PET_TAME_FAILURE] = HandleSMSGPetTameFailure;
            _handlers[(uint)Opcodes.SMSG_MOUNTSPECIAL_ANIM] = HandleSMSGMountSpecialAnim;
            _handlers[(uint)Opcodes.SMSG_DISMOUNTRESULT] = HandleSMSGDismountResult;
            _handlers[(uint)Opcodes.SMSG_MOUNTRESULT] = HandleSMSGMountResult;
            _handlers[(uint)Opcodes.SMSG_DUEL_WINNER] = HandleSMSGDuelWinner;
            _handlers[(uint)Opcodes.SMSG_DUEL_COMPLETE] = HandleSMSGDuelComplete;
            _handlers[(uint)Opcodes.SMSG_DUEL_INBOUNDS] = HandleSMSGDuelInBounds;
            _handlers[(uint)Opcodes.SMSG_DUEL_OUTOFBOUNDS] = HandleSMSGDuelOutOfBounds;
            _handlers[(uint)Opcodes.SMSG_DUEL_REQUESTED] = HandleSMSGDuelRequested;
            _handlers[(uint)Opcodes.SMSG_ITEM_PUSH_RESULT] = HandleSMSGItemPushResult;
            _handlers[(uint)Opcodes.SMSG_LOOT_CLEAR_MONEY] = HandleSMSGLootClearMoney;
            _handlers[(uint)Opcodes.SMSG_LOOT_MONEY_NOTIFY] = HandleSMSGLootMoneyNotify;
            _handlers[(uint)Opcodes.SMSG_LOOT_REMOVED] = HandleSMSGLootRemoved;
            _handlers[(uint)Opcodes.SMSG_LOOT_RELEASE_RESPONSE] = HandleSMSGLootReleaseResponse;
            _handlers[(uint)Opcodes.SMSG_LOOT_RESPONSE] = HandleSMSGLootResponse;
            _handlers[(uint)Opcodes.SMSG_RESURRECT_REQUEST] = HandleSMSGResurrectRequest;
            _handlers[(uint)Opcodes.SMSG_PLAYERBOUND] = HandleSMSGPlayerBound;
            _handlers[(uint)Opcodes.SMSG_BINDPOINTUPDATE] = HandleSMSGBindPointUpdate;
            _handlers[(uint)Opcodes.SMSG_SPELLENERGIZELOG] = HandleSMSGSpellEnergizeLog;
            _handlers[(uint)Opcodes.SMSG_SPELLHEALLOG] = HandleSMSGSpellHealLog;
            _handlers[(uint)Opcodes.SMSG_CANCEL_COMBAT] = HandleSMSGCancelCombat;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_CANT_ATTACK] = HandleSMSGAttackSwingCantAttack;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_DEADTARGET] = HandleSMSGAttackSwingDeadTarget;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_NOTSTANDING] = HandleSMSGAttackSwingNotStanding;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_BADFACING] = HandleSMSGAttackSwingBadFacing;
            _handlers[(uint)Opcodes.SMSG_ATTACKSWING_NOTINRANGE] = HandleSMSGAttackSwingNotInRange;
            _handlers[(uint)Opcodes.SMSG_ATTACKSTOP] = HandleSMSGAttackStop;
            _handlers[(uint)Opcodes.SMSG_ATTACKSTART] = HandleSMSGAttackStart;
            _handlers[(uint)Opcodes.SMSG_AI_REACTION] = HandleSMSGAIReaction;
            _handlers[(uint)Opcodes.SMSG_PET_CAST_FAILED] = HandleSMSGPetCastFailed;
            _handlers[(uint)Opcodes.SMSG_UPDATE_AURA_DURATION] = HandleSMSGUpdateAuraDuration;
            _handlers[(uint)Opcodes.SMSG_COOLDOWN_EVENT] = HandleSMSGCooldownEvent;
            _handlers[(uint)Opcodes.SMSG_SPELL_COOLDOWN] = HandleSMSGSpellCooldown;
            _handlers[(uint)Opcodes.SMSG_SPELL_FAILURE] = HandleSMSGSpellFailure;
            _handlers[(uint)Opcodes.SMSG_SPELL_GO] = HandleSMSGSpellGo;
            _handlers[(uint)Opcodes.SMSG_SPELL_START] = HandleSMSGSpellStart;
            _handlers[(uint)Opcodes.SMSG_SUPERCEDED_SPELL] = HandleSMSGSupersededSpell;
            _handlers[(uint)Opcodes.SMSG_LEARNED_SPELL] = HandleSMSGLearnedSpell;
            _handlers[(uint)Opcodes.SMSG_INITIAL_SPELLS] = HandleSMSGInitialSpells;
            _handlers[(uint)Opcodes.SMSG_ACTION_BUTTONS] = HandleSMSGActionButtons;
            _handlers[(uint)Opcodes.SMSG_SET_PROFICIENCY] = HandleSMSGSetProficiency;
            _handlers[(uint)Opcodes.SMSG_CHANNEL_LIST] = HandleSMSGChannelList;
            _handlers[(uint)Opcodes.SMSG_GUILD_COMMAND_RESULT] = HandleSMSGGuildCommandResult;
            _handlers[(uint)Opcodes.SMSG_GUILD_EVENT] = HandleSMSGGuildEvent;
            _handlers[(uint)Opcodes.SMSG_FRIEND_STATUS] = HandleSMSGFriendStatus;
            _handlers[(uint)Opcodes.SMSG_PLAYED_TIME] = HandleSMSGPlayedTime;
            _handlers[(uint)Opcodes.SMSG_TRADE_STATUS] = HandleSMSGTradeStatus;
            _handlers[(uint)Opcodes.SMSG_PETITION_SHOW_SIGNATURES] = HandleSMSGPetitionShowSignatures;
            _handlers[(uint)Opcodes.SMSG_LOGOUT_COMPLETE] = HandleSMSGLogoutComplete;
            _handlers[(uint)Opcodes.SMSG_LOGOUT_RESPONSE] = HandleSMSGLogoutResponse;
            _handlers[(uint)Opcodes.SMSG_GROUP_UNINVITE] = HandleSMSGGroupUninvite;
            _handlers[(uint)Opcodes.SMSG_GROUP_DESTROYED] = HandleSMSGGroupDestroyed;
            _handlers[(uint)Opcodes.SMSG_CHAT_WRONG_FACTION] = HandleSMSGChatWrongFaction;
            _handlers[(uint)Opcodes.SMSG_CHAT_PLAYER_NOT_FOUND] = HandleSMSGChatPlayerNotFound;
            _handlers[(uint)Opcodes.SMSG_ACCOUNT_DATA_TIMES] = HandleSMSGAccountDataTimes;
            _handlers[(uint)Opcodes.SMSG_CHANNEL_NOTIFY] = HandleSMSGChannelNotify;
            _handlers[(uint)Opcodes.SMSG_PONG] = HandleSMSG_Pong;
            _handlers[(uint)Opcodes.SMSG_CHAR_RENAME] = HandleSMSGCharRename;
            _handlers[(uint)Opcodes.SMSG_CHAR_DELETE] = HandleSMSGCharDelete;
            _handlers[(uint)Opcodes.SMSG_CHAR_CREATE] = HandleSMSGCharCreate;
            _handlers[(uint)Opcodes.SMSG_ZONE_UNDER_ATTACK] = HandleZoneUnderAttack;
            _handlers[(uint)Opcodes.SMSG_PARTY_MEMBER_STATS] = HandlePartyMemberStats;
            _handlers[(uint)Opcodes.SMSG_GROUP_SET_LEADER] = HandleGroupSetLeader;
            _handlers[(uint)Opcodes.SMSG_GROUP_INVITE] = HandleGroupInvite;
            _handlers[(uint)Opcodes.SMSG_IGNORE_LIST] = HandleIgnoreList;
            _handlers[(uint)Opcodes.SMSG_FRIEND_LIST] = HandleFriendList;
            _handlers[(uint)Opcodes.SMSG_WHOIS] = HandleWhois;
            _handlers[(uint)Opcodes.SMSG_CREATURE_QUERY_RESPONSE] = HandleCreatureQueryResponse;
            _handlers[(uint)Opcodes.SMSG_GAMEOBJECT_QUERY_RESPONSE] = HandleGameObjectQueryResponse;
            _handlers[(uint)Opcodes.SMSG_QUEST_QUERY_RESPONSE] = HandleQuestQueryResponse;
            _handlers[(uint)Opcodes.SMSG_PAGE_TEXT_QUERY_RESPONSE] = HandlePageTextQueryResponse;
            _handlers[(uint)Opcodes.SMSG_ITEM_QUERY_SINGLE_RESPONSE] = HandleItemQuerySingleResponse;
            _handlers[(uint)Opcodes.SMSG_GUILD_QUERY_RESPONSE] = HandleGuildQueryResponse;
            _handlers[(uint)Opcodes.SMSG_NAME_QUERY_RESPONSE] = HandleNameQueryResponse;
            _handlers[(uint)Opcodes.SMSG_CHARACTER_LOGIN_FAILED] = HandleCharacterLoginFailed;
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

        private void HandleCompressedUpdateObject(byte[] body)
        {
            //Console.WriteLine("[WorldClient][HandleCompressedUpdateObject] Received compressed data.");

            using (var memoryStream = new MemoryStream(body))
            using (var reader = new BinaryReader(memoryStream))
            {
                uint compressSize = reader.ReadUInt32();

                byte[] compressedData = reader.ReadBytes((int)(memoryStream.Length - 4));
                byte[] decompressedData = DecompressData(compressedData, compressSize);

                if (decompressedData.Length != compressSize)
                {
                    Console.WriteLine($" *********** UNCOMPRESSED SIZE [{compressSize}] does not equal INFLATED SIZE [{decompressedData.Length}]");
                }

                if (decompressedData.Length > 0)
                {
                    // Handle the decompressed data as a new packet
                    HandleUpdateObject(decompressedData);
                }
                else
                {
                    Console.WriteLine("[WorldClient][HandleCompressedUpdateObject] Decompressed data is empty.");
                }
            }
        }

        private byte[] DecompressData(byte[] data, uint expectedSize)
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
                    uint amountOfObjects = reader.ReadUInt32();
                    //Console.WriteLine($"[WorldClient][HandleUpdateObject] Amount of objects: {amountOfObjects}");

                    for (uint i = 0; i < amountOfObjects; i++)
                    {
                        byte opTypeValue = reader.ReadByte();
                        if (!Enum.IsDefined(typeof(OpType), opTypeValue))
                        {
                            Console.WriteLine($"[WorldClient][HandleUpdateObject] Unknown operation type: {opTypeValue}");
                            continue;
                        }

                        Operation operation = new Operation
                        {
                            OType = (OpType)opTypeValue
                        };

                        switch (operation.OType)
                        {
                            case OpType.UPDATE_PARTIAL:
                                operation.FobjGUID = ReadPackedGuid(reader);
                                ExtractUpdateBlock(reader, operation);
                                break;
                            case OpType.UPDATE_MOVEMENT:
                                operation.FobjGUID = ReadPackedGuid(reader);
                                ExtractMovementBlock(reader, operation);
                                break;
                            case OpType.UPDATE_FULL:
                                operation.FobjGUID = ReadPackedGuid(reader);
                                UpdateFull(reader, operation);
                                break;
                            case OpType.UPDATE_OUT_OF_RANGE:
                                operation.NumGUIDs = reader.ReadUInt32();
                                UpdateOutOfRange(reader, operation);
                                break;
                            case OpType.UPDATE_IN_RANGE:
                                operation.NumGUIDs = reader.ReadUInt32();
                                UpdateInRange(reader, operation);
                                break;
                            default:
                                //Console.WriteLine($"[WorldClient][HandleUpdateObject] Unknown operation type: {operation.OType}");
                                break;
                        }
                    }
                }
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"[WorldClient][HandleUpdateObject] EndOfStreamException: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[WorldClient][HandleUpdateObject] Exception: {e.Message}");
            }
        }

        private void ExtractUpdateBlock(BinaryReader reader, Operation operation)
        {
            operation.NumUpdateMaskBlocks = reader.ReadByte();
            if (operation.NumUpdateMaskBlocks > 64)
            {
                Console.WriteLine($"[ExtractUpdateBlock] NumUpdateMaskBlocks is unusually high: {operation.NumUpdateMaskBlocks}. Aborting.");
                return;
            }

            operation.MaskBlocks = new uint[operation.NumUpdateMaskBlocks];
            for (int i = 0; i < operation.NumUpdateMaskBlocks; i++)
            {
                if (reader.BaseStream.Position + 4 > reader.BaseStream.Length)
                {
                    Console.WriteLine("[ExtractUpdateBlock] Not enough data to read MaskBlocks.");
                    return;
                }
                operation.MaskBlocks[i] = reader.ReadUInt32();
                Console.WriteLine($"[ExtractUpdateBlock] MaskBlock[{i}]: {operation.MaskBlocks[i]:X8}");
            }

            operation.Masks = new uint[operation.NumUpdateMaskBlocks][];
            for (int i = 0; i < operation.NumUpdateMaskBlocks; i++)
            {
                uint bit = 1;
                int numBitsSet = 0;
                for (int j = 0; j < 32; j++)
                {
                    if ((operation.MaskBlocks[i] & bit) != 0)
                        numBitsSet++;
                    bit <<= 1;
                }
                operation.Masks[i] = new uint[numBitsSet];
                for (int j = 0; j < numBitsSet; j++)
                {
                    if (reader.BaseStream.Position + 4 > reader.BaseStream.Length)
                    {
                        Console.WriteLine("[ExtractUpdateBlock] Not enough data to read Masks.");
                        return;
                    }
                    operation.Masks[i][j] = reader.ReadUInt32();
                    Console.WriteLine($"[ExtractUpdateBlock] Mask[{i}][{j}]: {operation.Masks[i][j]:X8}");
                }
            }
        }

        private void ExtractMovementBlock(BinaryReader reader, Operation operation)
        {
            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(ulong))
            {
                Console.WriteLine("[ExtractMovementBlock] Not enough data to read MoveGUID.");
                return;
            }

            operation.MoveGUID = ReadPackedGuid(reader);
            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(uint))
            {
                Console.WriteLine("[ExtractMovementBlock] Not enough data to read UfFlags.");
                return;
            }

            operation.UfFlags = (UpdateFlags)reader.ReadUInt32();

            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(uint))
            {
                Console.WriteLine("[ExtractMovementBlock] Not enough data to read Uf2Flags.");
                return;
            }

            operation.Uf2Flags = reader.ReadUInt32();
            // Further parsing logic based on flags
        }

        private void UpdateFull(BinaryReader reader, Operation operation)
        {
            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(byte))
            {
                Console.WriteLine("[UpdateFull] Not enough data to read ObjType.");
                return;
            }

            operation.ObjType = reader.ReadByte();
            ExtractMovementBlock(reader, operation);
            ExtractUpdateBlock(reader, operation);
        }

        private void UpdateOutOfRange(BinaryReader reader, Operation operation)
        {
            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(uint))
            {
                Console.WriteLine("[UpdateOutOfRange] Not enough data to read NumGUIDs.");
                return;
            }

            operation.NumGUIDs = reader.ReadUInt32();
            operation.GUIDs = new ulong[operation.NumGUIDs];
            for (int i = 0; i < operation.NumGUIDs; i++)
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(ulong))
                {
                    Console.WriteLine("[UpdateOutOfRange] Not enough data to read GUID.");
                    return;
                }

                operation.GUIDs[i] = ReadPackedGuid(reader);
            }
            // Further processing if needed
        }

        private void UpdateInRange(BinaryReader reader, Operation operation)
        {
            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(uint))
            {
                Console.WriteLine("[UpdateInRange] Not enough data to read NumGUIDs.");
                return;
            }

            operation.NumGUIDs = reader.ReadUInt32();
            operation.GUIDs = new ulong[operation.NumGUIDs];
            for (int i = 0; i < operation.NumGUIDs; i++)
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(ulong))
                {
                    Console.WriteLine("[UpdateInRange] Not enough data to read GUID.");
                    return;
                }

                operation.GUIDs[i] = ReadPackedGuid(reader);
            }
            // Further processing if needed
        }

        private void ParseObject(BinaryReader reader, byte hasTransport)
        {
            ulong guid = ReadPackedGuid(reader);
            Console.WriteLine($"[WorldClient][ParseObject] GUID: {guid}");

            // Additional parsing logic for the object
            // Example: Read movement, values, create objects, etc.
            ParseMovement(reader);
            ParseValues(reader);
            ParseCreateObjects(reader);
        }

        private byte[] DecompressData(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                using (var zlibStream = new ZlibStream(ms, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    zlibStream.CopyTo(outputStream);
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient][DecompressData] Unexpected error: {ex}");
                return Array.Empty<byte>();
            }
        }

        private void ParseUpdateObject(BinaryReader reader)
        {
            int objectsCount = reader.ReadInt32();
            Console.WriteLine($"[WorldClient][HandleCompressedUpdateObject] Objects count: {objectsCount}");

            for (int i = 0; i < objectsCount; i++)
            {
                var updateType = (UpdateTypes)reader.ReadByte();
                switch (updateType)
                {
                    case UpdateTypes.UPDATETYPE_VALUES:
                        ParseValues(reader);
                        break;
                    case UpdateTypes.UPDATETYPE_MOVEMENT:
                        ParseMovement(reader);
                        break;
                    case UpdateTypes.UPDATETYPE_CREATE_OBJECT:
                    case UpdateTypes.UPDATETYPE_CREATE_OBJECT2:
                        ParseCreateObjects(reader);
                        break;
                    case UpdateTypes.UPDATETYPE_OUT_OF_RANGE_OBJECTS:
                        ParseOutOfRangeObjects(reader);
                        break;
                    case UpdateTypes.UPDATETYPE_NEAR_OBJECTS:
                        ParseNearObjects(reader);
                        break;
                    default:
                        Console.WriteLine($"Unknown update type: {updateType}");
                        break;
                }
            }
        }

        private void ParseValues(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            Console.WriteLine($"Parsing values for GUID: {guid}");

            int numFields = reader.ReadInt32();
            for (int i = 0; i < numFields; i++)
            {
                int fieldIndex = reader.ReadInt32();
                uint fieldValue = reader.ReadUInt32();
                Console.WriteLine($"Field Index: {fieldIndex}, Field Value: {fieldValue}");
            }
        }

        private void ParseMovement(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            //Console.WriteLine($"Parsing movement for GUID: {guid}");

            byte movementFlags = reader.ReadByte();
            byte movementFlags2 = reader.ReadByte();
            uint time = reader.ReadUInt32();

            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float orientation = reader.ReadSingle();

            Console.WriteLine($"Movement: Flags: {movementFlags}, Flags2: {movementFlags2}, Time: {time}, Position: ({x}, {y}, {z}), Orientation: {orientation}");

            if ((movementFlags & 0x20) != 0) // Check if there's transport data
            {
                ulong transportGuid = ReadPackedGuid(reader);
                float transX = reader.ReadSingle();
                float transY = reader.ReadSingle();
                float transZ = reader.ReadSingle();
                float transO = reader.ReadSingle();
                uint transTime = reader.ReadUInt32();
                byte transSeat = reader.ReadByte();
                //Console.WriteLine($"Transport: GUID: {transportGuid}, Position: ({transX}, {transY}, {transZ}), Orientation: {transO}, Time: {transTime}, Seat: {transSeat}");
            }

            if ((movementFlags & 0x200000) != 0) // Check if there's spline data
            {
                // Handle spline data
            }
        }

        private void ParseCreateObjects(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            //Console.WriteLine($"Creating object with GUID: {guid}");

            byte objectType = reader.ReadByte();
            uint mapId = reader.ReadUInt32();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float orientation = reader.ReadSingle();
            //Console.WriteLine($"Object Type: {objectType}, Map ID: {mapId}, Position: ({x}, {y}, {z}), Orientation: {orientation}");

            // Parse movement data if applicable
            if (objectType == 0x1) // If the object is a unit
            {
                ParseMovement(reader);
            }

            // Parse values
            ParseValues(reader);
        }

        private void ParseOutOfRangeObjects(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            //Console.WriteLine($"Number of out-of-range objects: {count}");
            for (int i = 0; i < count; i++)
            {
                ulong guid = ReadPackedGuid(reader);
                //Console.WriteLine($"Out-of-range object GUID: {guid}");
            }
        }

        private void ParseNearObjects(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            //Console.WriteLine($"Number of near objects: {count}");
            for (int i = 0; i < count; i++)
            {
                ulong guid = ReadPackedGuid(reader);
                //Console.WriteLine($"Near object GUID: {guid}");
            }
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

        private void HandleSMSGQuestGiverQuestList(byte[] body) { }

        private void HandleSMSGQuestGiverQuestDetails(byte[] body) { }

        private void HandleSMSGQuestGiverRequestItems(byte[] body) { }

        private void HandleSMSGQuestGiverOfferReward(byte[] body) { }

        private void HandleSMSGQuestGiverQuestInvalid(byte[] body) { }

        private void HandleSMSGQuestGiverQuestComplete(byte[] body) { }

        private void HandleSMSGQuestGiverQuestFailed(byte[] body) { }

        private void HandleSMSGQuestLogFull(byte[] body) { }

        private void HandleSMSGQuestUpdateFailed(byte[] body) { }

        private void HandleSMSGQuestUpdateFailedTimer(byte[] body) { }

        private void HandleSMSGQuestUpdateComplete(byte[] body) { }

        private void HandleSMSGQuestUpdateAddKill(byte[] body) { }

        private void HandleSMSGQuestUpdateAddItem(byte[] body) { }

        private void HandleSMSGQuestConfirmAccept(byte[] body) { }

        private void HandleSMSGNewTaxiPath(byte[] body) { }

        private void HandleSMSGTrainerList(byte[] body) { }

        private void HandleSMSGTrainerBuySucceeded(byte[] body) { }

        private void HandleSMSGTrainerBuyFailed(byte[] body) { }

        private void HandleSMSGShowBank(byte[] body) { }

        private void HandleSMSGBuyBankSlotResult(byte[] body) { }

        private void HandleSMSGPetitionSignResults(byte[] body) { }

        private void HandleSMSGTurnInPetitionResults(byte[] body) { }

        private void HandleSMSGPetitionQueryResponse(byte[] body) { }

        private void HandleSMSGFishNotHooked(byte[] body) { }

        private void HandleSMSGFishEscaped(byte[] body) { }

        private void HandleSMSGNotification(byte[] body) { }

        private void HandleSMSGQueryTimeResponse(byte[] body) { }

        private void HandleSMSGLogXpGain(byte[] body) { }

        private void HandleSMSGLevelUpInfo(byte[] body) { }

        private void HandleSMSGResistLog(byte[] body) { }

        private void HandleSMSGEnchantmentLog(byte[] body) { }

        private void HandleSMSGStartMirrorTimer(byte[] body) { }

        private void HandleSMSGPauseMirrorTimer(byte[] body) { }

        private void HandleSMSGStopMirrorTimer(byte[] body) { }

        private void HandleSMSGClearCooldown(byte[] body) { }

        private void HandleSMSGGameObjectCustomAnim(byte[] body) { }

        private void HandleSMSGQuestGiverStatus(byte[] body) { }

        private void HandleSMSGNpcTextUpdate(byte[] body) { }

        private void HandleSMSGGossipComplete(byte[] body) { }

        private void HandleSMSGGossipMessage(byte[] body) { }

        private void HandleSMSGPetMode(byte[] body) { }

        private void HandleSMSGPetSpells(byte[] body) { }

        private void HandleSMSGPetNameInvalid(byte[] body) { }

        private void HandleSMSGPetTameFailure(byte[] body) { }

        private void HandleSMSGMountSpecialAnim(byte[] body) { }

        private void HandleSMSGDismountResult(byte[] body) { }

        private void HandleSMSGMountResult(byte[] body) { }

        private void HandleSMSGDuelWinner(byte[] body) { }

        private void HandleSMSGDuelComplete(byte[] body) { }

        private void HandleSMSGDuelInBounds(byte[] body) { }

        private void HandleSMSGDuelOutOfBounds(byte[] body) { }

        private void HandleSMSGDuelRequested(byte[] body) { }

        private void HandleSMSGItemPushResult(byte[] body) { }

        private void HandleSMSGLootClearMoney(byte[] body) { }

        private void HandleSMSGLootMoneyNotify(byte[] body) { }

        private void HandleSMSGLootRemoved(byte[] body) { }

        private void HandleSMSGLootReleaseResponse(byte[] body) { }

        private void HandleSMSGLootResponse(byte[] body) { }

        private void HandleSMSGResurrectRequest(byte[] body) { }

        private void HandleSMSGPlayerBound(byte[] body) { }

        private void HandleSMSGBindPointUpdate(byte[] body) { }

        private void HandleSMSGSpellEnergizeLog(byte[] body) { }

        private void HandleSMSGSpellHealLog(byte[] body) { }

        private void HandleSMSGCancelCombat(byte[] body) { }

        private void HandleSMSGAttackSwingCantAttack(byte[] body) { }

        private void HandleSMSGAttackSwingDeadTarget(byte[] body) { }

        private void HandleSMSGAttackSwingNotStanding(byte[] body) { }

        private void HandleSMSGAttackSwingBadFacing(byte[] body) { }

        private void HandleSMSGAttackSwingNotInRange(byte[] body) { }

        private void HandleSMSGAttackStop(byte[] body) { }

        private void HandleSMSGAttackStart(byte[] body) { }

        private void HandleSMSGAIReaction(byte[] body) { }

        private void HandleSMSGPetCastFailed(byte[] body) { }

        private void HandleSMSGUpdateAuraDuration(byte[] body) { }

        private void HandleSMSGCooldownEvent(byte[] body) { }

        private void HandleSMSGSpellCooldown(byte[] body) { }

        private void HandleSMSGSpellFailure(byte[] body) { }

        private void HandleSMSGSpellGo(byte[] body) { }

        private void HandleSMSGSpellStart(byte[] body) { }

        private void HandleSMSGSupersededSpell(byte[] body) { }

        private void HandleSMSGLearnedSpell(byte[] body) { }

        private void HandleSMSGInitialSpells(byte[] body) { }

        private void HandleSMSGActionButtons(byte[] body) { }

        private void HandleSMSGSetProficiency(byte[] body) { }

        private void HandleSMSGChannelList(byte[] body) { }

        private void HandleSMSGGuildCommandResult(byte[] body) { }

        private void HandleSMSGGuildEvent(byte[] body) { }

        private void HandleSMSGFriendStatus(byte[] body) { }

        private void HandleSMSGPlayedTime(byte[] body) { }

        private void HandleSMSGTradeStatus(byte[] body) { }

        private void HandleSMSGPetitionShowSignatures(byte[] body) { }

        private void HandleSMSGLogoutComplete(byte[] body) { }

        private void HandleSMSGLogoutResponse(byte[] body) { }

        private void HandleSMSGGroupUninvite(byte[] body) { }

        private void HandleSMSGGroupDestroyed(byte[] body) { }

        private void HandleSMSGChatWrongFaction(byte[] body) { }

        private void HandleSMSGChatPlayerNotFound(byte[] body) { }

        private void HandleSMSGAccountDataTimes(byte[] body) { }

        private void HandleSMSGChannelNotify(byte[] body) { }

        private void HandleSMSG_Pong(byte[] body) { }

        private void HandleSMSGCharRename(byte[] body) { }

        private void HandleSMSGCharDelete(byte[] body) { }

        private void HandleSMSGCharCreate(byte[] body) { }

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

        private static void HandleSMSGCompressedMoves(byte[] body)
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

                // Handle specific movement opcodes
                switch (innerOpcode)
                {
                    case 0x0B5: // MSG_MOVE_START_FORWARD
                    case 0x0B6: // MSG_MOVE_START_BACKWARD
                    case 0x0B7: // MSG_MOVE_STOP
                    case 0x0B8: // MSG_MOVE_START_STRAFE_LEFT
                    case 0x0B9: // MSG_MOVE_START_STRAFE_RIGHT
                    case 0x0BA: // MSG_MOVE_STOP_STRAFE
                    case 0x0BB: // MSG_MOVE_JUMP
                    case 0x0BC: // MSG_MOVE_START_TURN_LEFT
                    case 0x0BD: // MSG_MOVE_START_TURN_RIGHT
                    case 0x0BE: // MSG_MOVE_STOP_TURN
                    case 0x0BF: // MSG_MOVE_START_PITCH_UP
                    case 0x0C0: // MSG_MOVE_START_PITCH_DOWN
                    case 0x0C1: // MSG_MOVE_STOP_PITCH
                    case 0x0C2: // MSG_MOVE_SET_RUN_MODE
                    case 0x0C3: // MSG_MOVE_SET_WALK_MODE
                    case 0x0C4: // MSG_MOVE_TOGGLE_LOGGING
                    case 0x0C5: // MSG_MOVE_TELEPORT
                        HandleMoveTeleport(innerPacketData);
                        break;
                    case 0x0C6: // MSG_MOVE_TELEPORT_CHEAT
                    case 0x0C7: // MSG_MOVE_TELEPORT_ACK
                    case 0x0C8: // MSG_MOVE_TOGGLE_FALL_LOGGING
                    case 0x0C9: // MSG_MOVE_FALL_LAND
                        HandleMoveFallLand(innerPacketData);
                        break;
                    case 0x0CA: // MSG_MOVE_START_SWIM
                    case 0x0CB: // MSG_MOVE_STOP_SWIM
                    case 0x0CC: // MSG_MOVE_SET_RUN_SPEED_CHEAT
                    case 0x0CD: // MSG_MOVE_SET_RUN_SPEED
                    case 0x0CE: // MSG_MOVE_SET_RUN_BACK_SPEED_CHEAT
                    case 0x0CF: // MSG_MOVE_SET_RUN_BACK_SPEED
                    case 0x0D0: // MSG_MOVE_SET_WALK_SPEED_CHEAT
                    case 0x0D1: // MSG_MOVE_SET_WALK_SPEED
                    case 0x0D2: // MSG_MOVE_SET_SWIM_SPEED_CHEAT
                    case 0x0D3: // MSG_MOVE_SET_SWIM_SPEED
                    case 0x0D4: // MSG_MOVE_SET_SWIM_BACK_SPEED_CHEAT
                    case 0x0D5: // MSG_MOVE_SET_SWIM_BACK_SPEED
                    case 0x0D6: // MSG_MOVE_SET_ALL_SPEED_CHEAT
                    case 0x0D7: // MSG_MOVE_SET_TURN_RATE_CHEAT
                    case 0x0D8: // MSG_MOVE_SET_TURN_RATE
                    case 0x0D9: // MSG_MOVE_TOGGLE_COLLISION_CHEAT
                    case 0x0DA: // MSG_MOVE_SET_FACING
                    case 0x0DB: // MSG_MOVE_SET_PITCH
                    case 0x0DC: // MSG_MOVE_WORLDPORT_ACK
                        HandleMoveAck(innerPacketData);
                        break;
                    default:
                        Console.WriteLine($"[WorldClient][HandleNetworkMessages] Unhandled inner opcode: {innerOpcode:X}");
                        break;
                }
            }
        }

        private static void HandleMoveTeleport(byte[] data) => Console.WriteLine("[WorldClient][HandleNetworkMessages] Handling MSG_MOVE_TELEPORT...");// Add logic to handle MSG_MOVE_TELEPORT

        private static void HandleMoveFallLand(byte[] data) => Console.WriteLine("[WorldClient][HandleNetworkMessages] Handling MSG_MOVE_FALL_LAND...");// Add logic to handle MSG_MOVE_FALL_LAND

        private static void HandleMoveAck(byte[] data) => Console.WriteLine("[WorldClient][HandleNetworkMessages] Handling MSG_MOVE_ACK...");// Add logic to handle various MSG_MOVE_ACK related opcodes

        private static void HandleUnknownOpcode(uint opcode, byte[] body)
        {
            //Console.WriteLine($"[OpCodeDispatcher] Unhandled opcode: {opcode:X}");
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
