using Newtonsoft.Json;
using System.Reflection;
using WoWStateManager.Models;

namespace WoWStateManager
{
    public class WoWStateManagerSettings
    {
        private static WoWStateManagerSettings _instance;
        public static WoWStateManagerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadConfig();
                }
                return _instance;
            }
        }

        private static void LoadConfig()
        {
            string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string WorldStateManagerSettingsFilePath = Path.Combine(currentFolder, "Settings\\WoWStateManagerSettings.json");

            _instance = JsonConvert.DeserializeObject<WoWStateManagerSettings>(File.ReadAllText(WorldStateManagerSettingsFilePath));
        }

        public void SaveConfig()
        {
            try
            {
                string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string botSettingsFilePath = Path.Combine(currentFolder, "Settings\\WoWStateManagerSettings.json");
                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);

                File.WriteAllText(botSettingsFilePath, json);

                Console.WriteLine("WoWStateManagerSettings: Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private WoWStateManagerSettings() { }
        public List<ActivityPreset> ActivityPresets { get; set; } = [];
    } 
}
