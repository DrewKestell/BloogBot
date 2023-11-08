using Newtonsoft.Json;
using System.Reflection;
using System;
using System.IO;

namespace RaidMemberBot
{
    public class RaidMemberSettings
    {
        private static RaidMemberSettings _instance;
        public static RaidMemberSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string raidLeaderBotSettingsFilePath = Path.Combine(currentFolder, "Settings\\RaidMemberSettings.json");

                    _instance = JsonConvert.DeserializeObject<RaidMemberSettings>(File.ReadAllText(raidLeaderBotSettingsFilePath));
                }
                return _instance;
            }
        }
        public void SaveConfig()
        {
            try
            {
                string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string botSettingsFilePath = Path.Combine(currentFolder, "Settings\\RaidMemberSettings.json");
                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);

                File.WriteAllText(botSettingsFilePath, json);

                Console.WriteLine($"RaidMemberSettings: Config saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public bool DiscordBotEnabled { get; set; }
        public string DiscordBotToken { get; set; }
        public string DiscordGuildId { get; set; }
        public string DiscordRoleId { get; set; }
        public string DiscordChannelId { get; set; }
        public bool UseVerboseLogging { get; set; }
        public string ListenAddress { get; set; }
        public int ConfigServerPort { get; set; }
    }
}
