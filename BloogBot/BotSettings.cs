namespace BloogBot
{
    public class BotSettings
    {
        public string DatabaseType { get; set; }

        public string DatabasePath { get; set; }

        public string Player1PreferredRace { get; set; }

        public string Player1PreferredClass { get; set; }

        public bool DiscordBotEnabled { get; set; }

        public string DiscordBotToken { get; set; }

        public string DiscordGuildId { get; set; }

        public string DiscordRoleId { get; set; }

        public string DiscordChannelId { get; set; }

        public bool UseVerboseLogging { get; set; }
    }
}
