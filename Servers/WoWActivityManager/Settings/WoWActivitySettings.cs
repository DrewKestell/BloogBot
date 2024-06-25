using System.Reflection;
using Newtonsoft.Json;

namespace WoWClientBot
{
    public class WoWActivitySettings
    {
        private static WoWActivitySettings _instance;
        public static WoWActivitySettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string activityMemberBotSettingsFilePath = Path.Combine(currentFolder, "Settings\\WoWActivitySettings.json");

                    _instance = JsonConvert.DeserializeObject<WoWActivitySettings>(File.ReadAllText(activityMemberBotSettingsFilePath));
                }
                return _instance;
            }
        }
        public void SaveConfig()
        {
            try
            {
                string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string botSettingsFilePath = Path.Combine(currentFolder, "Settings\\WoWActivitySettings.json");
                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);

                File.WriteAllText(botSettingsFilePath, json);

                Console.WriteLine($"WoWActivitySettings: Config saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
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
