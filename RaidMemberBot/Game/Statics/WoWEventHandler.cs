using Newtonsoft.Json;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Mem.Hooks;
using RaidMemberBot.Objects;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Contains WoW Events to subscribe to
    /// </summary>
    public sealed class WoWEventHandler
    {
        private Task thrLootUpdate;
        private Task thrMerchUpdate;

        private WoWEventHandler()
        {
            SignalEventManager.OnNewSignalEvent += EvaluateEvent;
        }

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
            Console.WriteLine(args.Location);
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
        public event EventHandler OnQuestComplete
        {
            add { QuestLog.Instance.OnQuestComplete += value; }
            remove { QuestLog.Instance.OnQuestComplete -= value; }
        }

        /// <summary>
        ///     Occurs when all objectives of a quest are done
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestObjectiveComplete
        {
            add { QuestLog.Instance.OnQuestObjectiveComplete += value; }
            remove { QuestLog.Instance.OnQuestObjectiveComplete -= value; }
        }

        /// <summary>
        ///     Occurs when the QuestFrame is opened
        /// </summary>
        public event EventHandler OnQuestFrameOpen
        {
            add { QuestFrame.OnQuestFrameOpen += value; }
            remove { QuestFrame.OnQuestFrameOpen -= value; }
        }

        /// <summary>
        ///     Occurs when the QuestFrame is closed
        /// </summary>
        public event EventHandler OnQuestFrameClosed
        {
            add { QuestFrame.OnQuestFrameClosed += value; }
            remove { QuestFrame.OnQuestFrameClosed -= value; }
        }

        /// <summary>
        ///     Occurs when the QuestGreetingFrame is opened
        /// </summary>
        public event EventHandler OnQuestGreetingFrameOpen
        {
            add { QuestGreetingFrame.OnQuestGreetingFrameOpen += value; }
            remove { QuestGreetingFrame.OnQuestGreetingFrameOpen -= value; }
        }

        /// <summary>
        ///     Occurs when the QUestGreetingFrame is closed
        /// </summary>
        public event EventHandler OnQuestGreetingFrameClosed
        {
            add { QuestGreetingFrame.OnQuestGreetingFrameClosed += value; }
            remove { QuestGreetingFrame.OnQuestGreetingFrameClosed -= value; }
        }

        /// <summary>
        ///     Occurs when a quest failed
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestFailed
        {
            add { QuestLog.Instance.OnQuestFailed += value; }
            remove { QuestLog.Instance.OnQuestFailed -= value; }
        }

        /// <summary>
        ///     Occurs on quest progress (required unit killed, item collected, event completed etc.)
        ///     Sender of type QuestLogEntry
        /// </summary>
        public event EventHandler OnQuestProgress
        {
            add { QuestLog.Instance.OnQuestProgress += value; }
            remove { QuestLog.Instance.OnQuestProgress -= value; }
        }

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


        private void EvaluateEvent(string parEvent, params object[] parArgs)
        {
            Task.Run(() =>
            {
                try
                {
                    _evaluteEvent(parEvent, parArgs); 
                    Console.WriteLine($"EVENT HANDLER: {parEvent} {JsonConvert.SerializeObject(parArgs)}");
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
                var args = new OnEventArgs(parEvent, parArgs);
                OnEvent.Invoke(this, args);
            }

            if (parEvent == "PLAYER_TARGET_CHANGED")
            {
                OnTargetChange?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "QUEST_FINISHED")
            {
                if (QuestFrame.IsOpen)
                    QuestFrame.Destroy();

                if (QuestGreetingFrame.IsOpen)
                    QuestGreetingFrame.Destroy();
            }
            else if (parEvent == "QUEST_PROGRESS" ||
                     parEvent == "QUEST_COMPLETE" ||
                     parEvent == "QUEST_DETAIL")
            {
                DirectX.Instance.Execute(QuestFrame.Create, 100);
            }
            else if (parEvent == "QUEST_GREETING")
            {
                DirectX.Instance.Execute(QuestGreetingFrame.Create, 100);
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
                var arg1 = (string)parArgs[0];
                var argArr = arg1.Split('|');
                var itemId = Convert.ToInt32(argArr[2].Split(':')[1]);
                var itemName = argArr[3].Substring(2, argArr[3].Length - 3);
                var itemCount = 1;
                if (argArr[5].Length != 2)
                    itemCount = Convert.ToInt32(argArr[5].Substring(2, argArr[5].Length - 3));
                OnLoot.Invoke(this, new OnLootArgs(itemId, itemName, itemCount));
            }
            else if (parEvent == "OPEN_STATUS_DIALOG")
            {
                if (OnWrongLogin == null) return;
                if (parArgs.Length != 2) return;
                if ((string)parArgs[0] != "OKAY") return;
                if ((string)parArgs[1] != "The information you have entered is not valid.") return;
                OnWrongLogin.Invoke(this, new EventArgs());
            }
            else if (parEvent == "UPDATE_SELECTED_CHARACTER")
            {
                OnCharacterListLoaded?.Invoke(this, new EventArgs());
            }
            else if (parEvent == "UPDATE_STATUS_DIALOG")
            {
                if (InServerQueue == null) return;
                if (parArgs.Length != 2) return;
                if (!((string)parArgs[0]).Contains("Location in queue:")) return;
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
                OnErrorMessage?.Invoke(this, new OnUiMessageArgs(((int)parArgs[0]).ToString()));
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
                var messageText = (string)parArgs[0];
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
                var unitName = (string)parArgs[1];
                var chatType = "Say";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];

                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);

                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_MONSTER_SAY")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Say";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Npc, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_MONSTER_YELL")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Yell";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Npc, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_YELL")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Yell";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_CHANNEL")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Channel " + (int)parArgs[7];
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_RAID")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Raid";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_GUILD")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, "Guild", chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_PARTY")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Party";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "CHAT_MSG_WHISPER")
            {
                if (OnChatMessage == null) return;
                var unitName = (string)parArgs[1];
                var chatType = "Whisper";
                var chatTag = (string)parArgs[5];
                var chatMessage = (string)parArgs[0];
                var args = new ChatMessageArgs(ChatSenderType.Player, chatTag, unitName, chatType, chatMessage);
                OnChatMessage.Invoke(this, args);
            }
            else if (parEvent == "DUEL_REQUESTED")
            {
                OnDuelRequest?.Invoke(this, new OnRequestArgs((string)parArgs[0]));
            }
            else if (parEvent == "GUILD_INVITE_REQUEST")
            {
                if (OnGuildInvite == null) return;
                var player = (string)parArgs[0];
                var guild = (string)parArgs[1];
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
                var str = (string)parArgs[0];
                var regex = new Regex("(?i)you gain [0-9]+");
                var match = regex.Match(str);
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
                Console.WriteLine("MERCHANT_SHOW occured");
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
            TrainerFrame.Destroy();
            OnTrainerClosed?.Invoke(this, new EventArgs());
        }

        private void TAXIMAP_CLOSED()
        {
            TaxiFrame.Destroy();
            OnTaxiClosed?.Invoke(this, new EventArgs());
        }

        private void GOSSIP_CLOSED()
        {
            GossipFrame.Destroy();
            OnGossipClosed?.Invoke(this, new EventArgs());
        }

        private void TRAINER_SHOW()
        {
            if (TrainerFrame.IsOpen)
                TrainerFrame.Destroy();
            TrainerFrame.Create();

            OnTrainerShow?.Invoke(this, new EventArgs());
        }

        private void LOOT_HANDLE(LootState parState)
        {
            switch (parState)
            {
                case LootState.SHOW:
                    Action tmpAction = () =>
                    {
                        thrLootUpdate = new Task(() =>
                        {
                            if (ObjectManager.Instance.Player.CurrentLootGuid == 0) return;
                            if (LootFrame.IsOpen)
                                LootFrame.Destroy();
                            LootFrame.Create();
                        });
                        thrLootUpdate.ContinueWith(task =>
                        {
                            if (!LootFrame.IsOpen) return;

                            OnLootOpened?.Invoke(this, new EventArgs());
                        });
                        thrLootUpdate.Start();
                    };
                    if (thrLootUpdate != null && thrLootUpdate.Status == TaskStatus.Running) return;
                    ThreadSynchronizer.Instance.Invoke(tmpAction);
                    break;

                case LootState.CLOSE:
                    LootFrame.Destroy();
                    thrLootUpdate = null;
                    OnLootClosed?.Invoke(this, new EventArgs());
                    break;
            }
        }

        private void TAXIMAP_OPENED()
        {
            if (TaxiFrame.IsOpen)
                TaxiFrame.Destroy();
            TaxiFrame.Create();

            OnTaxiShow?.Invoke(this, new EventArgs());
        }

        private void GOSSIP_SHOW()
        {
            if (GossipFrame.IsOpen)
                GossipFrame.Destroy();
            GossipFrame.Create();

            OnGossipShow?.Invoke(this, new EventArgs());
        }

        private void MERCHANT_HANDLE(MerchantState parState)
        {
            Console.WriteLine("Handling merchant");
            switch (parState)
            {
                case MerchantState.SHOW:
                    if (thrMerchUpdate != null && thrMerchUpdate.Status == TaskStatus.Running)
                    {
                        Console.WriteLine("Task is stil running. Returning here");
                        return;
                    }
                    Console.WriteLine("Creating the action");
                    Action tmpAction = () =>
                    {
                        thrMerchUpdate = new Task(() =>
                        {
                            Console.WriteLine("Checking Vendor Guid");
                            if (ObjectManager.Instance.Player.VendorGuid == 0)
                            {
                                Console.WriteLine("Vendor Guid is 0 ... returning");
                                return;
                            }
                            if (MerchantFrame.IsOpen)
                            {
                                Console.WriteLine("Merchant frame marked as open. Destroying the old object and recreating");
                                MerchantFrame.Destroy();
                            }
                            Console.WriteLine("Creating the new frame");
                            MerchantFrame.Create();
                        });
                        thrMerchUpdate.ContinueWith(task =>
                        {
                            Console.WriteLine("Finalising merchant event");
                            if (!MerchantFrame.IsOpen) return;

                            Console.WriteLine("Invoking OnMerchantShow");
                            OnMerchantShow?.Invoke(this, new EventArgs());
                        });
                        thrMerchUpdate.Start();
                    };
                    Console.WriteLine("Running the action from EndScene");
                    DirectX.Instance.Execute(tmpAction, 200);
                    break;

                case MerchantState.CLOSE:
                    MerchantFrame.Destroy();
                    thrMerchUpdate = null;
                    OnMerchantClosed?.Invoke(this, new EventArgs());
                    break;
            }
        }

        /// <summary>
        ///     Loot event args
        /// </summary>
        public class OnLootArgs : EventArgs
        {
            internal OnLootArgs(int itemId, string itemName, int count)
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

            public override string ToString()
            {
                return "[" + Time.ToShortTimeString() + "] " + Count + "x " + ItemName + " (" + ItemId + ")";
            }
        }

        /// <summary>
        ///     UI message args
        /// </summary>
        public class OnUiMessageArgs : EventArgs
        {
            public readonly string Message;

            internal OnUiMessageArgs(string message)
            {
                Message = message;
            }
        }

        /// <summary>
        ///     On xp args
        /// </summary>
        public class OnXpGainArgs : EventArgs
        {
            public readonly int Xp;

            internal OnXpGainArgs(int xp)
            {
                Xp = xp;
            }
        }

        /// <summary>
        ///     On aura changed args
        /// </summary>
        public class AuraChangedArgs : EventArgs
        {
            public readonly string AffectedUnit;

            internal AuraChangedArgs(string affectedUnit)
            {
                AffectedUnit = affectedUnit;
            }
        }

        /// <summary>
        ///     On request args
        /// </summary>
        public class OnRequestArgs : EventArgs
        {
            public readonly string Player;

            internal OnRequestArgs(string player)
            {
                Player = player;
            }
        }

        /// <summary>
        ///     chat message args
        /// </summary>
        public class ChatMessageArgs : EventArgs
        {
            internal ChatMessageArgs(ChatSenderType unitType, string chatTag, string unitName, string chatChannel,
                string message)
            {
                UnitType = unitType;
                ChatTag = chatTag;
                UnitName = unitName;
                ChatChannel = chatChannel;
                Message = message;
                Time = DateTime.Now;
            }

            public ChatSenderType UnitType { get; private set; }
            public string ChatTag { get; }
            public string UnitName { get; }
            public string ChatChannel { get; }
            public string Message { get; }
            public DateTime Time { get; }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            /// <filterpriority>2</filterpriority>
            public override string ToString()
            {
                return "[" + Time.ToShortTimeString() + "]" + (ChatTag != "" ? " [" + ChatTag + " " : " [") + UnitName +
                       "] "
                       + "[" + ChatChannel + "]: " + Message;
            }
        }

        /// <summary>
        ///     Guild invite args
        /// </summary>
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

        /// <summary>
        ///     On CTM args
        /// </summary>
        public class OnCtmArgs : EventArgs
        {
            /// <summary>
            ///     The type of CTM as int
            /// </summary>
            public readonly int CtmType;

            /// <summary>
            ///     The Location of the Ctm as Location
            /// </summary>
            public readonly Location Location;

            internal OnCtmArgs(Location position, int ctmType)
            {
                Location = position;
                CtmType = ctmType;
            }
        }

        /// <summary>
        ///     On event args
        /// </summary>
        public class OnEventArgs : EventArgs
        {
            /// <summary>
            ///     Name of the WoW event
            /// </summary>
            public readonly string EventName;

            /// <summary>
            ///     Parameters of the event (can be null)
            /// </summary>
            public readonly object[] Parameters;

            internal OnEventArgs(string eventName, object[] parameters)
            {
                EventName = eventName;
                Parameters = parameters;
            }
        }

        private enum MerchantState
        {
            SHOW = 1,
            CLOSE = 2
        }

        private enum LootState
        {
            SHOW = 1,
            CLOSE = 2
        }
    }
}
