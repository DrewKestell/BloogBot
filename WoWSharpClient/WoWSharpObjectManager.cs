using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Screens;

namespace WoWSharpClient
{
    public class WoWSharpObjectManager : IObjectManager
    {
        private readonly ILogger<WoWSharpObjectManager> _logger;

        // Wrapper client for both auth and world transactions
        private readonly WoWClient _woWClient;

        private readonly WoWSharpEventEmitter _eventEmitter = new();

        public WoWSharpEventEmitter EventEmitter => _eventEmitter;

        private readonly LoginScreen _loginScreen;
        private readonly RealmSelectScreen _realmScreen;
        private readonly CharacterSelectScreen _characterSelectScreen;
        public WoWSharpObjectManager(string ipAddress, ILogger<WoWSharpObjectManager> logger)
        {
            _logger = logger;

            _eventEmitter.OnLoginFailure += EventEmitter_OnLoginFailure;
            _eventEmitter.OnWorldSessionStart += EventEmitter_OnWorldSessionStart;
            _eventEmitter.OnWorldSessionEnd += EventEmitter_OnWorldSessionEnd;
            _eventEmitter.OnCharacterListLoaded += EventEmitter_OnCharacterListLoaded;
            _eventEmitter.OnChatMessage += EventEmitter_OnChatMessage;
            _eventEmitter.OnGameObjectCreated += EventEmitter_OnGameObjectCreated;

            _woWClient = new(ipAddress, this);

            _loginScreen = new(_woWClient);
            _realmScreen = new(_woWClient);
            _characterSelectScreen = new(_woWClient);
        }

        private void EventEmitter_OnCharacterListLoaded(object? sender, EventArgs e)
        {
            _characterSelectScreen.HasReceivedCharacterList = true;
        }

        private void EventEmitter_OnWorldSessionStart(object? sender, EventArgs e)
        {
            _characterSelectScreen.RefreshCharacterListFromServer();
        }

        private void EventEmitter_OnGameObjectCreated(object? sender, GameObjectCreatedArgs e) => _woWClient.SendNameQuery(e.Guid);
        private void EventEmitter_OnChatMessage(object? sender, ChatMessageArgs e)
        {
            Console.ResetColor();
            StringBuilder sb = new();
            switch (e.MsgType)
            {
                case ChatMsg.CHAT_MSG_SAY:
                case ChatMsg.CHAT_MSG_MONSTER_SAY:
                    sb.Append($"[{e.SenderGuid}]");

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

        private void EventEmitter_OnLoginFailure(object? sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Main]Failed to login to WoW server");
            _woWClient.Dispose();
        }
        private void EventEmitter_OnWorldSessionEnd(object? sender, EventArgs e)
        {
            HasEnteredWorld = false;
        }
        public ILoginScreen LoginScreen => _loginScreen;
        public IRealmSelectScreen RealmSelectScreen => _realmScreen;
        public ICharacterSelectScreen CharacterSelectScreen => _characterSelectScreen;
        public bool HasEnteredWorld { get; internal set; }

        public IEnumerable<WoWObject> ObjectsBuffer = [];
        public List<Spell> Spells { get; internal set; } = [];
        public List<Cooldown> Cooldowns { get; internal set; } = [];

        public HighGuid PlayerGuid { get; } = new HighGuid(new byte[4], new byte[4]);

        public uint MapId => throw new NotImplementedException();

        public string ZoneText => throw new NotImplementedException();

        public string MinimapZoneText => throw new NotImplementedException();

        public string ServerName => throw new NotImplementedException();

        public IGossipFrame GossipFrame => throw new NotImplementedException();

        public ILootFrame LootFrame => throw new NotImplementedException();

        public IMerchantFrame MerchantFrame => throw new NotImplementedException();

        public ICraftFrame CraftFrame => throw new NotImplementedException();

        public IQuestFrame QuestFrame => throw new NotImplementedException();

        public IQuestGreetingFrame QuestGreetingFrame => throw new NotImplementedException();

        public ITaxiFrame TaxiFrame => throw new NotImplementedException();

        public ITradeFrame TradeFrame => throw new NotImplementedException();

        public ITrainerFrame TrainerFrame => throw new NotImplementedException();

        public ITalentFrame TalentFrame => throw new NotImplementedException();

        public IWoWLocalPlayer Player => throw new NotImplementedException();

        public IWoWLocalPet Pet => throw new NotImplementedException();

        public IList<IWoWObject> Objects { get; } = [];

        public IEnumerable<IWoWGameObject> GameObjects => Objects.OfType<IWoWGameObject>();

        public IEnumerable<IWoWUnit> Units => Objects.OfType<IWoWUnit>();

        public IEnumerable<IWoWPlayer> Players => Objects.OfType<IWoWPlayer>();

        public IEnumerable<IWoWItem> Items => Objects.OfType<IWoWItem>();

        public IEnumerable<IWoWContainer> Containers => Objects.OfType<IWoWContainer>();

        public IEnumerable<IWoWUnit> CasterAggressors => Objects.OfType<IWoWUnit>();

        public IEnumerable<IWoWUnit> MeleeAggressors => Objects.OfType<IWoWUnit>();

        public IEnumerable<IWoWUnit> Aggressors => Objects.OfType<IWoWUnit>();

        public IEnumerable<IWoWUnit> Hostiles => Objects.OfType<IWoWUnit>();

        public IEnumerable<IWoWPlayer> PartyMembers => Objects.OfType<IWoWPlayer>();

        public IWoWPlayer PartyLeader => throw new NotImplementedException();

        public ulong PartyLeaderGuid => throw new NotImplementedException();

        public ulong Party1Guid => throw new NotImplementedException();

        public ulong Party2Guid => throw new NotImplementedException();

        public ulong Party3Guid => throw new NotImplementedException();

        public ulong Party4Guid => throw new NotImplementedException();

        public ulong StarTargetGuid => throw new NotImplementedException();

        public ulong CircleTargetGuid => throw new NotImplementedException();

        public ulong DiamondTargetGuid => throw new NotImplementedException();

        public ulong TriangleTargetGuid => throw new NotImplementedException();

        public ulong MoonTargetGuid => throw new NotImplementedException();

        public ulong SquareTargetGuid => throw new NotImplementedException();

        public ulong CrossTargetGuid => throw new NotImplementedException();

        public ulong SkullTargetGuid => throw new NotImplementedException();

        public string GlueDialogText => throw new NotImplementedException();

        public LoginStates LoginState => throw new NotImplementedException();

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
