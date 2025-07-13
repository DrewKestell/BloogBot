using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using System.Reflection;

namespace WoWSharpClient
{
    public class WoWSharpEventEmitter : IWoWEventHandler
    {
        private static WoWSharpEventEmitter _instance;

        public static WoWSharpEventEmitter Instance
        {
            get
            {
                _instance ??= new WoWSharpEventEmitter();

                return _instance;
            }
        }
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
        public event EventHandler OnClientControlUpdate;
        public event EventHandler<WorldInfo> OnLoginVerifyWorld;
        public EventHandler<OnSetTimeSpeedArgs> OnSetTimeSpeed;
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
        public event EventHandler<CharacterActionArgs> OnCharacterJumpStart;
        public event EventHandler<CharacterActionArgs> OnCharacterFallLand;
        public event EventHandler<CharacterActionArgs> OnCharacterStartForward;
        public event EventHandler<CharacterActionArgs> OnCharacterMoveStop;
        public event EventHandler<CharacterActionArgs> OnCharacterStartStrafeLeft;
        public event EventHandler<CharacterActionArgs> OnCharacterStartStrafeRight;
        public event EventHandler<CharacterActionArgs> OnCharacterStopStrafe;
        public event EventHandler<CharacterActionArgs> OnCharacterStartTurnLeft;
        public event EventHandler<CharacterActionArgs> OnCharacterStartTurnRight;
        public event EventHandler<CharacterActionArgs> OnCharacterStopTurn;
        public event EventHandler<CharacterActionArgs> OnCharacterSetFacing;
        public event EventHandler<CharacterActionArgs> OnCharacterStartBackwards;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceMoveRoot;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceMoveUnroot;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceRunSpeedChange;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceRunBackSpeedChange;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceSwimSpeedChange;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceMoveKnockBack;
        public event EventHandler<RequiresAcknowledgementArgs> OnForceTimeSkipped;
        public event EventHandler<RequiresAcknowledgementArgs> OnTeleport;
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
        public event EventHandler<GameObjectCreatedArgs> OnGameObjectCreated;

        private WoWSharpEventEmitter() { }
        /// <summary>
        /// Clear every handler from every event/delegate on the singleton.
        /// Call this from your test fixture’s Dispose().
        /// </summary>
        public void Reset()
        {
            lock (this)                    // cheap, coarse-grained; good enough for tests
            {
                foreach (var field in GetType().GetFields(
                             BindingFlags.Instance |
                             BindingFlags.Public |   // catches OnSetTimeSpeed, a public delegate
                             BindingFlags.NonPublic))  // catches the private backing fields for events
                {
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        field.SetValue(this, null);   // one-line wipe-out
                    }
                }
            }
        }
        private void FireEvent(EventHandler handler) => handler?.Invoke(this, EventArgs.Empty);
        private void FireEvent<T>(EventHandler<T> handler, T args) where T : EventArgs => handler?.Invoke(this, args);
        private void FireEvent(EventHandler<EventArgs> handler, EventArgs args) => handler?.Invoke(this, args);

        public void FireOnLoginConnect() => FireEvent(OnLoginConnect);
        public void FireOnLoginSuccess() => FireEvent(OnLoginSuccess);
        public void FireOnHandshakeBegin() => FireEvent(OnHandshakeBegin);
        public void FireOnLoginFailure() => FireEvent(OnLoginFailure);
        public void FireOnChooseRealm() => FireEvent(OnChooseRealm);
        public void FireInServerQueue() => FireEvent(InServerQueue);
        public void FireOnWorldSessionStart() => FireEvent(OnWorldSessionStart);
        public void FireOnCharacterListLoaded() => FireEvent(OnCharacterListLoaded);
        public void FireOnWorldSessionEnd() => FireEvent(OnWorldSessionEnd);
        public void FireOnGameObjectCreated(GameObjectCreatedArgs args) => FireEvent(OnGameObjectCreated, args);
        public void FireOnLoginVerifyWorld(WorldInfo worldInfo) => OnLoginVerifyWorld?.Invoke(this, worldInfo);
        public void FireOnStandStateUpdate(byte standState) => OnStandStateUpdate?.Invoke(this, standState);
        public void FireOnDisconnect() => FireEvent(OnDisconnect);
        public void FireLevelUp() => FireEvent(LevelUp);
        public void FireOnLoot(int itemId, string itemName, int count) => FireEvent(OnLoot, new OnLootArgs(itemId, itemName, count));
        public void FireOnErrorMessage(string message) => FireEvent(OnErrorMessage, new OnUiMessageArgs(message));
        public void FireOnUiMessage(string message) => FireEvent(OnUiMessage, new OnUiMessageArgs(message));
        public void FireOnSystemMessage(string message) => FireEvent(OnSystemMessage, new OnUiMessageArgs(message));
        public void FireOnSkillMessage(string message) => FireEvent(OnSkillMessage, new OnUiMessageArgs(message));
        public void FireOnBlockParryDodge() => FireEvent(OnBlockParryDodge, new EventArgs());
        public void FireOnParry() => FireEvent(OnParry, new EventArgs());
        public void FireOnSlamReady() => FireEvent(OnSlamReady, new EventArgs());
        public void FireOnFightStart() => FireEvent(OnFightStart);
        public void FireOnFightStop() => FireEvent(OnFightStop);
        public void FireOnUnitKilled() => FireEvent(OnUnitKilled);
        public void FireOnPartyInvite(string player) => FireEvent(OnPartyInvite, new OnRequestArgs(player));
        public void FireOnDeath() => FireEvent(OnDeath);
        public void FireOnResurrect() => FireEvent(OnResurrect);
        public void FireOnCorpseInRange() => FireEvent(OnCorpseInRange);
        public void FireOnCorpseOutOfRange() => FireEvent(OnCorpseOutOfRange);
        public void FireOnLootOpened() => FireEvent(OnLootOpened);
        public void FireOnLootClosed() => FireEvent(OnLootClosed);
        public void FireOnGossipShow() => FireEvent(OnGossipShow);
        public void FireOnGossipClosed() => FireEvent(OnGossipClosed);
        public void FireOnMerchantShow() => FireEvent(OnMerchantShow);
        public void FireOnMerchantClosed() => FireEvent(OnMerchantClosed);
        public void FireOnTaxiShow() => FireEvent(OnTaxiShow);
        public void FireOnTaxiClosed() => FireEvent(OnTaxiClosed);
        public void FireOnTrainerShow() => FireEvent(OnTrainerShow);
        public void FireOnTrainerClosed() => FireEvent(OnTrainerClosed);
        public void FireOnXpGain(int xp) => FireEvent(OnXpGain, new OnXpGainArgs(xp));
        public void FireAuraChanged(string affectedUnit) => FireEvent(AuraChanged, new AuraChangedArgs(affectedUnit));
        public void FireOnDuelRequest(string player) => FireEvent(OnDuelRequest, new OnRequestArgs(player));
        public void FireOnGuildInvite(string player, string guild) => FireEvent(OnGuildInvite, new GuildInviteArgs(player, guild));
        public void FireOnChatMessage(ChatMsg msgtype, Language language, ulong senderGuid, ulong targetGuid, string senderName, string channelName, byte playerRank, string text, PlayerChatTag playerChatTag) => FireEvent(OnChatMessage, new ChatMessageArgs(msgtype, language, senderGuid, targetGuid, senderName, channelName, playerRank, text, playerChatTag));
        public void FireOnEvent(string eventName, object[] parameters) => FireEvent(OnEvent, new OnEventArgs(eventName, parameters));
        public void FireOnPlayerInit() => FireEvent(OnPlayerInit);
        public void FireOnInitialSpellsLoaded() => FireEvent(OnInitialSpellsLoaded);
        public void FireOnCtm(Position position, int ctmType) => FireEvent(OnCtm, new OnCtmArgs(position, ctmType));
        public void FireOnTradeShow() => FireEvent(OnTradeShow);
        public void FireOnMoneyChange() => FireEvent(OnMoneyChange);
        public void FireOnTargetChange() => FireEvent(OnTargetChange);
        public void FireOnQuestComplete() => FireEvent(OnQuestComplete);
        public void FireOnQuestObjectiveComplete() => FireEvent(OnQuestObjectiveComplete);
        public void FireOnQuestFrameOpen() => FireEvent(OnQuestFrameOpen);
        public void FireOnQuestFrameClosed() => FireEvent(OnQuestFrameClosed);
        public void FireOnQuestGreetingFrameOpen() => FireEvent(OnQuestGreetingFrameOpen);
        public void FireOnQuestGreetingFrameClosed() => FireEvent(OnQuestGreetingFrameClosed);
        public void FireOnQuestFailed() => FireEvent(OnQuestFailed);
        public void FireOnQuestProgress() => FireEvent(OnQuestProgress);
        public void FireOnMailboxOpen() => FireEvent(OnMailboxOpen);
        public void FireOnMailboxClosed() => FireEvent(OnMailboxClosed);
        public void FireOnBankFrameOpen() => FireEvent(OnBankFrameOpen);
        public void FireOnBankFrameClosed() => FireEvent(OnBankFrameClosed);
        public void FireOnCharacterCreateResponse(CharCreateResponse response) => FireEvent(OnCharacterCreateResponse, response);
        public void FireOnCharacterDeleteResponse(CharDeleteResponse response) => FireEvent(OnCharacterDeleteResponse, response);
        public void FireOnSpellGo(uint spellId, ulong casterGUID, ulong targetGUID) => FireEvent(OnSpellGo);
        public void FireOnSpellLogMiss(uint spellId, ulong casterGUID, ulong targetGUID, uint missReason) => FireEvent(OnSpellLogMiss);
        public void FireOnWorldStatesInit(List<WorldState> worldStates) => OnWorldStatesInit?.Invoke(this, worldStates);
        public void FireOnSetRestStart() => FireEvent(OnSetRestStart);
        public void FireOnCharacterJumpStart(ulong guid) => OnCharacterJumpStart?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterFallLand(ulong guid) => OnCharacterFallLand?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStartForward(ulong guid) => OnCharacterStartForward?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterMoveStop(ulong guid) => OnCharacterMoveStop?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStartStrafeLeft(ulong guid) => OnCharacterStartStrafeLeft?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStartStrafeRight(ulong guid) => OnCharacterStartStrafeRight?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStopStrafe(ulong guid) => OnCharacterStopStrafe?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStartTurnLeft(ulong guid) => OnCharacterStartTurnLeft?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStartTurnRight(ulong guid) => OnCharacterStartTurnRight?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStopTurn(ulong guid) => OnCharacterStopTurn?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterSetFacing(ulong guid) => OnCharacterSetFacing?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnCharacterStartBackwards(ulong guid) => OnCharacterStartBackwards?.Invoke(this, new CharacterActionArgs(guid));
        internal void FireOnForceMoveRoot(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceMoveRoot?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnForceMoveUnroot(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceMoveUnroot?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnForceRunSpeedChange(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceRunSpeedChange?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnForceRunBackSpeedChange(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceRunBackSpeedChange?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnForceSwimSpeedChange(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceSwimSpeedChange?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnForceMoveKnockBack(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceMoveKnockBack?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnMoveTimeSkipped(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnForceTimeSkipped?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnTeleport(RequiresAcknowledgementArgs requiresAcknowledgementArgs) => OnTeleport?.Invoke(this, requiresAcknowledgementArgs);
        internal void FireOnClientControlUpdate() => FireEvent(OnClientControlUpdate);
        internal void FireOnSetTimeSpeed(OnSetTimeSpeedArgs onSetTimeSpeedArgs) => OnSetTimeSpeed?.Invoke(this, onSetTimeSpeedArgs);
    }
}
