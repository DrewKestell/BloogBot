using BloogBot.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
namespace BloogBot
{
    static public class Repository
    {
        static IRepository databaseWrapper;
        static internal void Initialize(string databaseType, string parConnectionString)
        {
            switch (databaseType.ToLower())
            {
                case "sqlite":
                    databaseWrapper = new SqliteRepository();
                    break;
                case "mssql":
                    databaseWrapper = new TSqlRepository();
                    break;
                default:
                    throw new NotImplementedException();

            }

            databaseWrapper.Initialize(parConnectionString);

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        static public Npc AddNpc(
            string name,
            bool isInnkeeper,
            bool sellsAmmo,
            bool repairs,
            bool quest,
            bool horde,
            bool alliance,
            float positionX,
            float positionY,
            float positionZ,
            string zone)
        {
            var encodedName = Encode(name);
            var encodedZone = Encode(zone);

            return databaseWrapper.AddNpc(encodedName,
             isInnkeeper,
             sellsAmmo,
             repairs,
             quest,
             horde,
             alliance,
             positionX,
             positionY,
             positionZ,
             encodedZone);
        }

        static public bool NpcExists(string name)
        {
            var encodedName = Encode(name);
            return databaseWrapper.NpcExists(encodedName);
        }

        static public bool BlacklistedMobExists(ulong guid)
        {
            return databaseWrapper.BlacklistedMobExists(guid);
        }

        static public TravelPath AddTravelPath(string name, Position[] waypoints)
        {
            var encodedName = Encode(name);

            var waypointsJson = JsonConvert.SerializeObject(waypoints);

            return databaseWrapper.AddTravelPath(encodedName, waypointsJson);

        }

        static public IEnumerable<TravelPath> ListTravelPaths()
        {
            return databaseWrapper.ListTravelPaths();
        }

        static public bool TravelPathExists(string name)
        {
            return databaseWrapper.TravelPathExists(name);
        }

        static public Hotspot AddHotspot(
            string zone,
            string description,
            string faction,
            Position[] waypoints,
            Npc innkeeper,
            Npc repairVendor,
            Npc ammoVendor,
            int minLevel,
            TravelPath travelPath,
            bool safeForGrinding)
        {
            var encodedZone = Encode(zone);
            var encodedDescription = Encode(description);
            var encodedFaction = Encode(faction);

            var waypointsJson = JsonConvert.SerializeObject(waypoints);
            return databaseWrapper.AddHotspot(description, zone, faction, waypointsJson, innkeeper, repairVendor, ammoVendor, minLevel, travelPath, safeForGrinding, waypoints);
        }

        static public IEnumerable<Hotspot> ListHotspots()
        {
            var hotspots = new List<Hotspot>();
            hotspots = databaseWrapper.ListHotspots();
            return hotspots;
        }



        static public void AddBlacklistedMob(ulong guid)
        {
            databaseWrapper.AddBlacklistedMob(guid);
        }

        static public IList<ulong> ListBlacklistedMobIds()
        {
            return databaseWrapper.ListBlacklistedMobs();
        }

        static public void RemoveBlacklistedMob(ulong guid)
        {
            databaseWrapper.RemoveBlacklistedMob(guid);
        }


        static public IList<Npc> ListNpcs()
        {
            return databaseWrapper.ListNPCs();
        }

        //HERE

        static public IList<CommandModel> GetCommandsForPlayer(string playerName)
        {
            return databaseWrapper.GetCommandsForPlayer(playerName);
        }

        static public void DeleteCommand(int id)
        {
            databaseWrapper.DeleteCommand(id);
        }

        static public void DeleteCommandsForPlayer(string player)
        {
            databaseWrapper.DeleteCommandsForPlayer(player);
        }

        static public ReportSummary GetLatestReportSignatures()
        {
            return databaseWrapper.GetLatestReportSignatures();
        }

        static public void AddReportSignature(string playerName, int commandId)
        {
            databaseWrapper.AddReportSignature(playerName, commandId);
        }

        static string Encode(string value) => value.Replace("'", "''");
    }
}
