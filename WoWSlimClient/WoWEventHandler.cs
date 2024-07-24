using WoWSlimClient.Handlers;
using WoWSlimClient.Models;


namespace WoWSlimClient
{
    public class WoWEventHandler
    {
        private static WoWEventHandler _instance;

        private WoWEventHandler() { }

        public static WoWEventHandler Instance => _instance ??= new WoWEventHandler();

        public event EventHandler OnLoginConnect;
        public event EventHandler OnHandshakeBegin;
        public event EventHandler OnLoginSuccess;
        public event EventHandler OnLoginFailure;
        public event EventHandler OnWorldSessionStart;
        public event EventHandler OnWorldSessionEnd;
        public event EventHandler OnCharacterListLoaded;
        public event EventHandler OnSpellLogMiss;
        public event EventHandler OnSpellGo;
        public event EventHandler OnSetRestStart;
        public event EventHandler<WorldInfo> OnLoginVerifyWorld; 
        public event EventHandler<byte> OnStandStateUpdate;
        public event EventHandler LevelUp;
        public event EventHandler<OnLootArgs> OnLoot;
        public event EventHandler InServerQueue;
        public event EventHandler OnChooseRealm;
        public event EventHandler OnDisconnect;
        public event EventHandler<List<WorldState>> OnWorldStatesInit;
        public event EventHandler<OnUiMessageArgs> OnErrorMessage;
        public event EventHandler<OnUiMessageArgs> OnUiMessage;
        public event EventHandler<OnUiMessageArgs> OnSystemMessage;
        public event EventHandler<OnUiMessageArgs> OnSkillMessage;
        public event EventHandler<EventArgs> OnBlockParryDodge;
        public event EventHandler<EventArgs> OnParry;
        public event EventHandler<EventArgs> OnSlamReady;
        public event EventHandler OnFightStart;
        public event EventHandler OnFightStop;
        public event EventHandler OnInitialSpellsLoaded;
        public event EventHandler OnUnitKilled;
        public event EventHandler<OnRequestArgs> OnPartyInvite;
        public event EventHandler OnDeath;
        public event EventHandler OnResurrect;
        public event EventHandler OnCorpseInRange;
        public event EventHandler OnCorpseOutOfRange;
        public event EventHandler OnLootOpened;
        public event EventHandler OnLootClosed;
        public event EventHandler OnGossipShow;
        public event EventHandler OnGossipClosed;
        public event EventHandler OnMerchantShow;
        public event EventHandler OnMerchantClosed;
        public event EventHandler OnTaxiShow;
        public event EventHandler OnTaxiClosed;
        public event EventHandler OnTrainerShow;
        public event EventHandler OnTrainerClosed;
        public event EventHandler<OnXpGainArgs> OnXpGain;
        public event EventHandler<AuraChangedArgs> AuraChanged;
        public event EventHandler<OnRequestArgs> OnDuelRequest;
        public event EventHandler<GuildInviteArgs> OnGuildInvite;
        public event EventHandler<ChatMessageArgs> OnChatMessage;
        public event EventHandler<OnEventArgs> OnEvent;
        public event EventHandler OnPlayerInit;
        public event EventHandler<OnCtmArgs> OnCtm;
        public event EventHandler OnTradeShow;
        public event EventHandler OnMoneyChange;
        public event EventHandler OnTargetChange;
        public event EventHandler OnQuestComplete;
        public event EventHandler OnQuestObjectiveComplete;
        public event EventHandler OnQuestFrameOpen;
        public event EventHandler OnQuestFrameClosed;
        public event EventHandler OnQuestGreetingFrameOpen;
        public event EventHandler OnQuestGreetingFrameClosed;
        public event EventHandler OnQuestFailed;
        public event EventHandler OnQuestProgress;
        public event EventHandler OnMailboxOpen;
        public event EventHandler OnMailboxClosed;
        public event EventHandler OnBankFrameOpen;
        public event EventHandler OnBankFrameClosed;
        public event EventHandler<CharCreateResponse> OnCharacterCreateResponse;
        public event EventHandler<CharDeleteResponse> OnCharacterDeleteResponse;

        private void FireEvent(EventHandler handler) => handler?.Invoke(this, EventArgs.Empty);
        private void FireEvent<T>(EventHandler<T> handler, T args) where T : EventArgs => handler?.Invoke(this, args);
        private void FireEvent(EventHandler<EventArgs> handler, EventArgs args) => handler?.Invoke(this, args);

        internal void FireOnLoginConnect() => FireEvent(OnLoginConnect);
        internal void FireOnLoginSuccess() => FireEvent(OnLoginSuccess);
        internal void FireOnHandshakeBegin() => FireEvent(OnHandshakeBegin);
        internal void FireOnLoginFailure() => FireEvent(OnLoginFailure);
        internal void FireOnChooseRealm() => FireEvent(OnChooseRealm);
        internal void FireInServerQueue() => FireEvent(InServerQueue);
        internal void FireOnWorldSessionStart() => FireEvent(OnWorldSessionStart);
        internal void FireOnCharacterListLoaded() => FireEvent(OnCharacterListLoaded);
        internal void FireOnWorldSessionEnd() => FireEvent(OnWorldSessionEnd);
        internal void FireOnLoginVerifyWorld(WorldInfo worldInfo) => OnLoginVerifyWorld?.Invoke(this, worldInfo);
        internal void FireOnStandStateUpdate(byte standState) => OnStandStateUpdate?.Invoke(this, standState);
        internal void FireOnDisconnect() => FireEvent(OnDisconnect);
        internal void FireLevelUp() => FireEvent(LevelUp);
        internal void FireOnLoot(int itemId, string itemName, int count) => FireEvent(OnLoot, new OnLootArgs(itemId, itemName, count));
        internal void FireOnErrorMessage(string message) => FireEvent(OnErrorMessage, new OnUiMessageArgs(message));
        internal void FireOnUiMessage(string message) => FireEvent(OnUiMessage, new OnUiMessageArgs(message));
        internal void FireOnSystemMessage(string message) => FireEvent(OnSystemMessage, new OnUiMessageArgs(message));
        internal void FireOnSkillMessage(string message) => FireEvent(OnSkillMessage, new OnUiMessageArgs(message));
        internal void FireOnBlockParryDodge() => FireEvent(OnBlockParryDodge, new EventArgs());
        internal void FireOnParry() => FireEvent(OnParry, new EventArgs());
        internal void FireOnSlamReady() => FireEvent(OnSlamReady, new EventArgs());
        internal void FireOnFightStart() => FireEvent(OnFightStart);
        internal void FireOnFightStop() => FireEvent(OnFightStop);
        internal void FireOnUnitKilled() => FireEvent(OnUnitKilled);
        internal void FireOnPartyInvite(string player) => FireEvent(OnPartyInvite, new OnRequestArgs(player));
        internal void FireOnDeath() => FireEvent(OnDeath);
        internal void FireOnResurrect() => FireEvent(OnResurrect);
        internal void FireOnCorpseInRange() => FireEvent(OnCorpseInRange);
        internal void FireOnCorpseOutOfRange() => FireEvent(OnCorpseOutOfRange);
        internal void FireOnLootOpened() => FireEvent(OnLootOpened);
        internal void FireOnLootClosed() => FireEvent(OnLootClosed);
        internal void FireOnGossipShow() => FireEvent(OnGossipShow);
        internal void FireOnGossipClosed() => FireEvent(OnGossipClosed);
        internal void FireOnMerchantShow() => FireEvent(OnMerchantShow);
        internal void FireOnMerchantClosed() => FireEvent(OnMerchantClosed);
        internal void FireOnTaxiShow() => FireEvent(OnTaxiShow);
        internal void FireOnTaxiClosed() => FireEvent(OnTaxiClosed);
        internal void FireOnTrainerShow() => FireEvent(OnTrainerShow);
        internal void FireOnTrainerClosed() => FireEvent(OnTrainerClosed);
        internal void FireOnXpGain(int xp) => FireEvent(OnXpGain, new OnXpGainArgs(xp));
        internal void FireAuraChanged(string affectedUnit) => FireEvent(AuraChanged, new AuraChangedArgs(affectedUnit));
        internal void FireOnDuelRequest(string player) => FireEvent(OnDuelRequest, new OnRequestArgs(player));
        internal void FireOnGuildInvite(string player, string guild) => FireEvent(OnGuildInvite, new GuildInviteArgs(player, guild));
        internal void FireOnChatMessage(ChatSenderType unitType, string chatTag, string chatChannel, string message) => FireEvent(OnChatMessage, new ChatMessageArgs(unitType, chatTag, chatChannel, message));
        internal void FireOnEvent(string eventName, object[] parameters) => FireEvent(OnEvent, new OnEventArgs(eventName, parameters));
        internal void FireOnPlayerInit() => FireEvent(OnPlayerInit);
        internal void FireOnInitialSpellsLoaded() => FireEvent(OnInitialSpellsLoaded);
        internal void FireOnCtm(Position position, int ctmType) => FireEvent(OnCtm, new OnCtmArgs(position, ctmType));
        internal void FireOnTradeShow() => FireEvent(OnTradeShow);
        internal void FireOnMoneyChange() => FireEvent(OnMoneyChange);
        internal void FireOnTargetChange() => FireEvent(OnTargetChange);
        internal void FireOnQuestComplete() => FireEvent(OnQuestComplete);
        internal void FireOnQuestObjectiveComplete() => FireEvent(OnQuestObjectiveComplete);
        internal void FireOnQuestFrameOpen() => FireEvent(OnQuestFrameOpen);
        internal void FireOnQuestFrameClosed() => FireEvent(OnQuestFrameClosed);
        internal void FireOnQuestGreetingFrameOpen() => FireEvent(OnQuestGreetingFrameOpen);
        internal void FireOnQuestGreetingFrameClosed() => FireEvent(OnQuestGreetingFrameClosed);
        internal void FireOnQuestFailed() => FireEvent(OnQuestFailed);
        internal void FireOnQuestProgress() => FireEvent(OnQuestProgress);
        internal void FireOnMailboxOpen() => FireEvent(OnMailboxOpen);
        internal void FireOnMailboxClosed() => FireEvent(OnMailboxClosed);
        internal void FireOnBankFrameOpen() => FireEvent(OnBankFrameOpen);
        internal void FireOnBankFrameClosed() => FireEvent(OnBankFrameClosed);
        internal void FireOnCharacterCreateResponse(CharCreateResponse response) => FireEvent(OnCharacterCreateResponse, response);
        internal void FireOnCharacterDeleteResponse(CharDeleteResponse response) => FireEvent(OnCharacterDeleteResponse, response);
        internal void FireOnSpellGo(uint spellId, ulong casterGUID, ulong targetGUID) => FireEvent(OnSpellGo);
        internal void FireOnSpellLogMiss(uint spellId, ulong casterGUID, ulong targetGUID, uint missReason) => FireEvent(OnSpellLogMiss);
        internal void FireOnWorldStatesInit(List<WorldState> worldStates) => OnWorldStatesInit?.Invoke(this, worldStates);

        internal void FireOnSetRestStart() => FireEvent(OnSetRestStart);

    }
    public class OnLootArgs : EventArgs
    {
        public OnLootArgs(int itemId, string itemName, int count)
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

        public override string ToString() => $"[{Time.ToShortTimeString()}] {Count}x {ItemName} ({ItemId})";
    }

    public class OnUiMessageArgs : EventArgs
    {
        public readonly string Message;

        internal OnUiMessageArgs(string message)
        {
            Message = message;
        }
    }

    public class OnXpGainArgs : EventArgs
    {
        public readonly int Xp;

        internal OnXpGainArgs(int xp)
        {
            Xp = xp;
        }
    }

    public class AuraChangedArgs : EventArgs
    {
        public readonly string AffectedUnit;

        internal AuraChangedArgs(string affectedUnit)
        {
            AffectedUnit = affectedUnit;
        }
    }

    public class OnRequestArgs : EventArgs
    {
        public readonly string Player;

        internal OnRequestArgs(string player)
        {
            Player = player;
        }
    }

    public class ChatMessageArgs : EventArgs
    {
        internal ChatMessageArgs(ChatSenderType unitType, string chatTag, string chatChannel, string message)
        {
            UnitType = unitType;
            ChatTag = chatTag;
            ChatChannel = chatChannel;
            Message = message;
            Time = DateTime.Now;
        }

        public ChatSenderType UnitType { get; }
        public string ChatTag { get; }
        public string ChatChannel { get; }
        public string Message { get; }
        public DateTime Time { get; }

        public override string ToString() => $"[{Time.ToShortTimeString()}] [{ChatTag}] [{ChatChannel}]: {Message}";
    }

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

    public class OnCtmArgs : EventArgs
    {
        public readonly int CtmType;
        public readonly Position Position;

        internal OnCtmArgs(Position position, int ctmType)
        {
            Position = position;
            CtmType = ctmType;
        }
    }

    public class OnEventArgs : EventArgs
    {
        public readonly string EventName;
        public readonly object[] Parameters;

        internal OnEventArgs(string eventName, object[] parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }
    }
    public class CharCreateResponse : EventArgs
    {
        public CharCreateResponse(CreateCharacterResult result)
        {
            Result = result;
        }

        public CreateCharacterResult Result { get; }
    }

    public class CharDeleteResponse : EventArgs
    {
        public CharDeleteResponse(DeleteCharacterResult result)
        {
            Result = result;
        }

        public DeleteCharacterResult Result { get; }
    }
}
