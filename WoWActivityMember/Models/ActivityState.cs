using System.ComponentModel;
using System.Text.Json.Serialization;

namespace WoWActivityMember.Models
{
    public class ActivityState
    {
        public int ProcessId { get; set; }
        public bool IsConnected { get; set; }
        public bool IsFocused { get; set; }
        public bool ShouldRun { get; set; }
        public ActivityType ActivityType { get; set; }
        public List<ActivityMemberPreset> ActivityMemberPresets { get; set; } = [];
        [JsonIgnore]
        public int MaxActivitySize
        {
            get
            {
                switch (ActivityType)
                {
                    case ActivityType.PvEBlackrockDepths:
                    case ActivityType.PvEDireMaul:
                    case ActivityType.PvEScholomance:
                    case ActivityType.PvEStratholmeAlive:
                    case ActivityType.PvEStratholmeUndead:
                        return 5;
                    //case ActivityType.PvEUpperBlackrockSpire:
                    //case ActivityType.PvPWarsongGulch19:
                    //case ActivityType.PvPWarsongGulch29:
                    //case ActivityType.PvPWarsongGulch39:
                    //case ActivityType.PvPWarsongGulch49:
                    //case ActivityType.PvPWarsongGulch59:
                    //case ActivityType.PvPWarsongGulch60:
                    //case ActivityType.PvETempleOfAtalHakkar:
                    //case ActivityType.PvERagefireChasm:
                    //case ActivityType.PvEWailingCaverns:
                    //case ActivityType.PvETheDeadmines:
                    //case ActivityType.PvEShadowfangKeep:
                    //case ActivityType.PvETheStockade:
                    //case ActivityType.PvERazorfenKraul:
                    //case ActivityType.PvEBlackfathomDeeps:
                    //case ActivityType.PvEGnomeregan:
                    //case ActivityType.PvESMGraveyard:
                    //case ActivityType.PvESMLibrary:
                    //case ActivityType.PvESMArmory:
                    //case ActivityType.PvESMCathedral:
                    //case ActivityType.PvERazorfenDowns:
                    //case ActivityType.PvEUldaman:
                    //case ActivityType.PvEZulFarrak:
                    //case ActivityType.PvEMaraudonWickedGrotto:
                    //case ActivityType.PvEMaraudonFoulsporeCavern:
                    //case ActivityType.PvEMaraudonEarthSongFalls:
                    //    return 10;
                    case ActivityType.PvPArathiBasin29:
                    case ActivityType.PvPArathiBasin39:
                    case ActivityType.PvPArathiBasin49:
                    case ActivityType.PvPArathiBasin59:
                    case ActivityType.PvPArathiBasin60:
                    case ActivityType.PvELowerBlackrockSpire:
                        return 15;
                    case ActivityType.PvEZulGurub:
                    case ActivityType.PvERuinsOfAhnQiraj:
                        return 20;
                    case ActivityType.Idle:
                    case ActivityType.PvPAlteracValley:
                    case ActivityType.PvEBlackwingLair:
                    case ActivityType.PvEMoltenCore:
                    case ActivityType.PvENaxxramas:
                    case ActivityType.PvEOnyxiasLair:
                    case ActivityType.PvETempleOfAhnQiraj:
                    case ActivityType.PvEHoggerRaid:
                        return 40;

                }
                return 10;
            }
        }
        [JsonIgnore]
        public int MinActivitySize
        {
            get
            {
                switch (ActivityType)
                {
                    //case ActivityType.PvETempleOfAtalHakkar:
                    //case ActivityType.PvERagefireChasm:
                    //case ActivityType.PvEWailingCaverns:
                    //case ActivityType.PvETheDeadmines:
                    //case ActivityType.PvEShadowfangKeep:
                    //case ActivityType.PvETheStockade:
                    //case ActivityType.PvERazorfenKraul:
                    //case ActivityType.PvEBlackfathomDeeps:
                    //case ActivityType.PvEGnomeregan:
                    //case ActivityType.PvESMGraveyard:
                    //case ActivityType.PvESMLibrary:
                    //case ActivityType.PvESMArmory:
                    //case ActivityType.PvESMCathedral:
                    //case ActivityType.PvERazorfenDowns:
                    //case ActivityType.PvEUldaman:
                    //case ActivityType.PvEZulFarrak:
                    //case ActivityType.PvEMaraudonWickedGrotto:
                    //case ActivityType.PvEMaraudonFoulsporeCavern:
                    //case ActivityType.PvEMaraudonEarthSongFalls:
                    //case ActivityType.PvEBlackrockDepths:
                    //case ActivityType.PvEDireMaul:
                    //case ActivityType.PvEScholomance:
                    //case ActivityType.PvEStratholmeAlive:
                    //case ActivityType.PvEStratholmeUndead:
                    //case ActivityType.Idle:
                    //case ActivityType.PvEBlackwingLair:
                    //case ActivityType.PvEMoltenCore:
                    //case ActivityType.PvENaxxramas:
                    //case ActivityType.PvEOnyxiasLair:
                    //case ActivityType.PvETempleOfAhnQiraj:
                    //case ActivityType.PvEZulGurub:
                    //case ActivityType.PvERuinsOfAhnQiraj:
                    //    return 2;
                    case ActivityType.PvEUpperBlackrockSpire:
                    case ActivityType.PvPWarsongGulch19:
                    case ActivityType.PvPWarsongGulch29:
                    case ActivityType.PvPWarsongGulch39:
                    case ActivityType.PvPWarsongGulch49:
                    case ActivityType.PvPWarsongGulch59:
                    case ActivityType.PvPWarsongGulch60:
                        return 8;
                    case ActivityType.PvPArathiBasin29:
                    case ActivityType.PvPArathiBasin39:
                    case ActivityType.PvPArathiBasin49:
                    case ActivityType.PvPArathiBasin59:
                    case ActivityType.PvPArathiBasin60:
                    case ActivityType.PvPAlteracValley:
                        return 10;
                }
                return 2;
            }
        }
    }
    public enum ActivityType
    {
        [Description("Idle")]
        Idle,
        [Description("PvP : Warsong Gultch [10-19]")]
        PvPWarsongGulch19,
        [Description("PvP : Warsong Gultch [20-29]")]
        PvPWarsongGulch29,
        [Description("PvP : Warsong Gultch [30-39]")]
        PvPWarsongGulch39,
        [Description("PvP : Warsong Gultch [40-49]")]
        PvPWarsongGulch49,
        [Description("PvP : Warsong Gultch [50-59]")]
        PvPWarsongGulch59,
        [Description("PvP : Warsong Gultch [60]")]
        PvPWarsongGulch60,
        [Description("PvP : Arathi Basin [20-29]")]
        PvPArathiBasin29,
        [Description("PvP : Arathi Basin [30-39]")]
        PvPArathiBasin39,
        [Description("PvP : Arathi Basin [40-49]")]
        PvPArathiBasin49,
        [Description("PvP : Arathi Basin [50-59]")]
        PvPArathiBasin59,
        [Description("PvP : Arathi Basin [60]")]
        PvPArathiBasin60,
        [Description("PvP : Alterac Valley [51-60]")]
        PvPAlteracValley,
        [Description("PvE : Ragefire Chasm [8]")]
        PvERagefireChasm,
        [Description("PvE : Wailing Caverns [10]")]
        PvEWailingCaverns,
        [Description("PvE : The Deadmines [10]")]
        PvETheDeadmines,
        [Description("PvE : Shadowfang Keep [10]")]
        PvEShadowfangKeep,
        [Description("PvE : The Stockade [15]")]
        PvETheStockade,
        [Description("PvE : Razorfen Kraul [17]")]
        PvERazorfenKraul,
        [Description("PvE : Blackfathom Deeps [19]")]
        PvEBlackfathomDeeps,
        [Description("PvE : Gnomeregan [20]")]
        PvEGnomeregan,
        [Description("PvE : The Scarlet Monastery - Graveyard [20]")]
        PvESMGraveyard,
        [Description("PvE : The Scarlet Monastery - Library [20]")]
        PvESMLibrary,
        [Description("PvE : The Scarlet Monastery - Armory [20]")]
        PvESMArmory,
        [Description("PvE : The Scarlet Monastery - Cathedral [20]")]
        PvESMCathedral,
        [Description("PvE : Razorfen Downs [25]")]
        PvERazorfenDowns,
        [Description("PvE : Uldaman [30]")]
        PvEUldaman,
        [Description("PvE : Zul'Farrak [35]")]
        PvEZulFarrak,
        [Description("PvE : Maraudon - Wicked Grotto [30]")]
        PvEMaraudonWickedGrotto,
        [Description("PvE : Maraudon - Foulspore Cavern [30]")]
        PvEMaraudonFoulsporeCavern,
        [Description("PvE : Maraudon - Earth Song Falls [30]")]
        PvEMaraudonEarthSongFalls,
        [Description("PvE : Temple of Atal'Hakkar [35]")]
        PvETempleOfAtalHakkar,
        [Description("PvE : Blackrock Depths [45]")]
        PvEBlackrockDepths,
        [Description("PvE : Lower Blackrock Spire [45]")]
        PvELowerBlackrockSpire,
        [Description("PvE : Upper Blackrock Spire [45]")]
        PvEUpperBlackrockSpire,
        [Description("PvE : Dire Maul [45]")]
        PvEDireMaul,
        [Description("PvE : Stratholme - Alive [45]")]
        PvEStratholmeAlive,
        [Description("PvE : Stratholme - Undead [45]")]
        PvEStratholmeUndead,
        [Description("PvE : Scholomance [45]")]
        PvEScholomance,
        [Description("PvE : Onyxia's Lair [50]")]
        PvEOnyxiasLair,
        [Description("PvE : Zul'Gurub [50]")]
        PvEZulGurub,
        [Description("PvE : Molten Core [50]")]
        PvEMoltenCore,
        [Description("PvE : Blackwing Lair [60]")]
        PvEBlackwingLair,
        [Description("PvE : Ruins of Ahn'Qiraj [60]")]
        PvERuinsOfAhnQiraj,
        [Description("PvE : Temple of Ahn'Qiraj [60]")]
        PvETempleOfAhnQiraj,
        [Description("PvE : Naxxramas [8]")]
        PvENaxxramas,
        [Description("PvE : Hogger Raid [1]")]
        PvEHoggerRaid,
    }

}
