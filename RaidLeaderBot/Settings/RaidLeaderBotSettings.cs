using System.Collections.Generic;

namespace RaidLeaderBot
{
    public class RaidLeaderBotSettings
    {
        public string PathToWoW { get; set; }
        public int Port { get; set; }
        public string ListenAddress { get; set; }
        public string Activity { get; set; }
        public bool ShouldRaid { get; set; }
        public Dictionary<string, List<List<RaidMemberPreset>>> ActivityPresets { get; set; } = new Dictionary<string, List<List<RaidMemberPreset>>>();
    }
    public class RaidMemberPreset
    {
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; } = 1;
        public string BotProfileName { get; set; }
    }
}
