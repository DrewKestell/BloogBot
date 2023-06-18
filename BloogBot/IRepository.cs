using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot
{
    internal interface IRepository
    {
        void AddBlacklistedMob(ulong guid);
        Hotspot AddHotspot(string description, string zone = "", string faction = "", string waypointsJson = "", Npc innkeeper = null, Npc repairVendor = null, Npc ammoVendor = null, int minLevel = 0, TravelPath travelPath = null, bool safeForGrinding = false, Position[] waypoints = null);
        Npc AddNpc(string name, bool isInnkeeper, bool sellsAmmo, bool repairs, bool quest, bool horde, bool alliance, float positionX, float positionY, float positionZ, string zone);
        void AddReportSignature(string playerName, int commandId);
        TravelPath AddTravelPath(string name, string waypointsJson);
        bool BlacklistedMobExists(ulong guid);
        void DeleteCommand(int id);
        void DeleteCommandsForPlayer(string player);
        IList<CommandModel> GetCommandsForPlayer(string playerName);
        ReportSummary GetLatestReportSignatures();
        void Initialize(string connectionString);
        List<ulong> ListBlacklistedMobs();
        List<Hotspot> ListHotspots();
        List<Npc> ListNPCs();
        List<TravelPath> ListTravelPaths();
        bool NpcExists(string name);
        void RemoveBlacklistedMob(ulong guid);
        bool RowExistsSql(string sql);
        bool TravelPathExists(string name);
    }
}