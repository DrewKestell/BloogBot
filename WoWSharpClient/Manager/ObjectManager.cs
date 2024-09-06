using BotRunner.Base;
using BotRunner.Models;
using WoWSharpClient.Models;

namespace WoWSharpClient.Manager
{
    public class ObjectManager : BaseObjectManager
    {
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

        public IEnumerable<Models.Object> ObjectsBuffer = [];
        public List<Spell> Spells { get; internal set; }
        public List<Cooldown> Cooldowns { get; internal set; }

        public readonly List<CharacterSelect> CharacterSelects = [];

        public ObjectManager(WoWSharpEventEmitter woWSharpEventEmitter, ActivityMemberState parProbe) : base(woWSharpEventEmitter, parProbe)
        {
            woWSharpEventEmitter.OnLoginConnect += Instance_OnLoginConnect;
            woWSharpEventEmitter.OnLoginSuccess += Instance_OnLoginSuccess;
            woWSharpEventEmitter.OnLoginFailure += Instance_OnLoginFail;
            woWSharpEventEmitter.OnWorldSessionStart += Instance_OnWorldSessionStart;
            woWSharpEventEmitter.OnWorldSessionEnd += Instance_OnWorldSessionEnd;
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

        public override void AcceptGroupInvite()
        {
            throw new NotImplementedException();
        }

        public override void AntiAfk()
        {
            throw new NotImplementedException();
        }

        public override void ConfirmItemEquip()
        {
            throw new NotImplementedException();
        }

        public override void ConvertToRaid()
        {
            throw new NotImplementedException();
        }

        public override int CountFreeSlots(bool v)
        {
            throw new NotImplementedException();
        }

        public override void DefaultServerLogin(string accountName, string password)
        {
            throw new NotImplementedException();
        }

        public override void DeleteCursorItem()
        {
            throw new NotImplementedException();
        }

        public override void EnterWorld()
        {
            throw new NotImplementedException();
        }

        public override void EquipCursorItem()
        {
            throw new NotImplementedException();
        }

        public override uint GetBagId(ulong guid)
        {
            throw new NotImplementedException();
        }
    }
}
