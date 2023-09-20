namespace BloogBot
{
    public class BotSettings
    {
        public string DatabaseType { get; set; }
        public string DatabasePath { get; set; }

        public bool DiscordBotEnabled { get; set; }

        public string DiscordBotToken { get; set; }

        public string DiscordGuildId { get; set; }

        public string DiscordRoleId { get; set; }

        public string DiscordChannelId { get; set; }

        public string CurrentBotName { get; set; }

        public bool UseVerboseLogging { get; set; }
    }
}
