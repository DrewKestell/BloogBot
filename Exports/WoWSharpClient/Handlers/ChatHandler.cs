using GameData.Core.Enums;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    public class ChatHandler(WoWSharpObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = objectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = objectManager;

        public void HandleServerChatMessage(Opcode opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                ChatMsg chatType = (ChatMsg)reader.ReadByte();
                Language language = (Language)reader.ReadInt32();

                string senderName = string.Empty;
                string channelName = string.Empty;
                ulong senderGuid = 0;
                ulong targetGuid = 0;
                byte playerRank = 0;
                PlayerChatTag playerChatTag;

                switch (chatType)
                {
                    case ChatMsg.CHAT_MSG_MONSTER_WHISPER:
                    case ChatMsg.CHAT_MSG_RAID_BOSS_WHISPER:
                    case ChatMsg.CHAT_MSG_RAID_BOSS_EMOTE:
                    case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                        reader.ReadUInt32(); // senderNameLength, discard
                        senderName = ReaderUtils.ReadCString(reader);
                        targetGuid = ReaderUtils.ReadPackedGuid(reader);
                        break;

                    case ChatMsg.CHAT_MSG_SAY:
                    case ChatMsg.CHAT_MSG_PARTY:
                    case ChatMsg.CHAT_MSG_YELL:
                        senderGuid = reader.ReadUInt64();
                        reader.ReadUInt64(); // duplicate sender GUID, discard
                        break;

                    case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    case ChatMsg.CHAT_MSG_MONSTER_YELL:
                        senderGuid = reader.ReadUInt64();
                        reader.ReadUInt32(); // senderNameLength, discard
                        senderName = ReaderUtils.ReadCString(reader);
                        targetGuid = reader.ReadUInt64();
                        break;

                    case ChatMsg.CHAT_MSG_CHANNEL:
                        channelName = ReaderUtils.ReadCString(reader);
                        playerRank = (byte)reader.ReadUInt32(); // raw uint to byte
                        senderGuid = reader.ReadUInt64();
                        break;

                    default:
                        senderGuid = reader.ReadUInt64();
                        break;
                }

                // Special case handling (e.g., swap sender/receiver for battleground messages)
                if (chatType == ChatMsg.CHAT_MSG_BG_SYSTEM_ALLIANCE || chatType == ChatMsg.CHAT_MSG_BG_SYSTEM_HORDE)
                {
                    (senderGuid, targetGuid) = (targetGuid, senderGuid);
                }

                uint textLength = reader.ReadUInt32();
                string text = ReaderUtils.ReadString(reader, textLength);
                playerChatTag = (PlayerChatTag)reader.ReadByte();

                _woWSharpEventEmitter.FireOnChatMessage(
                    chatType,
                    language,
                    senderGuid,
                    targetGuid,
                    senderName,
                    channelName,
                    playerRank,
                    text,
                    playerChatTag
                );
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading chat message: {e.Message}");
            }
        }
    }
}
