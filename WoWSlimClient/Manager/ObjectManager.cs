using WoWSlimClient.Handlers;
using WoWSlimClient.Models;

namespace WoWSlimClient.Manager
{
    public class ObjectManager
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

        public List<WoWObject> Objects { get; internal set; } = new List<WoWObject>();
        public WoWLocalPlayer Player { get; internal set; }
        public uint MapId { get; internal set; }
        public IEnumerable<WoWUnit> Units => Objects.OfType<WoWUnit>();

        public IEnumerable<WoWUnit> PartyMembers { get; internal set; } = new List<WoWUnit>();
        public IEnumerable<WoWUnit> Aggressors { get; internal set; } = new List<WoWUnit>();
        public ulong PartyLeaderGuid { get; internal set; }
        public ulong SkullTargetGuid { get; internal set; }
        public IEnumerable<WoWItem> Items { get; internal set; } = new List<WoWItem>();
        public IEnumerable<WoWUnit> Hostiles { get; internal set; } = new List<WoWUnit>();
        public WoWUnit PartyLeader { get; internal set; }
        public List<Spell> Spells { get; internal set; }
        public List<Cooldown> Cooldowns { get; internal set; }

        public readonly List<CharacterSelect> CharacterSelects = new List<CharacterSelect>();

        private ObjectManager()
        {
        }

        public void AddOrUpdateObject(WoWObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var existingObj = Objects.FirstOrDefault(o => o.Guid == obj.Guid);
            if (existingObj != null)
            {
                // Update existing object
                int index = Objects.IndexOf(existingObj);
                Objects[index] = obj;
            }
            else
            {
                // Add new object
                Objects.Add(obj);
            }
        }

        public void RemoveObject(ulong guid)
        {
            var obj = Objects.FirstOrDefault(o => o.Guid == guid);
            if (obj != null)
            {
                Objects.Remove(obj);
            }
        }

        public void Initialize()
        {
            WoWEventHandler.Instance.OnLoginConnect += Instance_OnLoginConnect;
            WoWEventHandler.Instance.OnLoginSuccess += Instance_OnLoginSuccess;
            WoWEventHandler.Instance.OnLoginFailure += Instance_OnLoginFail;
            WoWEventHandler.Instance.OnWorldSessionStart += Instance_OnWorldSessionStart;
            WoWEventHandler.Instance.OnWorldSessionEnd += Instance_OnWorldSessionEnd;
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
    }
}
