using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using Communication;
using Microsoft.Extensions.Logging;
using System.Text;
using WoWSharpClient.Client;
using WoWSharpClient.Models;

namespace WoWSharpClient.Manager
{
    public class ObjectManager : BaseObjectManager, IObjectManager
    {
        private readonly WoWClient _woWClient;
        private readonly ILogger<ObjectManager> _logger;
        private readonly ActivitySnapshot _activitySnapshot;

        private bool _isLoginConnected;
        private bool _isWorldConnected;
        private bool _isLoggedIn;
        private bool _hasEnteredWorld;

        public bool IsLoginConnected => _isLoginConnected;
        public bool IsWorldConnected => _isWorldConnected;
        public bool IsLoggedIn => _isLoggedIn;
        public bool HasEnteredWorld => _hasEnteredWorld;
        public Realm CurrentRealm { get; set; } = new();
        public bool HasRealmSelected => CurrentRealm != null;

        public IEnumerable<Models.Object> ObjectsBuffer = [];
        public List<Spell> Spells { get; internal set; } = [];
        public List<Cooldown> Cooldowns { get; internal set; } = [];

        public readonly List<CharacterSelect> CharacterSelects = [];

        public ObjectManager(WoWSharpEventEmitter woWSharpEventEmitter, 
                                ActivitySnapshot parProbe) : base(woWSharpEventEmitter, parProbe)
        {
            woWSharpEventEmitter.OnLoginConnect += Instance_OnLoginConnected;
            woWSharpEventEmitter.OnLoginSuccess += Instance_OnLoginSuccess;
            woWSharpEventEmitter.OnLoginFailure += Instance_OnLoginFail;
            woWSharpEventEmitter.OnWorldSessionStart += Instance_OnWorldSessionStart;
            woWSharpEventEmitter.OnWorldSessionEnd += Instance_OnWorldSessionEnd;
            woWSharpEventEmitter.OnHandshakeBegin += Instance_OnHandshakeBegin;
            woWSharpEventEmitter.OnLoginFailure += Instance_OnLoginFailure;
            woWSharpEventEmitter.OnCharacterListLoaded += Instance_OnCharacterListLoaded;
            woWSharpEventEmitter.OnChatMessage += Instance_OnChatMessage;
            woWSharpEventEmitter.OnGameObjectCreated += Instance_OnGameObjectCreated;
        }
        private void Instance_OnGameObjectCreated(object? sender, GameObjectCreatedArgs e)
        {
            _woWClient.SendNameQuery(e.Guid);
        }

        private async void Instance_OnChatMessage(object? sender, ChatMessageArgs e)
        {
            Console.ResetColor();
            StringBuilder sb = new();
            switch (e.MsgType)
            {
                case ChatMsg.CHAT_MSG_SAY:
                case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    sb.Append($"[{e.SenderGuid}]");

                    if (e.SenderGuid != PlayerGuid.FullGuid)
                    {
                        _woWClient.SendChatMessage(ChatMsg.CHAT_MSG_SAY, Language.Orcish, "Dallawha", e.Text);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case ChatMsg.CHAT_MSG_YELL:
                case ChatMsg.CHAT_MSG_MONSTER_YELL:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case ChatMsg.CHAT_MSG_WHISPER:
                case ChatMsg.CHAT_MSG_MONSTER_WHISPER:                    
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    if (e.SenderGuid != PlayerGuid.FullGuid)
                    {
                        _woWClient.SendChatMessage(ChatMsg.CHAT_MSG_WHISPER, Language.Orcish, "Dallawha", "");
                    }
                    break;

                case ChatMsg.CHAT_MSG_WHISPER_INFORM:
                    sb.Append($"To[{e.SenderGuid}]");
                    break;
                case ChatMsg.CHAT_MSG_EMOTE:
                case ChatMsg.CHAT_MSG_TEXT_EMOTE:
                case ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                case ChatMsg.CHAT_MSG_RAID_BOSS_EMOTE:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case ChatMsg.CHAT_MSG_SYSTEM:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    sb.Append($"[System]");
                    break;

                case ChatMsg.CHAT_MSG_PARTY:
                case ChatMsg.CHAT_MSG_RAID:
                case ChatMsg.CHAT_MSG_GUILD:
                case ChatMsg.CHAT_MSG_OFFICER:
                    sb.Append($"[{e.SenderGuid}]");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;

                case ChatMsg.CHAT_MSG_CHANNEL:
                case ChatMsg.CHAT_MSG_CHANNEL_NOTICE:
                    sb.Append($"[Channel]");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case ChatMsg.CHAT_MSG_RAID_WARNING:
                    sb.Append($"[Raid Warning]");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;

                case ChatMsg.CHAT_MSG_LOOT:
                    sb.Append($"[Loot]");
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                default:
                    sb.Append($"[{e.SenderGuid}][{e.MsgType}]");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            sb.Append(e.Text);

            Console.WriteLine(sb.ToString());
        }
        private void Instance_OnLoginConnected(object? sender, EventArgs e)
        {
            _isLoginConnected = true;
            _logger.LogInformation($"[Main] {_activitySnapshot.AccountName} Connected to WoW Login server");
            _woWClient.Login(_activitySnapshot.AccountName, "password");
        }

        private void Instance_OnHandshakeBegin(object? sender, EventArgs e)
        {
            _logger.LogInformation($"Starting handshake for {_activitySnapshot.AccountName}");
        }

        private void Instance_OnLoginSuccess(object? sender, EventArgs e)
        {
            _isLoggedIn = true;
            List<Realm> realms = _woWClient.GetRealmList();

            if (realms.Count > 0)
            {
                _woWClient.SelectRealm(realms[0]);

                if (HasRealmSelected)
                {
                    _woWClient.RefreshCharacterSelects();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Main]No realm selected");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Main]No realms listed");
            }
        }

        private void Instance_OnLoginFailure(object? sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Main]Failed to login to WoW server");
            _woWClient.Dispose();
        }

        private async void Instance_OnCharacterListLoaded(object? sender, EventArgs e)
        {
            await Task.Delay(100);

            if (CharacterSelects.Count > 0)
                _woWClient.EnterWorld(CharacterSelects[0].Guid);
        }
        private void Instance_OnLoginFail(object? sender, EventArgs e) => _isLoggedIn = false;
        private void Instance_OnWorldSessionStart(object? sender, EventArgs e) => _isWorldConnected = true;
        private void Instance_OnWorldSessionEnd(object? sender, EventArgs e)
        {
            _hasEnteredWorld = false;
            _isWorldConnected = false;
        }

        public void AntiAfk()
        {
            
        }

        public IWoWUnit GetTarget(IWoWUnit woWUnit)
        {
            return null;
        }

        public sbyte GetTalentRank(uint tabIndex, uint talentIndex)
        {
            return 0;
        }

        public void PickupInventoryItem(uint inventorySlot)
        {
            
        }

        public void DeleteCursorItem()
        {
            
        }

        public void EquipCursorItem()
        {
            
        }

        public void ConfirmItemEquip()
        {
            
        }

        public void SendChatMessage(string chatMessage)
        {
            
        }

        public void SetRaidTarget(IWoWUnit target, TargetMarker v)
        {
            
        }

        public void JoinBattleGroundQueue()
        {
            
        }

        public void ResetInstances()
        {
            
        }

        public void PickupMacro(uint v)
        {
            
        }

        public void PlaceAction(uint v)
        {
            
        }

        public void InviteToGroup(ulong guid)
        {
            
        }

        public void KickPlayer(ulong guid)
        {
            
        }

        public void AcceptGroupInvite()
        {
            
        }

        public void DeclineGroupInvite()
        {
            
        }

        public void LeaveGroup()
        {
            
        }

        public void DisbandGroup()
        {
            
        }

        public void ConvertToRaid()
        {
            
        }

        public bool HasPendingGroupInvite()
        {
            return false;
        }

        public bool HasLootRollWindow(int itemId)
        {
            return false;
        }

        public void LootPass(int itemId)
        {
            
        }

        public void LootRollGreed(int itemId)
        {
            
        }

        public void LootRollNeed(int itemId)
        {
            
        }

        public void AssignLoot(int itemId, ulong playerGuid)
        {
            
        }

        public void SetGroupLoot(GroupLootSetting setting)
        {
            
        }

        public void PromoteLootManager(ulong playerGuid)
        {
            
        }

        public void PromoteAssistant(ulong playerGuid)
        {
            
        }

        public void PromoteLeader(ulong playerGuid)
        {
            
        }
    }
}
