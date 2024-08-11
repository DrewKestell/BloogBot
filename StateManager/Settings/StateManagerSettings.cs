using System.Reflection;
using Communication;
using Newtonsoft.Json;

namespace StateManager.Settings
{
    public class StateManagerSettings
    {
        private static StateManagerSettings _instance;
        public static StateManagerSettings Instance
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
            string WorldStateManagerSettingsFilePath = Path.Combine(currentFolder, "Settings\\StateManagerSettings.json");

            _instance = JsonConvert.DeserializeObject<StateManagerSettings>(File.ReadAllText(WorldStateManagerSettingsFilePath));
        }

        public void SaveConfig()
        {
            try
            {
                string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string botSettingsFilePath = Path.Combine(currentFolder, "Settings\\StateManagerSettings.json");
                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);

                File.WriteAllText(botSettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private StateManagerSettings() { }
        public List<ActivityMember> ActivityMemberPresets { get; set; } = [];
    }
}
