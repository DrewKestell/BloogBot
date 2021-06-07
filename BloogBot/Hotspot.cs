using BloogBot.Game;

namespace BloogBot
{
    public class Hotspot
    {
        public Hotspot(
            int id,
            string zone,
            string description,
            string faction,
            int minLevel,
            Position[] waypoints,
            Npc innkeeper,
            Npc repairVendor,
            Npc ammoVendor,
            TravelPath travelPath,
            bool safeForGrinding
            )
        {
            Id = id;
            Zone = zone;
            Description = description;
            Faction = faction;
            MinLevel = minLevel;
            Waypoints = waypoints;
            Innkeeper = innkeeper;
            RepairVendor = repairVendor;
            AmmoVendor = ammoVendor;
            TravelPath = travelPath;
            SafeForGrinding = safeForGrinding;
        }

        public int Id { get; }

        public string Zone { get; }

        public string Description { get; }

        public string Faction { get; }

        public int MinLevel { get; }

        public Position[] Waypoints { get; }

        public Npc Innkeeper { get; }

        public Npc RepairVendor { get; }

        public Npc AmmoVendor { get; }

        public TravelPath TravelPath { get; }

        public bool SafeForGrinding { get; }

        public string DisplayName => $"{MinLevel} - {Zone}: {Description}";
    }
}
