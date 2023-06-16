﻿using BloogBot.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;

namespace BloogBot
{
    internal class TSqlRepository : IRepository
    {
        string connectionString;

        public void Initialize(string connectionString)
        {
            this.connectionString = connectionString;

            string strExeFilePath = Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strExeFilePath);

            // Run TSqlSchema.SQL regardless - The SQL checks if tables exists before creating the

            using (var db = this.NewConnection())
            {
                string script = File.ReadAllText(Path.Combine(strWorkPath, "TSqlSchema.SQL"));
                var command = this.NewCommand(script, db);
                db.Open();
                command.ExecuteNonQuery();
                db.Close();
            }

        }

        public dynamic NewConnection()
        {
            return new SqlConnection(connectionString);
        }

        public dynamic NewCommand(string sql, dynamic db)
        {
            return new SqlCommand(sql, db);
        }
        public void RunSqlQuery(string sql)
        {
            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
                command.Prepare();
                command.ExecuteNonQuery();

                db.Close();
            }
        }

        public void AddBlacklistedMob(ulong guid)
        {
            string sql = $"INSERT INTO BlacklistedMobs VALUES ('{guid}');";

            RunSqlQuery(sql);
        }

        public Hotspot AddHotspot(string description, string zone = "", string faction = "", string waypointsJson = "", Npc innkeeper = null, Npc repairVendor = null, Npc ammoVendor = null, int minLevel = 0, TravelPath travelPath = null, bool safeForGrinding = false, Position[] waypoints = null)
        {
            string insertSql = $"INSERT INTO Hotspots VALUES ('{zone}', '{description}', '{faction}', '{waypointsJson}', {innkeeper?.Id.ToString() ?? "NULL"}, {repairVendor?.Id.ToString() ?? "NULL"}, {ammoVendor?.Id.ToString() ?? "NULL"}, {minLevel}, {travelPath?.Id.ToString() ?? "NULL"}, {Convert.ToInt32(safeForGrinding)});";
            string selectSql = $"SELECT TOP 1 * FROM Hotspots WHERE Description = '{description}';";
            int id;

            RunSqlQuery(insertSql);

            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(selectSql, db);
                var reader = command.ExecuteReader();
                reader.Read();

                id = Convert.ToInt32(reader["Id"]);

                reader.Close();
                db.Close();
            }

            return new Hotspot(id, zone, description, faction, minLevel, waypoints, innkeeper, repairVendor, ammoVendor, travelPath, safeForGrinding);
        }

        public Npc AddNpc(string name, bool isInnkeeper, bool sellsAmmo, bool repairs, bool quest, bool horde, bool alliance, float positionX, float positionY, float positionZ, string zone)
        {
            string insertSql = $"INSERT INTO Npcs VALUES ('{name}', {Convert.ToInt32(isInnkeeper)}, {Convert.ToInt32(sellsAmmo)}, {Convert.ToInt32(repairs)}, {Convert.ToInt32(quest)}, {Convert.ToInt32(horde)}, {Convert.ToInt32(alliance)}, {positionX}, {positionY}, {positionZ}, '{zone}');";
            string selectSql = $"SELECT TOP 1 * FROM Npcs WHERE Name = '{name}';";

            RunSqlQuery(insertSql);

            using (var db = NewConnection())
            {
                db.Open();
                var command = NewCommand(selectSql, db);
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

        public void AddReportSignature(string playerName, int commandId)
        {
            string sql = $"INSERT INTO ReportSignatures VALUES ('{playerName}', {commandId})";


            using (var db = NewConnection())
            {
                db.Open();
                var command = NewCommand(sql, db);
                command.ExecuteNonQuery();
                db.Close();
            }

        }

        public TravelPath AddTravelPath(string name, string waypointsJson)
        {

            string insertSql = $"INSERT INTO TravelPaths VALUES ('{name}', '{waypointsJson}');";
            string selectSql = $"SELECT TOP 1 * FROM TravelPaths WHERE Name = '{name}';";

            TravelPath travelPath;

            RunSqlQuery(insertSql);


            Position[] waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);

            using (var db = NewConnection())
            {
                db.Open();
                var command = NewCommand(selectSql, db);
                var reader = command.ExecuteReader();
                reader.Read();

                var id = Convert.ToInt32(reader["Id"]);
                travelPath = new TravelPath(id, name, waypoints);

                reader.Close();
                db.Close();

            }
            return travelPath;
        }

        public bool BlacklistedMobExists(ulong guid)
        {
            string sql = $"SELECT TOP 1 Id FROM BlacklistedMobs WHERE Guid = '{guid}';";
            return RowExistsSql(sql);
        }

        public void DeleteCommand(int id)
        {
            string sql = $"DELETE FROM Commands WHERE Id = {id}";

            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }

        }

        public void DeleteCommandsForPlayer(string player)
        {
            string sql = $"DELETE FROM Commands WHERE Player = '{player}'";

            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
                command.ExecuteNonQuery();

                db.Close();
            }

        }

        public IList<CommandModel> GetCommandsForPlayer(string playerName)
        {
            string sql = $"SELECT * FROM Commands WHERE Player = '{playerName}'";
            IList<CommandModel> commands = new List<CommandModel>();

            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
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

        public ReportSummary GetLatestReportSignatures()
        {
            string sql = $"SELECT TOP 1 Id FROM Commands WHERE Command = '!report' ORDER BY Id DESC";

            using (var db = NewConnection())
            {
                var reportSignatures = new List<ReportSignature>();

                db.Open();

                var command = NewCommand(sql, db);
                var reader = command.ExecuteReader();

                var commandId = -1;

                if (reader.Read())
                    commandId = Convert.ToInt32(reader["Id"]);

                reader.Close();

                if (commandId != -1)
                {
                    var sql1 = $"SELECT * FROM ReportSignatures s WHERE s.CommandId = {commandId}";
                    var command1 = this.NewCommand(sql1, db);
                    var reader1 = command1.ExecuteReader();

                    while (reader1.Read())
                    {
                        reportSignatures.Add(new ReportSignature(
                            Convert.ToInt32(reader1["Id"]),
                            Convert.ToString(reader1["Player"]),
                            Convert.ToInt32(reader1["CommandId"])));
                    }

                    reader1.Close();
                }

                db.Close();
                return new ReportSummary(commandId, reportSignatures);
            }

        }


        public List<ulong> ListBlacklistedMobs()
        {
            List<ulong> mobIds = new List<ulong>();
            string sql = $"SELECT Guid FROM BlacklistedMobs;";

            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
                var reader = command.ExecuteReader();
                while (reader.Read())
                    mobIds.Add(Convert.ToUInt64(reader["Guid"]));

                reader.Close();
                db.Close();

                return mobIds;
            }
        }

        public List<Hotspot> ListHotspots()
        {
            string sql = @"
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

            List<Hotspot> hotspots = new List<Hotspot>();

            using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
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
                }

            return hotspots;
        }

        public List<Npc> ListNPCs()
        {
            List<Npc> npcs = new List<Npc>();
            string sql = $"SELECT * FROM Npcs;";

            using (var db = NewConnection())
            {
                db.Open();

                var command = NewCommand(sql, db);
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

        public List<TravelPath> ListTravelPaths()
        {
            string sql = $"SELECT * FROM TravelPaths ORDER BY Name";

            var travelPaths = new List<TravelPath>();
 
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
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



        public bool NpcExists(string name)
        {
            string sql= $"SELECT TOP 1 Id FROM Npcs WHERE Name = '{name}';";
            return RowExistsSql(sql);
        }

        public void RemoveBlacklistedMob(ulong guid)
        {
            string sql = $"DELETE FROM BlacklistedMobs WHERE Guid = '{guid}';";
            RunSqlQuery(sql);
        }

        public bool RowExistsSql(string sql)
        {
            using (var db = this.NewConnection())
            {
                db.Open();

                var command = this.NewCommand(sql, db);
                var exists = command.ExecuteReader().HasRows;

                db.Close();

                return exists;
            }
        }



        public bool TravelPathExists(string name)
        {
            string sql = $"SELECT TOP 1 Id FROM TravelPaths WHERE Name = '{name}'";
            return this.RowExistsSql(sql);
        }

        public Npc ParseNpcFromQueryResult(dynamic reader, int id, string prefix)
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

        public Npc BuildStartNpc(string prefix, dynamic reader)
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

        public TravelPath ParseTravelPathFromQueryResult(dynamic reader, int id, string prefix)
        {
            var name = Convert.ToString(reader[$"{prefix}Name"]);
            var waypointsJson = Convert.ToString(reader[$"{prefix}Waypoints"]);
            var waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);

            return new TravelPath(id, name, waypoints);
        }
    }
}
