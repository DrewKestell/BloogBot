using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static RaidMemberBot.Constants.Enums;

namespace RaidLeaderBot
{
    public class RaidLeaderBotSettings
    {
        private static RaidLeaderBotSettings _instance;
        public static RaidLeaderBotSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string raidLeaderBotSettingsFilePath = Path.Combine(currentFolder, "Settings\\RaidLeaderBotSettings.json");

                    _instance = JsonConvert.DeserializeObject<RaidLeaderBotSettings>(File.ReadAllText(raidLeaderBotSettingsFilePath));
                }
                return _instance;
            }
        }
        public void SaveConfig()
        {
            try
            {
                string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string botSettingsFilePath = Path.Combine(currentFolder, "Settings\\RaidLeaderBotSettings.json");
                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);

                File.WriteAllText(botSettingsFilePath, json);

                Console.WriteLine($"RaidLeaderBotSettings: Config saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private RaidLeaderBotSettings() { }
        public string PathToWoW { get; set; }
        public int ConfigServerPort { get; set; }
        public int DatabasePort { get; set; }
        public int NavigationPort { get; set; }
        public string ListenAddress { get; set; }
        public List<List<RaidPreset>> ActivityPresets { get; set; }
    }
    public class RaidPreset
    {
        public int RaidLeaderPort { get; set; }
        public bool IsAlliance { get; set; }
        public ActivityType Activity { get; set; }
        public List<RaidMemberPreset> RaidMemberPresets { get; set; }
    }
    public class RaidMemberPreset
    {
        public int Level { get; set; } = 1;
        public Race Race { get; set; }
        public Class Class { get; set; } = Class.Warrior;
        public bool IsMainTank { get; set; }
        public bool IsOffTank { get; set; }
        public bool IsMainHealer { get; set; }
        public bool IsOffHealer { get; set; }
        public bool ShouldCleanse { get; set; }
        public bool ShouldRebuff { get; set; }
        public bool IsRole1 { get; set; }
        public bool IsRole2 { get; set; }
        public bool IsRole3 { get; set; }
        public bool IsRole4 { get; set; }
        public bool IsRole5 { get; set; }
        public bool IsRole6 { get; set; }
        public List<int> Talents { get; set; } = new List<int>();
        public Dictionary<InventoryType, int> EquipmentItems { get; set; } = new Dictionary<InventoryType, int>();
        public Dictionary<InventoryType, int> EquipmentEnchants { get; set; } = new Dictionary<InventoryType, int>();
    }
}
