using BotRunner.Constants;
using System.Text;
using WoWSharpClient.Manager;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    public class ChatHandler(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;

        public void HandleServerChatMessage(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                ChatMsg msgtype = (ChatMsg)reader.ReadByte();
                Language language = (Language)reader.ReadInt32();
                ulong senderGuid = 0;
                ulong targetGuid = 0;
                uint senderNameLength = 0;
                string senderName = string.Empty;
                string channelName = string.Empty;
                byte playerRank = 0;
                PlayerChatTag playerChatTag = PlayerChatTag.CHAT_TAG_NONE;

                switch (msgtype)
                {
                    case ChatMsg.CHAT_MSG_MONSTER_WHISPER:
                    case ChatMsg.CHAT_MSG_RAID_BOSS_WHISPER:
                    case ChatMsg.CHAT_MSG_RAID_BOSS_EMOTE:
                    case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                        senderNameLength = reader.ReadUInt32();
                        senderName = ReaderUtils.ReadCString(reader);
                        senderGuid = ReaderUtils.ReadPackedGuid(reader);
                        break;

                    case ChatMsg.CHAT_MSG_SAY:
                    case ChatMsg.CHAT_MSG_PARTY:
                    case ChatMsg.CHAT_MSG_YELL:
                        senderGuid = reader.ReadUInt64();
                        senderGuid = reader.ReadUInt64();
                        break;

                    case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    case ChatMsg.CHAT_MSG_MONSTER_YELL:
                        senderGuid = reader.ReadUInt64();
                        senderNameLength = reader.ReadUInt32();
                        senderName = ReaderUtils.ReadCString(reader); // Skip the sender name
                        targetGuid = reader.ReadUInt64();
                        break;

                    case ChatMsg.CHAT_MSG_CHANNEL:
                        channelName = ReaderUtils.ReadCString(reader); // Skip the sender name
                        playerRank = (byte)reader.ReadUInt32();
                        senderGuid = reader.ReadUInt64();
                        break;

                    default:
                        senderGuid = reader.ReadUInt64();
                        break;
                }

                uint textLength = reader.ReadUInt32();
                string text = ReaderUtils.ReadCString(reader);

                _woWSharpEventEmitter.FireOnChatMessage(msgtype, language, senderGuid, targetGuid, senderName, channelName, playerRank, text, playerChatTag);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading chat message: {e.Message}");
            }
        }        
    }
}
