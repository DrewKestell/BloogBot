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

        public List<WoWObject> Objects { get; internal set; }
        public WoWLocalPlayer Player { get; internal set; }
        public uint MapId { get; internal set; }
        public IEnumerable<WoWUnit> Units => Objects.OfType<WoWUnit>();

        public IEnumerable<WoWUnit> PartyMembers { get; internal set; }
        public IEnumerable<WoWUnit> Aggressors { get; internal set; }
        public ulong PartyLeaderGuid { get; internal set; }
        public ulong SkullTargetGuid { get; internal set; }
        public IEnumerable<WoWItem> Items { get; internal set; }
        public IEnumerable<WoWUnit> Hostiles { get; internal set; }
        public WoWUnit PartyLeader { get; internal set; }

        public readonly List<CharacterSelect> CharacterSelects = [];

        private ObjectManager()
        {
        }
    }
}
