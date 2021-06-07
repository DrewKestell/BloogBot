using BloogBot.Game.Frames;
using System;

namespace BloogBot.Game
{
    public class WoWEventHandler
    {
        static WoWEventHandler()
        {
            SignalEventManager.OnNewSignalEvent += EvaluateEvent;
            SignalEventManager.OnNewSignalEventNoArgs += EvaluateEvent;
        }

        static void EvaluateEvent(string eventName, object[] args)
        {
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                switch (eventName)
                {
                    case "LOOT_OPENED":
                        OpenLootFrame();
                        break;
                    case "GOSSIP_SHOW":
                        OpenDialogFrame();
                        break;
                    case "MERCHANT_SHOW":
                        OpenMerchantFrame();
                        break;
                    case "UNIT_COMBAT":
                        // "NONE" represents a partial block (damage reduction). "BLOCK" represents a full block (damage avoidance).
                        if ((string)args[0] == "player" && ((string)args[1] == "DODGE" || (string)args[1] == "PARRY" || (string)args[1] == "NONE" || (string)args[1] == "BLOCK"))
                            OnBlockParryDodge?.Invoke(null, new EventArgs());
                        if ((string)args[0] == "player" && (string)args[1] == "PARRY")
                            OnParry?.Invoke(null, new EventArgs());
                        break;
                    case "UI_ERROR_MESSAGE":
                        OnErrorMessage?.Invoke(null, new OnUiMessageArgs((string)args[0]));
                        break;
                    case "CHAT_MSG_COMBAT_SELF_HITS":
                    case "CHAT_MSG_COMBAT_SELF_MISSES":
                        OnSlamReady?.Invoke(null, new EventArgs());
                        break;
                    case "CHAT_MSG_SPELL_SELF_DAMAGE":
                        var messageText = (string)args[0];
                        if (messageText.Contains("Heroic Strike") || messageText.Contains("Cleave"))
                            OnSlamReady?.Invoke(null, new EventArgs());
                        break;
                    case "CHAT_MSG_SAY":
                        OnChatMessage?.Invoke(null, new OnChatMessageArgs((string)args[0], (string)args[1], "Say"));
                        break;
                    case "CHAT_MSG_WHISPER":
                        OnChatMessage?.Invoke(null, new OnChatMessageArgs((string)args[0], (string)args[1], "Whisper"));
                        break;
                    case "UNIT_LEVEL":
                        if ((string)args[0] == "player")
                            OnLevelUp?.Invoke(null, new EventArgs());
                        break;
                }
            });
        }

        static public event EventHandler<EventArgs> OnBlockParryDodge;

        static public event EventHandler<EventArgs> OnParry;

        static public event EventHandler<OnLootFrameOpenArgs> OnLootOpened;

        static public event EventHandler<OnDialogFrameOpenArgs> OnDialogOpened;

        static public event EventHandler<OnMerchantFrameOpenArgs> OnMerchantFrameOpened;

        static public event EventHandler<OnUiMessageArgs> OnErrorMessage;

        static public event EventHandler<EventArgs> OnSlamReady;

        static public event EventHandler<OnChatMessageArgs> OnChatMessage;

        static public event EventHandler<EventArgs> OnLevelUp;

        static public void ClearOnErrorMessage()
        {
            OnErrorMessage = null;
        }

        static void OpenLootFrame()
        {
            var lootFrame = new LootFrame();
            OnLootOpened?.Invoke(null, new OnLootFrameOpenArgs(lootFrame));
        }

        static void OpenDialogFrame()
        {
            var dialogFrame = new DialogFrame();
            OnDialogOpened?.Invoke(null, new OnDialogFrameOpenArgs(dialogFrame));
        }

        static void OpenMerchantFrame()
        {
            var merchantFrame = new MerchantFrame();
            OnMerchantFrameOpened?.Invoke(null, new OnMerchantFrameOpenArgs(merchantFrame));
        }
    }

    public class OnLootFrameOpenArgs : EventArgs
    {
        public readonly LootFrame LootFrame;

        internal OnLootFrameOpenArgs(LootFrame lootFrame)
        {
            LootFrame = lootFrame;
        }
    }

    public class OnDialogFrameOpenArgs : EventArgs
    {
        public readonly DialogFrame DialogFrame;

        internal OnDialogFrameOpenArgs(DialogFrame dialogFrame)
        {
            DialogFrame = dialogFrame;
        }
    }

    public class OnMerchantFrameOpenArgs : EventArgs
    {
        public readonly MerchantFrame MerchantFrame;

        internal OnMerchantFrameOpenArgs(MerchantFrame merchantFrame)
        {
            MerchantFrame = merchantFrame;
        }
    }

    public class OnUiMessageArgs : EventArgs
    {
        public readonly string Message;

        internal OnUiMessageArgs(string message)
        {
            Message = message;
        }
    }

    public class OnChatMessageArgs : EventArgs
    {
        public readonly string Message;
        public readonly string UnitName;
        public readonly string ChatChannel;

        internal OnChatMessageArgs(string message, string unitName, string chatChannel)
        {
            Message = message;
            UnitName = unitName;
            ChatChannel = chatChannel;
        }
    }
}
