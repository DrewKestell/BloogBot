using Ionic.Zlib;
using Org.BouncyCastle.Utilities.Zlib;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using WoWSlimClient.Constants;
using WoWSlimClient.Models;
using WowSrp.Header;

namespace WoWSlimClient.Client
{
    internal class WorldClient
    {
        private IPAddress _ipAddress = IPAddress.Loopback;
        private int _port = 0;

        private bool _isConnected;
        private bool _hasEnteredWorld;
        private bool _hasReceivedCharListReply;

        private TcpClient? _client = null;
        private VanillaDecryption _vanillaDecryption;
        private VanillaEncryption _vanillaEncryption;
        private NetworkStream _stream = null;
        private List<CharacterSelect> _characters = new();
        private Task _asyncListener;

        public bool IsConnected => _isConnected;
        public bool HasEnteredWorld => _hasEnteredWorld; 

        public List<CharacterSelect> GetCharactersOnRealm()
        {
            _hasReceivedCharListReply = false;

            SendCMSGCharEnum();

            while (!_hasReceivedCharListReply)
            {
                Task.Delay(100);
            }

            return _characters;
        }

        public void Connect(string username, IPAddress ipAddress, byte[] sessionKey, int port = 8085)
        {
            try
            {
                _ipAddress = ipAddress;
                _port = port;

                _vanillaDecryption = new VanillaDecryption(sessionKey);
                _vanillaEncryption = new VanillaEncryption(sessionKey);

                _client?.Close();
                _client = new TcpClient(AddressFamily.InterNetwork);
                _client.Connect(_ipAddress, _port);

                _stream = _client.GetStream();

                HandleAuthChallenge(username, sessionKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void HandleAuthChallenge(string username, byte[] sessionKey)
        {
            using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
            byte[] header = reader.ReadBytes(4);

            if (header.Length < 4)
            {
                Console.WriteLine($"[WorldClient] Received incomplete SMSG_AUTH_CHALLENGE header.");
                return;
            }

            ushort size = BitConverter.ToUInt16(header.Take(2).Reverse().ToArray(), 0);
            ushort opcode = BitConverter.ToUInt16(header.Skip(2).Take(2).ToArray(), 0);

            byte[] serverSeed = reader.ReadBytes(size - sizeof(ushort));
            if (serverSeed.Length < 4)
            {
                Console.WriteLine($"[WorldClient] Incomplete SMSG_AUTH_CHALLENGE packet.");
                return;
            }

            SendCMSGAuthSession(username, serverSeed, sessionKey);
        }

        private void SendCMSGAuthSession(string username, byte[] serverSeed, byte[] sessionKey)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);

                uint opcode = 0x01ED; // Opcode for CMSG_AUTH_SESSION
                uint build = 5875; // Revision of the client
                uint serverId = 1; // Server ID, this value may vary
                uint clientSeed = (uint)new Random().Next(int.MinValue, int.MaxValue); // Generate a random client seed

                byte[] clientProof = GenerateClientProof(username, clientSeed, serverSeed, sessionKey); // Generate the client proof

                byte[] decompressedAddonInfo = GenerateAddonInfo(); // Generate addon info

                byte[] compressedAddonInfo = CompressAddonInfo(decompressedAddonInfo); // Compress the addon info

                uint decompressedAddonInfoSize = (uint)decompressedAddonInfo.Length;

                ushort packetSize = (ushort)(4 + 4 + 4 + username.Length + 1 + 4 + clientProof.Length + 4 + compressedAddonInfo.Length);

                writer.Write(BitConverter.GetBytes(packetSize).Reverse().ToArray()); // Packet size (big-endian)
                writer.Write(opcode); // Opcode (little-endian)
                writer.Write(build); // Client build
                writer.Write(serverId); // Server ID
                writer.Write(Encoding.UTF8.GetBytes(username)); // Username
                writer.Write((byte)0); // Null terminator for username
                writer.Write(clientSeed); // Client seed
                writer.Write(clientProof); // Client proof
                writer.Write(decompressedAddonInfoSize); // Decompressed addon info size
                writer.Write(compressedAddonInfo);

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);

                _asyncListener = HandleNetworkMessagesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient] An error occurred while sending CMSG_AUTH_SESSION: {ex}");
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
                _isConnected = true;
            }
            else
            {
                _isConnected = false;
            }
        }

        private static void HandleRealmList(byte[] body)
        {
            if (body.Length < 2)
            {
                Console.WriteLine("[WorldClient] Incomplete REALM_LIST packet.");
                return;
            }

            int numberOfRealms = BitConverter.ToInt16(body.Take(2).ToArray(), 0);
        }

        private void HandleCharEnum(byte[] body)
        {
            _characters.Clear();

            try
            {
                using var memoryStream = new MemoryStream(body);
                using var reader = new BinaryReader(memoryStream);
                byte numOfCharacters = reader.ReadByte();
                ulong guid = 0;
                for (int i = 0; i < numOfCharacters; i++)
                {
                    _characters.Add(new CharacterSelect()
                    {
                        Guid = reader.ReadUInt64(),
                        Name = ReadCString(reader),
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
                _hasReceivedCharListReply = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SendCMSGCharEnum()
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                byte[] header = _vanillaEncryption.CreateClientHeader(4, 0x037);
                writer.Write(header); // Packet size: 4 (big-endian)

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();

                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending CMSG_CHAR_ENUM: " + ex.Message);
            }
        }

        public void EnterWorld(ulong characterGuid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                byte[] header = _vanillaEncryption.CreateClientHeader(12, 0x03D);
                writer.Write(header); // Opcode: CMSG_PLAYER_LOGIN
                writer.Write(characterGuid); // Character GUID

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WorldClient] An error occurred while sending CMSG_PLAYER_LOGIN: " + ex.Message);
            }
        }

        private async Task HandleNetworkMessagesAsync()
        {
            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                while (true) // Loop to continuously read messages
                {

                    // Read the header first to determine the size and opcode
                    byte[] header = await ReadAsync(reader, 4); // Adjust size if header structure is different
                    if (header.Length == 0)
                    {
                        _isConnected = false;
                        _hasEnteredWorld = false;
                        break; // Exit if we cannot read a full header
                    }

                    HeaderData headerData = _vanillaDecryption.ReadServerHeader(header);

                    // Read the packet body
                    byte[] body = await ReadAsync(reader, (int)(headerData.Size - sizeof(ushort))); // Adjust based on actual header size

                    if (headerData.Opcode == 0x010)
                    {
                        HandleRealmList(body);
                    }
                    else
                    {
                        // Switch based on the opcode to handle different messages
                        switch ((Opcodes)headerData.Opcode)
                        {
                            case Opcodes.SMSG_AUTH_RESPONSE:
                                HandleAuthResponse(body);
                                break;
                            case Opcodes.SMSG_CHAR_ENUM:
                                HandleCharEnum(body);
                                break;
                            case Opcodes.SMSG_COMPRESSED_MOVES:
                                HandleSMSGCompressedMoves(body);
                                break;
                            case Opcodes.SMSG_UPDATE_OBJECT:
                                HandleUpdateObject(body);
                                break;
                            case Opcodes.SMSG_ADDON_INFO:
                                HandleAddonInfo(body);
                                break;
                            case Opcodes.SMSG_LOGIN_VERIFY_WORLD:
                                HandleLoginVerifyWorld(body);
                                break;
                            case Opcodes.SMSG_CHARACTER_LOGIN_FAILED:
                                HandleCharacterLoginFailed(body);
                                break;
                            case Opcodes.SMSG_NAME_QUERY_RESPONSE:
                                HandleNameQueryResponse(body);
                                break;
                            case Opcodes.SMSG_GUILD_QUERY_RESPONSE:
                                HandleGuildQueryResponse(body);
                                break;
                            case Opcodes.SMSG_ITEM_QUERY_SINGLE_RESPONSE:
                                HandleItemQuerySingleResponse(body);
                                break;
                            case Opcodes.SMSG_PAGE_TEXT_QUERY_RESPONSE:
                                HandlePageTextQueryResponse(body);
                                break;
                            case Opcodes.SMSG_QUEST_QUERY_RESPONSE:
                                HandleQuestQueryResponse(body);
                                break;
                            case Opcodes.SMSG_GAMEOBJECT_QUERY_RESPONSE:
                                HandleGameObjectQueryResponse(body);
                                break;
                            case Opcodes.SMSG_CREATURE_QUERY_RESPONSE:
                                HandleCreatureQueryResponse(body);
                                break;
                            case Opcodes.SMSG_WHOIS:
                                HandleWhois(body);
                                break;
                            case Opcodes.SMSG_FRIEND_LIST:
                                HandleFriendList(body);
                                break;
                            case Opcodes.SMSG_IGNORE_LIST:
                                HandleIgnoreList(body);
                                break;
                            case Opcodes.SMSG_GROUP_INVITE:
                                HandleGroupInvite(body);
                                break;
                            case Opcodes.SMSG_GROUP_SET_LEADER:
                                HandleGroupSetLeader(body);
                                break;
                            case Opcodes.SMSG_PARTY_MEMBER_STATS:
                                HandlePartyMemberStats(body);
                                break;
                            case Opcodes.SMSG_ZONE_UNDER_ATTACK:
                                HandleZoneUnderAttack(body);
                                break;
                            case Opcodes.SMSG_CHAR_CREATE:
                                HandleSMSGCharCreate(body);
                                break;
                            case Opcodes.SMSG_CHAR_DELETE:
                                HandleSMSGCharDelete(body);
                                break;
                            case Opcodes.SMSG_CHAR_RENAME:
                                HandleSMSGCharRename(body);
                                break;
                            case Opcodes.SMSG_PONG:
                                HandleSMSG_Pong(body);
                                break;
                            case Opcodes.SMSG_ACCOUNT_DATA_TIMES:
                                HandleSMSGAccountDataTimes(body);
                                break;
                            case Opcodes.SMSG_CHANNEL_NOTIFY:
                                HandleSMSGChannelNotify(body);
                                break;
                            case Opcodes.SMSG_CHAT_PLAYER_NOT_FOUND:
                                HandleSMSGChatPlayerNotFound(body);
                                break;
                            case Opcodes.SMSG_CHAT_WRONG_FACTION:
                                HandleSMSGChatWrongFaction(body);
                                break;
                            case Opcodes.SMSG_GROUP_DESTROYED:
                                HandleSMSGGroupDestroyed(body);
                                break;
                            case Opcodes.SMSG_GROUP_UNINVITE:
                                HandleSMSGGroupUninvite(body);
                                break;
                            case Opcodes.SMSG_LOGOUT_RESPONSE:
                                HandleSMSGLogoutResponse(body);
                                break;
                            case Opcodes.SMSG_LOGOUT_COMPLETE:
                                HandleSMSGLogoutComplete(body);
                                break;
                            case Opcodes.SMSG_PETITION_SHOW_SIGNATURES:
                                HandleSMSGPetitionShowSignatures(body);
                                break;
                            case Opcodes.SMSG_TRADE_STATUS:
                                HandleSMSGTradeStatus(body);
                                break;
                            case Opcodes.SMSG_PLAYED_TIME:
                                HandleSMSGPlayedTime(body);
                                break;
                            case Opcodes.SMSG_FRIEND_STATUS:
                                HandleSMSGFriendStatus(body);
                                break;
                            case Opcodes.SMSG_GUILD_EVENT:
                                HandleSMSGGuildEvent(body);
                                break;
                            case Opcodes.SMSG_GUILD_COMMAND_RESULT:
                                HandleSMSGGuildCommandResult(body);
                                break;
                            case Opcodes.SMSG_CHANNEL_LIST:
                                HandleSMSGChannelList(body);
                                break;
                            case Opcodes.SMSG_SET_PROFICIENCY:
                                HandleSMSGSetProficiency(body);
                                break;
                            case Opcodes.SMSG_ACTION_BUTTONS:
                                HandleSMSGActionButtons(body);
                                break;
                            case Opcodes.SMSG_INITIAL_SPELLS:
                                HandleSMSGInitialSpells(body);
                                break;
                            case Opcodes.SMSG_LEARNED_SPELL:
                                HandleSMSGLearnedSpell(body);
                                break;
                            case Opcodes.SMSG_SUPERCEDED_SPELL:
                                HandleSMSGSupersededSpell(body);
                                break;
                            case Opcodes.SMSG_SPELL_START:
                                HandleSMSGSpellStart(body);
                                break;
                            case Opcodes.SMSG_SPELL_GO:
                                HandleSMSGSpellGo(body);
                                break;
                            case Opcodes.SMSG_SPELL_FAILURE:
                                HandleSMSGSpellFailure(body);
                                break;
                            case Opcodes.SMSG_SPELL_COOLDOWN:
                                HandleSMSGSpellCooldown(body);
                                break;
                            case Opcodes.SMSG_COOLDOWN_EVENT:
                                HandleSMSGCooldownEvent(body);
                                break;
                            case Opcodes.SMSG_UPDATE_AURA_DURATION:
                                HandleSMSGUpdateAuraDuration(body);
                                break;
                            case Opcodes.SMSG_PET_CAST_FAILED:
                                HandleSMSGPetCastFailed(body);
                                break;
                            case Opcodes.SMSG_AI_REACTION:
                                HandleSMSGAIReaction(body);
                                break;
                            case Opcodes.SMSG_ATTACKSTART:
                                HandleSMSGAttackStart(body);
                                break;
                            case Opcodes.SMSG_ATTACKSTOP:
                                HandleSMSGAttackStop(body);
                                break;
                            case Opcodes.SMSG_ATTACKSWING_NOTINRANGE:
                                HandleSMSGAttackSwingNotInRange(body);
                                break;
                            case Opcodes.SMSG_ATTACKSWING_BADFACING:
                                HandleSMSGAttackSwingBadFacing(body);
                                break;
                            case Opcodes.SMSG_ATTACKSWING_NOTSTANDING:
                                HandleSMSGAttackSwingNotStanding(body);
                                break;
                            case Opcodes.SMSG_ATTACKSWING_DEADTARGET:
                                HandleSMSGAttackSwingDeadTarget(body);
                                break;
                            case Opcodes.SMSG_ATTACKSWING_CANT_ATTACK:
                                HandleSMSGAttackSwingCantAttack(body);
                                break;
                            case Opcodes.SMSG_CANCEL_COMBAT:
                                HandleSMSGCancelCombat(body);
                                break;
                            case Opcodes.SMSG_SPELLHEALLOG:
                                HandleSMSGSpellHealLog(body);
                                break;
                            case Opcodes.SMSG_SPELLENERGIZELOG:
                                HandleSMSGSpellEnergizeLog(body);
                                break;
                            case Opcodes.SMSG_BINDPOINTUPDATE:
                                HandleSMSGBindPointUpdate(body);
                                break;
                            case Opcodes.SMSG_PLAYERBOUND:
                                HandleSMSGPlayerBound(body);
                                break;
                            case Opcodes.SMSG_RESURRECT_REQUEST:
                                HandleSMSGResurrectRequest(body);
                                break;
                            case Opcodes.SMSG_LOOT_RESPONSE:
                                HandleSMSGLootResponse(body);
                                break;
                            case Opcodes.SMSG_LOOT_RELEASE_RESPONSE:
                                HandleSMSGLootReleaseResponse(body);
                                break;
                            case Opcodes.SMSG_LOOT_REMOVED:
                                HandleSMSGLootRemoved(body);
                                break;
                            case Opcodes.SMSG_LOOT_MONEY_NOTIFY:
                                HandleSMSGLootMoneyNotify(body);
                                break;
                            case Opcodes.SMSG_LOOT_CLEAR_MONEY:
                                HandleSMSGLootClearMoney(body);
                                break;
                            case Opcodes.SMSG_ITEM_PUSH_RESULT:
                                HandleSMSGItemPushResult(body);
                                break;
                            case Opcodes.SMSG_DUEL_REQUESTED:
                                HandleSMSGDuelRequested(body);
                                break;
                            case Opcodes.SMSG_DUEL_OUTOFBOUNDS:
                                HandleSMSGDuelOutOfBounds(body);
                                break;
                            case Opcodes.SMSG_DUEL_INBOUNDS:
                                HandleSMSGDuelInBounds(body);
                                break;
                            case Opcodes.SMSG_DUEL_COMPLETE:
                                HandleSMSGDuelComplete(body);
                                break;
                            case Opcodes.SMSG_DUEL_WINNER:
                                HandleSMSGDuelWinner(body);
                                break;
                            case Opcodes.SMSG_MOUNTRESULT:
                                HandleSMSGMountResult(body);
                                break;
                            case Opcodes.SMSG_DISMOUNTRESULT:
                                HandleSMSGDismountResult(body);
                                break;
                            case Opcodes.SMSG_MOUNTSPECIAL_ANIM:
                                HandleSMSGMountSpecialAnim(body);
                                break;
                            case Opcodes.SMSG_PET_TAME_FAILURE:
                                HandleSMSGPetTameFailure(body);
                                break;
                            case Opcodes.SMSG_PET_NAME_INVALID:
                                HandleSMSGPetNameInvalid(body);
                                break;
                            case Opcodes.SMSG_PET_SPELLS:
                                HandleSMSGPetSpells(body);
                                break;
                            case Opcodes.SMSG_PET_MODE:
                                HandleSMSGPetMode(body);
                                break;
                            case Opcodes.SMSG_GOSSIP_MESSAGE:
                                HandleSMSGGossipMessage(body);
                                break;
                            case Opcodes.SMSG_GOSSIP_COMPLETE:
                                HandleSMSGGossipComplete(body);
                                break;
                            case Opcodes.SMSG_NPC_TEXT_UPDATE:
                                HandleSMSGNpcTextUpdate(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_STATUS:
                                HandleSMSGQuestGiverStatus(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_QUEST_LIST:
                                HandleSMSGQuestGiverQuestList(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_QUEST_DETAILS:
                                HandleSMSGQuestGiverQuestDetails(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_REQUEST_ITEMS:
                                HandleSMSGQuestGiverRequestItems(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_OFFER_REWARD:
                                HandleSMSGQuestGiverOfferReward(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_QUEST_INVALID:
                                HandleSMSGQuestGiverQuestInvalid(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_QUEST_COMPLETE:
                                HandleSMSGQuestGiverQuestComplete(body);
                                break;
                            case Opcodes.SMSG_QUESTGIVER_QUEST_FAILED:
                                HandleSMSGQuestGiverQuestFailed(body);
                                break;
                            case Opcodes.SMSG_QUESTLOG_FULL:
                                HandleSMSGQuestLogFull(body);
                                break;
                            case Opcodes.SMSG_QUESTUPDATE_FAILED:
                                HandleSMSGQuestUpdateFailed(body);
                                break;
                            case Opcodes.SMSG_QUESTUPDATE_FAILEDTIMER:
                                HandleSMSGQuestUpdateFailedTimer(body);
                                break;
                            case Opcodes.SMSG_QUESTUPDATE_COMPLETE:
                                HandleSMSGQuestUpdateComplete(body);
                                break;
                            case Opcodes.SMSG_QUESTUPDATE_ADD_KILL:
                                HandleSMSGQuestUpdateAddKill(body);
                                break;
                            case Opcodes.SMSG_QUESTUPDATE_ADD_ITEM:
                                HandleSMSGQuestUpdateAddItem(body);
                                break;
                            case Opcodes.SMSG_QUEST_CONFIRM_ACCEPT:
                                HandleSMSGQuestConfirmAccept(body);
                                break;
                            case Opcodes.SMSG_NEW_TAXI_PATH:
                                HandleSMSGNewTaxiPath(body);
                                break;
                            case Opcodes.SMSG_TRAINER_LIST:
                                HandleSMSGTrainerList(body);
                                break;
                            case Opcodes.SMSG_TRAINER_BUY_SUCCEEDED:
                                HandleSMSGTrainerBuySucceeded(body);
                                break;
                            case Opcodes.SMSG_TRAINER_BUY_FAILED:
                                HandleSMSGTrainerBuyFailed(body);
                                break;
                            case Opcodes.SMSG_SHOW_BANK:
                                HandleSMSGShowBank(body);
                                break;
                            case Opcodes.SMSG_BUY_BANK_SLOT_RESULT:
                                HandleSMSGBuyBankSlotResult(body);
                                break;
                            case Opcodes.SMSG_PETITION_SIGN_RESULTS:
                                HandleSMSGPetitionSignResults(body);
                                break;
                            case Opcodes.SMSG_TURN_IN_PETITION_RESULTS:
                                HandleSMSGTurnInPetitionResults(body);
                                break;
                            case Opcodes.SMSG_PETITION_QUERY_RESPONSE:
                                HandleSMSGPetitionQueryResponse(body);
                                break;
                            case Opcodes.SMSG_FISH_NOT_HOOKED:
                                HandleSMSGFishNotHooked(body);
                                break;
                            case Opcodes.SMSG_FISH_ESCAPED:
                                HandleSMSGFishEscaped(body);
                                break;
                            case Opcodes.SMSG_NOTIFICATION:
                                HandleSMSGNotification(body);
                                break;
                            case Opcodes.SMSG_QUERY_TIME_RESPONSE:
                                HandleSMSGQueryTimeResponse(body);
                                break;
                            case Opcodes.SMSG_LOG_XPGAIN:
                                HandleSMSGLogXpGain(body);
                                break;
                            case Opcodes.SMSG_LEVELUP_INFO:
                                HandleSMSGLevelUpInfo(body);
                                break;
                            case Opcodes.SMSG_RESISTLOG:
                                HandleSMSGResistLog(body);
                                break;
                            case Opcodes.SMSG_ENCHANTMENTLOG:
                                HandleSMSGEnchantmentLog(body);
                                break;
                            case Opcodes.SMSG_START_MIRROR_TIMER:
                                HandleSMSGStartMirrorTimer(body);
                                break;
                            case Opcodes.SMSG_PAUSE_MIRROR_TIMER:
                                HandleSMSGPauseMirrorTimer(body);
                                break;
                            case Opcodes.SMSG_STOP_MIRROR_TIMER:
                                HandleSMSGStopMirrorTimer(body);
                                break;
                            case Opcodes.SMSG_CLEAR_COOLDOWN:
                                HandleSMSGClearCooldown(body);
                                break;
                            case Opcodes.SMSG_GAMEOBJECT_CUSTOM_ANIM:
                                HandleSMSGGameObjectCustomAnim(body);
                                break;
                            case Opcodes.SMSG_SPELLLOGMISS:

                                break;
                            case Opcodes.SMSG_GROUP_LIST:

                                break;
                            case Opcodes.SMSG_MESSAGECHAT:

                                break;
                            case Opcodes.SMSG_FORCE_MOVE_UNROOT:

                                break;
                            case Opcodes.SMSG_STANDSTATE_UPDATE:

                                break;
                            case Opcodes.SMSG_SET_REST_START:

                                break;
                            case Opcodes.SMSG_LOGIN_SETTIMESPEED:

                                break;
                            case Opcodes.SMSG_TUTORIAL_FLAGS:

                                break;
                            case Opcodes.SMSG_INITIALIZE_FACTIONS:

                                break;
                            case Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT:
                                HandleCompressedUpdateObject(body);
                                break;
                            case Opcodes.SMSG_INIT_WORLD_STATES:

                                break;
                            case Opcodes.MSG_MOVE_SET_FACING:

                                break;
                            case Opcodes.SMSG_CLIENT_CONTROL_UPDATE:

                                break;
                            case Opcodes.MSG_MOVE_START_FORWARD:

                                break;
                            case Opcodes.MSG_MOVE_START_BACKWARD:

                                break;
                            case Opcodes.MSG_MOVE_START_STRAFE_LEFT:

                                break;
                            case Opcodes.MSG_MOVE_START_STRAFE_RIGHT:

                                break;
                            case Opcodes.SMSG_RAID_INSTANCE_MESSAGE:

                                break;
                            case Opcodes.SMSG_WEATHER:

                                break;
                            default:
                                Console.WriteLine($"[WorldClient][HandleNetworkMessages] Unhandled opcode: {headerData.Opcode:X}");
                                HandleUnknownOpcode((ushort)headerData.Opcode, body);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient][HandleNetworkMessages] An error occurred while handling network messages: {ex}");
            }
        }

        private void HandleCompressedUpdateObject(byte[] body)
        {
            Console.WriteLine("[WorldClient][HandleCompressedUpdateObject] Received compressed data.");
            try
            {
                byte[] decompressedData = DecompressData(body);
                if (decompressedData.Length == 0)
                {
                    Console.WriteLine("[WorldClient][HandleCompressedUpdateObject] Decompressed data is empty.");
                    return;
                }

                using (var reader = new BinaryReader(new MemoryStream(decompressedData)))
                {
                    int objectsCount = reader.ReadInt32();
                    Console.WriteLine($"[WorldClient][HandleCompressedUpdateObject] Objects count: {objectsCount}");

                    for (int i = 0; i < objectsCount; i++)
                    {
                        UpdateTypes updateType = (UpdateTypes)reader.ReadByte();
                        Console.WriteLine($"[WorldClient][HandleCompressedUpdateObject] UpdateType: {updateType}");

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
                                Console.WriteLine($"[WorldClient][HandleCompressedUpdateObject] Unknown update type: {updateType}");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient][HandleCompressedUpdateObject] Error: {ex.Message}");
            }
        }

        private byte[] DecompressData(byte[] compressedData)
        {
            try
            {
                using (var compressedStream = new MemoryStream(compressedData))
                using (var decompressionStream = new ZInputStream(compressedStream))
                using (var resultStream = new MemoryStream())
                {
                    decompressionStream.CopyTo(resultStream);
                    return resultStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient][DecompressData] Unexpected error: {ex.Message}");
                return Array.Empty<byte>();
            }
        }


        private void ParseValues(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            Console.WriteLine($"[WorldClient][ParseValues] Object guid: 0x{guid:X16}");
        }

        private void ParseMovement(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            Console.WriteLine($"[WorldClient][ParseMovement] Object guid: 0x{guid:X16}");
        }

        private void ParseCreateObjects(BinaryReader reader)
        {
            ulong guid = ReadPackedGuid(reader);
            Console.WriteLine($"[WorldClient][ParseCreateObjects] Object guid: 0x{guid:X16}");

            var objectTypeId = (ObjectTypes)reader.ReadByte();
        }

        private void ParseOutOfRangeObjects(BinaryReader reader)
        {
            uint count = reader.ReadUInt32();
            Console.WriteLine($"[WorldClient][ParseOutOfRangeObjects] OOR Objects count: {count}");

            for (int i = 0; i < count; i++)
            {
                ulong guid = ReadPackedGuid(reader);
                Console.WriteLine($"[WorldClient][ParseOutOfRangeObjects] OOR Object {i} Guid: 0x{guid:X16}");
            }
        }

        private void ParseNearObjects(BinaryReader reader)
        {
            uint count = reader.ReadUInt32();
            Console.WriteLine($"[WorldClient][ParseNearObjects] Near Objects count: {count}");

            for (int i = 0; i < count; i++)
            {
                ulong guid = ReadPackedGuid(reader);
                Console.WriteLine($"[WorldClient][ParseNearObjects] Near Object {i} Guid: 0x{guid:X16}");
            }
        }

        private ulong ReadPackedGuid(BinaryReader reader)
        {
            // Implement the packed GUID reading logic here
            return 0; // Placeholder
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

                Console.WriteLine($"Map ID: {mapId}, Position: ({x}, {y}, {z}), Orientation: {orientation}");
            }
        }
        private void HandleTutorialFlags(byte[] body)
        {
            using (var reader = new BinaryReader(new MemoryStream(body)))
            {
                byte[] tutorialFlags = reader.ReadBytes(32); // Assuming the packet contains 32 bytes

                bool allDisabled = tutorialFlags.All(b => b == 0xFF);
                Console.WriteLine($"Tutorials disabled: {allDisabled}");
            }
        }
        private void HandleUpdateObject(byte[] body)
        {
            using (var reader = new BinaryReader(new MemoryStream(body)))
            {
                int blockCount = reader.ReadInt32();
                byte hasTransport = reader.ReadByte();
                byte updateType = reader.ReadByte();
                ulong guid = ReadPackedGuid(reader);
                uint objectType = reader.ReadByte();
                uint updateFlags = reader.ReadByte();
                uint movementFlags = reader.ReadUInt32();
                uint timeStamp = reader.ReadUInt32();

                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                float posZ = reader.ReadSingle();
                float orientation = reader.ReadSingle();
                float fallTime = reader.ReadSingle();
                float walkSpeed = reader.ReadSingle();
                float runSpeed = reader.ReadSingle();
                float runBackSpeed = reader.ReadSingle();
                float swimSpeed = reader.ReadSingle();
                float swimBackSpeed = reader.ReadSingle();
                float turnSpeed = reader.ReadSingle();

                byte isPlayer = reader.ReadByte();

                // Skip unknown hardcoded bytes
                reader.BaseStream.Seek(3, SeekOrigin.Current);

                int maskBlockCount = reader.ReadByte();
                uint[] updateMask = new uint[maskBlockCount];

                for (int i = 0; i < maskBlockCount; i++)
                {
                    updateMask[i] = reader.ReadUInt32();
                }

                for (int i = 0; i < maskBlockCount; i++)
                {
                    if ((updateMask[i] & 0x01) != 0)
                    {
                        ulong guidField = reader.ReadUInt64();
                        Console.WriteLine($"GUID: {guidField}");
                    }
                    if ((updateMask[i] & 0x02) != 0)
                    {
                        uint typeField = reader.ReadUInt32();
                        Console.WriteLine($"Type: {typeField}");
                    }
                    if ((updateMask[i] & 0x04) != 0)
                    {
                        float scaleField = reader.ReadSingle();
                        Console.WriteLine($"Scale: {scaleField}");
                    }
                    if ((updateMask[i] & 0x08) != 0)
                    {
                        uint healthField = reader.ReadUInt32();
                        Console.WriteLine($"Health: {healthField}");
                    }
                    if ((updateMask[i] & 0x10) != 0)
                    {
                        uint maxHealthField = reader.ReadUInt32();
                        Console.WriteLine($"Max Health: {maxHealthField}");
                    }
                    if ((updateMask[i] & 0x20) != 0)
                    {
                        uint levelField = reader.ReadUInt32();
                        Console.WriteLine($"Level: {levelField}");
                    }
                    if ((updateMask[i] & 0x40) != 0)
                    {
                        uint factionTemplateField = reader.ReadUInt32();
                        Console.WriteLine($"Faction Template: {factionTemplateField}");
                    }
                    if ((updateMask[i] & 0x80) != 0)
                    {
                        byte[] bytesField = reader.ReadBytes(4);
                        Console.WriteLine($"Bytes: {BitConverter.ToString(bytesField)}");
                    }
                }

                Console.WriteLine($"Update Object: GUID: {guid}, Type: {objectType}, Position: ({posX}, {posY}, {posZ}), Orientation: {orientation}");
            }
        }


        private void HandleSMSGQuestGiverQuestList(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverQuestDetails(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverRequestItems(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverOfferReward(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverQuestInvalid(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverQuestComplete(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverQuestFailed(byte[] body)
        {

        }

        private void HandleSMSGQuestLogFull(byte[] body)
        {

        }

        private void HandleSMSGQuestUpdateFailed(byte[] body)
        {

        }

        private void HandleSMSGQuestUpdateFailedTimer(byte[] body)
        {

        }

        private void HandleSMSGQuestUpdateComplete(byte[] body)
        {

        }

        private void HandleSMSGQuestUpdateAddKill(byte[] body)
        {

        }

        private void HandleSMSGQuestUpdateAddItem(byte[] body)
        {

        }

        private void HandleSMSGQuestConfirmAccept(byte[] body)
        {

        }

        private void HandleSMSGNewTaxiPath(byte[] body)
        {

        }

        private void HandleSMSGTrainerList(byte[] body)
        {

        }

        private void HandleSMSGTrainerBuySucceeded(byte[] body)
        {

        }

        private void HandleSMSGTrainerBuyFailed(byte[] body)
        {

        }

        private void HandleSMSGShowBank(byte[] body)
        {

        }

        private void HandleSMSGBuyBankSlotResult(byte[] body)
        {

        }

        private void HandleSMSGPetitionSignResults(byte[] body)
        {

        }

        private void HandleSMSGTurnInPetitionResults(byte[] body)
        {

        }

        private void HandleSMSGPetitionQueryResponse(byte[] body)
        {

        }

        private void HandleSMSGFishNotHooked(byte[] body)
        {

        }

        private void HandleSMSGFishEscaped(byte[] body)
        {

        }

        private void HandleSMSGNotification(byte[] body)
        {

        }

        private void HandleSMSGQueryTimeResponse(byte[] body)
        {

        }

        private void HandleSMSGLogXpGain(byte[] body)
        {

        }

        private void HandleSMSGLevelUpInfo(byte[] body)
        {

        }

        private void HandleSMSGResistLog(byte[] body)
        {

        }

        private void HandleSMSGEnchantmentLog(byte[] body)
        {

        }

        private void HandleSMSGStartMirrorTimer(byte[] body)
        {

        }

        private void HandleSMSGPauseMirrorTimer(byte[] body)
        {

        }

        private void HandleSMSGStopMirrorTimer(byte[] body)
        {

        }

        private void HandleSMSGClearCooldown(byte[] body)
        {

        }

        private void HandleSMSGGameObjectCustomAnim(byte[] body)
        {

        }

        private void HandleSMSGQuestGiverStatus(byte[] body)
        {

        }

        private void HandleSMSGNpcTextUpdate(byte[] body)
        {

        }

        private void HandleSMSGGossipComplete(byte[] body)
        {

        }

        private void HandleSMSGGossipMessage(byte[] body)
        {

        }

        private void HandleSMSGPetMode(byte[] body)
        {

        }

        private void HandleSMSGPetSpells(byte[] body)
        {

        }

        private void HandleSMSGPetNameInvalid(byte[] body)
        {

        }

        private void HandleSMSGPetTameFailure(byte[] body)
        {

        }

        private void HandleSMSGMountSpecialAnim(byte[] body)
        {

        }

        private void HandleSMSGDismountResult(byte[] body)
        {

        }

        private void HandleSMSGMountResult(byte[] body)
        {

        }

        private void HandleSMSGDuelWinner(byte[] body)
        {

        }

        private void HandleSMSGDuelComplete(byte[] body)
        {

        }

        private void HandleSMSGDuelInBounds(byte[] body)
        {

        }

        private void HandleSMSGDuelOutOfBounds(byte[] body)
        {

        }

        private void HandleSMSGDuelRequested(byte[] body)
        {

        }

        private void HandleSMSGItemPushResult(byte[] body)
        {

        }

        private void HandleSMSGLootClearMoney(byte[] body)
        {

        }

        private void HandleSMSGLootMoneyNotify(byte[] body)
        {

        }

        private void HandleSMSGLootRemoved(byte[] body)
        {

        }

        private void HandleSMSGLootReleaseResponse(byte[] body)
        {

        }

        private void HandleSMSGLootResponse(byte[] body)
        {

        }

        private void HandleSMSGResurrectRequest(byte[] body)
        {

        }

        private void HandleSMSGPlayerBound(byte[] body)
        {

        }

        private void HandleSMSGBindPointUpdate(byte[] body)
        {

        }

        private void HandleSMSGSpellEnergizeLog(byte[] body)
        {

        }

        private void HandleSMSGSpellHealLog(byte[] body)
        {

        }

        private void HandleSMSGCancelCombat(byte[] body)
        {

        }

        private void HandleSMSGAttackSwingCantAttack(byte[] body)
        {

        }

        private void HandleSMSGAttackSwingDeadTarget(byte[] body)
        {

        }

        private void HandleSMSGAttackSwingNotStanding(byte[] body)
        {

        }

        private void HandleSMSGAttackSwingBadFacing(byte[] body)
        {

        }

        private void HandleSMSGAttackSwingNotInRange(byte[] body)
        {

        }

        private void HandleSMSGAttackStop(byte[] body)
        {

        }

        private void HandleSMSGAttackStart(byte[] body)
        {

        }

        private void HandleSMSGAIReaction(byte[] body)
        {

        }

        private void HandleSMSGPetCastFailed(byte[] body)
        {

        }

        private void HandleSMSGUpdateAuraDuration(byte[] body)
        {

        }

        private void HandleSMSGCooldownEvent(byte[] body)
        {

        }

        private void HandleSMSGSpellCooldown(byte[] body)
        {

        }

        private void HandleSMSGSpellFailure(byte[] body)
        {

        }

        private void HandleSMSGSpellGo(byte[] body)
        {

        }

        private void HandleSMSGSpellStart(byte[] body)
        {

        }

        private void HandleSMSGSupersededSpell(byte[] body)
        {

        }

        private void HandleSMSGLearnedSpell(byte[] body)
        {

        }

        private void HandleSMSGInitialSpells(byte[] body)
        {

        }

        private void HandleSMSGActionButtons(byte[] body)
        {

        }

        private void HandleSMSGSetProficiency(byte[] body)
        {

        }

        private void HandleSMSGChannelList(byte[] body)
        {

        }

        private void HandleSMSGGuildCommandResult(byte[] body)
        {

        }

        private void HandleSMSGGuildEvent(byte[] body)
        {

        }

        private void HandleSMSGFriendStatus(byte[] body)
        {

        }

        private void HandleSMSGPlayedTime(byte[] body)
        {

        }

        private void HandleSMSGTradeStatus(byte[] body)
        {

        }

        private void HandleSMSGPetitionShowSignatures(byte[] body)
        {

        }

        private void HandleSMSGLogoutComplete(byte[] body)
        {

        }

        private void HandleSMSGLogoutResponse(byte[] body)
        {

        }

        private void HandleSMSGGroupUninvite(byte[] body)
        {

        }

        private void HandleSMSGGroupDestroyed(byte[] body)
        {

        }

        private void HandleSMSGChatWrongFaction(byte[] body)
        {

        }

        private void HandleSMSGChatPlayerNotFound(byte[] body)
        {

        }

        private void HandleSMSGAccountDataTimes(byte[] body)
        {

        }

        private void HandleSMSGChannelNotify(byte[] body)
        {

        }

        private void HandleSMSG_Pong(byte[] body)
        {

        }

        private void HandleSMSGCharRename(byte[] body)
        {

        }

        private void HandleSMSGCharDelete(byte[] body)
        {

        }

        private void HandleSMSGCharCreate(byte[] body)
        {

        }

        private void HandleZoneUnderAttack(byte[] body)
        {

        }

        private void HandlePartyMemberStats(byte[] body)
        {

        }

        private void HandleGuildRank(byte[] body)
        {

        }

        private void HandleGroupSetLeader(byte[] body)
        {

        }

        private void HandleGroupInvite(byte[] body)
        {

        }

        private void HandleIgnoreList(byte[] body)
        {

        }

        private void HandleFriendList(byte[] body)
        {

        }

        private void HandleWhois(byte[] body)
        {

        }

        private void HandleCreatureQueryResponse(byte[] body)
        {

        }

        private void HandleGameObjectQueryResponse(byte[] body)
        {

        }

        private void HandleQuestQueryResponse(byte[] body)
        {

        }

        private void HandlePageTextQueryResponse(byte[] body)
        {

        }

        private void HandleItemQuerySingleResponse(byte[] body)
        {

        }

        private void HandleGuildQueryResponse(byte[] body)
        {

        }

        private void HandleNameQueryResponse(byte[] body)
        {

        }

        private void HandleCharacterLoginFailed(byte[] body)
        {

        }

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

        private static void HandleUnknownOpcode(ushort opcode, byte[] body)
        {
            // Handle the unknown opcodes by logging them
            //Console.WriteLine($"[WorldClient][HandleNetworkMessages] Unhandled opcode: {opcode:X}");
            //Console.WriteLine($"[WorldClient][HandleNetworkMessages] Body: {BitConverter.ToString(body)}");
        }

        private static byte[] GenerateClientProof(string username, uint clientSeed, byte[] serverSeed, byte[] sessionKey)
        {
            using SHA1 sha1 = SHA1.Create();
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(Encoding.UTF8.GetBytes(username)); // Ensure username is correct
            writer.Write(new byte[4]); // Write t (all zeroes)
            writer.Write(clientSeed);
            writer.Write(serverSeed);
            writer.Write(sessionKey);

            return sha1.ComputeHash(ms.ToArray());
        }

        private static byte[] GenerateAddonInfo()
        {
            string addonData =
                "Blizzard_AuctionUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_BattlefieldMinimap\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_BindingUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_CombatText\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_CraftUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_GMSurveyUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_InspectUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_MacroUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_RaidUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_TalentUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_TradeSkillUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
                "Blizzard_TrainerUI\x00\x01mw\x1cL\x00\x00\x00\x00";

            return Encoding.UTF8.GetBytes(addonData);
        }

        private static byte[] CompressAddonInfo(byte[] addonInfo)
        {
            using var outputStream = new MemoryStream();
            using (var compressionStream = new ZlibStream(outputStream, CompressionMode.Compress, CompressionLevel.Default))
            {
                compressionStream.Write(addonInfo, 0, addonInfo.Length);
            }
            return outputStream.ToArray();
        }

        private static string ReadCString(BinaryReader reader)
        {
            StringBuilder sb = new();
            char c;
            while ((c = reader.ReadChar()) != '\0')
            {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
