using System.Collections.Generic;

namespace Bootstrapper
{
    public class BootstrapperSettings
    {
        public string PathToWoW { get; set; }
        public int Port { get; set; }
        public string ListenAddress { get; set; }
        public string Activity { get; set; }
        public bool ShouldParty { get; set; }
        public Dictionary<string, List<List<PartyMemberPreset>>> ActivityPresets { get; set; } = new Dictionary<string, List<List<PartyMemberPreset>>>();
    }
    public class PartyMemberPreset
    {
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; } = 1;
        public string BotProfileName { get; set; }
    }
}
