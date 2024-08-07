using System.Text.RegularExpressions;
using Newtonsoft.Json;
using BotRunner.Constants;
using BotRunner.Interfaces;
using ActivityForegroundMember.Mem.Hooks;

namespace ActivityForegroundMember.Game.Statics
{
    /// <summary>
    ///     Contains WoW Events to subscribe to
    /// </summary>
    public sealed class WoWEventHandler : IWoWEventHandler
    {
        private readonly Task thrLootUpdate;
        private readonly Task thrMerchUpdate;

        private WoWEventHandler() => SignalEventManager.OnNewSignalEvent += EvaluateEvent;

        /// <summary>
        ///     Access to the WoWEventHandler
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static WoWEventHandler Instance { get; } = new WoWEventHandler();


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

        internal void FireOnPlayerInit()
        {
            Task.Run(() => OnPlayerInit?.Invoke(this, new EventArgs()));
        }


        /// <summary>
        ///     Occurs on a click to move action
        /// </summary>
        public event EventHandler<OnCtmArgs> OnCtm;

        internal void TriggerCtmEvent(OnCtmArgs args)
        {
#if DEBUG
            Console.WriteLine(args.Position);
#endif
            Task.Run(() => OnCtm?.Invoke(this, args));
        }

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
        public event EventHandler OnLoginConnect;
        public event EventHandler OnLoginSuccess;
        public event EventHandler OnLoginFailure;
        public event EventHandler OnWorldSessionStart;
        public event EventHandler OnWorldSessionEnd;
        public event EventHandler OnSpellLogMiss;
        public event EventHandler OnSpellGo;
        public event EventHandler OnSetRestStart;
        public event EventHandler<WorldInfo> OnLoginVerifyWorld;
        public event EventHandler<byte> OnStandStateUpdate;
        public event EventHandler<List<WorldState>> OnWorldStatesInit;
        public event EventHandler OnInitialSpellsLoaded;
        public event EventHandler<CharCreateResponse> OnCharacterCreateResponse;
        public event EventHandler<CharDeleteResponse> OnCharacterDeleteResponse;

        private void EvaluateEvent(string parEvent, params object[] parArgs)
        {
            Task.Run(() =>
            {
                try
                {
                    _evaluteEvent(parEvent, parArgs);
                    //Console.WriteLine($"EVENT HANDLER: {parEvent} {JsonConvert.SerializeObject(parArgs)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EVENT HANDLER: {parEvent} {JsonConvert.SerializeObject(parArgs)} {ex.Message}");
                }
            });
        }

        private void _evaluteEvent(string parEvent, object[] parArgs)
        {
            if (OnEvent != null)
            {
                OnEventArgs args = new(parEvent, parArgs);
                OnEvent.Invoke(this, args);
            }

            if (parEvent == "PLAYER_TARGET_CHANGED")
            {
                OnTargetChange?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "QUEST_FINISHED")
            {

            }
            else if (parEvent == "QUEST_PROGRESS" ||
                     parEvent == "QUEST_COMPLETE" ||
                     parEvent == "QUEST_DETAIL")
            {

            }
            else if (parEvent == "QUEST_GREETING")
            {

            }
            else if (parEvent == "UNIT_LEVEL")
            {
                if ((string)parArgs[0] != "player") return;
                LevelUp?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "PLAYER_MONEY")
            {
                OnMoneyChange?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "CHAT_MSG_LOOT")
            {
                // You receive loot: |cff9d9d9d|Hitem:7098:0:0:0|h[Splintered Tusk]|h|rx2.
                if (OnLoot == null)
                    return;
                string arg1 = (string)parArgs[0];
                string[] argArr = arg1.Split('|');
                int itemId = Convert.ToInt32(argArr[2].Split(':')[1]);
                string itemName = argArr[3].Substring(2, argArr[3].Length - 3);
                int itemCount = 1;
                if (argArr[5].Length != 2)
                    itemCount = Convert.ToInt32(argArr[5].Substring(2, argArr[5].Length - 3));
                OnLoot.Invoke(this, new OnLootArgs(itemId, itemName, itemCount));
            }
            else if (parEvent == "OPEN_STATUS_DIALOG")
            {
                if (OnWrongLogin == null) return;
                if (parArgs.Length != 2) return;
                if (((string)parArgs[1]).Contains("The information you have entered is not valid."))
                {
                    OnWrongLogin.Invoke(this, new EventArgs());
                    return;
                }
                if (((string)parArgs[1]).Contains("Handshaking"))
                {
                    OnHandshakeBegin.Invoke(this, new EventArgs());
                    return;
                }
            }
            else if (parEvent == "UPDATE_SELECTED_CHARACTER")
            {
                OnCharacterListLoaded?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "UPDATE_STATUS_DIALOG")
            {
                if (InServerQueue == null) return;
                if (parArgs.Length != 2) return;
                if (!((string)parArgs[0]).Contains("Position in queue:")) return;
                if ((string)parArgs[1] != "Change Realm") return;
                InServerQueue.Invoke(this, new EventArgs());
            }
            else if (parEvent == "GET_PREFERRED_REALM_INFO")
            {
                OnChooseRealm?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "DISCONNECTED_FROM_SERVER")
            {
                OnDisconnect?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "UI_ERROR_MESSAGE")
            {
                OnErrorMessage?.Invoke(this, new OnUiMessageArgs((string)parArgs[0]));
            }
            else if (parEvent == "UI_INFO_MESSAGE")
            {
                OnUiMessage?.Invoke(this, new OnUiMessageArgs((string)parArgs[0]));
            }
            else if (parEvent == "CHAT_MSG_SYSTEM")
            {
                OnSystemMessage?.Invoke(this, new OnUiMessageArgs((string)parArgs[0]));
            }
            else if (parEvent == "PLAYER_REGEN_ENABLED")
            {
                OnFightStop?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "PLAYER_REGEN_DISABLED")
            {
                OnFightStart?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "UNIT_COMBAT")
            {
                // "NONE" represents a partial block (damage reduction). "BLOCK" represents a full block (damage avoidance).
                if ((string)parArgs[0] == "player" && ((string)parArgs[1] == "DODGE" || (string)parArgs[1] == "PARRY" || (string)parArgs[1] == "NONE" || (string)parArgs[1] == "BLOCK"))
                    OnBlockParryDodge?.Invoke(null, new EventArgs());
                if ((string)parArgs[0] == "player" && (string)parArgs[1] == "PARRY")
                    OnParry?.Invoke(null, new EventArgs());

            }
            else if (parEvent == "CHAT_MSG_COMBAT_SELF_HITS" || parEvent == "CHAT_MSG_COMBAT_SELF_MISSES")
            {
                OnSlamReady?.Invoke(null, new EventArgs());
            }
            else if (parEvent == "CHAT_MSG_SPELL_SELF_DAMAGE")
            {
                string messageText = (string)parArgs[0];
                if (messageText.Contains("Heroic Strike") || messageText.Contains("Cleave"))
                    OnSlamReady?.Invoke(null, new EventArgs());
            }
            else if (parEvent == "CHAT_MSG_COMBAT_HOSTILE_DEATH")
            {
                if (OnUnitKilled == null) return;
                if (!((string)parArgs[0]).Contains("You have")) return;
                OnUnitKilled.Invoke(this, new EventArgs());
            }
            else if (parEvent == "CHAT_MSG_SAY")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Say";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];

                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);

                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_MONSTER_SAY")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Say";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Npc, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_MONSTER_YELL")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Yell";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Npc, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_YELL")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Yell";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_CHANNEL")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Channel " + (int)parArgs[7];
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_RAID")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Raid";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_GUILD")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, "Guild", chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_PARTY")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Party";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_WHISPER")
            {
                if (OnChatMessage == null) return;
                string unitName = (string)parArgs[1];
                string chatType = "Whisper";
                string chatTag = (string)parArgs[5];
                string chatMessage = (string)parArgs[0];
                ChatMessageArgs args = new(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "DUEL_REQUESTED")
            {
                OnDuelRequest?.Invoke(this, new OnRequestArgs((string)parArgs[0]));
            }
            else if (parEvent == "GUILD_INVITE_REQUEST")
            {
                if (OnGuildInvite == null) return;
                string player = (string)parArgs[0];
                string guild = (string)parArgs[1];
                OnGuildInvite.Invoke(this, new GuildInviteArgs(player, guild));
            }
            else if (parEvent == "TRADE_SHOW")
            {
                OnTradeShow?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "PARTY_INVITE_REQUEST")
            {
                OnPartyInvite?.Invoke(this, new OnRequestArgs((string)parArgs[0]));
            }
            else if (parEvent == "PLAYER_DEAD")
            {
                OnDeath?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "PLAYER_UNGHOST")
            {
                OnResurrect?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "CORPSE_IN_RANGE")
            {
                OnCorpseInRange?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "CORPSE_OUT_OF_RANGE")
            {
                OnCorpseOutOfRange?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "LOOT_OPENED")
            {
                LOOT_HANDLE(LootState.SHOW);
            }
            else if (parEvent == "LOOT_CLOSED")
            {
                LOOT_HANDLE(LootState.CLOSE);
            }
            else if (parEvent == "UNIT_AURA")
            {
                AuraChanged?.Invoke(this, new AuraChangedArgs((string)parArgs[0]));
            }
            else if (parEvent == "CHAT_MSG_SKILL")
            {
                OnSkillMessage?.Invoke(this, new OnUiMessageArgs((string)parArgs[0]));
            }
            else if (parEvent == "CHAT_MSG_COMBAT_XP_GAIN")
            {
                if (OnXpGain == null) return;
                string str = (string)parArgs[0];
                Regex regex = new("(?i)you gain [0-9]+");
                Match match = regex.Match(str);
                regex = new Regex("[0-9]+");
                str = regex.Match(match.Value).Value;
                OnXpGain?.Invoke(this, new OnXpGainArgs(Convert.ToInt32(str)));
            }
            else if (parEvent == "UNIT_MODEL_CHANGED")
            {
            }
            else if (parEvent == "GOSSIP_SHOW")
            {
                GOSSIP_SHOW();
            }
            else if (parEvent == "GOSSIP_CLOSED")
            {
                GOSSIP_CLOSED();
            }
            else if (parEvent == "MERCHANT_SHOW")
            {
                MERCHANT_HANDLE(MerchantState.SHOW);
            }
            else if (parEvent == "MERCHANT_CLOSED")
            {
                MERCHANT_HANDLE(MerchantState.CLOSE);
            }
            else if (parEvent == "TAXIMAP_OPENED")
            {
                TAXIMAP_OPENED();
            }
            else if (parEvent == "TAXIMAP_CLOSED")
            {
                TAXIMAP_CLOSED();
            }
            else if (parEvent == "TRAINER_SHOW")
            {
                TRAINER_SHOW();
            }
            else if (parEvent == "TRAINER_CLOSED")
            {
                TRAINER_CLOSED();
            }
            else if (parEvent == "BANKFRAME_OPENED")
            {
                OnBankFrameOpen?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "BANKFRAME_CLOSED")
            {
                OnBankFrameClosed?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "MAIL_SHOW")
            {
                OnMailboxOpen?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "MAIL_CLOSED")
            {
                OnMailboxClosed?.Invoke(this, new EventArgs());
            }
        }

        private void TRAINER_CLOSED()
        {
            OnTrainerClosed?.Invoke(this, new EventArgs());
        }

        private void TAXIMAP_CLOSED()
        {
            OnTaxiClosed?.Invoke(this, new EventArgs());
        }

        private void GOSSIP_CLOSED()
        {
            OnGossipClosed?.Invoke(this, new EventArgs());
        }

        private void TRAINER_SHOW()
        {
            OnTrainerShow?.Invoke(this, new EventArgs());
        }

        private void LOOT_HANDLE(LootState parState)
        {

        }

        private void TAXIMAP_OPENED()
        {

        }

        private void GOSSIP_SHOW()
        {

        }

        private void MERCHANT_HANDLE(MerchantState parState)
        {

        }
    }
}
