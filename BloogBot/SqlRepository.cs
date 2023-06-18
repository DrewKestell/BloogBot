using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BloogBot.Game;
using Newtonsoft.Json;

namespace BloogBot
{
    public abstract class SqlRepository
    {
        public abstract dynamic NewConnection();

        public abstract dynamic NewCommand(string sql, dynamic db);

        public abstract void Initialize(string connectionString);

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
