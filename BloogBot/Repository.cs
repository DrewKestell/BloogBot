using BloogBot.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace BloogBot
{
    static public class Repository
    {
        static string connectionString;

        static internal void Initialize(string parConnectionString)
        {
            connectionString = parConnectionString;
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

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                // first insert
                var sql = $"INSERT INTO Npcs VALUES ('{encodedName}', {Convert.ToInt32(isInnkeeper)}, {Convert.ToInt32(sellsAmmo)}, {Convert.ToInt32(repairs)}, {Convert.ToInt32(quest)}, {Convert.ToInt32(horde)}, {Convert.ToInt32(alliance)}, {positionX}, {positionY}, {positionZ}, '{encodedZone}');";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                // then retrieve it so we have the id
                sql = $"SELECT TOP 1 * FROM Npcs WHERE Name = '{encodedName}';";
                command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                reader.Read();

                var id = Convert.ToInt32(reader["Id"]);
                var npc = new Npc(id,
                    name,
                    isInnkeeper,
                    sellsAmmo,
                    repairs,
                    quest,
                    horde,
                    alliance,
                    new Position(positionX, positionY, positionZ),
                    zone);

                reader.Close();
                db.Close();

                return npc;
            }
        }

        static public bool NpcExists(string name)
        {
            var encodedName = Encode(name);

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"SELECT TOP 1 Id FROM Npcs WHERE Name = '{encodedName}';";
                var command = new SqlCommand(sql, db);
                var exists = command.ExecuteReader().HasRows;

                db.Close();

                return exists;
            }
        }

        static public bool BlacklistedMobExists(ulong guid)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"SELECT TOP 1 Id FROM BlacklistedMobs WHERE Guid = '{guid}';";
                var command = new SqlCommand(sql, db);
                var exists = command.ExecuteReader().HasRows;

                db.Close();

                return exists;
            }
        }

        static public TravelPath AddTravelPath(string name, Position[] waypoints)
        {
            var encodedName = Encode(name);

            var waypointsJson = JsonConvert.SerializeObject(waypoints);

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                // first insert
                var sql = $"INSERT INTO TravelPaths VALUES ('{encodedName}', '{waypointsJson}');";
                var command = new SqlCommand(sql, db);
                command.Prepare();
                command.ExecuteNonQuery();

                // then retrieve it so we have the id
                sql = $"SELECT TOP 1 * FROM TravelPaths WHERE Name = '{encodedName}';";
                command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                reader.Read();

                var id = Convert.ToInt32(reader["Id"]);
                var travelPath = new TravelPath(id, name, waypoints);

                reader.Close();
                db.Close();

                return travelPath;
            }
        }

        static public IEnumerable<TravelPath> ListTravelPaths()
        {
            var travelPaths = new List<TravelPath>();

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = "SELECT * FROM TravelPaths ORDER BY Name;";
                var command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["Id"]);
                    var name = Convert.ToString(reader["Name"]);
                    var waypointsJson = Convert.ToString(reader["Waypoints"]);
                    var waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);
                    travelPaths.Add(new TravelPath(id, name, waypoints));
                }

                reader.Close();
                db.Close();

                return travelPaths;
            }
        }

        static public bool TravelPathExists(string name)
        {
            var encodedName = Encode(name);

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"SELECT TOP 1 Id FROM TravelPaths WHERE Name = '{encodedName}'";
                var command = new SqlCommand(sql, db);
                var exists = command.ExecuteReader().HasRows;

                db.Close();

                return exists;
            }
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

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                // first insert
                var sql = $"INSERT INTO Hotspots VALUES ('{encodedZone}', '{encodedDescription}', '{encodedFaction}', '{waypointsJson}', {innkeeper?.Id.ToString() ?? "NULL"}, {repairVendor?.Id.ToString() ?? "NULL"}, {ammoVendor?.Id.ToString() ?? "NULL"}, {minLevel}, {travelPath?.Id.ToString() ?? "NULL"}, {Convert.ToInt32(safeForGrinding)});";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                // then retrieve it so we have the id
                sql = $"SELECT TOP 1 * FROM Hotspots WHERE Description = '{encodedDescription}';";
                command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                reader.Read();

                var id = Convert.ToInt32(reader["Id"]);
                var hotspot = new Hotspot(
                    id,
                    zone,
                    description,
                    faction,
                    minLevel,
                    waypoints,
                    innkeeper,
                    repairVendor,
                    ammoVendor,
                    travelPath,
                    safeForGrinding);

                reader.Close();
                db.Close();

                return hotspot;
            }
        }

        static public IEnumerable<Hotspot> ListHotspots()
        {
            var hotspots = new List<Hotspot>();

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = @"
                    SELECT
	                    h.Zone, h.Description, h.Faction, h.Waypoints, h.InnkeeperId, h.RepairVendorId, h.AmmoVendorId, h.MinimumLevel, h.TravelPathId, h.SafeForGrinding, h.Id,
	                    i.Name as Innkeeper_Name, i.IsInnkeeper as Innkeeper_IsInnkeeper, i.SellsAmmo as Innkeeper_SellsAmmo, i.Repairs as Innkeeper_Repairs, i.Quest as Innkeeper_Quest, i.Horde as Innkeeper_Horde, i.Alliance as Innkeeper_Alliance, i.PositionX as Innkeeper_PositionX, i.PositionY as Innkeeper_PositionY, i.PositionZ as Innkeeper_PositionZ, i.Zone as Innkeeper_Zone, i.Id as Innkeeper_Id,
	                    a.Name as AmmoVendor_Name, a.IsInnkeeper as AmmoVendor_IsInnkeeper, a.SellsAmmo as AmmoVendor_SellsAmmo, a.Repairs as AmmoVendor_Repairs, a.Quest as AmmoVendor_Quest, a.Horde as AmmoVendor_Horde, a.Alliance as AmmoVendor_Alliance, a.PositionX as AmmoVendor_PositionX, a.PositionY as AmmoVendor_PositionY, a.PositionZ as AmmoVendor_PositionZ, a.Zone as AmmoVendor_Zone, a.Id as AmmoVendor_Id,
	                    r.Name as RepairVendor_Name, r.IsInnkeeper as RepairVendor_IsInnkeeper, r.SellsAmmo as RepairVendor_SellsAmmo, r.Repairs as RepairVendor_Repairs, r.Quest as RepairVendor_Quest, r.Horde as RepairVendor_Horde, r.Alliance as RepairVendor_Alliance, r.PositionX as RepairVendor_PositionX, r.PositionY as RepairVendor_PositionY, r.PositionZ as RepairVendor_PositionZ, r.Zone as RepairVendor_Zone, r.Id as RepairVendor_Id,
                        t.Name as TravelPath_Name, t.Waypoints as TravelPath_Waypoints, t.Id as TravelPath_Id
                    FROM Hotspots h
	                    LEFT JOIN Npcs i ON h.InnkeeperId = i.Id
	                    LEFT JOIN Npcs a ON h.AmmoVendorId = a.Id
	                    LEFT JOIN Npcs r ON h.RepairVendorId = r.Id
                        LEFT JOIN TravelPaths t ON h.TravelPathId = t.Id";

                var command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = Convert.ToInt32(reader["Id"]);
                    var zone = Convert.ToString(reader["Zone"]);
                    var description = Convert.ToString(reader["Description"]);
                    var faction = Convert.ToString(reader["Faction"]);
                    var minLevel = Convert.ToInt32(reader["MinimumLevel"]);
                    var waypointsJson = Convert.ToString(reader["Waypoints"]);
                    var waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);
                    var safeForGrinding = Convert.ToBoolean(reader["SafeForGrinding"]);

                    Npc innkeeper = null;
                    if (reader["InnkeeperId"].GetType() != typeof(DBNull))
                        innkeeper = ParseNpcFromQueryResult(reader, Convert.ToInt32(reader["InnkeeperId"]), "Innkeeper_");

                    Npc repairVendor = null;
                    if (reader["RepairVendorId"].GetType() != typeof(DBNull))
                        repairVendor = ParseNpcFromQueryResult(reader, Convert.ToInt32(reader["RepairVendorId"]), "RepairVendor_");

                    Npc ammoVendor = null;
                    if (reader["AmmoVendorId"].GetType() != typeof(DBNull))
                        ammoVendor = ParseNpcFromQueryResult(reader, Convert.ToInt32(reader["AmmoVendorId"]), "AmmoVendor_");

                    TravelPath travelPath = null;
                    if (reader["TravelPathId"].GetType() != typeof(DBNull))
                        travelPath = ParseTravelPathFromQueryResult(reader, Convert.ToInt32(reader["TravelPathId"]), "TravelPath_");

                    hotspots.Add(new Hotspot(
                        id,
                        zone,
                        description,
                        faction,
                        minLevel,
                        waypoints,
                        innkeeper,
                        repairVendor,
                        ammoVendor,
                        travelPath,
                        safeForGrinding));
                }

                reader.Close();
                db.Close();

                return hotspots;
            }
        }

        static public Npc ParseNpcFromQueryResult(SqlDataReader reader, int id, string prefix)
        {
            var positionX = (float)Convert.ToDecimal(reader[$"{prefix}PositionX"]);
            var positionY = (float)Convert.ToDecimal(reader[$"{prefix}PositionY"]);
            var positionZ = (float)Convert.ToDecimal(reader[$"{prefix}PositionZ"]);
            var position = new Position(positionX, positionY, positionZ);
            return new Npc(
                id,
                Convert.ToString(reader[$"{prefix}Name"]),
                Convert.ToBoolean(reader[$"{prefix}IsInnkeeper"]),
                Convert.ToBoolean(reader[$"{prefix}SellsAmmo"]),
                Convert.ToBoolean(reader[$"{prefix}Repairs"]),
                Convert.ToBoolean(reader[$"{prefix}Quest"]),
                Convert.ToBoolean(reader[$"{prefix}Horde"]),
                Convert.ToBoolean(reader[$"{prefix}Alliance"]),
                position,
                Convert.ToString(reader[$"{prefix}Zone"]));
        }

        static public Npc BuildStartNpc(string prefix, SqlDataReader reader)
        {
            var positionX = (float)Convert.ToDecimal(reader[$"{prefix}NpcPositionX"]);
            var positionY = (float)Convert.ToDecimal(reader[$"{prefix}NpcPositionY"]);
            var positionZ = (float)Convert.ToDecimal(reader[$"{prefix}NpcPositionZ"]);
            var position = new Position(positionX, positionY, positionZ);
            return new Npc(
                Convert.ToInt32(reader[$"{prefix}NpcId"]),
                Convert.ToString(reader[$"{prefix}NpcName"]),
                Convert.ToBoolean(reader[$"{prefix}NpcIsInnkeeper"]),
                Convert.ToBoolean(reader[$"{prefix}NpcSellsAmmo"]),
                Convert.ToBoolean(reader[$"{prefix}NpcRepairs"]),
                Convert.ToBoolean(reader[$"{prefix}NpcQuest"]),
                Convert.ToBoolean(reader[$"{prefix}NpcHorde"]),
                Convert.ToBoolean(reader[$"{prefix}NpcAlliance"]),
                position,
                Convert.ToString(reader[$"{prefix}NpcZone"]));
        }

        static public TravelPath ParseTravelPathFromQueryResult(SqlDataReader reader, int id, string prefix)
        {
            var name = Convert.ToString(reader[$"{prefix}Name"]);
            var waypointsJson = Convert.ToString(reader[$"{prefix}Waypoints"]);
            var waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);

            return new TravelPath(id, name, waypoints);
        }

        static public void AddBlacklistedMob(ulong guid)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"INSERT INTO BlacklistedMobs VALUES ('{guid}');";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        static public void RemoveBlacklistedMob(ulong guid)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"DELETE FROM BlacklistedMobs WHERE Guid = '{guid}';";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        static public IList<ulong> ListBlacklistedMobIds()
        {
            IList<ulong> mobIds = new List<ulong>();

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"SELECT Guid FROM BlacklistedMobs;";
                var command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                while (reader.Read())
                    mobIds.Add(Convert.ToUInt64(reader["Guid"]));

                reader.Close();
                db.Close();

                return mobIds;
            }
        }

        static public IList<Npc> ListNpcs()
        {
            IList<Npc> npcs = new List<Npc>();

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"SELECT * FROM Npcs;";
                var command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var positionX = (float)Convert.ToDecimal(reader["PositionX"]);
                    var positionY = (float)Convert.ToDecimal(reader["PositionY"]);
                    var positionZ = (float)Convert.ToDecimal(reader["PositionZ"]);
                    npcs.Add(new Npc(
                        Convert.ToInt32(reader["Id"]),
                        Convert.ToString(reader["Name"]),
                        Convert.ToBoolean(reader["IsInnkeeper"]),
                        Convert.ToBoolean(reader["SellsAmmo"]),
                        Convert.ToBoolean(reader["Repairs"]),
                        Convert.ToBoolean(reader["Quest"]),
                        Convert.ToBoolean(reader["Horde"]),
                        Convert.ToBoolean(reader["Alliance"]),
                        new Position(positionX, positionY, positionZ),
                        Convert.ToString(reader["Zone"])));
                }

                reader.Close();
                db.Close();

                return npcs;
            }
        }

        static public IList<CommandModel> GetCommandsForPlayer(string playerName)
        {
            IList<CommandModel> commands = new List<CommandModel>();

            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"SELECT * FROM Commands WHERE Player = '{playerName}'";
                var command = new SqlCommand(sql, db);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    commands.Add(new CommandModel(
                        Convert.ToInt32(reader["Id"]),
                        Convert.ToString(reader["Command"]),
                        Convert.ToString(reader["Player"]),
                        Convert.ToString(reader["Args"])));
                }

                reader.Close();
                db.Close();

                return commands;
            }
        }

        static public void DeleteCommand(int id)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"DELETE FROM Commands WHERE Id = {id}";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        static public void DeleteCommandsForPlayer(string player)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"DELETE FROM Commands WHERE Player = '{player}'";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        static public ReportSummary GetLatestReportSignatures()
        {
            using (var db = new SqlConnection(connectionString))
            {
                var reportSignatures = new List<ReportSignature>();

                db.Open();

                var sql1 = "SELECT TOP 1 Id FROM Commands WHERE Command = '!report' ORDER BY Id DESC";
                var command1 = new SqlCommand(sql1, db);
                var reader1 = command1.ExecuteReader();

                var commandId = -1;

                if (reader1.Read())
                    commandId = Convert.ToInt32(reader1["Id"]);

                reader1.Close();

                if (commandId != -1)
                {
                    var sql2 = $"SELECT * FROM ReportSignatures s WHERE s.CommandId = {commandId}";
                    var command2 = new SqlCommand(sql2, db);
                    var reader2 = command2.ExecuteReader();

                    while (reader2.Read())
                    {
                        reportSignatures.Add(new ReportSignature(
                            Convert.ToInt32(reader2["Id"]),
                            Convert.ToString(reader2["Player"]),
                            Convert.ToInt32(reader2["CommandId"])));
                    }

                    reader2.Close();
                }

                db.Close();

                return new ReportSummary(commandId, reportSignatures);
            }
        }

        static public void AddReportSignature(string playerName, int commandId)
        {
            using (var db = new SqlConnection(connectionString))
            {
                db.Open();

                var sql = $"INSERT INTO ReportSignatures VALUES ('{playerName}', {commandId})";
                var command = new SqlCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        static string Encode(string value) => value.Replace("'", "''");
    }
}
