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
    }
}
