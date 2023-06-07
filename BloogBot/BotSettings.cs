using Newtonsoft.Json;
using System.Collections.Generic;

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

        public string Food { get; set; }

        public string Drink { get; set; }

        public string TargetingIncludedNames { get; set; }

        public string TargetingExcludedNames { get; set; }

        public int LevelRangeMin { get; set; }

        public int LevelRangeMax { get; set; }

        public bool CreatureTypeBeast { get; set; }

        public bool CreatureTypeDragonkin { get; set; }

        public bool CreatureTypeDemon { get; set; }

        public bool CreatureTypeElemental { get; set; }

        public bool CreatureTypeHumanoid { get; set; }

        public bool CreatureTypeUndead { get; set; }

        public bool CreatureTypeGiant { get; set; }
        
        public bool UnitReactionHostile { get; set; }

        public bool UnitReactionUnfriendly { get; set; }

        public bool UnitReactionNeutral { get; set; }
        
        public bool LootPoor { get; set; }

        public bool LootCommon { get; set; }

        public bool LootUncommon { get; set; }

        public string LootExcludedNames { get; set; }

        public bool SellPoor { get; set; }

        public bool SellCommon { get; set; }

        public bool SellUncommon { get; set; }

        public string SellExcludedNames { get; set; }

        public int? GrindingHotspotId { get; set; }

        public int? CurrentTravelPathId { get; set; }

        public string CurrentBotName { get; set; }

        public bool UseTeleportKillswitch { get; set; }

        public bool UseStuckInPositionKillswitch { get; set; }

        public bool UseStuckInStateKillswitch { get; set; }

        public bool UsePlayerTargetingKillswitch { get; set; }

        public bool UsePlayerProximityKillswitch { get; set; }

        public string PowerlevelPlayerName { get; set; }

        public int TargetingWarningTimer { get; set; }

        public int TargetingStopTimer { get; set; }

        public int ProximityWarningTimer { get; set; }

        public int ProximityStopTimer { get; set; }

        public bool UseVerboseLogging { get; set; }

        [JsonIgnore]
        public Hotspot GrindingHotspot { get; set; }

        [JsonIgnore]
        public TravelPath CurrentTravelPath { get; set; }

        [JsonIgnore]
        public IList<string> CreatureTypes
        {
            get
            {
                var creatureTypes = new List<string>();

                if (CreatureTypeBeast) creatureTypes.Add("Beast");
                if (CreatureTypeDragonkin) creatureTypes.Add("Dragonkin");
                if (CreatureTypeDemon) creatureTypes.Add("Demon");
                if (CreatureTypeElemental) creatureTypes.Add("Elemental");
                if (CreatureTypeHumanoid) creatureTypes.Add("Humanoid");
                if (CreatureTypeUndead) creatureTypes.Add("Undead");
                if (CreatureTypeGiant) creatureTypes.Add("Giant");

                return creatureTypes;
            }
        }

        [JsonIgnore]
        public IList<string> UnitReactions
        {
            get
            {
                var unitReactions = new List<string>();

                if (UnitReactionHostile) unitReactions.Add("Hostile");
                if (UnitReactionUnfriendly) unitReactions.Add("Unfriendly");
                if (UnitReactionNeutral) unitReactions.Add("Neutral");

                return unitReactions;
            }
        }
    }
}
