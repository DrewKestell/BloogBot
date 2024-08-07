using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using Communication;
using WoWSharpClient.Models;

namespace WoWSharpClient.Manager
{
    public class ObjectManager : IObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        private bool _isLoginConnected;
        private bool _isWorldConnected;
        private bool _isLoggedIn;
        private bool _hasEnteredWorld;

        public bool IsLoginConnected => _isLoginConnected;
        public bool IsWorldConnected => _isWorldConnected;
        public bool IsLoggedIn => _isLoggedIn;
        public bool HasEnteredWorld => _hasEnteredWorld;
        public Realm CurrentRealm { get; set; }
        public bool HasRealmSelected => CurrentRealm != null;

        public ulong PlayerGuid { get; internal set; }
        public ILocalPlayer Player { get; internal set; }
        public uint MapId { get; internal set; }

        public ulong PartyLeaderGuid { get; internal set; }
        public ulong SkullTargetGuid { get; internal set; }

        public IList<Models.Object> Objects = [];
        public IList<Models.Object> ObjectsBuffer = [];
        public List<Spell> Spells { get; internal set; }
        public List<Cooldown> Cooldowns { get; internal set; }

        public string ZoneText { get; internal set; }
        public string MinimapZoneText { get; internal set; }
        public string ServerName { get; internal set; }

        public ILocalPet Pet { get; internal set; }

        public IEnumerable<IWoWGameObject> GameObjects { get; internal set; }
        public IEnumerable<IWoWUnit> Units { get; internal set; }
        public IEnumerable<IWoWPlayer> Players { get; internal set; }
        public IEnumerable<IWoWItem> Items { get; internal set; }
        public IEnumerable<IWoWContainer> Containers { get; internal set; }
        public IEnumerable<IWoWPlayer> PartyMembers { get; internal set; }
        public IWoWPlayer PartyLeader { get; internal set; }

        public ulong Party1Guid { get; internal set; }
        public ulong Party2Guid { get; internal set; }
        public ulong Party3Guid { get; internal set; }
        public ulong Party4Guid { get; internal set; }

        public IEnumerable<IWoWUnit> CasterAggressors { get; internal set; }
        public IEnumerable<IWoWUnit> MeleeAggressors { get; internal set; }
        public IEnumerable<IWoWUnit> Aggressors { get; internal set; }
        public IEnumerable<IWoWUnit> Hostiles { get; internal set; }

        public ulong StarTargetGuid => throw new NotImplementedException();

        public ulong CircleTargetGuid => throw new NotImplementedException();

        public ulong DiamondTargetGuid => throw new NotImplementedException();

        public ulong TriangleTargetGuid => throw new NotImplementedException();

        public ulong MoonTargetGuid => throw new NotImplementedException();

        public ulong SquareTargetGuid => throw new NotImplementedException();

        public ulong CrossTargetGuid => throw new NotImplementedException();

        public string GlueDialogText => throw new NotImplementedException();

        public LoginStates LoginState => throw new NotImplementedException();

        public readonly List<CharacterSelect> CharacterSelects = [];

        private ObjectManager()
        {
        }
        public void Initialize(ActivityMemberState parProbe)
        {
            WoWSharpEventEmitter.Instance.OnLoginConnect += Instance_OnLoginConnect;
            WoWSharpEventEmitter.Instance.OnLoginSuccess += Instance_OnLoginSuccess;
            WoWSharpEventEmitter.Instance.OnLoginFailure += Instance_OnLoginFail;
            WoWSharpEventEmitter.Instance.OnWorldSessionStart += Instance_OnWorldSessionStart;
            WoWSharpEventEmitter.Instance.OnWorldSessionEnd += Instance_OnWorldSessionEnd;
        }

        private void Instance_OnLoginConnect(object? sender, EventArgs e) => _isLoginConnected = true;
        private void Instance_OnLoginFail(object? sender, EventArgs e) => _isLoggedIn = false;
        private void Instance_OnLoginSuccess(object? sender, EventArgs e) => _isLoggedIn = true;
        private void Instance_OnWorldSessionStart(object? sender, EventArgs e) => _isWorldConnected = true;
        private void Instance_OnWorldSessionEnd(object? sender, EventArgs e)
        {
            _hasEnteredWorld = false;
            _isWorldConnected = false;
        }

        public void AntiAfk()
        {
            
        }

        public sbyte GetTalentRank(int tabIndex, int talentIndex)
        {
            return -1;
        }

        public void PickupInventoryItem(int inventorySlot)
        {
            
        }

        public void DeleteCursorItem()
        {
            
        }

        public void SendChatMessage(string chatMessage)
        {
            
        }

        public void SetRaidTarget(IWoWUnit target, TargetMarker v)
        {

        }

        public void Initialize(ActivityMemberState parProbe, IWoWEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public sbyte GetTalentRank(uint tabIndex, uint talentIndex)
        {
            throw new NotImplementedException();
        }

        public void PickupInventoryItem(uint inventorySlot)
        {
            throw new NotImplementedException();
        }

        public void EquipCursorItem()
        {
            throw new NotImplementedException();
        }

        public void ConfirmItemEquip()
        {
            throw new NotImplementedException();
        }

        public void AcceptGroupInvite()
        {
            throw new NotImplementedException();
        }

        public void LeaveGroup()
        {
            throw new NotImplementedException();
        }

        public void EnterWorld()
        {
            throw new NotImplementedException();
        }

        public void DefaultServerLogin(string accountName, string password)
        {
            throw new NotImplementedException();
        }

        public void ResetLogin()
        {
            throw new NotImplementedException();
        }

        public void JoinBattleGroundQueue()
        {
            throw new NotImplementedException();
        }

        public void ResetInstances()
        {
            throw new NotImplementedException();
        }

        public void PickupMacro(uint v)
        {
            throw new NotImplementedException();
        }

        public void PlaceAction(uint v)
        {
            throw new NotImplementedException();
        }

        public void ConvertToRaid()
        {
            throw new NotImplementedException();
        }

        public void InviteToGroup(string characterName)
        {
            throw new NotImplementedException();
        }

        public uint GetItemCount(uint itemId)
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetEquippedItem(EquipSlot ranged)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IWoWItem> GetEquippedItems()
        {
            throw new NotImplementedException();
        }

        public int CountFreeSlots(bool v)
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetItem(int v1, int v2)
        {
            throw new NotImplementedException();
        }

        public void UseContainerItem(int v1, int v2)
        {
            throw new NotImplementedException();
        }

        public uint GetBagId(ulong guid)
        {
            throw new NotImplementedException();
        }

        public uint GetSlotId(ulong guid)
        {
            throw new NotImplementedException();
        }

        public void PickupContainerItem(uint v1, uint v2)
        {
            throw new NotImplementedException();
        }
    }
}
