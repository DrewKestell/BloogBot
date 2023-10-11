using RaidMemberBot.Models;
using System;
using System.Data.SQLite;
using System.IO;

namespace RaidMemberBot
{
    internal class SqliteRepository
    {
        private static readonly string ConnectionString = "Data Source=database.db;";
        private static readonly string PreparedSql = @"Repository\SqliteSchema.SQL";

        public static void Initialize()
        {
            if (!File.Exists("database.db"))
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Open();

                        var command = new SQLiteCommand(File.ReadAllText(PreparedSql), connection);

                        command.ExecuteReader();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SQL REPO: {ex.Message} {ex.StackTrace}");
                }
            }
        }
        public static AreaTriggerTeleport GetAreaTriggerTeleportById(int id)
        {
            AreaTriggerTeleport teleport = null;
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();


                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * " +
                            "   FROM areatrigger_teleport" +
                            "   WHERE id = @id";

                command.Parameters.AddWithValue("id", id);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teleport = new AreaTriggerTeleport
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = Convert.ToString(reader["name"]),
                            RequiredLevel = Convert.ToByte(reader["required_level"]),
                            RequiredItem = Convert.ToInt32(reader["required_item"]),
                            RequiredItem2 = Convert.ToInt32(reader["required_item2"]),
                            RequiredQuestDone = Convert.ToInt32(reader["required_quest_done"]),
                            TargetMap = Convert.ToUInt16(reader["target_map"]),
                            TargetPositionX = Convert.ToSingle(reader["target_position_x"]),
                            TargetPositionY = Convert.ToSingle(reader["target_position_y"]),
                            TargetPositionZ = Convert.ToSingle(reader["target_position_z"]),
                            TargetOrientation = Convert.ToSingle(reader["target_orientation"]),
                            StatusFailedText = Convert.ToString(reader["status_failed_text"]),
                            ConditionId = Convert.ToInt32(reader["condition_id"])
                        };
                    }
                }
                connection.Close();
            }

            return teleport;
        }
    }
}
