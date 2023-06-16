using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using BloogBot.Game;
using Newtonsoft.Json;

namespace BloogBot
{
    internal class DatabaseWrapper
    {
        private string connectionString;
        private string databaseType;
        public DatabaseWrapper(string dbType, string connString)
        {
            databaseType = dbType.ToLower();
            connectionString = connString;
            this.Initialize();
        }
        public bool IsSQL()
        {
            return databaseType.Contains("sql");
        }
        public void Initialize()
        {

            string strExeFilePath = Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strExeFilePath);

            switch (databaseType)
            {
                case "sqlite":
                    string dbPath = Path.Combine(strWorkPath, "db.db");
                    connectionString = String.Format("Data Source={0};Version=3;New=True;Compress=True;", dbPath);
                    if (!File.Exists(dbPath))
                    {
                        SQLiteConnection.CreateFile(dbPath);
                        using (var db = this.NewConnection())
                        {
                            string script = File.ReadAllText(Path.Combine(strWorkPath, "SqliteSchema.SQL"));
                            var command = this.NewCommand(script, db);
                            db.Open();
                            command.ExecuteNonQuery();
                            db.Close();
                        }
                    }
                    break;
                case "mssql":
                    using (var db = this.NewConnection())
                    {
                        string script = File.ReadAllText(Path.Combine(strWorkPath, "TSqlSchema.SQL"));
                        var command = this.NewCommand(script, db);
                        db.Open();
                        command.ExecuteNonQuery();
                        db.Close();
                    }
                    break;
            }
        }

        public dynamic NewConnection()
        {
            switch (databaseType)
            {
                case "sqlite":
                    return new SQLiteConnection(connectionString);
                case "mssql":
                    return new SqlConnection(connectionString);
            }
            return null;
        }

        public dynamic NewCommand(string sql, dynamic db)
        {
            switch (databaseType)
            {
                case "sqlite":
                    return new SQLiteCommand(sql, db);
                case "mssql":
                    return new SqlCommand(sql, db);
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
        }

        public Npc AddNpc(string name,
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

            string insertSql;
            string selectSql;
            switch (databaseType)
            {
                case "sqlite":
                    insertSql = $"INSERT INTO Npcs (Name, IsInnKeeper, SellsAmmo, Repairs, Quest, Horde, Alliance, PositionX, PositionY, PositionZ, Zone) VALUES ('{name}', {Convert.ToInt32(isInnkeeper)}, {Convert.ToInt32(sellsAmmo)}, {Convert.ToInt32(repairs)}, {Convert.ToInt32(quest)}, {Convert.ToInt32(horde)}, {Convert.ToInt32(alliance)}, {positionX}, {positionY}, {positionZ}, '{zone}');";
                    selectSql = $"SELECT * FROM Npcs WHERE Name = '{name}' LIMIT 1;";
                    this.SqlQuery(insertSql);
                    break;
                case "mssql":
                    insertSql = $"INSERT INTO Npcs VALUES ('{name}', {Convert.ToInt32(isInnkeeper)}, {Convert.ToInt32(sellsAmmo)}, {Convert.ToInt32(repairs)}, {Convert.ToInt32(quest)}, {Convert.ToInt32(horde)}, {Convert.ToInt32(alliance)}, {positionX}, {positionY}, {positionZ}, '{zone}');";
                    selectSql = $"SELECT TOP 1 * FROM Npcs WHERE Name = '{name}';";
                    this.SqlQuery(insertSql);
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();
                    var command = this.NewCommand(selectSql, db);
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
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public bool RowExistsSql(string sql)
        {
            if (this.IsSQL())
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
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public void SqlQuery(string sql)
        {
            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    db.Close();
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public bool NpcExists(string name)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                    sql = $"SELECT Id FROM Npcs WHERE Name = '{name}' LIMIT 1;";
                    return this.RowExistsSql(sql);
                case "mssql":
                    sql = $"SELECT TOP 1 Id FROM Npcs WHERE Name = '{name}';";
                    return this.RowExistsSql(sql);
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
        }
        public bool BlacklistedMobExists(ulong guid)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                    sql = $"SELECT Id FROM BlacklistedMobs WHERE Guid = '{guid}' LIMIT 1;";
                    return this.RowExistsSql(sql);
                case "mssql":
                    sql = $"SELECT TOP 1 Id FROM BlacklistedMobs WHERE Guid = '{guid}';";
                    return this.RowExistsSql(sql);
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
        }

        public bool TravelPathExists(string name)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                    sql = $"SELECT Id FROM TravelPaths WHERE Name = '{name}' LIMIT 1";
                    return this.RowExistsSql(sql);
                case "mssql":
                    sql = $"SELECT TOP 1 Id FROM TravelPaths WHERE Name = '{name}'";
                    return this.RowExistsSql(sql);
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
        }

        public TravelPath AddTravelPath(string name, string waypointsJson)
        {
            string insertSql;
            string selectSql;
            switch (databaseType)
            {
                case "sqlite":
                    insertSql = $"INSERT INTO TravelPaths (Name, Waypoints) VALUES ('{name}', '{waypointsJson}');";
                    selectSql = $"SELECT * FROM TravelPaths WHERE Name = '{name}' LIMIT 1;";
                    this.SqlQuery(insertSql);
                    break;
                case "mssql":
                    insertSql = $"INSERT INTO TravelPaths VALUES ('{name}', '{waypointsJson}');";
                    selectSql = $"SELECT TOP 1 * FROM TravelPaths WHERE Name = '{name}';";
                    this.SqlQuery(insertSql);
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }


            Position[] waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();
                    var command = this.NewCommand(selectSql, db);
                    var reader = command.ExecuteReader();
                    reader.Read();

                    var id = Convert.ToInt32(reader["Id"]);
                    var travelPath = new TravelPath(id, name, waypoints);

                    reader.Close();
                    db.Close();

                    return travelPath;
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }

        }


        public List<TravelPath> ListTravelPaths()
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"SELECT * FROM TravelPaths ORDER BY Name";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            var travelPaths = new List<TravelPath>();
            if (this.IsSQL())
            {
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
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }


        public Hotspot AddHotspot(
            string description, string zone = "",
            string faction = "",
            string waypointsJson = "",
            Npc innkeeper = null,
            Npc repairVendor = null,
            Npc ammoVendor = null,
            int minLevel = 0,
            TravelPath travelPath = null,
            bool safeForGrinding = false,
            Position[] waypoints = null
            )
        {

            string insertSql;
            string selectSql;
            int id;

            switch (databaseType)
            {
                case "sqlite":
                    insertSql = $"INSERT INTO Hotspots (Zone, Description, Faction, Waypoints, InnkeeperId, RepairVendorId, AmmoVendorId, MinimumLevel, TravelPathId, SafeForGrinding) VALUES ('{zone}', '{description}', '{faction}', '{waypointsJson}', {innkeeper?.Id.ToString() ?? "NULL"}, {repairVendor?.Id.ToString() ?? "NULL"}, {ammoVendor?.Id.ToString() ?? "NULL"}, {minLevel}, {travelPath?.Id.ToString() ?? "NULL"}, {Convert.ToInt32(safeForGrinding)});";
                    selectSql = $"SELECT * FROM Hotspots WHERE Description = '{description}' LIMIT 1;";
                    this.SqlQuery(insertSql);
                    break;
                case "mssql":
                    insertSql = $"INSERT INTO Hotspots VALUES ('{zone}', '{description}', '{faction}', '{waypointsJson}', {innkeeper?.Id.ToString() ?? "NULL"}, {repairVendor?.Id.ToString() ?? "NULL"}, {ammoVendor?.Id.ToString() ?? "NULL"}, {minLevel}, {travelPath?.Id.ToString() ?? "NULL"}, {Convert.ToInt32(safeForGrinding)});";
                    selectSql = $"SELECT TOP 1 * FROM Hotspots WHERE Description = '{description}';";
                    this.SqlQuery(insertSql);
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {

                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(selectSql, db);
                    var reader = command.ExecuteReader();
                    reader.Read();

                    id = Convert.ToInt32(reader["Id"]);

                    reader.Close();
                    db.Close();
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
            return new Hotspot(id, zone, description, faction, minLevel, waypoints, innkeeper, repairVendor, ammoVendor, travelPath, safeForGrinding);
        }

        public List<Hotspot> ListHotspots()
        {
            string sql;
            List<Hotspot> hotspots = new List<Hotspot>();
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = @"
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
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (this.IsSQL())
            {
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
            }
            else
            {
                throw new NotImplementedException();
            }
            return hotspots;
        }

        static public Npc ParseNpcFromQueryResult(dynamic reader, int id, string prefix)
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

        static public Npc BuildStartNpc(string prefix, dynamic reader)
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

        static public TravelPath ParseTravelPathFromQueryResult(dynamic reader, int id, string prefix)
        {
            var name = Convert.ToString(reader[$"{prefix}Name"]);
            var waypointsJson = Convert.ToString(reader[$"{prefix}Waypoints"]);
            var waypoints = JsonConvert.DeserializeObject<Position[]>(waypointsJson);

            return new TravelPath(id, name, waypoints);
        }

        public void AddBlacklistedMob(ulong guid)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                    sql = $"INSERT INTO BlacklistedMobs (Guid) VALUES ('{guid}');";
                    this.SqlQuery(sql);
                    break;
                case "mssql":
                    sql = $"INSERT INTO BlacklistedMobs VALUES ('{guid}');";
                    this.SqlQuery(sql);
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

        }

        public void RemoveBlacklistedMob(ulong guid)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"DELETE FROM BlacklistedMobs WHERE Guid = '{guid}';";
                    this.SqlQuery(sql);
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
        }

        public List<ulong> ListBlacklistedMobs()
        {
            List<ulong> mobIds = new List<ulong>();
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"SELECT Guid FROM BlacklistedMobs;";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }
            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                        mobIds.Add(Convert.ToUInt64(reader["Guid"]));

                    reader.Close();
                    db.Close();

                    return mobIds;
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public List<Npc> ListNPCs()
        {
            List<Npc> npcs = new List<Npc>();
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"SELECT * FROM Npcs;";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
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
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public IList<CommandModel> GetCommandsForPlayer(string playerName)
        {
            string sql;
            IList<CommandModel> commands = new List<CommandModel>();
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"SELECT * FROM Commands WHERE Player = '{playerName}'";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
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
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }
        public void DeleteCommand(int id)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"DELETE FROM Commands WHERE Id = {id}";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
                    command.ExecuteNonQuery();

                    db.Close();
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public void DeleteCommandsForPlayer(string player)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                case "mssql":
                    sql = $"DELETE FROM Commands WHERE Player = '{player}'";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();

                    var command = this.NewCommand(sql, db);
                    command.ExecuteNonQuery();

                    db.Close();
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }


        public ReportSummary GetLatestReportSignatures()
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                    sql = $"SELECT Id FROM Commands WHERE Command = '!report' ORDER BY Id DESC LIMIT 1";
                    break;
                case "mssql":
                    sql = $"SELECT TOP 1 Id FROM Commands WHERE Command = '!report' ORDER BY Id DESC";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    var reportSignatures = new List<ReportSignature>();

                    db.Open();

                    var command1 = this.NewCommand(sql, db);
                    var reader1 = command1.ExecuteReader();

                    var commandId = -1;

                    if (reader1.Read())
                        commandId = Convert.ToInt32(reader1["Id"]);

                    reader1.Close();

                    if (commandId != -1)
                    {
                        var sql2 = $"SELECT * FROM ReportSignatures s WHERE s.CommandId = {commandId}";
                        var command2 = this.NewCommand(sql2, db);
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
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }

        public void AddReportSignature(string playerName, int commandId)
        {
            string sql;
            switch (databaseType)
            {
                case "sqlite":
                    sql = $"INSERT INTO ReportSignatures (Player, CommandId) VALUES ('{playerName}', {commandId})";
                    break;
                case "mssql":
                    sql = $"INSERT INTO ReportSignatures VALUES ('{playerName}', {commandId})";
                    break;
                default:
                    // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                    throw new NotImplementedException();
            }

            if (this.IsSQL())
            {
                using (var db = this.NewConnection())
                {
                    db.Open();
                    var command = this.NewCommand(sql, db);
                    command.ExecuteNonQuery();
                    db.Close();
                }
            }
            else
            {
                // Should be unreachable if configured correctly - Code can be extended or removed for alternative storage types.
                throw new NotImplementedException();
            }
        }
    }
}
