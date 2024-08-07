using BotRunner.Constants;
using System.Text;

namespace WoWSharpClient.Handlers
{
    public static class ChatHandler
    {
        public static void HandleServerChatMessage(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                //Console.WriteLine($"[HandleServerChatMessage][OpCode:{opcode}][Data:{BitConverter.ToString(data)}]");
                ChatMessageType messageType = (ChatMessageType)reader.ReadByte();
                Language language = (Language)reader.ReadInt32();
                ulong senderGUID = reader.ReadUInt64();

                if (messageType == ChatMessageType.Say)
                {
                    ulong anotherGUID = reader.ReadUInt64();
                }
                byte[] metadata = reader.ReadBytes(4);

                string text = string.Empty;

                text = ReadCString(reader);
                ChatSenderType senderType = MapMessageTypeToSenderType(messageType);

#if DEBUG
                LogChatMessage(senderType, messageType, senderGUID, text);
#endif

                WoWSharpEventEmitter.Instance.FireOnChatMessage(senderType, metadata.ToString(), senderGUID.ToString(), messageType.ToString(), text);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading chat message: {e.Message}");
            }
        }

        private static ChatSenderType MapMessageTypeToSenderType(ChatMessageType messageType)
        {
            return messageType switch
            {
                ChatMessageType.Say => ChatSenderType.Player,
                ChatMessageType.Yell => ChatSenderType.Player,
                ChatMessageType.Party => ChatSenderType.Player,
                ChatMessageType.Guild => ChatSenderType.Player,
                ChatMessageType.Officer => ChatSenderType.Player,
                ChatMessageType.Whisper => ChatSenderType.Player,
                ChatMessageType.Channel => ChatSenderType.Player,
                ChatMessageType.Raid => ChatSenderType.Player,
                ChatMessageType.Emote => ChatSenderType.Player,
                ChatMessageType.System => ChatSenderType.Npc,
                ChatMessageType.MonsterSay => ChatSenderType.Npc,
                ChatMessageType.MonsterYell => ChatSenderType.Npc,
                ChatMessageType.MonsterEmote => ChatSenderType.Npc,
                _ => ChatSenderType.Player
            };
        }

        private static string ReadCString(BinaryReader reader)
        {
            List<byte> bytes = [];
            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                bytes.Add(b);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

#if DEBUG
        private static void LogChatMessage(ChatSenderType senderType, ChatMessageType messageType, ulong senderGuid, string text)
        {
            StringBuilder sb = new();
            switch (messageType)
            {
                case ChatMessageType.Say:
                case ChatMessageType.MonsterSay:
                    Console.ForegroundColor = ConsoleColor.White;
                    sb.Append($"[{senderGuid}] says: ");
                    break;
                case ChatMessageType.BattlegroundLeader:
                case ChatMessageType.Battleground:
                case ChatMessageType.Party:
                case ChatMessageType.MonsterParty:
                    if (messageType == ChatMessageType.BattlegroundLeader)
                        sb.Append($"[Battleground Leader]");
                    else if (messageType == ChatMessageType.Battleground)
                        sb.Append($"[Battleground]");
                    else if (messageType == ChatMessageType.Party)
                        sb.Append($"[Party]");
                    else if (messageType == ChatMessageType.MonsterParty)
                        sb.Append($"[Monster Party]");
                    sb.Append($"[{senderGuid}]: ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case ChatMessageType.RaidLeader:
                case ChatMessageType.Raid:
                case ChatMessageType.TextEmote:
                case ChatMessageType.MonsterEmote:
                    if (messageType == ChatMessageType.RaidLeader)
                        sb.Append("[Raid Leader][{senderGuid}]: ");
                    else if (messageType == ChatMessageType.Battleground)
                        sb.Append("[Battleground][{senderGuid}]: ");
                    else if (messageType == ChatMessageType.MonsterEmote || messageType == ChatMessageType.TextEmote)
                        sb.Append($"{senderGuid} ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case ChatMessageType.Guild:
                    sb.Append("$[Guild][{senderGuid}]: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case ChatMessageType.Officer:
                    sb.Append("$[Guild][Officer][{senderGuid}]: ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case ChatMessageType.Yell:
                case ChatMessageType.MonsterYell:
                case ChatMessageType.RaidWarning:
                    if (messageType == ChatMessageType.MonsterYell || messageType == ChatMessageType.Yell)
                        sb.Append($"{senderGuid} yells: ");
                    else if (messageType == ChatMessageType.RaidWarning)
                        sb.Append($"[Raid Warning]");
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ChatMessageType.Whisper:
                case ChatMessageType.WhisperInform:
                case ChatMessageType.WhisperInformForeign:
                case ChatMessageType.Reply:
                    if (messageType == ChatMessageType.Whisper)
                        sb.Append($"[{senderGuid}]: ");
                    else if (messageType == ChatMessageType.WhisperInform)
                        sb.Append($"[{senderGuid}]: ");
                    else if (messageType == ChatMessageType.WhisperInformForeign)
                        sb.Append($"[{senderGuid}]: ");
                    else if (messageType == ChatMessageType.Reply)
                        sb.Append($"To [{senderGuid}]: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case ChatMessageType.System:
                case ChatMessageType.Loot:
                case ChatMessageType.Afk:
                case ChatMessageType.Dnd:
                    if (messageType == ChatMessageType.Loot)
                        sb.Append($"{senderGuid} ");
                    else if (messageType == ChatMessageType.Afk)
                        sb.Append($"[AFK][{senderGuid}] ");
                    else if (messageType == ChatMessageType.Dnd)
                        sb.Append($"[DND][{senderGuid}] ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ChatMessageType.Channel:
                case ChatMessageType.ChannelJoin:
                case ChatMessageType.ChannelLeave:
                case ChatMessageType.ChannelList:
                case ChatMessageType.ChannelNotice:
                case ChatMessageType.ChannelNoticeUser:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case ChatMessageType.Ignored:
                case ChatMessageType.Skill:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    break;
            }
            sb.Append(text);
            Console.WriteLine(sb.ToString());
        }
#endif
    }
}
