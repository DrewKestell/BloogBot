using BloogBot.Models;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Bootstrapper
{
    public class BootstrapperSettings
    {
        public string PathToWoW { get; set; }
        public int Port { get; set; }
        public string ListenAddress { get; set; }
        public string Activity { get; set; }
        public bool ShouldParty { get; set; }
        public string Dungeon { get; set; }
        public int DungeonPartySize { get; set; }
        public string PvPSelection { get; set; }
        public List<PartyMemberPreference> PartyPreferences { get; set; }
    }
}
