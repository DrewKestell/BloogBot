using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWEventHandler
    {
        event EventHandler OnLoginConnect;
        event EventHandler OnHandshakeBegin;
        event EventHandler OnLoginSuccess;
        event EventHandler OnLoginFailure;
        event EventHandler OnWorldSessionStart;
        event EventHandler OnWorldSessionEnd;
        event EventHandler OnCharacterListLoaded;
        event EventHandler OnSpellLogMiss;
        event EventHandler OnSpellGo;
        event EventHandler OnSetRestStart;
        event EventHandler<WorldInfo> OnLoginVerifyWorld;
        event EventHandler<byte> OnStandStateUpdate;
        event EventHandler LevelUp;
        event EventHandler<OnLootArgs> OnLoot;
        event EventHandler InServerQueue;
        event EventHandler OnChooseRealm;
        event EventHandler OnDisconnect;
        event EventHandler<List<WorldState>> OnWorldStatesInit;
        event EventHandler<OnUiMessageArgs> OnErrorMessage;
        event EventHandler<OnUiMessageArgs> OnUiMessage;
        event EventHandler<OnUiMessageArgs> OnSystemMessage;
        event EventHandler<OnUiMessageArgs> OnSkillMessage;
        event EventHandler<EventArgs> OnBlockParryDodge;
        event EventHandler<EventArgs> OnParry;
        event EventHandler<EventArgs> OnSlamReady;
        event EventHandler OnFightStart;
        event EventHandler OnFightStop;
        event EventHandler OnInitialSpellsLoaded;
        event EventHandler OnUnitKilled;
        event EventHandler<OnRequestArgs> OnPartyInvite;
        event EventHandler OnDeath;
        event EventHandler OnResurrect;
        event EventHandler OnCorpseInRange;
        event EventHandler OnCorpseOutOfRange;
        event EventHandler OnLootOpened;
        event EventHandler OnLootClosed;
        event EventHandler OnGossipShow;
        event EventHandler OnGossipClosed;
        event EventHandler OnMerchantShow;
        event EventHandler OnMerchantClosed;
        event EventHandler OnTaxiShow;
        event EventHandler OnTaxiClosed;
        event EventHandler OnTrainerShow;
        event EventHandler OnTrainerClosed;
        event EventHandler<OnXpGainArgs> OnXpGain;
        event EventHandler<AuraChangedArgs> AuraChanged;
        event EventHandler<OnRequestArgs> OnDuelRequest;
        event EventHandler<GuildInviteArgs> OnGuildInvite;
        event EventHandler<ChatMessageArgs> OnChatMessage;
        event EventHandler<OnEventArgs> OnEvent;
        event EventHandler OnPlayerInit;
        event EventHandler<OnCtmArgs> OnCtm;
        event EventHandler OnTradeShow;
        event EventHandler OnMoneyChange;
        event EventHandler OnTargetChange;
        event EventHandler OnQuestComplete;
        event EventHandler OnQuestObjectiveComplete;
        event EventHandler OnQuestFrameOpen;
        event EventHandler OnQuestFrameClosed;
        event EventHandler OnQuestGreetingFrameOpen;
        event EventHandler OnQuestGreetingFrameClosed;
        event EventHandler OnQuestFailed;
        event EventHandler OnQuestProgress;
        event EventHandler OnMailboxOpen;
        event EventHandler OnMailboxClosed;
        event EventHandler OnBankFrameOpen;
        event EventHandler OnBankFrameClosed;
        event EventHandler<CharCreateResponse> OnCharacterCreateResponse;
        event EventHandler<CharDeleteResponse> OnCharacterDeleteResponse;
        event EventHandler<GameObjectCreatedArgs> OnGameObjectCreated;
    }
    public class WorldInfo
    {
        public uint MapId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float Facing { get; set; }
    }
    public class WorldState
    {
        public uint StateId { get; set; }
        public uint StateValue { get; set; }
    }
    public class OnLootArgs(int itemId, string itemName, int count) : EventArgs
    {
        public int ItemId { get; } = itemId;
        public string ItemName { get; } = itemName;
        public int Count { get; } = count;
        public DateTime Time { get; } = DateTime.Now;

        public override string ToString() => $"[{Time.ToShortTimeString()}] {Count}x {ItemName} ({ItemId})";
    }

    public class OnUiMessageArgs(string message) : EventArgs
    {
        public readonly string Message = message;
    }

    public class OnXpGainArgs(int xp) : EventArgs
    {
        public readonly int Xp = xp;
    }

    public class AuraChangedArgs(string affectedUnit) : EventArgs
    {
        public readonly string AffectedUnit = affectedUnit;
    }

    public class OnRequestArgs(string player) : EventArgs
    {
        public readonly string Player = player;
    }

    public class ChatMessageArgs(ChatMsg msgtype, Language language, ulong senderGuid, ulong targetGuid, string senderName, string channelName, byte playerRank, string text, PlayerChatTag playerChatTag) : EventArgs
    {
        public readonly ChatMsg MsgType = msgtype;
        public readonly Language Language = language;
        public readonly ulong SenderGuid = senderGuid;
        public readonly ulong TargetGuid = targetGuid;
        public readonly string SenderName = senderName;
        public readonly string ChannelName = channelName;
        public readonly byte PlayerRank = playerRank;
        public readonly string Text = text;
        public readonly PlayerChatTag PlayerChatTag = playerChatTag;
        public DateTime Time { get; } = DateTime.Now;

        public override string ToString() => $"[{Time.ToShortTimeString()}]";
    }

    public class GuildInviteArgs(string player, string guild) : EventArgs
    {
        internal readonly string Guild = guild;
        internal readonly string Player = player;
    }

    public class OnCtmArgs(Position position, int ctmType) : EventArgs
    {
        public readonly int CtmType = ctmType;
        public readonly Position Position = position;
    }

    public class OnEventArgs(string eventName, object[] parameters) : EventArgs
    {
        public readonly string EventName = eventName;
        public readonly object[] Parameters = parameters;
    }
    public class CharCreateResponse(CreateCharacterResult result) : EventArgs
    {
        public CreateCharacterResult Result { get; } = result;
    }

    public class CharDeleteResponse(DeleteCharacterResult result) : EventArgs
    {
        public DeleteCharacterResult Result { get; } = result;
    }
    public class GameObjectCreatedArgs(ulong guid, WoWObjectType objectType) : EventArgs
    {
        public readonly ulong Guid = guid;
        public readonly WoWObjectType ObjectType = objectType;
    }
    public class CharacterActionArgs(ulong guid)
    {
        public readonly ulong Guid = guid;
    }
}
