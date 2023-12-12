using Newtonsoft.Json;
using RaidLeaderBot.UI.Views.Talents;
using RaidMemberBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public int HeadItem { get; set; }
        public int NeckItem { get; set; }
        public int ShoulderItem { get; set; }
        public int ChestItem { get; set; }
        public int BackItem { get; set; }
        public int ShirtItem { get; set; }
        public int RobeItem { get; set; }
        public int WristsItem { get; set; }
        public int HandsItem { get; set; }
        public int WaistItem { get; set; }
        public int LegsItem { get; set; }
        public int FeetItem { get; set; }
        public int Finger1Item { get; set; }
        public int Finger2Item { get; set; }
        public int Trinket1Item { get; set; }
        public int Trinket2Item { get; set; }
        public int MainHandItem { get; set; }
        public int OffHandItem { get; set; }
        public int RangedItem { get; set; }
        [JsonIgnore]
        public List<int> Skills
        {
            get
            {
                var list = new List<int>();

                switch(Class)
                {
                    case Class.Druid:
                        list.Add(160);
                        list.Add(54);
                        list.Add(473);
                        list.Add(173);
                        list.Add(136);
                        break;
                    case Class.Hunter:
                        list.Add(176);
                        list.Add(46);
                        list.Add(226);
                        list.Add(45);
                        list.Add(55);
                        list.Add(172);
                        list.Add(44);
                        list.Add(43);
                        list.Add(173);
                        list.Add(136);

                        if (Level > 19)
                        {
                            list.Add(229);
                        }
                        break;
                    case Class.Mage:
                        list.Add(228);
                        list.Add(43);
                        list.Add(173);
                        list.Add(136);
                        break;
                    case Class.Paladin:
                        list.Add(55);
                        list.Add(172);
                        list.Add(44);
                        list.Add(43);
                        list.Add(160);
                        list.Add(54);

                        if (Level > 19)
                        {
                            list.Add(229);
                        }
                        break;
                    case Class.Priest:
                        list.Add(228);
                        list.Add(54);
                        list.Add(173);
                        list.Add(136);
                        break;
                    case Class.Rogue:
                        list.Add(176);
                        list.Add(46);
                        list.Add(226);
                        list.Add(45);
                        list.Add(43);
                        list.Add(54);
                        list.Add(173);
                        break;
                    case Class.Shaman:
                        list.Add(44);
                        list.Add(54);
                        list.Add(136);
                        list.Add(173);
                        break;
                    case Class.Warlock:
                        list.Add(228);
                        list.Add(43);
                        list.Add(136);
                        list.Add(173);
                        break;
                    case Class.Warrior:
                        list.Add(176);
                        list.Add(46);
                        list.Add(226);
                        list.Add(45);
                        list.Add(55);
                        list.Add(172);
                        list.Add(44);
                        list.Add(43);
                        list.Add(160);
                        list.Add(54);
                        list.Add(136);
                        list.Add(173);

                        if (Level > 19)
                        {
                            list.Add(229);
                        }
                        break;
                }

                return list;
            }
        }
        [JsonIgnore]
        public List<int> Spells
        {
            get
            {
                var list = Talents.ToList();

                switch (Class)
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

                                                        list.Remove(6807);

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

                                                                    list.Remove(779);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(1850);
                                                                        list.Add(5189);
                                                                        list.Add(8949);
                                                                        list.Add(6809);
                                                                        list.Add(2893);

                                                                        list.Remove(6808);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(2091);
                                                                            list.Add(8998);
                                                                            list.Add(3029);
                                                                            list.Add(5159);
                                                                            list.Add(5209);
                                                                            list.Add(9492);
                                                                            list.Add(8927);

                                                                            list.Remove(1082);
                                                                            list.Remove(1079);

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

                                                                                list.Remove(5221);
                                                                                list.Remove(5211);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(9490);
                                                                                    list.Add(22568);
                                                                                    list.Add(5225);
                                                                                    list.Add(6778);
                                                                                    list.Add(6785);

                                                                                    list.Remove(1735);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(769);
                                                                                        list.Add(1823);
                                                                                        list.Add(3627);
                                                                                        list.Add(8914);
                                                                                        list.Add(8972);
                                                                                        list.Add(8928);
                                                                                        list.Add(8950);

                                                                                        list.Remove(780);
                                                                                        list.Remove(1822);
                                                                                        list.Remove(6809);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(9005);
                                                                                            list.Add(9493);
                                                                                            list.Add(6793);
                                                                                            list.Add(8941);
                                                                                            list.Add(22842);

                                                                                            list.Remove(5217);
                                                                                            list.Remove(9492);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(5196);
                                                                                                list.Add(8955);
                                                                                                list.Add(18657);
                                                                                                list.Add(8903);
                                                                                                list.Add(8992);
                                                                                                list.Add(5201);
                                                                                                list.Add(6780);

                                                                                                list.Remove(6800);
                                                                                                list.Remove(1823);

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

                                                                                                    list.Remove(22568);
                                                                                                    list.Remove(8998);
                                                                                                    list.Remove(5215);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(9747);
                                                                                                        list.Add(6787);
                                                                                                        list.Add(8951);
                                                                                                        list.Add(9745);
                                                                                                        list.Add(9750);
                                                                                                        list.Add(9749);

                                                                                                        list.Remove(8972);
                                                                                                        list.Remove(9490);
                                                                                                        list.Remove(6785);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(9758);
                                                                                                            list.Add(22812);
                                                                                                            list.Add(9754);
                                                                                                            list.Add(9752);
                                                                                                            list.Add(9756);
                                                                                                            list.Add(1824);

                                                                                                            list.Remove(9493);
                                                                                                            list.Remove(1823);
                                                                                                            list.Remove(769);

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

                                                                                                                list.Remove(8992);
                                                                                                                list.Remove(9005);
                                                                                                                list.Remove(22842);
                                                                                                                list.Remove(6798);
                                                                                                                list.Remove(1850);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(22828);
                                                                                                                    list.Add(9849);
                                                                                                                    list.Add(9856);
                                                                                                                    list.Add(9852);
                                                                                                                    list.Add(9845);

                                                                                                                    list.Remove(22827);
                                                                                                                    list.Remove(5201);
                                                                                                                    list.Remove(6793);

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

                                                                                                                        list.Remove(9745);
                                                                                                                        list.Remove(6787);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(9840);
                                                                                                                            list.Add(9834);
                                                                                                                            list.Add(9898);
                                                                                                                            list.Add(9892);
                                                                                                                            list.Add(9894);

                                                                                                                            list.Remove(9747);
                                                                                                                            list.Remove(9000);
                                                                                                                            list.Remove(9752);

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

                                                                                                                                list.Remove(1824);
                                                                                                                                list.Remove(9829);
                                                                                                                                list.Remove(9754);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(22829);
                                                                                                                                    list.Add(22896);
                                                                                                                                    list.Add(9889);
                                                                                                                                    list.Add(9827);
                                                                                                                                    list.Add(9845);

                                                                                                                                    list.Remove(22828);
                                                                                                                                    list.Remove(22895);
                                                                                                                                    list.Remove(9823);

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

                                                                                                                                        list.Remove(33876);
                                                                                                                                        list.Remove(33878);
                                                                                                                                        list.Remove(9866);
                                                                                                                                        list.Remove(9880);
                                                                                                                                        list.Remove(9849);

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

                                                                                                                                            list.Remove(33876);
                                                                                                                                            list.Remove(33878);
                                                                                                                                            list.Remove(9866);
                                                                                                                                            list.Remove(9880);
                                                                                                                                            list.Remove(9845);
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
                        list.Add(75);
                        list.Add(2973);
                        list.Add(1494);

                        if (Level > 3)
                        {
                            list.Add(1978);
                            list.Add(13163);

                            if (Level > 5)
                            {
                                list.Add(3044);
                                list.Add(1130);

                                if (Level > 7)
                                {
                                    list.Add(14260);
                                    list.Add(5116);

                                    if (Level > 9)
                                    {
                                        list.Add(6991);
                                        list.Add(1515);
                                        list.Add(2641);
                                        list.Add(4195);
                                        list.Add(19883);
                                        list.Add(24547);
                                        list.Add(883);
                                        list.Add(982);
                                        list.Add(13165);
                                        list.Add(13549);

                                        if (Level > 11)
                                        {
                                            list.Add(20736);
                                            list.Add(14281);
                                            list.Add(136);
                                            list.Add(2974);
                                            list.Add(4196);
                                            list.Add(24556);

                                            list.Remove(4195);
                                            list.Remove(24547);

                                            if (Level > 13)
                                            {
                                                list.Add(1513);
                                                list.Add(6197);
                                                list.Add(1002);

                                                if (Level > 15)
                                                {
                                                    list.Add(13795);
                                                    list.Add(14261);
                                                    list.Add(1495);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(19884);
                                                        list.Add(2643);
                                                        list.Add(4197);
                                                        list.Add(14318);
                                                        list.Add(24557);
                                                        list.Add(13550);

                                                        list.Remove(4196);
                                                        list.Remove(24556);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(5118);
                                                            list.Add(14274);
                                                            list.Add(14923);
                                                            list.Add(781);
                                                            list.Add(14282);
                                                            list.Add(24475);
                                                            list.Add(3111);
                                                            list.Add(24490);
                                                            list.Add(24494);
                                                            list.Add(24495);
                                                            list.Add(1499);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(3043);
                                                                list.Add(14323);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(19885);
                                                                    list.Add(1462);
                                                                    list.Add(14262);
                                                                    list.Add(4198);
                                                                    list.Add(24558);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(3045);
                                                                        list.Add(13551);
                                                                        list.Add(14302);
                                                                        list.Add(19880);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(13809);
                                                                            list.Add(3661);
                                                                            list.Add(14319);
                                                                            list.Add(14283);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(5384);
                                                                                list.Add(14288);
                                                                                list.Add(13161);
                                                                                list.Add(14269);
                                                                                list.Add(4199);
                                                                                list.Add(14326);
                                                                                list.Add(24476);
                                                                                list.Add(24511);
                                                                                list.Add(14924);
                                                                                list.Add(24441);
                                                                                list.Add(24514);
                                                                                list.Add(24559);
                                                                                list.Add(15629);
                                                                                list.Add(24508);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(19878);
                                                                                    list.Add(1543);
                                                                                    list.Add(14263);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(13813);
                                                                                        list.Add(13552);
                                                                                        list.Add(14272);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(3034);
                                                                                            list.Add(14284);
                                                                                            list.Add(3662);
                                                                                            list.Add(4200);
                                                                                            list.Add(24560);
                                                                                            list.Add(14303);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(14267);
                                                                                                list.Add(14320);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(13159);
                                                                                                    list.Add(13424);
                                                                                                    list.Add(1510);
                                                                                                    list.Add(19882);
                                                                                                    list.Add(14310);
                                                                                                    list.Add(24509);
                                                                                                    list.Add(24512);
                                                                                                    list.Add(14925);
                                                                                                    list.Add(15630);
                                                                                                    list.Add(24463);
                                                                                                    list.Add(24477);
                                                                                                    list.Add(24515);
                                                                                                    list.Add(14264);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(14289);
                                                                                                        list.Add(4201);
                                                                                                        list.Add(13553);
                                                                                                        list.Add(24561);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(14316);
                                                                                                            list.Add(14285);
                                                                                                            list.Add(13542);
                                                                                                            list.Add(14270);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(14327);
                                                                                                                list.Add(14304);
                                                                                                                list.Add(20043);
                                                                                                                list.Add(14279);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(4202);
                                                                                                                    list.Add(14265);
                                                                                                                    list.Add(14273);
                                                                                                                    list.Add(24562);
                                                                                                                    list.Add(14321);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(14294);
                                                                                                                        list.Add(19879);
                                                                                                                        list.Add(15631);
                                                                                                                        list.Add(24510);
                                                                                                                        list.Add(13554);
                                                                                                                        list.Add(14926);
                                                                                                                        list.Add(24464);
                                                                                                                        list.Add(24478);
                                                                                                                        list.Add(24513);
                                                                                                                        list.Add(24516);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(14286);
                                                                                                                            list.Add(13543);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(14317);
                                                                                                                                list.Add(14290);
                                                                                                                                list.Add(5048);
                                                                                                                                list.Add(24631);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(14266);
                                                                                                                                    list.Add(14280);
                                                                                                                                    list.Add(14305);
                                                                                                                                    list.Add(20190);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(14325);
                                                                                                                                        list.Add(14322);
                                                                                                                                        list.Add(14295);
                                                                                                                                        list.Add(13555);
                                                                                                                                        list.Add(14271);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(5049);
                                                                                                                                            list.Add(14311);
                                                                                                                                            list.Add(19801);
                                                                                                                                            list.Add(25294);
                                                                                                                                            list.Add(13544);
                                                                                                                                            list.Add(15632);
                                                                                                                                            list.Add(14268);
                                                                                                                                            list.Add(14287);
                                                                                                                                            list.Add(24632);
                                                                                                                                            list.Add(25295);
                                                                                                                                            list.Add(25296);
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
                        list.Add(133);
                        list.Add(168);
                        list.Add(1459);

                        if (Level > 3)
                        {
                            list.Add(116);
                            list.Add(5504);

                            if (Level > 5)
                            {
                                list.Add(143);
                                list.Add(2136);
                                list.Add(587);

                                if (Level > 7)
                                {
                                    list.Add(118);
                                    list.Add(205);
                                    list.Add(5143);

                                    if (Level > 9)
                                    {
                                        list.Add(7300);
                                        list.Add(122);
                                        list.Add(5505);

                                        if (Level > 11)
                                        {
                                            list.Add(604);
                                            list.Add(597);
                                            list.Add(130);
                                            list.Add(145);

                                            if (Level > 13)
                                            {
                                                list.Add(1460);
                                                list.Add(1449);
                                                list.Add(837);
                                                list.Add(2137);

                                                if (Level > 15)
                                                {
                                                    list.Add(2120);
                                                    list.Add(5144);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(1008);
                                                        list.Add(475);
                                                        list.Add(3140);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(543);
                                                            list.Add(1953);
                                                            list.Add(5506);
                                                            list.Add(10);
                                                            list.Add(7322);
                                                            list.Add(12051);
                                                            list.Add(12824);
                                                            list.Add(1463);
                                                            list.Add(7301);

                                                            switch (Race)
                                                            {
                                                                case Race.Human:
                                                                case Race.Dwarf:
                                                                case Race.NightElf:
                                                                case Race.Gnome:
                                                                    list.Add(3561);
                                                                    break;
                                                                case Race.Orc:
                                                                case Race.Undead:
                                                                case Race.Tauren:
                                                                case Race.Troll:
                                                                    list.Add(3563);
                                                                    list.Add(3567);
                                                                    break;
                                                            }
                                                            if (Level > 21)
                                                            {
                                                                list.Add(8437);
                                                                list.Add(990);
                                                                list.Add(2948);
                                                                list.Add(6143);
                                                                list.Add(2138);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(2139);
                                                                    list.Add(8450);
                                                                    list.Add(8400);
                                                                    list.Add(2121);
                                                                    list.Add(5145);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(120);
                                                                        list.Add(865);
                                                                        list.Add(8406);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(1461);
                                                                            list.Add(6141);
                                                                            list.Add(8494);
                                                                            list.Add(759);
                                                                            list.Add(8444);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(45438);
                                                                                list.Add(8457);
                                                                                list.Add(8412);
                                                                                list.Add(8455);
                                                                                list.Add(8401);
                                                                                list.Add(8438);
                                                                                list.Add(7302);

                                                                                switch (Race)
                                                                                {
                                                                                    case Race.Human:
                                                                                    case Race.Dwarf:
                                                                                    case Race.NightElf:
                                                                                    case Race.Gnome:
                                                                                        list.Add(3565);
                                                                                        break;
                                                                                    case Race.Orc:
                                                                                    case Race.Undead:
                                                                                    case Race.Tauren:
                                                                                    case Race.Troll:
                                                                                        list.Add(3566);
                                                                                        break;
                                                                                }
                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(6129);
                                                                                    list.Add(8407);
                                                                                    list.Add(8461);
                                                                                    list.Add(8416);
                                                                                    list.Add(8422);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(6117);
                                                                                        list.Add(8445);
                                                                                        list.Add(8492);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(8495);
                                                                                            list.Add(8451);
                                                                                            list.Add(8402);
                                                                                            list.Add(8427);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(8439);
                                                                                                list.Add(8408);
                                                                                                list.Add(8413);
                                                                                                list.Add(3552);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(7320);
                                                                                                    list.Add(8458);
                                                                                                    list.Add(8446);
                                                                                                    list.Add(10138);
                                                                                                    list.Add(8417);
                                                                                                    list.Add(6131);
                                                                                                    list.Add(12825);
                                                                                                    list.Add(8423);

                                                                                                    switch (Race)
                                                                                                    {
                                                                                                        case Race.Human:
                                                                                                        case Race.Dwarf:
                                                                                                        case Race.NightElf:
                                                                                                        case Race.Gnome:
                                                                                                            list.Add(10059);
                                                                                                            list.Add(11416);
                                                                                                            break;
                                                                                                        case Race.Orc:
                                                                                                        case Race.Undead:
                                                                                                        case Race.Tauren:
                                                                                                        case Race.Troll:
                                                                                                            list.Add(11418);
                                                                                                            list.Add(11417);
                                                                                                            break;
                                                                                                    }
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(10156);
                                                                                                        list.Add(10169);
                                                                                                        list.Add(10148);
                                                                                                        list.Add(8462);
                                                                                                        list.Add(10144);
                                                                                                        list.Add(10159);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(10191);
                                                                                                            list.Add(10185);
                                                                                                            list.Add(10179);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(10197);
                                                                                                                list.Add(22782);
                                                                                                                list.Add(10201);
                                                                                                                list.Add(10205);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(10053);
                                                                                                                    list.Add(10173);
                                                                                                                    list.Add(10211);
                                                                                                                    list.Add(10215);
                                                                                                                    list.Add(10149);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(10223);
                                                                                                                        list.Add(10160);
                                                                                                                        list.Add(10139);
                                                                                                                        list.Add(10180);
                                                                                                                        list.Add(10219);

                                                                                                                        switch (Race)
                                                                                                                        {
                                                                                                                            case Race.Human:
                                                                                                                            case Race.Dwarf:
                                                                                                                            case Race.NightElf:
                                                                                                                            case Race.Gnome:
                                                                                                                                list.Add(11419);
                                                                                                                                break;
                                                                                                                            case Race.Orc:
                                                                                                                            case Race.Undead:
                                                                                                                            case Race.Tauren:
                                                                                                                            case Race.Troll:
                                                                                                                                list.Add(11420);
                                                                                                                                break;
                                                                                                                        }
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(10177);
                                                                                                                            list.Add(10145);
                                                                                                                            list.Add(10192);
                                                                                                                            list.Add(10186);
                                                                                                                            list.Add(10206);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(10170);
                                                                                                                                list.Add(10202);
                                                                                                                                list.Add(10230);
                                                                                                                                list.Add(10150);
                                                                                                                                list.Add(10199);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(10157);
                                                                                                                                    list.Add(23028);
                                                                                                                                    list.Add(10181);
                                                                                                                                    list.Add(10212);
                                                                                                                                    list.Add(10216);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(10054);
                                                                                                                                        list.Add(10161);
                                                                                                                                        list.Add(22783);
                                                                                                                                        list.Add(10207);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(10187);
                                                                                                                                            list.Add(25306);
                                                                                                                                            list.Add(10174);
                                                                                                                                            list.Add(10225);
                                                                                                                                            list.Add(10151);
                                                                                                                                            list.Add(12826);
                                                                                                                                            list.Add(28609);
                                                                                                                                            list.Add(10140);
                                                                                                                                            list.Add(10193);
                                                                                                                                            list.Add(10220);
                                                                                                                                            list.Add(25304);
                                                                                                                                            list.Add(25345);
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
                        list.Add(635);
                        list.Add(20154);
                        list.Add(465);
                        list.Add(21084);

                        if (Level > 3)
                        {
                            list.Add(20271);
                            list.Add(19740);

                            if (Level > 5)
                            {
                                list.Add(21183);
                                list.Add(498);
                                list.Add(21082);
                                list.Add(639);

                                if (Level > 7)
                                {
                                    list.Add(1152);
                                    list.Add(853);

                                    if (Level > 9)
                                    {
                                        list.Add(1022);
                                        list.Add(10290);
                                        list.Add(633);
                                        list.Add(20287);

                                        if (Level > 11)
                                        {
                                            list.Add(7328);
                                            list.Add(19834);
                                            list.Add(20162);

                                            if (Level > 13)
                                            {
                                                list.Add(647);
                                                list.Add(19742);

                                                if (Level > 15)
                                                {
                                                    list.Add(25780);
                                                    list.Add(7294);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(5573);
                                                        list.Add(1044);
                                                        list.Add(20288);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(26573);
                                                            list.Add(19750);
                                                            list.Add(879);
                                                            list.Add(5502);
                                                            list.Add(643);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(19746);
                                                                list.Add(20305);
                                                                list.Add(20164);
                                                                list.Add(1026);
                                                                list.Add(19835);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(19850);
                                                                    list.Add(5588);
                                                                    list.Add(5599);
                                                                    list.Add(10322);
                                                                    list.Add(2878);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(1038);
                                                                        list.Add(10298);
                                                                        list.Add(19939);
                                                                        list.Add(20289);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(5614);
                                                                            list.Add(19876);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(19752);
                                                                                list.Add(13819);
                                                                                list.Add(20116);
                                                                                list.Add(10291);
                                                                                list.Add(1042);
                                                                                list.Add(2800);
                                                                                list.Add(20165);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(20306);
                                                                                    list.Add(19836);
                                                                                    list.Add(19888);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(642);
                                                                                        list.Add(20290);
                                                                                        list.Add(19852);
                                                                                        list.Add(19940);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(5615);
                                                                                            list.Add(19891);
                                                                                            list.Add(10324);
                                                                                            list.Add(10299);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(5627);
                                                                                                list.Add(3472);
                                                                                                list.Add(10278);
                                                                                                list.Add(20166);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(5589);
                                                                                                    list.Add(20922);
                                                                                                    list.Add(19895);
                                                                                                    list.Add(1032);
                                                                                                    list.Add(20347);
                                                                                                    list.Add(19977);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(4987);
                                                                                                        list.Add(19941);
                                                                                                        list.Add(20291);
                                                                                                        list.Add(19837);
                                                                                                        list.Add(20307);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(10312);
                                                                                                            list.Add(19897);
                                                                                                            list.Add(19853);
                                                                                                            list.Add(24275);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(10300);
                                                                                                                list.Add(6940);
                                                                                                                list.Add(10328);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(31895);
                                                                                                                    list.Add(19899);
                                                                                                                    list.Add(20356);
                                                                                                                    list.Add(20772);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(1020);
                                                                                                                        list.Add(2812);
                                                                                                                        list.Add(19942);
                                                                                                                        list.Add(20292);
                                                                                                                        list.Add(19978);
                                                                                                                        list.Add(20348);
                                                                                                                        list.Add(20923);
                                                                                                                        list.Add(10310);
                                                                                                                        list.Add(10292);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(10326);
                                                                                                                            list.Add(19838);
                                                                                                                            list.Add(25782);
                                                                                                                            list.Add(10313);
                                                                                                                            list.Add(24274);
                                                                                                                            list.Add(20308);
                                                                                                                            list.Add(19896);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(10308);
                                                                                                                                list.Add(25894);
                                                                                                                                list.Add(20729);
                                                                                                                                list.Add(10329);
                                                                                                                                list.Add(19854);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(10301);
                                                                                                                                    list.Add(19898);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(19943);
                                                                                                                                        list.Add(20293);
                                                                                                                                        list.Add(20357);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(25898);
                                                                                                                                            list.Add(23214);
                                                                                                                                            list.Add(10314);
                                                                                                                                            list.Add(25290);
                                                                                                                                            list.Add(25895);
                                                                                                                                            list.Add(10293);
                                                                                                                                            list.Add(20773);
                                                                                                                                            list.Add(25916);
                                                                                                                                            list.Add(25890);
                                                                                                                                            list.Add(25918);
                                                                                                                                            list.Add(10318);
                                                                                                                                            list.Add(19979);
                                                                                                                                            list.Add(25291);
                                                                                                                                            list.Add(20349);
                                                                                                                                            list.Add(24239);
                                                                                                                                            list.Add(25292);
                                                                                                                                            list.Add(19900);
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
                        list.Add(1243);
                        list.Add(2050);
                        list.Add(585);

                        if (Level > 3)
                        {
                            list.Add(589);
                            list.Add(2052);

                            if (Level > 5)
                            {
                                list.Add(17);
                                list.Add(591);

                                if (Level > 7)
                                {
                                    list.Add(586);
                                    list.Add(139);

                                    if (Level > 9)
                                    {
                                        list.Add(2053);
                                        list.Add(8092);
                                        list.Add(2006);

                                        switch (Race)
                                        {
                                            case Race.Human:
                                            case Race.Dwarf:
                                                list.Add(13908);
                                                break;
                                            case Race.NightElf:
                                                list.Add(10797);
                                                break;
                                            case Race.Troll:
                                                list.Add(9035);
                                                break;
                                            case Race.Undead:
                                                list.Add(2652);
                                                break;
                                        }

                                        if (Level > 11)
                                        {
                                            list.Add(588);
                                            list.Add(592);
                                            list.Add(1244);

                                            if (Level > 13)
                                            {
                                                list.Add(528);
                                                list.Add(6074);
                                                list.Add(598);
                                                list.Add(8122);

                                                if (Level > 15)
                                                {
                                                    list.Add(2054);
                                                    list.Add(8102);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(527);
                                                        list.Add(600);
                                                        list.Add(970);

                                                        switch (Race)
                                                        {
                                                            case Race.Human:
                                                            case Race.Dwarf:
                                                                list.Add(19236);
                                                                break;
                                                            case Race.NightElf:
                                                                list.Add(19296);
                                                                break;
                                                        }
                                                        if (Level > 19)
                                                        {
                                                            list.Add(14914);
                                                            list.Add(6346);
                                                            list.Add(7128);
                                                            list.Add(9484);
                                                            list.Add(2061);
                                                            list.Add(6075);
                                                            list.Add(453);
                                                            list.Add(9578);

                                                            switch (Race)
                                                            {
                                                                case Race.Troll:
                                                                    list.Add(19281);
                                                                    break;
                                                                case Race.Undead:
                                                                    list.Add(19261);
                                                                    list.Add(2944);
                                                                    break;
                                                            }
                                                            if (Level > 21)
                                                            {
                                                                list.Add(8103);
                                                                list.Add(984);
                                                                list.Add(2010);
                                                                list.Add(2055);
                                                                list.Add(2096);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(1245);
                                                                    list.Add(3747);
                                                                    list.Add(8129);
                                                                    list.Add(15262);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(9472);
                                                                        list.Add(992);
                                                                        list.Add(6076);
                                                                        list.Add(15262);

                                                                        switch (Race)
                                                                        {
                                                                            case Race.Human:
                                                                            case Race.Dwarf:
                                                                                list.Add(19238);
                                                                                break;
                                                                            case Race.NightElf:
                                                                                list.Add(19299);
                                                                                break;
                                                                        }
                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(6063);
                                                                            list.Add(8104);
                                                                            list.Add(8124);

                                                                            if (Race == Race.Undead)
                                                                            {
                                                                                list.Add(19276);
                                                                            }

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(605);
                                                                                list.Add(9579);
                                                                                list.Add(1004);
                                                                                list.Add(976);
                                                                                list.Add(6065);
                                                                                list.Add(15263);
                                                                                list.Add(596);
                                                                                list.Add(602);

                                                                                switch (Race)
                                                                                {
                                                                                    case Race.Troll:
                                                                                        list.Add(19282);
                                                                                        break;
                                                                                    case Race.Undead:
                                                                                        list.Add(19262);
                                                                                        break;
                                                                                }
                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(8131);
                                                                                    list.Add(9473);
                                                                                    list.Add(552);
                                                                                    list.Add(6077);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(1706);
                                                                                        list.Add(2767);
                                                                                        list.Add(10880);
                                                                                        list.Add(6064);
                                                                                        list.Add(8105);

                                                                                        switch (Race)
                                                                                        {
                                                                                            case Race.Human:
                                                                                            case Race.Dwarf:
                                                                                                list.Add(19240);
                                                                                                break;
                                                                                            case Race.NightElf:
                                                                                                list.Add(19302);
                                                                                                break;
                                                                                        }
                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(988);
                                                                                            list.Add(15264);
                                                                                            list.Add(2791);
                                                                                            list.Add(15264);
                                                                                            list.Add(2791);
                                                                                            list.Add(6066);
                                                                                            list.Add(8192);

                                                                                            if (Race == Race.Undead)
                                                                                            {
                                                                                                list.Add(19277);
                                                                                            }
                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(9474);
                                                                                                list.Add(6078);
                                                                                                list.Add(6060);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(9485);
                                                                                                    list.Add(996);
                                                                                                    list.Add(2060);
                                                                                                    list.Add(1006);
                                                                                                    list.Add(8106);
                                                                                                    list.Add(9592);
                                                                                                    list.Add(10874);

                                                                                                    switch (Race)
                                                                                                    {
                                                                                                        case Race.Troll:
                                                                                                            list.Add(19238);
                                                                                                            break;
                                                                                                        case Race.Undead:
                                                                                                            list.Add(19264);
                                                                                                            break;
                                                                                                    }
                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(10888);
                                                                                                        list.Add(10898);
                                                                                                        list.Add(15265);
                                                                                                        list.Add(10892);
                                                                                                        list.Add(10957);

                                                                                                        switch (Race)
                                                                                                        {
                                                                                                            case Race.Human:
                                                                                                            case Race.Dwarf:
                                                                                                                list.Add(19241);
                                                                                                                break;
                                                                                                            case Race.NightElf:
                                                                                                                list.Add(19303);
                                                                                                                break;
                                                                                                        }
                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(10911);
                                                                                                            list.Add(10915);
                                                                                                            list.Add(10927);
                                                                                                            list.Add(10909);

                                                                                                            if (Race == Race.Undead)
                                                                                                            {
                                                                                                                list.Add(19278);
                                                                                                            }
                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(10881);
                                                                                                                list.Add(10933);
                                                                                                                list.Add(10945);
                                                                                                                list.Add(10963);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(21562);
                                                                                                                    list.Add(10875);
                                                                                                                    list.Add(10899);
                                                                                                                    list.Add(15266);
                                                                                                                    list.Add(10937);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(10916);
                                                                                                                        list.Add(10960);
                                                                                                                        list.Add(10951);
                                                                                                                        list.Add(10941);
                                                                                                                        list.Add(10893);
                                                                                                                        list.Add(10928);

                                                                                                                        switch (Race)
                                                                                                                        {
                                                                                                                            case Race.Human:
                                                                                                                            case Race.Dwarf:
                                                                                                                                list.Add(19242);
                                                                                                                                break;
                                                                                                                            case Race.NightElf:
                                                                                                                                list.Add(19304);
                                                                                                                                break;
                                                                                                                            case Race.Undead:
                                                                                                                                list.Add(19304);
                                                                                                                                break;
                                                                                                                            case Race.Troll:
                                                                                                                                list.Add(19284);
                                                                                                                                break;
                                                                                                                        }
                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(10964);
                                                                                                                            list.Add(10953);
                                                                                                                            list.Add(10946);

                                                                                                                            if (Race == Race.Undead)
                                                                                                                            {
                                                                                                                                list.Add(19279);
                                                                                                                            }
                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(15267);
                                                                                                                                list.Add(10900);
                                                                                                                                list.Add(10934);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(27683);
                                                                                                                                    list.Add(10917);
                                                                                                                                    list.Add(10929);
                                                                                                                                    list.Add(10890);
                                                                                                                                    list.Add(10958);
                                                                                                                                    list.Add(10876);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(10947);
                                                                                                                                        list.Add(10894);
                                                                                                                                        list.Add(10912);
                                                                                                                                        list.Add(10965);

                                                                                                                                        switch (Race)
                                                                                                                                        {
                                                                                                                                            case Race.Human:
                                                                                                                                            case Race.Dwarf:
                                                                                                                                                list.Add(19243);
                                                                                                                                                break;
                                                                                                                                            case Race.NightElf:
                                                                                                                                                list.Add(19305);
                                                                                                                                                break;
                                                                                                                                        }
                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(10961);
                                                                                                                                            list.Add(21564);
                                                                                                                                            list.Add(25314);
                                                                                                                                            list.Add(10938);
                                                                                                                                            list.Add(25316);
                                                                                                                                            list.Add(10942);
                                                                                                                                            list.Add(10952);
                                                                                                                                            list.Add(15261);
                                                                                                                                            list.Add(10901);
                                                                                                                                            list.Add(10955);
                                                                                                                                            list.Add(25315);
                                                                                                                                            list.Add(27681);

                                                                                                                                            switch (Race)
                                                                                                                                            {
                                                                                                                                                case Race.Human:
                                                                                                                                                case Race.Dwarf:
                                                                                                                                                    list.Add(19243);
                                                                                                                                                    break;
                                                                                                                                                case Race.Undead:
                                                                                                                                                    list.Add(19280);
                                                                                                                                                    list.Add(19266);
                                                                                                                                                    break;
                                                                                                                                                case Race.Troll:
                                                                                                                                                    list.Add(19285);
                                                                                                                                                    break;
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
                        }
                        break;
                    case Class.Rogue:
                        list.Add(1804);
                        list.Add(1752);
                        list.Add(2098);
                        list.Add(1784);

                        if (Level > 3)
                        {
                            list.Add(53);
                            list.Add(921);

                            if (Level > 5)
                            {
                                list.Add(1776);
                                list.Add(1757);

                                list.Remove(1752);

                                if (Level > 7)
                                {
                                    list.Add(5277);
                                    list.Add(6760);

                                    list.Remove(2098);

                                    if (Level > 9)
                                    {
                                        list.Add(2983);
                                        list.Add(6770);
                                        list.Add(5171);

                                        if (Level > 11)
                                        {
                                            list.Add(2589);
                                            list.Add(1766);

                                            list.Remove(53);

                                            if (Level > 13)
                                            {
                                                list.Add(703);
                                                list.Add(8647);
                                                list.Add(1758);

                                                list.Remove(1757);

                                                if (Level > 15)
                                                {
                                                    list.Add(6761);
                                                    list.Add(1966);

                                                    list.Remove(6760);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(8676);
                                                        list.Add(1777);

                                                        list.Remove(1776);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(2842);
                                                            list.Add(1785);
                                                            list.Add(1943);
                                                            list.Add(3420);
                                                            list.Add(2590);
                                                            list.Add(8681);

                                                            list.Remove(1784);
                                                            list.Remove(2589);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(1725);
                                                                list.Add(1856);
                                                                list.Add(8631);
                                                                list.Add(1759);

                                                                list.Remove(703);
                                                                list.Remove(1758);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(5763);
                                                                    list.Add(2836);
                                                                    list.Add(6762);

                                                                    list.Remove(6761);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(1767);
                                                                        list.Add(1833);
                                                                        list.Add(8649);
                                                                        list.Add(8724);

                                                                        list.Remove(1766);
                                                                        list.Remove(8647);
                                                                        list.Remove(8676);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(2591);
                                                                            list.Add(2070);
                                                                            list.Add(6768);
                                                                            list.Add(8639);
                                                                            list.Add(8687);

                                                                            list.Remove(2590);
                                                                            list.Remove(6770);
                                                                            list.Remove(1966);
                                                                            list.Remove(1943);
                                                                            list.Remove(8681);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(408);
                                                                                list.Add(2835);
                                                                                list.Add(1760);
                                                                                list.Add(1842);
                                                                                list.Add(8632);

                                                                                list.Remove(1759);
                                                                                list.Remove(8631);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(13220);
                                                                                    list.Add(8623);
                                                                                    list.Add(8629);
                                                                                    list.Add(1842);

                                                                                    list.Remove(6762);
                                                                                    list.Remove(1777);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(2094);
                                                                                        list.Add(8696);
                                                                                        list.Add(8725);

                                                                                        list.Remove(2983);
                                                                                        list.Remove(8724);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(8640);
                                                                                            list.Add(8650);
                                                                                            list.Add(8691);
                                                                                            list.Add(8721);

                                                                                            list.Remove(8639);
                                                                                            list.Remove(8649);
                                                                                            list.Remove(8687);
                                                                                            list.Remove(2591);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(8633);
                                                                                                list.Add(2837);
                                                                                                list.Add(8621);
                                                                                                list.Add(8694);

                                                                                                list.Remove(8632);
                                                                                                list.Remove(2835);
                                                                                                list.Remove(1760);
                                                                                                list.Remove(5763);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(8624);
                                                                                                    list.Add(1860);
                                                                                                    list.Add(1786);
                                                                                                    list.Add(8637);
                                                                                                    list.Add(13228);

                                                                                                    list.Remove(8623);
                                                                                                    list.Remove(1785);
                                                                                                    list.Remove(6768);
                                                                                                    list.Remove(13220);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(6774);
                                                                                                        list.Add(1768);
                                                                                                        list.Add(1857);
                                                                                                        list.Add(11267);

                                                                                                        list.Remove(5171);
                                                                                                        list.Remove(1767);
                                                                                                        list.Remove(1856);
                                                                                                        list.Remove(8725);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(11273);
                                                                                                            list.Add(11279);
                                                                                                            list.Add(11341);

                                                                                                            list.Remove(8639);
                                                                                                            list.Remove(8721);
                                                                                                            list.Remove(8691);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(11293);
                                                                                                                list.Add(11357);
                                                                                                                list.Add(11285);
                                                                                                                list.Add(11197);
                                                                                                                list.Add(11289);

                                                                                                                list.Remove(8621);
                                                                                                                list.Remove(2837);
                                                                                                                list.Remove(8629);
                                                                                                                list.Remove(8650);
                                                                                                                list.Remove(8633);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(11297);
                                                                                                                    list.Add(13229);
                                                                                                                    list.Add(11299);

                                                                                                                    list.Remove(2070);
                                                                                                                    list.Remove(13228);
                                                                                                                    list.Remove(8624);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(26669);
                                                                                                                        list.Add(8643);
                                                                                                                        list.Add(3421);
                                                                                                                        list.Add(11268);
                                                                                                                        list.Add(34411);

                                                                                                                        list.Remove(5277);
                                                                                                                        list.Remove(408);
                                                                                                                        list.Remove(3420);
                                                                                                                        list.Remove(11267);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(11400);
                                                                                                                            list.Add(11274);
                                                                                                                            list.Add(11342);
                                                                                                                            list.Add(11280);
                                                                                                                            list.Add(11303);

                                                                                                                            list.Remove(8694);
                                                                                                                            list.Remove(11273);
                                                                                                                            list.Remove(11341);
                                                                                                                            list.Remove(11279);
                                                                                                                            list.Remove(8637);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(11358);
                                                                                                                                list.Add(11290);
                                                                                                                                list.Add(11294);

                                                                                                                                list.Remove(11357);
                                                                                                                                list.Remove(11289);
                                                                                                                                list.Remove(11293);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(11198);
                                                                                                                                    list.Add(13230);
                                                                                                                                    list.Add(11300);

                                                                                                                                    list.Remove(13229);
                                                                                                                                    list.Remove(11299);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(11269);
                                                                                                                                        list.Add(1769);
                                                                                                                                        list.Add(11305);

                                                                                                                                        list.Remove(11268);
                                                                                                                                        list.Remove(1768);
                                                                                                                                        list.Remove(8696);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(34412);
                                                                                                                                            list.Add(25347);
                                                                                                                                            list.Add(11286);
                                                                                                                                            list.Add(1787);
                                                                                                                                            list.Add(11343);
                                                                                                                                            list.Add(31016);
                                                                                                                                            list.Add(11275);
                                                                                                                                            list.Add(11281);
                                                                                                                                            list.Add(11286);

                                                                                                                                            list.Remove(11358);
                                                                                                                                            list.Remove(11285);
                                                                                                                                            list.Remove(1786);
                                                                                                                                            list.Remove(11342);
                                                                                                                                            list.Remove(11274);
                                                                                                                                            list.Remove(11299);
                                                                                                                                            list.Remove(11303);
                                                                                                                                            list.Remove(11280);
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
                        list.Add(8017);
                        list.Add(403);
                        list.Add(331);

                        if (Level > 3)
                        {
                            list.Add(8042);
                            list.Add(8071);

                            if (Level > 5)
                            {
                                list.Add(2484);
                                list.Add(332);

                                if (Level > 7)
                                {
                                    list.Add(324);
                                    list.Add(8044);
                                    list.Add(529);
                                    list.Add(5730);
                                    list.Add(8018);

                                    if (Level > 9)
                                    {
                                        list.Add(8024);
                                        list.Add(8050);
                                        list.Add(8075);
                                        list.Add(3599);

                                        if (Level > 11)
                                        {
                                            list.Add(370);
                                            list.Add(1535);
                                            list.Add(2008);
                                            list.Add(547);

                                            if (Level > 13)
                                            {
                                                list.Add(8154);
                                                list.Add(8045);
                                                list.Add(548);

                                                if (Level > 15)
                                                {
                                                    list.Add(8019);
                                                    list.Add(526);
                                                    list.Add(325);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(8143);
                                                        list.Add(8027);
                                                        list.Add(8052);
                                                        list.Add(6390);
                                                        list.Add(913);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(2645);
                                                            list.Add(6363);
                                                            list.Add(8004);
                                                            list.Add(8056);
                                                            list.Add(915);
                                                            list.Add(5394);
                                                            list.Add(8033);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(131);
                                                                list.Add(8498);
                                                                list.Add(8166);
                                                                list.Add(2870);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(905);
                                                                    list.Add(939);
                                                                    list.Add(8181);
                                                                    list.Add(8046);
                                                                    list.Add(20609);
                                                                    list.Add(8155);
                                                                    list.Add(8160);
                                                                    list.Add(10399);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(5675);
                                                                        list.Add(8190);
                                                                        list.Add(943);
                                                                        list.Add(6196);
                                                                        list.Add(8030);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(546);
                                                                            list.Add(8008);
                                                                            list.Add(8038);
                                                                            list.Add(8053);
                                                                            list.Add(8184);
                                                                            list.Add(8227);
                                                                            list.Add(6391);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(20608);
                                                                                list.Add(8177);
                                                                                list.Add(6375);
                                                                                list.Add(6364);
                                                                                list.Add(8232);
                                                                                list.Add(10595);
                                                                                list.Add(556);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(421);
                                                                                    list.Add(8512);
                                                                                    list.Add(6041);
                                                                                    list.Add(8012);
                                                                                    list.Add(959);
                                                                                    list.Add(8499);
                                                                                    list.Add(945);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(8058);
                                                                                        list.Add(10406);
                                                                                        list.Add(16314);
                                                                                        list.Add(6495);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(10585);
                                                                                            list.Add(15107);
                                                                                            list.Add(20610);
                                                                                            list.Add(8010);
                                                                                            list.Add(10495);
                                                                                            list.Add(10412);
                                                                                            list.Add(16339);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(8170);
                                                                                                list.Add(10478);
                                                                                                list.Add(10391);
                                                                                                list.Add(6392);
                                                                                                list.Add(8249);
                                                                                                list.Add(8161);
                                                                                                list.Add(10456);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(10447);
                                                                                                    list.Add(1064);
                                                                                                    list.Add(6365);
                                                                                                    list.Add(6377);
                                                                                                    list.Add(8005);
                                                                                                    list.Add(8134);
                                                                                                    list.Add(8235);
                                                                                                    list.Add(930);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(8835);
                                                                                                        list.Add(10613);
                                                                                                        list.Add(10537);
                                                                                                        list.Add(11314);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(10466);
                                                                                                            list.Add(16315);
                                                                                                            list.Add(10407);
                                                                                                            list.Add(10600);
                                                                                                            list.Add(10392);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(10586);
                                                                                                                list.Add(10496);
                                                                                                                list.Add(15111);
                                                                                                                list.Add(10472);
                                                                                                                list.Add(16341);
                                                                                                                list.Add(10622);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(10395);
                                                                                                                    list.Add(2860);
                                                                                                                    list.Add(10427);
                                                                                                                    list.Add(10431);
                                                                                                                    list.Add(10526);
                                                                                                                    list.Add(10413);
                                                                                                                    list.Add(16355);
                                                                                                                    list.Add(20776);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(25908);
                                                                                                                        list.Add(15207);
                                                                                                                        list.Add(10437);
                                                                                                                        list.Add(10462);
                                                                                                                        list.Add(10486);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(10614);
                                                                                                                            list.Add(11315);
                                                                                                                            list.Add(10442);
                                                                                                                            list.Add(10448);
                                                                                                                            list.Add(10467);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(10623);
                                                                                                                                list.Add(10408);
                                                                                                                                list.Add(10479);
                                                                                                                                list.Add(16316);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(10587);
                                                                                                                                    list.Add(16342);
                                                                                                                                    list.Add(10396);
                                                                                                                                    list.Add(15208);
                                                                                                                                    list.Add(10497);
                                                                                                                                    list.Add(10605);
                                                                                                                                    list.Add(10627);
                                                                                                                                    list.Add(15112);
                                                                                                                                    list.Add(10432);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(10473);
                                                                                                                                        list.Add(10538);
                                                                                                                                        list.Add(10428);
                                                                                                                                        list.Add(16356);
                                                                                                                                        list.Add(16387);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(25359);
                                                                                                                                            list.Add(10468);
                                                                                                                                            list.Add(25361);
                                                                                                                                            list.Add(10414);
                                                                                                                                            list.Add(29228);
                                                                                                                                            list.Add(20777);
                                                                                                                                            list.Add(10463);
                                                                                                                                            list.Add(10601);
                                                                                                                                            list.Add(10438);
                                                                                                                                            list.Add(16362);
                                                                                                                                            list.Add(25357);
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
                        list.Add(688);
                        list.Add(348);
                        list.Add(686);
                        list.Add(687);

                        if (Level > 3)
                        {
                            list.Add(172);
                            list.Add(702);

                            if (Level > 5)
                            {
                                list.Add(695);
                                list.Add(1454);

                                if (Level > 7)
                                {
                                    list.Add(980);
                                    list.Add(5782);

                                    if (Level > 9)
                                    {
                                        list.Add(6201);
                                        list.Add(697);
                                        list.Add(696);
                                        list.Add(1120);
                                        list.Add(707);

                                        if (Level > 11)
                                        {
                                            list.Add(705);
                                            list.Add(1108);
                                            list.Add(755);

                                            if (Level > 13)
                                            {
                                                list.Add(689);
                                                list.Add(6222);
                                                list.Add(704);

                                                if (Level > 15)
                                                {
                                                    list.Add(5697);
                                                    list.Add(1455);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(693);
                                                        list.Add(5676);
                                                        list.Add(1014);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(698);
                                                            list.Add(712);
                                                            list.Add(1094);
                                                            list.Add(3698);
                                                            list.Add(706);
                                                            list.Add(1088);
                                                            list.Add(5740);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(6205);
                                                                list.Add(126);
                                                                list.Add(699);
                                                                list.Add(6202);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(5138);
                                                                    list.Add(5500);
                                                                    list.Add(6223);
                                                                    list.Add(8288);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(132);
                                                                        list.Add(1714);
                                                                        list.Add(1456);
                                                                        list.Add(17919);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(710);
                                                                            list.Add(3699);
                                                                            list.Add(6217);
                                                                            list.Add(1106);
                                                                            list.Add(7658);
                                                                            list.Add(6366);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(1098);
                                                                                list.Add(691);
                                                                                list.Add(1949);
                                                                                list.Add(1086);
                                                                                list.Add(2941);
                                                                                list.Add(5784);
                                                                                list.Add(20752);
                                                                                list.Add(709);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(1490);
                                                                                    list.Add(6213);
                                                                                    list.Add(7646);
                                                                                    list.Add(6229);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(5699);
                                                                                        list.Add(6226);
                                                                                        list.Add(6219);
                                                                                        list.Add(17920);
                                                                                        list.Add(7648);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(7641);
                                                                                            list.Add(17951);
                                                                                            list.Add(2362);
                                                                                            list.Add(3700);
                                                                                            list.Add(11687);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(11711);
                                                                                                list.Add(7651);
                                                                                                list.Add(8289);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(5484);
                                                                                                    list.Add(11665);
                                                                                                    list.Add(11733);
                                                                                                    list.Add(20755);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(11739);
                                                                                                        list.Add(6789);
                                                                                                        list.Add(7659);
                                                                                                        list.Add(11683);
                                                                                                        list.Add(11707);
                                                                                                        list.Add(17921);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(11659);
                                                                                                            list.Add(11703);
                                                                                                            list.Add(11671);
                                                                                                            list.Add(11693);
                                                                                                            list.Add(11725);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(11729);
                                                                                                                list.Add(11677);
                                                                                                                list.Add(11699);
                                                                                                                list.Add(11688);
                                                                                                                list.Add(17952);
                                                                                                                list.Add(11721);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(18647);
                                                                                                                    list.Add(6353);
                                                                                                                    list.Add(17727);
                                                                                                                    list.Add(11712);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(11719);
                                                                                                                        list.Add(17925);
                                                                                                                        list.Add(17922);
                                                                                                                        list.Add(11667);
                                                                                                                        list.Add(1122);
                                                                                                                        list.Add(11734);
                                                                                                                        list.Add(20756);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(11740);
                                                                                                                            list.Add(11675);
                                                                                                                            list.Add(11708);
                                                                                                                            list.Add(11694);
                                                                                                                            list.Add(11660);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(11684);
                                                                                                                                list.Add(17928);
                                                                                                                                list.Add(11700);
                                                                                                                                list.Add(11672);
                                                                                                                                list.Add(11704);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(6215);
                                                                                                                                    list.Add(11689);
                                                                                                                                    list.Add(17924);
                                                                                                                                    list.Add(11717);
                                                                                                                                    list.Add(17953);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(11713);
                                                                                                                                        list.Add(11726);
                                                                                                                                        list.Add(17926);
                                                                                                                                        list.Add(11678);
                                                                                                                                        list.Add(17923);
                                                                                                                                        list.Add(11730);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(11668);
                                                                                                                                            list.Add(18540);
                                                                                                                                            list.Add(23161);
                                                                                                                                            list.Add(25311);
                                                                                                                                            list.Add(11735);
                                                                                                                                            list.Add(25307);
                                                                                                                                            list.Add(28610);
                                                                                                                                            list.Add(603);
                                                                                                                                            list.Add(11695);
                                                                                                                                            list.Add(11661);
                                                                                                                                            list.Add(11722);
                                                                                                                                            list.Add(25309);
                                                                                                                                            list.Add(17728);
                                                                                                                                            list.Add(20757);
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
                        list.Add(7919);
                        list.Add(2480);
                        list.Add(2764);
                        list.Add(7918);

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

                                    list.Remove(78);

                                    if (Level > 9)
                                    {
                                        list.Add(71);
                                        list.Add(355);
                                        list.Add(7386);
                                        list.Add(6546);
                                        list.Add(2687);

                                        list.Remove(772);

                                        if (Level > 11)
                                        {
                                            list.Add(7384);
                                            list.Add(5242);
                                            list.Add(72);

                                            list.Remove(6673);

                                            if (Level > 13)
                                            {
                                                list.Add(6572);
                                                list.Add(1160);

                                                if (Level > 15)
                                                {
                                                    list.Add(285);
                                                    list.Add(694);
                                                    list.Add(2565);

                                                    list.Remove(284);

                                                    if (Level > 17)
                                                    {
                                                        list.Add(8198);
                                                        list.Add(676);

                                                        list.Remove(6343);

                                                        if (Level > 19)
                                                        {
                                                            list.Add(845);
                                                            list.Add(12678);
                                                            list.Add(20230);
                                                            list.Add(6547);

                                                            list.Remove(6546);

                                                            if (Level > 21)
                                                            {
                                                                list.Add(5246);
                                                                list.Add(7405);
                                                                list.Add(6192);

                                                                list.Remove(7386);
                                                                list.Remove(5242);

                                                                if (Level > 23)
                                                                {
                                                                    list.Add(5308);
                                                                    list.Add(6574);
                                                                    list.Add(1608);
                                                                    list.Add(6190);

                                                                    list.Remove(6572);
                                                                    list.Remove(285);
                                                                    list.Remove(6190);

                                                                    if (Level > 25)
                                                                    {
                                                                        list.Add(7400);
                                                                        list.Add(1161);
                                                                        list.Add(6178);

                                                                        list.Remove(694);
                                                                        list.Remove(100);

                                                                        if (Level > 27)
                                                                        {
                                                                            list.Add(7887);
                                                                            list.Add(8204);
                                                                            list.Add(871);

                                                                            list.Remove(7384);
                                                                            list.Remove(8198);

                                                                            if (Level > 29)
                                                                            {
                                                                                list.Add(2458);
                                                                                list.Add(1464);
                                                                                list.Add(6548);
                                                                                list.Add(7369);
                                                                                list.Add(20252);

                                                                                list.Remove(6547);
                                                                                list.Remove(845);

                                                                                if (Level > 31)
                                                                                {
                                                                                    list.Add(18449);
                                                                                    list.Add(11549);
                                                                                    list.Add(1671);
                                                                                    list.Add(7372);
                                                                                    list.Add(11564);
                                                                                    list.Add(20658);

                                                                                    list.Remove(6192);
                                                                                    list.Remove(72);
                                                                                    list.Remove(1715);
                                                                                    list.Remove(1608);
                                                                                    list.Remove(5308);

                                                                                    if (Level > 33)
                                                                                    {
                                                                                        list.Add(8380);
                                                                                        list.Add(7379);
                                                                                        list.Add(11554);

                                                                                        list.Remove(7405);
                                                                                        list.Remove(6574);
                                                                                        list.Remove(6190);

                                                                                        if (Level > 35)
                                                                                        {
                                                                                            list.Add(1680);
                                                                                            list.Add(7402);

                                                                                            list.Remove(7400);

                                                                                            if (Level > 37)
                                                                                            {
                                                                                                list.Add(8205);
                                                                                                list.Add(6552);
                                                                                                list.Add(8820);

                                                                                                list.Remove(8204);
                                                                                                list.Remove(1464);

                                                                                                if (Level > 39)
                                                                                                {
                                                                                                    list.Add(11572);
                                                                                                    list.Add(20660);
                                                                                                    list.Add(11565);
                                                                                                    list.Add(11608);

                                                                                                    list.Remove(6548);
                                                                                                    list.Remove(20658);
                                                                                                    list.Remove(11564);
                                                                                                    list.Remove(7369);

                                                                                                    if (Level > 41)
                                                                                                    {
                                                                                                        list.Add(20616);
                                                                                                        list.Add(11550);

                                                                                                        list.Remove(20252);
                                                                                                        list.Remove(11549);

                                                                                                        if (Level > 43)
                                                                                                        {
                                                                                                            list.Add(11584);
                                                                                                            list.Add(11600);
                                                                                                            list.Add(11555);

                                                                                                            list.Remove(7887);
                                                                                                            list.Remove(7379);
                                                                                                            list.Remove(11554);

                                                                                                            if (Level > 45)
                                                                                                            {
                                                                                                                list.Add(11596);
                                                                                                                list.Add(11578);
                                                                                                                list.Add(11604);
                                                                                                                list.Add(20559);

                                                                                                                list.Remove(8380);
                                                                                                                list.Remove(6178);
                                                                                                                list.Remove(8820);
                                                                                                                list.Remove(7402);

                                                                                                                if (Level > 47)
                                                                                                                {
                                                                                                                    list.Add(11580);
                                                                                                                    list.Add(11566);
                                                                                                                    list.Add(20661);

                                                                                                                    list.Remove(8205);
                                                                                                                    list.Remove(11565);
                                                                                                                    list.Remove(20660);

                                                                                                                    if (Level > 49)
                                                                                                                    {
                                                                                                                        list.Add(1719);
                                                                                                                        list.Add(11573);
                                                                                                                        list.Add(11609);

                                                                                                                        list.Remove(11572);
                                                                                                                        list.Remove(11608);

                                                                                                                        if (Level > 51)
                                                                                                                        {
                                                                                                                            list.Add(20617);
                                                                                                                            list.Add(1672);
                                                                                                                            list.Add(11551);

                                                                                                                            list.Remove(20616);
                                                                                                                            list.Remove(1671);
                                                                                                                            list.Remove(11550);

                                                                                                                            if (Level > 53)
                                                                                                                            {
                                                                                                                                list.Add(11601);
                                                                                                                                list.Add(7373);
                                                                                                                                list.Add(11556);
                                                                                                                                list.Add(11605);

                                                                                                                                list.Remove(11600);
                                                                                                                                list.Remove(7372);
                                                                                                                                list.Remove(11555);
                                                                                                                                list.Remove(11604);

                                                                                                                                if (Level > 55)
                                                                                                                                {
                                                                                                                                    list.Add(11567);
                                                                                                                                    list.Add(20662);
                                                                                                                                    list.Add(20560);

                                                                                                                                    list.Remove(11566);
                                                                                                                                    list.Remove(20660);
                                                                                                                                    list.Remove(7402);

                                                                                                                                    if (Level > 57)
                                                                                                                                    {
                                                                                                                                        list.Add(11597);
                                                                                                                                        list.Add(6554);
                                                                                                                                        list.Add(11581);

                                                                                                                                        list.Remove(11596);
                                                                                                                                        list.Remove(6552);
                                                                                                                                        list.Remove(11580);

                                                                                                                                        if (Level > 59)
                                                                                                                                        {
                                                                                                                                            list.Add(25288);
                                                                                                                                            list.Add(11585);
                                                                                                                                            list.Add(25286);
                                                                                                                                            list.Add(11574);
                                                                                                                                            list.Add(20569);
                                                                                                                                            list.Add(25289);

                                                                                                                                            list.Remove(11601);
                                                                                                                                            list.Remove(11584);
                                                                                                                                            list.Remove(11567);
                                                                                                                                            list.Remove(11573);
                                                                                                                                            list.Remove(11609);
                                                                                                                                            list.Remove(11551);
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

                for (int i = 0; i < Talents.Count; i++)
                {
                    list.Remove(Talents[i]);
                }

                return list.OrderBy(x => x).ToList();
            }
        }
    }
}
