using System.Collections.Concurrent;
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

        public WoWLocalPlayer Player { get; internal set; }
        public uint MapId { get; internal set; }

        public ulong PartyLeaderGuid { get; internal set; }
        public ulong SkullTargetGuid { get; internal set; }
        public WoWUnit PartyLeader { get; internal set; }

        public IList<WoWObject> Objects = new List<WoWObject>();
        public IList<WoWObject> ObjectsBuffer = new List<WoWObject>();
        public IEnumerable<WoWUnit> Units => Objects.OfType<WoWUnit>();
        public IEnumerable<WoWUnit> PartyMembers => Objects.OfType<WoWUnit>();
        public IEnumerable<WoWUnit> Aggressors => Objects.OfType<WoWUnit>();
        public IEnumerable<WoWUnit> Hostiles => Objects.OfType<WoWUnit>();
        public IEnumerable<WoWItem> Items => Objects.OfType<WoWItem>();
        public List<Spell> Spells { get; internal set; }
        public List<Cooldown> Cooldowns { get; internal set; }

        public readonly List<CharacterSelect> CharacterSelects = [];

        private ObjectManager()
        {
        }

        //public void AddOrUpdateObject(WoWObject obj)
        //{
        //    if (obj == null || obj.Guid == 0)
        //        throw new ArgumentNullException(nameof(obj));

        //    Objects.AddOrUpdate(obj.Guid, obj, (key, oldValue) => obj);
        //}
        //public WoWObject GetObject(ulong guid)
        //{
        //    if (guid == 0)
        //        throw new ArgumentNullException(nameof(guid));

        //    Objects.TryGetValue(guid, out var obj);
        //    return obj;
        //}

        //public void RemoveObject(ulong guid)
        //{
        //    Objects.TryRemove(guid, out var obj);
        //}

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
