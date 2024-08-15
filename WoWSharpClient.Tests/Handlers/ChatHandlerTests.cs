using BotRunner.Constants;
using Communication;
using WoWSharpClient.Handlers;
using WoWSharpClient.Manager;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    public class ChatHandlerTests
    {
        private readonly ChatHandler _chatHandler;
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter;
        private readonly ObjectManager _objectManager;
        private readonly ActivityMemberState _activityMember;

        public ChatHandlerTests()
        {
            _woWSharpEventEmitter = new WoWSharpEventEmitter();
            _activityMember = new ActivityMemberState();
            _objectManager = new ObjectManager(_woWSharpEventEmitter, _activityMember);

            _chatHandler = new ChatHandler(_woWSharpEventEmitter, _objectManager);
        }

        [Fact]
        public void ShouldProcessPatchMessage()
        {
            // Arrange
            var opcode = Opcodes.SMSG_MESSAGECHAT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\PatchMessage.bin");

            bool eventFired = false;
            ChatMsg msgtype = default;
            Language language = default;
            ulong senderGuid = 0;
            ulong targetGuid = 0;
            string channelName = string.Empty;
            byte playerRank = 0;
            string text = string.Empty;
            PlayerChatTag playerChatTag = PlayerChatTag.CHAT_TAG_NONE;

            _woWSharpEventEmitter.OnChatMessage += (objc, args) =>
            {
                msgtype = args.MsgType;
                language = args.Language;
                senderGuid = args.SenderGuid;    
                targetGuid = args.TargetGuid;
                channelName = args.ChannelName;
                playerRank = 0;
                text = args.Text;
                playerChatTag = args.PlayerChatTag;
                eventFired = true;
            };

            // Act
            _chatHandler.HandleServerChatMessage(opcode, data);

            // Assert
            Assert.True(eventFired, "The chat message event was not fired.");
            Assert.Equal(Language.Universal, language);
            Assert.Equal(ChatMsg.CHAT_MSG_SYSTEM, msgtype);
            Assert.Equal((ulong)0, senderGuid);
            Assert.Equal((ulong)0, targetGuid);
            Assert.Equal(string.Empty, channelName);
            Assert.Equal(0, playerRank);
            Assert.Equal("Patch 1.12: Drums of War is now live!", text);
            Assert.Equal(PlayerChatTag.CHAT_TAG_NONE, playerChatTag);
        }

        [Fact]
        public void ShouldProcessServerWelcomeMessage()
        {
            // Arrange
            var opcode = Opcodes.SMSG_MESSAGECHAT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\ServerWelcome.bin");

            bool eventFired = false;
            ChatMsg msgtype = default;
            Language language = default;
            ulong senderGuid = 0;
            ulong targetGuid = 0;
            string channelName = string.Empty;
            byte playerRank = 0;
            string text = string.Empty;
            PlayerChatTag playerChatTag = PlayerChatTag.CHAT_TAG_NONE;

            _woWSharpEventEmitter.OnChatMessage += (objc, args) =>
            {
                msgtype = args.MsgType;
                language = args.Language;
                senderGuid = args.SenderGuid;
                targetGuid = args.TargetGuid;
                channelName = args.ChannelName;
                playerRank = 0;
                text = args.Text;
                playerChatTag = args.PlayerChatTag;
                eventFired = true;
            };

            // Act
            _chatHandler.HandleServerChatMessage(opcode, data);

            // Assert
            Assert.True(eventFired, "The chat message event was not fired.");
            Assert.Equal(Language.Universal, language);
            Assert.Equal(ChatMsg.CHAT_MSG_SYSTEM, msgtype);
            Assert.Equal((ulong)0, senderGuid);
            Assert.Equal((ulong)0, targetGuid);
            Assert.Equal(string.Empty, channelName);
            Assert.Equal(0, playerRank);
            Assert.Equal("Welcome to Light's Hope!", text);
            Assert.Equal(PlayerChatTag.CHAT_TAG_NONE, playerChatTag);
        }

        [Fact]
        public void ShouldProcessCharacterSay()
        {
            // Arrange
            var opcode = Opcodes.SMSG_MESSAGECHAT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\Say.bin");

            bool eventFired = false;
            ChatMsg msgtype = default;
            Language language = default;
            ulong senderGuid = 0;
            ulong targetGuid = 0;
            string channelName = string.Empty;
            byte playerRank = 0;
            string text = string.Empty;
            PlayerChatTag playerChatTag = PlayerChatTag.CHAT_TAG_NONE;

            _woWSharpEventEmitter.OnChatMessage += (objc, args) =>
            {
                msgtype = args.MsgType;
                language = args.Language;
                senderGuid = args.SenderGuid;
                targetGuid = args.TargetGuid;
                channelName = args.ChannelName;
                playerRank = 0;
                text = args.Text;
                playerChatTag = args.PlayerChatTag;
                eventFired = true;
            };

            // Act
            _chatHandler.HandleServerChatMessage(opcode, data);

            // Assert
            Assert.True(eventFired, "The chat message event was not fired.");
            Assert.Equal(Language.Orcish, language);
            Assert.Equal(ChatMsg.CHAT_MSG_SAY, msgtype);
            Assert.Equal((ulong)147, senderGuid);
            Assert.Equal((ulong)0, targetGuid);
            Assert.Equal(string.Empty, channelName);
            Assert.Equal(0, playerRank);
            Assert.Equal("Hey", text);
            Assert.Equal(PlayerChatTag.CHAT_TAG_NONE, playerChatTag);
        }

        [Fact]
        public void ShouldProcessCharacterWhisper()
        {
            // Arrange
            var opcode = Opcodes.SMSG_MESSAGECHAT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\Whisper.bin");

            bool eventFired = false;
            ChatMsg msgtype = default;
            Language language = default;
            ulong senderGuid = 0;
            ulong targetGuid = 0;
            string channelName = string.Empty;
            byte playerRank = 0;
            string text = string.Empty;
            PlayerChatTag playerChatTag = PlayerChatTag.CHAT_TAG_NONE;

            _woWSharpEventEmitter.OnChatMessage += (objc, args) =>
            {
                msgtype = args.MsgType;
                language = args.Language;
                senderGuid = args.SenderGuid;
                targetGuid = args.TargetGuid;
                channelName = args.ChannelName;
                playerRank = 0;
                text = args.Text;
                playerChatTag = args.PlayerChatTag;
                eventFired = true;
            };

            // Act
            _chatHandler.HandleServerChatMessage(opcode, data);

            // Assert
            Assert.True(eventFired, "The chat message event was not fired.");
            Assert.Equal(Language.Universal, language);
            Assert.Equal(ChatMsg.CHAT_MSG_WHISPER, msgtype);
            Assert.Equal((ulong)147, senderGuid);
            Assert.Equal((ulong)0, targetGuid);
            Assert.Equal(string.Empty, channelName);
            Assert.Equal(0, playerRank);
            Assert.Equal("Hey", text);
            Assert.Equal(PlayerChatTag.CHAT_TAG_NONE, playerChatTag);
        }

        [Fact]
        public void ShouldProcessCharacterYell()
        {
            // Arrange
            var opcode = Opcodes.SMSG_MESSAGECHAT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\Yell.bin");

            bool eventFired = false;
            ChatMsg msgtype = default;
            Language language = default;
            ulong senderGuid = 0;
            ulong targetGuid = 0;
            string channelName = string.Empty;
            byte playerRank = 0;
            string text = string.Empty;
            PlayerChatTag playerChatTag = PlayerChatTag.CHAT_TAG_NONE;

            _woWSharpEventEmitter.OnChatMessage += (objc, args) =>
            {
                msgtype = args.MsgType;
                language = args.Language;
                senderGuid = args.SenderGuid;
                targetGuid = args.TargetGuid;
                channelName = args.ChannelName;
                playerRank = 0;
                text = args.Text;
                playerChatTag = args.PlayerChatTag;
                eventFired = true;
            };

            // Act
            _chatHandler.HandleServerChatMessage(opcode, data);

            // Assert
            Assert.True(eventFired, "The chat message event was not fired.");
            Assert.Equal(Language.Orcish, language);
            Assert.Equal(ChatMsg.CHAT_MSG_YELL, msgtype);
            Assert.Equal((ulong)147, senderGuid);
            Assert.Equal((ulong)0, targetGuid);
            Assert.Equal(string.Empty, channelName);
            Assert.Equal(0, playerRank);
            Assert.Equal("HEY!", text);
            Assert.Equal(PlayerChatTag.CHAT_TAG_NONE, playerChatTag);
        }
    }
}
