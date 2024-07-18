using WoWSlimClient.Client;
using WoWSlimClient.Models;

namespace WoWSlimClient.Manager
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();


        public bool IsLoginConnected { get; set; }
        public bool IsWorldConnected { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool HasEnteredWorld { get; set; }
        public Realm CurrentRealm { get; set; }
        public bool HasRealmSelected => CurrentRealm != null;

        public readonly List<CharacterSelect> CharacterSelects = [];

        private ObjectManager()
        {
        }

        public void Initialize()
        {
            OpCodeDispatcher.Instance.OnDisconnect += Instance_OnDisconnect;
            OpCodeDispatcher.Instance.OnWorldSessionStart += OnWorldSessionStart;
            OpCodeDispatcher.Instance.OnWorldSessionEnd += OnWorldSessionEnd;
        }

        private void Instance_OnDisconnect(object? sender, EventArgs e)
        {
            
        }

        private void OnWorldSessionStart(object? sender, EventArgs e)
        {

        }
        private void OnWorldSessionEnd(object? sender, EventArgs e)
        {
            
        }
    }
}
