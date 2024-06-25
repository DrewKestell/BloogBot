
using System.Text.Json.Serialization;
using WoWClientBot.Models;

namespace WoWStateManager.Models
{
    public class ActivityPreset
    {
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
}
