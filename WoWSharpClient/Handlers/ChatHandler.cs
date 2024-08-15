using BotRunner.Constants;
using System.Text;
using WoWSharpClient.Manager;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Handlers
{
    public class ChatHandler
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;
        private readonly ObjectManager _objectManager;

        public ChatHandler(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
        {
            _woWSharpEventEmitter = woWSharpEventEmitter;
            _objectManager = objectManager;
        }

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
                        senderName = ReaderUtils.ReadCString(reader); // Skip the sender name
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

                LogChatMessage(msgtype, senderGuid, text);

                _woWSharpEventEmitter.FireOnChatMessage(msgtype, language, senderGuid, targetGuid, senderName, channelName, playerRank, text, playerChatTag);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading chat message: {e.Message}");
            }
        }

        private void LogChatMessage(ChatMsg messageType, ulong senderGuid, string text)
        {
            StringBuilder sb = new();
            switch (messageType)
            {
                case ChatMsg.CHAT_MSG_SAY:
                case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    sb.Append($"[{senderGuid}] says: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case ChatMsg.CHAT_MSG_YELL:
                case ChatMsg.CHAT_MSG_MONSTER_YELL:
                    sb.Append($"[{senderGuid}] yells: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case ChatMsg.CHAT_MSG_WHISPER:
                case ChatMsg.CHAT_MSG_WHISPER_INFORM:
                case ChatMsg.CHAT_MSG_MONSTER_WHISPER:
                    sb.Append($"[{senderGuid}] whispers: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;

                case ChatMsg.CHAT_MSG_EMOTE:
                case ChatMsg.CHAT_MSG_TEXT_EMOTE:
                case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                case ChatMsg.CHAT_MSG_RAID_BOSS_EMOTE:
                    sb.Append($"[{senderGuid}] emotes: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case ChatMsg.CHAT_MSG_SYSTEM:
                case ChatMsg.CHAT_MSG_CHANNEL_NOTICE:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    sb.Append($"[System]: ");
                    break;

                case ChatMsg.CHAT_MSG_PARTY:
                case ChatMsg.CHAT_MSG_RAID:
                case ChatMsg.CHAT_MSG_GUILD:
                case ChatMsg.CHAT_MSG_OFFICER:
                    sb.Append($"[{senderGuid}]: ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;

                case ChatMsg.CHAT_MSG_CHANNEL:
                    sb.Append($"[Channel]: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case ChatMsg.CHAT_MSG_RAID_WARNING:
                    sb.Append($"[Raid Warning]: ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;

                case ChatMsg.CHAT_MSG_LOOT:
                    sb.Append($"[Loot]: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                default:
                    sb.Append($"[{senderGuid}]: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            sb.Append(text);
            Console.ResetColor();
            Console.WriteLine(sb.ToString());
        }
    }
}
