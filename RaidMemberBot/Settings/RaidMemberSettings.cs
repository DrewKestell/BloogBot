namespace RaidMemberBot
{
    public class RaidMemberSettings
    {

        public bool DiscordBotEnabled { get; set; }

        public string DiscordBotToken { get; set; }

        public string DiscordGuildId { get; set; }

        public string DiscordRoleId { get; set; }

        public string DiscordChannelId { get; set; }

        public bool UseVerboseLogging { get; set; }

        public string ListenAddress { get; set; }

        public int CommandPort { get; set; }
        public int DatabasePort { get; set; }
        public int PathfindingPort { get; set; }
    }
}
