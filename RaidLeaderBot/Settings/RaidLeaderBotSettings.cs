using Newtonsoft.Json;
using RaidLeaderBot.UI.Views.Talents;
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
        public List<int> Spells
        {
            get
            {
                var list = new List<int>();

                switch(Class)
                {
                    case Class.Druid:
                        list.Add(1126);
                        list.Add(5176);
                        list.Add(5185);

                        if (Level > 3)
                        {
                            list.Add(8921);
                            list.Add(774);

                            if (Level > 5)
                            {
                                list.Add(476);
                                list.Add(5177);

                                if (Level > 7)
                                {
                                    list.Add(339);
                                    list.Add(5186);

                                    if (Level > 9)
                                    {
                                        list.Add(5487);
                                        list.Add(18960);
                                        list.Add(6795);
                                        list.Add(6807);
                                        list.Add(8924);
                                        list.Add(99);
                                        list.Add(1058);
                                        list.Add(5232);

                                        if (Level > 11)
                                        {
                                            list.Add(5229);
                                            list.Add(8936);

                                            if (Level > 13)
                                            {
                                                list.Add(5178);
                                                list.Add(8946);
                                                list.Add(5187);
                                                list.Add(5211);
                                                list.Add(782);
                                                list.Add(8936);

                                                if (Level > 15)
                                                {
                                                    list.Add(1066);
                                                    list.Add(1430);
                                                    list.Add(8925);
                                                    list.Add(779);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(1062);
                                                        list.Add(2637);
                                                        list.Add(770);
                                                        list.Add(1062);
                                                        list.Add(6808);
                                                        list.Add(8938);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(768);
                                                            list.Add(1079);
                                                            list.Add(20484);
                                                            list.Add(1082);
                                                            list.Add(1735);
                                                            list.Add(5188);
                                                            list.Add(5215);
                                                            list.Add(6756);
                                                            list.Add(2912);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(5179);
                                                                list.Add(2908);
                                                                list.Add(8926);
                                                                list.Add(2090);
                                                                list.Add(5221);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(8939);
                                                                    list.Add(2782);
                                                                    list.Add(1075);
                                                                    list.Add(780);
                                                                    list.Add(1822);
                                                                    list.Add(5217);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(1850);
                                                                        list.Add(5189);
                                                                        list.Add(8949);
                                                                        list.Add(6809);
                                                                        list.Add(2893);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(2091);
                                                                            list.Add(8998);
                                                                            list.Add(3029);
                                                                            list.Add(5159);
                                                                            list.Add(5209);
                                                                            list.Add(9492);
                                                                            list.Add(8927);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(783);
                                                                                list.Add(740);
                                                                                list.Add(6800);
                                                                                list.Add(8940);
                                                                                list.Add(778);
                                                                                list.Add(6798);
                                                                                list.Add(5234);
                                                                                list.Add(5180);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(9490);
                                                                                    list.Add(22568);
                                                                                    list.Add(5225);
                                                                                    list.Add(6778);
                                                                                    list.Add(6785);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(769);
                                                                                        list.Add(1823);
                                                                                        list.Add(3627);
                                                                                        list.Add(8914);
                                                                                        list.Add(8972);
                                                                                        list.Add(8928);
                                                                                        list.Add(8950);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(9005);
                                                                                            list.Add(9493);
                                                                                            list.Add(6793);
                                                                                            list.Add(8941);
                                                                                            list.Add(22842);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(5196);
                                                                                                list.Add(8955);
                                                                                                list.Add(18657);
                                                                                                list.Add(8903);
                                                                                                list.Add(8992);
                                                                                                list.Add(5201);
                                                                                                list.Add(6780);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(9634);
                                                                                                    list.Add(29166);
                                                                                                    list.Add(8918);
                                                                                                    list.Add(8929);
                                                                                                    list.Add(20719);
                                                                                                    list.Add(6783);
                                                                                                    list.Add(8907);
                                                                                                    list.Add(8910);
                                                                                                    list.Add(9000);
                                                                                                    list.Add(20742);
                                                                                                    list.Add(22827);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(9747);
                                                                                                        list.Add(6787);
                                                                                                        list.Add(8951);
                                                                                                        list.Add(9745);
                                                                                                        list.Add(9750);
                                                                                                        list.Add(9749);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(9758);
                                                                                                            list.Add(22812);
                                                                                                            list.Add(9754);
                                                                                                            list.Add(9752);
                                                                                                            list.Add(9756);
                                                                                                            list.Add(1824);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(8983);
                                                                                                                list.Add(9821);
                                                                                                                list.Add(9823);
                                                                                                                list.Add(9829);
                                                                                                                list.Add(22895);
                                                                                                                list.Add(8905);
                                                                                                                list.Add(9833);
                                                                                                                list.Add(9839);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(22828);
                                                                                                                    list.Add(9849);
                                                                                                                    list.Add(9856);
                                                                                                                    list.Add(9852);
                                                                                                                    list.Add(9845);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(33876);
                                                                                                                        list.Add(9884);
                                                                                                                        list.Add(33878);
                                                                                                                        list.Add(9880);
                                                                                                                        list.Add(9862);
                                                                                                                        list.Add(9866);
                                                                                                                        list.Add(9875);
                                                                                                                        list.Add(17401);
                                                                                                                        list.Add(20747);
                                                                                                                        list.Add(9888);
                                                                                                                        list.Add(21849);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(9840);
                                                                                                                            list.Add(9834);
                                                                                                                            list.Add(9898);
                                                                                                                            list.Add(9892);
                                                                                                                            list.Add(9894);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(9910);
                                                                                                                                list.Add(9912);
                                                                                                                                list.Add(9904);
                                                                                                                                list.Add(9830);
                                                                                                                                list.Add(9901);
                                                                                                                                list.Add(9907);
                                                                                                                                list.Add(9908);
                                                                                                                                list.Add(9857);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(22829);
                                                                                                                                    list.Add(22896);
                                                                                                                                    list.Add(9889);
                                                                                                                                    list.Add(9827);
                                                                                                                                    list.Add(9845);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(9835);
                                                                                                                                        list.Add(9853);
                                                                                                                                        list.Add(33982);
                                                                                                                                        list.Add(9867);
                                                                                                                                        list.Add(9841);
                                                                                                                                        list.Add(9850);
                                                                                                                                        list.Add(9876);
                                                                                                                                        list.Add(9881);
                                                                                                                                        list.Add(18658);
                                                                                                                                        list.Add(33986);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(21850);
                                                                                                                                            list.Add(9896);
                                                                                                                                            list.Add(9846);
                                                                                                                                            list.Add(9858);
                                                                                                                                            list.Add(9863);
                                                                                                                                            list.Add(9913);
                                                                                                                                            list.Add(20748);
                                                                                                                                            list.Add(25298);
                                                                                                                                            list.Add(25297);
                                                                                                                                            list.Add(25299);
                                                                                                                                            list.Add(31018);
                                                                                                                                            list.Add(9885);
                                                                                                                                            list.Add(17402);
                                                                                                                                            list.Add(31709);
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Hunter:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Mage:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Paladin:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Priest:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Rogue:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Shaman:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Warlock:
                        if (Level > 3)
                        {
                            if (Level > 5)
                            {
                                if (Level > 7)
                                {
                                    if (Level > 9)
                                    {
                                        if (Level > 11)
                                        {
                                            if (Level > 13)
                                            {
                                                if (Level > 15)
                                                {
                                                    if (Level > 17)
                                                    {
                                                        if (Level > 19)
                                                        {
                                                            if (Level > 21)
                                                            {
                                                                if (Level > 23)
                                                                {
                                                                    if (Level > 25)
                                                                    {
                                                                        if (Level > 27)
                                                                        {
                                                                            if (Level > 29)
                                                                            {
                                                                                if (Level > 31)
                                                                                {
                                                                                    if (Level > 33)
                                                                                    {
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        if (Level > 59)
                                                                                                                                        {

                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Class.Warrior:
                        list.Add(78);
                        list.Add(2457);
                        list.Add(6673);

                        if (Level > 3)
                        {
                            list.Add(100);
                            list.Add(772);

                            if (Level > 5)
                            {
                                list.Add(6343);

                                if (Level > 7)
                                {
                                    list.Add(284);
                                    list.Add(1715);

                                    if (Level > 9)
                                    {
                                        list.Add(71);
                                        list.Add(355);
                                        list.Add(7386);
                                        list.Add(6546);
                                        list.Add(2687);

                                        if (Level > 11)
                                        {
                                            list.Add(7384);
                                            list.Add(5242);
                                            list.Add(72);

                                            if (Level > 13)
                                            {
                                                list.Add(6572);
                                                list.Add(1160);

                                                if (Level > 15)
                                                {
                                                    list.Add(285);
                                                    list.Add(694);
                                                    list.Add(2565);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(8198);
                                                        list.Add(676);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(845);
                                                            list.Add(12678);
                                                            list.Add(20230);
                                                            list.Add(6547);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(5246);
                                                                list.Add(7405);
                                                                list.Add(6192);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(5308);
                                                                    list.Add(6574);
                                                                    list.Add(1608);
                                                                    list.Add(6190);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(7400);
                                                                        list.Add(1161);
                                                                        list.Add(6178);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(7887);
                                                                            list.Add(8204);
                                                                            list.Add(871);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(2458);
                                                                                list.Add(1464);
                                                                                list.Add(6548);
                                                                                list.Add(7369);
                                                                                list.Add(20252);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(18449);
                                                                                    list.Add(11549);
                                                                                    list.Add(1671);
                                                                                    list.Add(7372);
                                                                                    list.Add(11564);
                                                                                    list.Add(20658);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(8380);
                                                                                        list.Add(7379);
                                                                                        list.Add(11554);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(1680);
                                                                                            list.Add(7402);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(8205);
                                                                                                list.Add(6552);
                                                                                                list.Add(8820);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(11572);
                                                                                                    list.Add(20660);
                                                                                                    list.Add(11565);
                                                                                                    list.Add(11608);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(20616);
                                                                                                        list.Add(11550);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(11584);
                                                                                                            list.Add(11600);
                                                                                                            list.Add(11555);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(11596);
                                                                                                                list.Add(11578);
                                                                                                                list.Add(11604);
                                                                                                                list.Add(20559);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(11580);
                                                                                                                    list.Add(11566);
                                                                                                                    list.Add(20661);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(1719);
                                                                                                                        list.Add(11573);
                                                                                                                        list.Add(11609);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(20617);
                                                                                                                            list.Add(1672);
                                                                                                                            list.Add(11551);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(11601);
                                                                                                                                list.Add(7373);
                                                                                                                                list.Add(11556);
                                                                                                                                list.Add(11605);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(11567);
                                                                                                                                    list.Add(20662);
                                                                                                                                    list.Add(20560);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(11597);
                                                                                                                                        list.Add(6554);
                                                                                                                                        list.Add(11581);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(25288);
                                                                                                                                            list.Add(11585);
                                                                                                                                            list.Add(25286);
                                                                                                                                            list.Add(11574);
                                                                                                                                            list.Add(20569);
                                                                                                                                            list.Add(25289);
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }

                return list;
            }
        }
    }
}
