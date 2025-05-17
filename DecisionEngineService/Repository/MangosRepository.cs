using Database;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;

namespace DecisionEngineService.Repository
{
    public class MangosRepository
    {
        private static readonly string ConnectionString = "server=localhost;user=app;database=mangos;port=3306;password=app";
        public static int GetRowCountForTable(string tableName)
        {
            int count = 0;

            // Sanitize tableName to avoid SQL injection
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty.");
            }

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Directly inject the sanitized table name into the query
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = $"SELECT COUNT(*) FROM `{tableName}`"; // Use backticks to escape the table name

                    count = Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return count;
        }

        public static List<AreaTriggerBgEntrance> GetAreaTriggerBgEntrances()
        {
            List<AreaTriggerBgEntrance> areaTriggers = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM areatrigger_bg_entrance";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AreaTriggerBgEntrance areaTrigger = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Name = reader.IsDBNull("name") ? string.Empty : reader.GetString("name"),
                            Team = reader.GetUInt32("team"),
                            BgTemplate = reader.GetUInt32("bg_template"),
                            ExitMap = reader.GetFloat("exit_map"),
                            ExitPositionX = reader.GetFloat("exit_position_x"),
                            ExitPositionY = reader.GetFloat("exit_position_y"),
                            ExitPositionZ = reader.GetFloat("exit_position_z"),
                            ExitOrientation = reader.GetFloat("exit_orientation")
                        };
                        areaTriggers.Add(areaTrigger);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return areaTriggers;
        }
        public static List<AreaTriggerInvolvedRelation> GetAreaTriggerInvolvedRelations()
        {
            List<AreaTriggerInvolvedRelation> areaTriggers = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM areatrigger_involvedrelation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AreaTriggerInvolvedRelation areaTrigger = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Quest = reader.GetUInt32("quest")
                        };
                        areaTriggers.Add(areaTrigger);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return areaTriggers;
        }
        public static List<AreaTriggerTavern> GetAreaTriggerTaverns()
        {
            List<AreaTriggerTavern> areaTriggerTaverns = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM areatrigger_tavern";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AreaTriggerTavern areaTriggerTavern = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Name = reader.IsDBNull("name") ? string.Empty : reader.GetString("name")
                        };
                        areaTriggerTaverns.Add(areaTriggerTavern);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return areaTriggerTaverns;
        }
        public static List<AreaTriggerTeleport> GetAreaTriggerTeleports()
        {
            List<AreaTriggerTeleport> areaTriggerTeleports = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM areatrigger_teleport";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AreaTriggerTeleport areaTriggerTeleport = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Patch = reader.GetUInt32("patch"),
                            Name = reader.IsDBNull("name") ? string.Empty : reader.GetString("name"),
                            RequiredLevel = reader.GetUInt32("required_level"),
                            RequiredItem = reader.GetUInt32("required_item"),
                            RequiredItem2 = reader.GetUInt32("required_item2"),
                            RequiredQuestDone = reader.GetUInt32("required_quest_done"),
                            RequiredEvent = reader.GetInt32("required_event"),
                            RequiredPvpRank = reader.GetUInt32("required_pvp_rank"),
                            RequiredTeam = reader.GetUInt32("required_team"),
                            RequiredFailedText = reader.IsDBNull("required_failed_text") ? string.Empty : reader.GetString("required_failed_text"),
                            TargetMap = reader.GetUInt32("target_map"),
                            TargetPositionX = reader.GetFloat("target_position_x"),
                            TargetPositionY = reader.GetFloat("target_position_y"),
                            TargetPositionZ = reader.GetFloat("target_position_z"),
                            TargetOrientation = reader.GetFloat("target_orientation")
                        };
                        areaTriggerTeleports.Add(areaTriggerTeleport);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return areaTriggerTeleports;
        }
        public static List<AreaTemplate> GetAreaTemplates()
        {
            List<AreaTemplate> areaTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM area_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AreaTemplate areaTemplate = new()
                        {
                            Entry = reader.GetUInt32("Entry"),
                            MapId = reader.GetUInt32("MapId"),
                            ZoneId = reader.GetUInt32("ZoneId"),
                            ExploreFlag = reader.GetUInt32("ExploreFlag"),
                            Flags = reader.GetUInt32("Flags"),
                            AreaLevel = reader.GetInt32("AreaLevel"),
                            Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),
                            Team = reader.GetUInt32("Team"),
                            LiquidTypeId = reader.GetUInt32("LiquidTypeId")
                        };
                        areaTemplates.Add(areaTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return areaTemplates;
        }
        public static List<AuctionHouseBot> GetAuctionHouseBots()
        {
            List<AuctionHouseBot> auctionHouseBots = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM auctionhousebot";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AuctionHouseBot auctionHouseBot = new()
                        {
                            Item = reader.GetInt32("item"),
                            Stack = reader.IsDBNull("stack") ? 0 : reader.GetInt32("stack"),
                            Bid = reader.IsDBNull("bid") ? 0 : reader.GetInt32("bid"),
                            Buyout = reader.IsDBNull("buyout") ? 0 : reader.GetInt32("buyout")
                        };
                        auctionHouseBots.Add(auctionHouseBot);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return auctionHouseBots;
        }
        public static List<AutoBroadcast> GetAutoBroadcasts()
        {
            List<AutoBroadcast> autoBroadcasts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM autobroadcast";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AutoBroadcast autoBroadcast = new()
                        {
                            Delay = reader.IsDBNull("delay") ? 0 : reader.GetInt32("delay"),
                            StringId = reader.IsDBNull("stringId") ? 0 : reader.GetInt32("stringId"),
                            Comments = reader.IsDBNull("comments") ? string.Empty : reader.GetString("comments")
                        };
                        autoBroadcasts.Add(autoBroadcast);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return autoBroadcasts;
        }
        public static List<BattlegroundEvent> GetBattlegroundEvents()
        {
            List<BattlegroundEvent> battlegroundEvents = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM battleground_events";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        BattlegroundEvent battlegroundEvent = new()
                        {
                            Map = reader.GetInt32("map"),
                            Event1 = reader.GetUInt32("event1"),
                            Event2 = reader.GetUInt32("event2"),
                            Description = reader.GetString("description")
                        };
                        battlegroundEvents.Add(battlegroundEvent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return battlegroundEvents;
        }
        public static List<BattlegroundTemplate> GetBattlegroundTemplates()
        {
            List<BattlegroundTemplate> battlegroundTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT * FROM battleground_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        BattlegroundTemplate battlegroundTemplate = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Patch = reader.GetUInt32("patch"),
                            MinPlayersPerTeam = reader.GetUInt32("MinPlayersPerTeam"),
                            MaxPlayersPerTeam = reader.GetUInt32("MaxPlayersPerTeam"),
                            MinLvl = reader.GetUInt32("MinLvl"),
                            MaxLvl = reader.GetUInt32("MaxLvl"),
                            AllianceWinSpell = reader.GetUInt32("AllianceWinSpell"),
                            AllianceLoseSpell = reader.GetUInt32("AllianceLoseSpell"),
                            HordeWinSpell = reader.GetUInt32("HordeWinSpell"),
                            HordeLoseSpell = reader.GetUInt32("HordeLoseSpell"),
                            AllianceStartLoc = reader.GetUInt32("AllianceStartLoc"),
                            AllianceStartO = reader.GetFloat("AllianceStartO"),
                            HordeStartLoc = reader.GetUInt32("HordeStartLoc"),
                            HordeStartO = reader.GetFloat("HordeStartO")
                        };
                        battlegroundTemplates.Add(battlegroundTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return battlegroundTemplates;
        }
        public static List<BattlemasterEntry> GetBattlemasterEntries()
        {
            List<BattlemasterEntry> battlemasterEntries = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM battlemaster_entry";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        BattlemasterEntry battlemasterEntry = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            BgTemplate = reader.GetUInt32("bg_template")
                        };
                        battlemasterEntries.Add(battlemasterEntry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return battlemasterEntries;
        }
        public static List<BroadcastText> GetBroadcastTexts()
        {
            List<BroadcastText> broadcastTexts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM broadcast_text";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        BroadcastText broadcastText = new()
                        {
                            Id = reader.GetUInt32("ID"),
                            MaleText = reader.IsDBNull("MaleText") ? string.Empty : reader.GetString("MaleText"),
                            FemaleText = reader.IsDBNull("FemaleText") ? string.Empty : reader.GetString("FemaleText"),
                            Sound = reader.GetUInt32("Sound"),
                            Type = reader.GetUInt32("Type"),
                            Language = reader.GetUInt32("Language"),
                            EmoteId0 = reader.GetUInt32("EmoteId0"),
                            EmoteId1 = reader.GetUInt32("EmoteId1"),
                            EmoteId2 = reader.GetUInt32("EmoteId2"),
                            EmoteDelay0 = reader.GetUInt32("EmoteDelay0"),
                            EmoteDelay1 = reader.GetUInt32("EmoteDelay1"),
                            EmoteDelay2 = reader.GetUInt32("EmoteDelay2")
                        };
                        broadcastTexts.Add(broadcastText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return broadcastTexts;
        }
        public static List<CinematicWaypoint> GetCinematicWaypoints()
        {
            List<CinematicWaypoint> waypoints = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM cinematic_waypoints";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CinematicWaypoint waypoint = new()
                        {
                            Cinematic = reader.GetInt32("cinematic"),
                            Timer = reader.GetInt32("timer"),
                            Posx = reader.IsDBNull("posx") ? 0 : reader.GetFloat("posx"),
                            Posy = reader.IsDBNull("posy") ? 0 : reader.GetFloat("posy"),
                            Posz = reader.IsDBNull("posz") ? 0 : reader.GetFloat("posz"),
                            Comment = reader.IsDBNull("comment") ? string.Empty : reader.GetString("comment")
                        };
                        waypoints.Add(waypoint);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return waypoints;
        }
        public static List<Command> GetCommands()
        {
            List<Command> commandResults = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM command";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Command commandResult = new()
                        {
                            Name = reader.GetString("name"),
                            Security = reader.GetUInt32("security"),
                            Help = reader.IsDBNull("help") ? string.Empty : reader.GetString("help"),
                            Flags = reader.GetInt32("flags")
                        };
                        commandResults.Add(commandResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return commandResults;
        }
        public static List<Condition> GetConditions()
        {
            List<Condition> conditionResults = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM conditions";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Condition conditionResult = new()
                        {
                            ConditionEntry = reader.GetUInt32("condition_entry"),
                            Type = reader.GetInt32("type"),
                            Value1 = reader.GetUInt32("value1"),
                            Value2 = reader.GetUInt32("value2"),
                            Flags = reader.GetUInt32("flags")
                        };
                        conditionResults.Add(conditionResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return conditionResults;
        }
        public static List<Creature> GetCreatures()
        {
            List<Creature> creatureResults = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Creature creatureResult = new()
                        {
                            Guid = reader.GetUInt32("guid"),
                            Id = reader.GetUInt32("id"),
                            Map = reader.GetUInt32("map"),
                            Modelid = reader.GetUInt32("modelid"),
                            EquipmentId = reader.GetUInt32("equipment_id"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Orientation = reader.GetFloat("orientation"),
                            Spawntimesecsmin = reader.GetUInt32("spawntimesecsmin"),
                            Spawntimesecsmax = reader.GetUInt32("spawntimesecsmax"),
                            Spawndist = reader.GetFloat("spawndist"),
                            Currentwaypoint = reader.GetUInt32("currentwaypoint"),
                            Curhealth = reader.GetUInt32("curhealth"),
                            Curmana = reader.GetUInt32("curmana"),
                            DeathState = reader.GetUInt32("DeathState"),
                            MovementType = reader.GetUInt32("MovementType"),
                            SpawnFlags = reader.GetUInt32("spawnFlags"),
                            Visibilitymod = reader.GetFloat("visibilitymod"),
                            PatchMin = reader.GetUInt32("patch_min"),
                            PatchMax = reader.GetUInt32("patch_max")
                        };
                        creatureResults.Add(creatureResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return creatureResults;
        }
        public static List<CreatureAddon> GetCreatureAddons()
        {
            List<CreatureAddon> creatureAddons = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_addon";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureAddon creatureAddon = new()
                        {
                            Guid = reader.GetUInt32("guid"),
                            Patch = reader.GetUInt32("patch"),
                            Mount = reader.GetUInt32("mount"),
                            Bytes1 = reader.GetUInt32("bytes1"),
                            B20Sheath = reader.GetUInt32("b2_0_sheath"),
                            B21Flags = reader.GetUInt32("b2_1_flags"),
                            Emote = reader.GetUInt32("emote"),
                            Moveflags = reader.GetUInt32("moveflags"),
                            Auras = reader.IsDBNull("auras") ? string.Empty : reader.GetString("auras")
                        };
                        creatureAddons.Add(creatureAddon);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANGOS REPO]{ex.Message} {ex.StackTrace}");
                }
            }

            return creatureAddons;
        }
        public static List<CreatureAIEvent> GetCreatureAIEvents()
        {
            List<CreatureAIEvent> aiEvents = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_ai_events";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureAIEvent aiEvent = new()
                        {
                            Id = reader.GetUInt32("id"),
                            CreatureId = reader.GetUInt32("creature_id"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            EventType = reader.GetByte("event_type"),
                            EventInversePhaseMask = reader.GetUInt32("event_inverse_phase_mask"),
                            EventChance = reader.GetUInt32("event_chance"),
                            EventFlags = reader.GetUInt32("event_flags"),
                            EventParam1 = reader.GetInt32("event_param1"),
                            EventParam2 = reader.GetInt32("event_param2"),
                            EventParam3 = reader.GetInt32("event_param3"),
                            EventParam4 = reader.GetInt32("event_param4"),
                            Action1Script = reader.GetUInt32("action1_script"),
                            Action2Script = reader.GetUInt32("action2_script"),
                            Action3Script = reader.GetUInt32("action3_script"),
                            Comment = reader.GetString("comment")
                        };
                        aiEvents.Add(aiEvent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return aiEvents;
        }
        public static List<CreatureBattleground> GetCreatureBattlegrounds()
        {
            List<CreatureBattleground> creatureBattlegrounds = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_battleground";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureBattleground creatureBattleground = new()
                        {
                            Guid = reader.GetUInt32("guid"),
                            Event1 = reader.GetByte("event1"),
                            Event2 = reader.GetByte("event2")
                        };
                        creatureBattlegrounds.Add(creatureBattleground);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatureBattlegrounds;
        }
        public static List<CreatureEquipTemplate> GetCreatureEquipTemplate()
        {
            List<CreatureEquipTemplate> equipTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_equip_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureEquipTemplate equipTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            Equipentry1 = reader.GetUInt32("equipentry1"),
                            Equipentry2 = reader.GetUInt32("equipentry2"),
                            Equipentry3 = reader.GetUInt32("equipentry3")
                        };
                        equipTemplates.Add(equipTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return equipTemplates;
        }
        public static List<CreatureEquipTemplateRaw> GetCreatureEquipTemplateRaws()
        {
            List<CreatureEquipTemplateRaw> equipTemplateRaws = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT *
                        FROM creature_equip_template_raw
                        ";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureEquipTemplateRaw equipTemplateRaw = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            Equipmodel1 = reader.GetUInt32("equipmodel1"),
                            Equipmodel2 = reader.GetUInt32("equipmodel2"),
                            Equipmodel3 = reader.GetUInt32("equipmodel3"),
                            Equipinfo1 = reader.GetUInt32("equipinfo1"),
                            Equipinfo2 = reader.GetUInt32("equipinfo2"),
                            Equipinfo3 = reader.GetUInt32("equipinfo3"),
                            Equipslot1 = reader.GetInt32("equipslot1"),
                            Equipslot2 = reader.GetInt32("equipslot2"),
                            Equipslot3 = reader.GetInt32("equipslot3")
                        };
                        equipTemplateRaws.Add(equipTemplateRaw);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return equipTemplateRaws;
        }
        public static List<CreatureAIScript> GetCreatureAIScripts()
        {
            List<CreatureAIScript> aiScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_ai_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureAIScript aiScript = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };
                        aiScripts.Add(aiScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return aiScripts;
        }
        public static List<CreatureInvolvedRelation> GetCreatureInvolvedRelations()
        {
            List<CreatureInvolvedRelation> relations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_involvedrelation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureInvolvedRelation relation = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Quest = reader.GetUInt32("quest"),
                            Patch = reader.GetUInt32("patch")
                        };
                        relations.Add(relation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return relations;
        }
        public static List<CreatureLinking> GetCreatureLinkings()
        {
            List<CreatureLinking> linkings = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_linking";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureLinking linking = new()
                        {
                            Guid = reader.GetUInt32("guid"),
                            MasterGuid = reader.GetUInt32("master_guid"),
                            Flag = reader.GetUInt32("flag")
                        };
                        linkings.Add(linking);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return linkings;
        }
        public static List<CreatureLinkingTemplate> GetCreatureLinkingTemplates()
        {
            List<CreatureLinkingTemplate> linkingTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_linking_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureLinkingTemplate linkingTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Map = reader.GetUInt32("map"),
                            MasterEntry = reader.GetUInt32("master_entry"),
                            Flag = reader.GetUInt32("flag"),
                            SearchRange = reader.GetUInt32("search_range")
                        };
                        linkingTemplates.Add(linkingTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return linkingTemplates;
        }
        public static List<CreatureLootTemplate> GetCreatureLootTemplates()
        {
            List<CreatureLootTemplate> lootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureLootTemplate lootTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            Groupid = reader.GetUInt32("groupid"),
                            MincountOrRef = reader.GetInt32("mincountOrRef"),
                            Maxcount = reader.GetUInt32("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetUInt32("patch_min"),
                            PatchMax = reader.GetUInt32("patch_max")
                        };
                        lootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return lootTemplates;
        }
        public static List<CreatureModelInfo> GetCreatureModelInfos()
        {
            List<CreatureModelInfo> modelInfos = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_model_info";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureModelInfo modelInfo = new()
                        {
                            Modelid = reader.GetUInt32("modelid"),
                            BoundingRadius = reader.GetFloat("bounding_radius"),
                            CombatReach = reader.GetFloat("combat_reach"),
                            Gender = reader.GetByte("gender"),
                            ModelidOtherGender = reader.GetUInt32("modelid_other_gender"),
                            ModelidOtherTeam = reader.GetUInt32("modelid_other_team")
                        };
                        modelInfos.Add(modelInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return modelInfos;
        }
        public static List<CreatureMovement> GetCreatureMovements()
        {
            List<CreatureMovement> movements = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_movement";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureMovement movement = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Point = reader.GetUInt32("point"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Waittime = reader.GetUInt32("waittime"),
                            ScriptId = reader.GetUInt32("script_id"),
                            Textid1 = reader.GetInt32("textid1"),
                            Textid2 = reader.GetInt32("textid2"),
                            Textid3 = reader.GetInt32("textid3"),
                            Textid4 = reader.GetInt32("textid4"),
                            Textid5 = reader.GetInt32("textid5"),
                            Emote = reader.GetUInt32("emote"),
                            Spell = reader.GetUInt32("spell"),
                            Orientation = reader.GetFloat("orientation"),
                            Model1 = reader.GetUInt32("model1"),
                            Model2 = reader.GetUInt32("model2")
                        };
                        movements.Add(movement);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return movements;
        }
        public static List<CreatureMovementScript> GetCreatureMovementScripts()
        {
            List<CreatureMovementScript> movementScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_movement_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureMovementScript movementScript = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };
                        movementScripts.Add(movementScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return movementScripts;
        }
        public static List<CreatureMovementSpecial> GetCreatureMovementSpecials()
        {
            List<CreatureMovementSpecial> movementSpecials = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_movement_special";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureMovementSpecial movementSpecial = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Point = reader.GetUInt32("point"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Waittime = reader.GetUInt32("waittime"),
                            ScriptId = reader.GetUInt32("script_id"),
                            Textid1 = reader.GetUInt32("textid1"),
                            Textid2 = reader.GetUInt32("textid2"),
                            Textid3 = reader.GetUInt32("textid3"),
                            Textid4 = reader.GetUInt32("textid4"),
                            Textid5 = reader.GetUInt32("textid5"),
                            Emote = reader.GetUInt32("emote"),
                            Spell = reader.GetUInt32("spell"),
                            Orientation = reader.GetFloat("orientation"),
                            Model1 = reader.GetUInt32("model1"),
                            Model2 = reader.GetUInt32("model2")
                        };
                        movementSpecials.Add(movementSpecial);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return movementSpecials;
        }
        public static List<CreatureMovementTemplate> GetCreatureMovementTemplates()
        {
            List<CreatureMovementTemplate> movementTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_movement_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureMovementTemplate movementTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Point = reader.GetUInt32("point"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Waittime = reader.GetUInt32("waittime"),
                            ScriptId = reader.GetUInt32("script_id"),
                            Textid1 = reader.GetUInt32("textid1"),
                            Textid2 = reader.GetUInt32("textid2"),
                            Textid3 = reader.GetUInt32("textid3"),
                            Textid4 = reader.GetUInt32("textid4"),
                            Textid5 = reader.GetUInt32("textid5"),
                            Emote = reader.GetUInt32("emote"),
                            Spell = reader.GetUInt32("spell"),
                            Orientation = reader.GetFloat("orientation"),
                            Model1 = reader.GetUInt32("model1"),
                            Model2 = reader.GetUInt32("model2")
                        };
                        movementTemplates.Add(movementTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return movementTemplates;
        }
        public static List<CreatureOnKillReputation> GetCreatureOnKillReputations()
        {
            List<CreatureOnKillReputation> reputations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_onkill_reputation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureOnKillReputation reputation = new()
                        {
                            CreatureId = reader.GetUInt32("creature_id"),
                            RewOnKillRepFaction1 = reader.GetInt16("RewOnKillRepFaction1"),
                            RewOnKillRepFaction2 = reader.GetInt16("RewOnKillRepFaction2"),
                            MaxStanding1 = reader.GetSByte("MaxStanding1"),
                            IsTeamAward1 = reader.GetBoolean("IsTeamAward1"),
                            RewOnKillRepValue1 = reader.GetInt32("RewOnKillRepValue1"),
                            MaxStanding2 = reader.GetSByte("MaxStanding2"),
                            IsTeamAward2 = reader.GetBoolean("IsTeamAward2"),
                            RewOnKillRepValue2 = reader.GetInt32("RewOnKillRepValue2"),
                            TeamDependent = reader.GetBoolean("TeamDependent")
                        };
                        reputations.Add(reputation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return reputations;
        }
        public static List<CreatureQuestRelation> GetCreatureQuestRelations()
        {
            List<CreatureQuestRelation> creatureQuestRelations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_questrelation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureQuestRelation creatureQuestRelation = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Quest = reader.GetUInt32("quest"),
                            Patch = reader.GetByte("patch")
                        };
                        creatureQuestRelations.Add(creatureQuestRelation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatureQuestRelations;
        }
        public static List<CreatureSpell> GetCreatureSpells()
        {
            List<CreatureSpell> creatureSpells = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_spells";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureSpell creatureSpell = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Name = reader.GetString("name"),

                            SpellId1 = reader.GetUInt16("spellId_1"),
                            Probability1 = reader.GetByte("probability_1"),
                            CastTarget1 = reader.GetByte("castTarget_1"),
                            TargetParam11 = reader.GetUInt16("targetParam1_1"),
                            TargetParam21 = reader.GetUInt16("targetParam2_1"),
                            CastFlags1 = reader.GetByte("castFlags_1"),
                            DelayInitialMin1 = reader.GetUInt16("delayInitialMin_1"),
                            DelayInitialMax1 = reader.GetUInt16("delayInitialMax_1"),
                            DelayRepeatMin1 = reader.GetUInt16("delayRepeatMin_1"),
                            DelayRepeatMax1 = reader.GetUInt16("delayRepeatMax_1"),
                            ScriptId1 = reader.GetUInt32("scriptId_1"),

                            SpellId2 = reader.GetUInt16("spellId_2"),
                            Probability2 = reader.GetByte("probability_2"),
                            CastTarget2 = reader.GetByte("castTarget_2"),
                            TargetParam12 = reader.GetUInt16("targetParam1_2"),
                            TargetParam22 = reader.GetUInt16("targetParam2_2"),
                            CastFlags2 = reader.GetByte("castFlags_2"),
                            DelayInitialMin2 = reader.GetUInt16("delayInitialMin_2"),
                            DelayInitialMax2 = reader.GetUInt16("delayInitialMax_2"),
                            DelayRepeatMin2 = reader.GetUInt16("delayRepeatMin_2"),
                            DelayRepeatMax2 = reader.GetUInt16("delayRepeatMax_2"),
                            ScriptId2 = reader.GetUInt32("scriptId_2"),

                            SpellId3 = reader.GetUInt16("spellId_3"),
                            Probability3 = reader.GetByte("probability_3"),
                            CastTarget3 = reader.GetByte("castTarget_3"),
                            TargetParam13 = reader.GetUInt16("targetParam1_3"),
                            TargetParam23 = reader.GetUInt16("targetParam2_3"),
                            CastFlags3 = reader.GetByte("castFlags_3"),
                            DelayInitialMin3 = reader.GetUInt16("delayInitialMin_3"),
                            DelayInitialMax3 = reader.GetUInt16("delayInitialMax_3"),
                            DelayRepeatMin3 = reader.GetUInt16("delayRepeatMin_3"),
                            DelayRepeatMax3 = reader.GetUInt16("delayRepeatMax_3"),
                            ScriptId3 = reader.GetUInt32("scriptId_3"),

                            SpellId4 = reader.GetUInt16("spellId_4"),
                            Probability4 = reader.GetByte("probability_4"),
                            CastTarget4 = reader.GetByte("castTarget_4"),
                            TargetParam14 = reader.GetUInt16("targetParam1_4"),
                            TargetParam24 = reader.GetUInt16("targetParam2_4"),
                            CastFlags4 = reader.GetByte("castFlags_4"),
                            DelayInitialMin4 = reader.GetUInt16("delayInitialMin_4"),
                            DelayInitialMax4 = reader.GetUInt16("delayInitialMax_4"),
                            DelayRepeatMin4 = reader.GetUInt16("delayRepeatMin_4"),
                            DelayRepeatMax4 = reader.GetUInt16("delayRepeatMax_4"),
                            ScriptId4 = reader.GetUInt32("scriptId_4"),

                            SpellId5 = reader.GetUInt16("spellId_5"),
                            Probability5 = reader.GetByte("probability_5"),
                            CastTarget5 = reader.GetByte("castTarget_5"),
                            TargetParam15 = reader.GetUInt16("targetParam1_5"),
                            TargetParam25 = reader.GetUInt16("targetParam2_5"),
                            CastFlags5 = reader.GetByte("castFlags_5"),
                            DelayInitialMin5 = reader.GetUInt16("delayInitialMin_5"),
                            DelayInitialMax5 = reader.GetUInt16("delayInitialMax_5"),
                            DelayRepeatMin5 = reader.GetUInt16("delayRepeatMin_5"),
                            DelayRepeatMax5 = reader.GetUInt16("delayRepeatMax_5"),
                            ScriptId5 = reader.GetUInt32("scriptId_5"),

                            SpellId6 = reader.GetUInt16("spellId_6"),
                            Probability6 = reader.GetByte("probability_6"),
                            CastTarget6 = reader.GetByte("castTarget_6"),
                            TargetParam16 = reader.GetUInt16("targetParam1_6"),
                            TargetParam26 = reader.GetUInt16("targetParam2_6"),
                            CastFlags6 = reader.GetByte("castFlags_6"),
                            DelayInitialMin6 = reader.GetUInt16("delayInitialMin_6"),
                            DelayInitialMax6 = reader.GetUInt16("delayInitialMax_6"),
                            DelayRepeatMin6 = reader.GetUInt16("delayRepeatMin_6"),
                            DelayRepeatMax6 = reader.GetUInt16("delayRepeatMax_6"),
                            ScriptId6 = reader.GetUInt32("scriptId_6"),

                            SpellId7 = reader.GetUInt16("spellId_7"),
                            Probability7 = reader.GetByte("probability_7"),
                            CastTarget7 = reader.GetByte("castTarget_7"),
                            TargetParam17 = reader.GetUInt16("targetParam1_7"),
                            TargetParam27 = reader.GetUInt16("targetParam2_7"),
                            CastFlags7 = reader.GetByte("castFlags_7"),
                            DelayInitialMin7 = reader.GetUInt16("delayInitialMin_7"),
                            DelayInitialMax7 = reader.GetUInt16("delayInitialMax_7"),
                            DelayRepeatMin7 = reader.GetUInt16("delayRepeatMin_7"),
                            DelayRepeatMax7 = reader.GetUInt16("delayRepeatMax_7"),
                            ScriptId7 = reader.GetUInt32("scriptId_7"),

                            SpellId8 = reader.GetUInt16("spellId_8"),
                            Probability8 = reader.GetByte("probability_8"),
                            CastTarget8 = reader.GetByte("castTarget_8"),
                            TargetParam18 = reader.GetUInt16("targetParam1_8"),
                            TargetParam28 = reader.GetUInt16("targetParam2_8"),
                            CastFlags8 = reader.GetByte("castFlags_8"),
                            DelayInitialMin8 = reader.GetUInt16("delayInitialMin_8"),
                            DelayInitialMax8 = reader.GetUInt16("delayInitialMax_8"),
                            DelayRepeatMin8 = reader.GetUInt16("delayRepeatMin_8"),
                            DelayRepeatMax8 = reader.GetUInt16("delayRepeatMax_8"),
                            ScriptId8 = reader.GetUInt32("scriptId_8")
                        };
                        creatureSpells.Add(creatureSpell);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatureSpells;
        }
        public static List<CreatureSpellScript> GetCreatureSpellsScripts()
        {
            List<CreatureSpellScript> creatureSpellsScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_spells_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureSpellScript creatureSpellsScript = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };
                        creatureSpellsScripts.Add(creatureSpellsScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatureSpellsScripts;
        }
        public static List<CreatureTemplate> GetCreatureTemplates()
        {
            List<CreatureTemplate> creatureTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureTemplate creatureTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            KillCredit1 = reader.GetUInt32("KillCredit1"),
                            KillCredit2 = reader.GetUInt32("KillCredit2"),
                            ModelId1 = reader.GetUInt32("modelid_1"),
                            ModelId2 = reader.GetUInt32("modelid_2"),
                            ModelId3 = reader.GetUInt32("modelid_3"),
                            ModelId4 = reader.GetUInt32("modelid_4"),
                            Name = reader.GetString("name"),
                            Subname = reader.IsDBNull("subname") ? string.Empty : reader.GetString("subname"),
                            GossipMenuId = reader.GetUInt32("gossip_menu_id"),
                            MinLevel = reader.GetByte("minlevel"),
                            MaxLevel = reader.GetByte("maxlevel"),
                            MinHealth = reader.GetUInt32("minhealth"),
                            MaxHealth = reader.GetUInt32("maxhealth"),
                            MinMana = reader.GetUInt32("minmana"),
                            MaxMana = reader.GetUInt32("maxmana"),
                            Armor = reader.GetUInt32("armor"),
                            FactionA = reader.GetUInt16("faction_A"),
                            FactionH = reader.GetUInt16("faction_H"),
                            NpcFlag = reader.GetUInt32("npcflag"),
                            SpeedWalk = reader.GetFloat("speed_walk"),
                            SpeedRun = reader.GetFloat("speed_run"),
                            Scale = reader.GetFloat("scale"),
                            Rank = reader.GetByte("rank"),
                            MinDmg = reader.GetFloat("mindmg"),
                            MaxDmg = reader.GetFloat("maxdmg"),
                            DmgSchool = reader.GetByte("dmgschool"),
                            AttackPower = reader.GetUInt32("attackpower"),
                            DmgMultiplier = reader.GetFloat("dmg_multiplier"),
                            BaseAttackTime = reader.GetUInt32("baseattacktime"),
                            RangeAttackTime = reader.GetUInt32("rangeattacktime"),
                            UnitClass = reader.GetByte("unit_class"),
                            UnitFlags = reader.GetUInt32("unit_flags"),
                            DynamicFlags = reader.GetUInt32("dynamicflags"),
                            Family = reader.GetByte("family"),
                            TrainerType = reader.GetByte("trainer_type"),
                            TrainerSpell = reader.GetUInt32("trainer_spell"),
                            TrainerClass = reader.GetByte("trainer_class"),
                            TrainerRace = reader.GetByte("trainer_race"),
                            MinRangedDmg = reader.GetFloat("minrangedmg"),
                            MaxRangedDmg = reader.GetFloat("maxrangedmg"),
                            RangedAttackPower = reader.GetUInt16("rangedattackpower"),
                            Type = reader.GetByte("type"),
                            TypeFlags = reader.GetUInt32("type_flags"),
                            LootId = reader.GetUInt32("lootid"),
                            PickpocketLoot = reader.GetUInt32("pickpocketloot"),
                            SkinLoot = reader.GetUInt32("skinloot"),
                            Resistance1 = reader.GetInt16("resistance1"),
                            Resistance2 = reader.GetInt16("resistance2"),
                            Resistance3 = reader.GetInt16("resistance3"),
                            Resistance4 = reader.GetInt16("resistance4"),
                            Resistance5 = reader.GetInt16("resistance5"),
                            Resistance6 = reader.GetInt16("resistance6"),
                            Spell1 = reader.GetUInt32("spell1"),
                            Spell2 = reader.GetUInt32("spell2"),
                            Spell3 = reader.GetUInt32("spell3"),
                            Spell4 = reader.GetUInt32("spell4"),
                            SpellsTemplate = reader.GetUInt32("spells_template"),
                            PetSpellDataId = reader.GetUInt32("PetSpellDataId"),
                            MinGold = reader.GetUInt32("mingold"),
                            MaxGold = reader.GetUInt32("maxgold"),
                            AiName = reader.GetString("AIName"),
                            MovementType = reader.GetByte("MovementType"),
                            InhabitType = reader.GetByte("InhabitType"),
                            Civilian = reader.GetByte("Civilian"),
                            RacialLeader = reader.GetByte("RacialLeader"),
                            RegenHealth = reader.GetByte("RegenHealth"),
                            EquipmentId = reader.GetUInt32("equipment_id"),
                            TrainerId = reader.GetUInt32("trainer_id"),
                            VendorId = reader.GetUInt32("vendor_id"),
                            MechanicImmuneMask = reader.GetUInt32("MechanicImmuneMask"),
                            SchoolImmuneMask = reader.GetUInt32("SchoolImmuneMask"),
                            FlagsExtra = reader.GetUInt32("flags_extra"),
                            ScriptName = reader.GetString("ScriptName")
                        };
                        creatureTemplates.Add(creatureTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatureTemplates;
        }
        public static List<CreatureTemplateAddon> GetCreatureTemplateAddons()
        {
            List<CreatureTemplateAddon> creatureTemplateAddons = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM creature_template_addon";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CreatureTemplateAddon creatureTemplateAddon = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            Mount = reader.GetUInt32("mount"),
                            Bytes1 = reader.GetUInt32("bytes1"),
                            B20Sheath = reader.GetByte("b2_0_sheath"),
                            B21Flags = reader.GetByte("b2_1_flags"),
                            Emote = reader.GetUInt32("emote"),
                            Moveflags = reader.GetUInt32("moveflags"),
                            Auras = reader.IsDBNull("auras") ? string.Empty : reader.GetString("auras")
                        };
                        creatureTemplateAddons.Add(creatureTemplateAddon);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatureTemplateAddons;
        }
        public static List<CustomText> GetCustomTexts()
        {
            List<CustomText> customTexts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM custom_texts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CustomText customText = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            ContentDefault = reader.GetString("content_default"),
                            ContentLoc1 = reader.IsDBNull("content_loc1") ? string.Empty : reader.GetString("content_loc1"),
                            ContentLoc2 = reader.IsDBNull("content_loc2") ? string.Empty : reader.GetString("content_loc2"),
                            ContentLoc3 = reader.IsDBNull("content_loc3") ? string.Empty : reader.GetString("content_loc3"),
                            ContentLoc4 = reader.IsDBNull("content_loc4") ? string.Empty : reader.GetString("content_loc4"),
                            ContentLoc5 = reader.IsDBNull("content_loc5") ? string.Empty : reader.GetString("content_loc5"),
                            ContentLoc6 = reader.IsDBNull("content_loc6") ? string.Empty : reader.GetString("content_loc6"),
                            ContentLoc7 = reader.IsDBNull("content_loc7") ? string.Empty : reader.GetString("content_loc7"),
                            ContentLoc8 = reader.IsDBNull("content_loc8") ? string.Empty : reader.GetString("content_loc8"),
                            Sound = reader.GetUInt32("sound"),
                            Type = reader.GetByte("type"),
                            Language = reader.GetByte("language"),
                            Emote = reader.GetUInt16("emote"),
                            Comment = reader.IsDBNull("comment") ? string.Empty : reader.GetString("comment")
                        };
                        customTexts.Add(customText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return customTexts;
        }
        public static List<DisenchantLootTemplate> GetDisenchantLootTemplates()
        {
            List<DisenchantLootTemplate> disenchantLootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM disenchant_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DisenchantLootTemplate disenchantLootTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            Groupid = reader.GetByte("groupid"),
                            MincountOrRef = reader.GetUInt32("mincountOrRef"),
                            Maxcount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };
                        disenchantLootTemplates.Add(disenchantLootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return disenchantLootTemplates;
        }
        public static List<EventScript> GetEventScripts()
        {
            List<EventScript> eventScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM event_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        EventScript eventScript = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetUInt32("dataint"),
                            Dataint2 = reader.GetUInt32("dataint2"),
                            Dataint3 = reader.GetUInt32("dataint3"),
                            Dataint4 = reader.GetUInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };
                        eventScripts.Add(eventScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return eventScripts;
        }
        public static List<ExplorationBaseXP> GetExplorationBaseXPs()
        {
            List<ExplorationBaseXP> explorationBaseXPs = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM exploration_basexp";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ExplorationBaseXP explorationBaseXP = new()
                        {
                            Level = reader.GetByte("level"),
                            Basexp = reader.GetUInt32("basexp")
                        };
                        explorationBaseXPs.Add(explorationBaseXP);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return explorationBaseXPs;
        }
        public static List<Faction> GetFactions()
        {
            List<Faction> factions = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM faction";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Faction faction = new()
                        {
                            Id = reader.GetUInt32("ID"),
                            ReputationListID = reader.GetInt32("reputationListID"),
                            BaseRepRaceMask1 = reader.GetUInt32("baseRepRaceMask1"),
                            BaseRepRaceMask2 = reader.GetUInt32("baseRepRaceMask2"),
                            BaseRepRaceMask3 = reader.GetUInt32("baseRepRaceMask3"),
                            BaseRepRaceMask4 = reader.GetUInt32("baseRepRaceMask4"),
                            BaseRepClassMask1 = reader.GetUInt32("baseRepClassMask1"),
                            BaseRepClassMask2 = reader.GetUInt32("baseRepClassMask2"),
                            BaseRepClassMask3 = reader.GetUInt32("baseRepClassMask3"),
                            BaseRepClassMask4 = reader.GetUInt32("baseRepClassMask4"),
                            BaseRepValue1 = reader.GetInt32("baseRepValue1"),
                            BaseRepValue2 = reader.GetInt32("baseRepValue2"),
                            BaseRepValue3 = reader.GetInt32("baseRepValue3"),
                            BaseRepValue4 = reader.GetInt32("baseRepValue4"),
                            ReputationFlags1 = reader.GetUInt32("reputationFlags1"),
                            ReputationFlags2 = reader.GetUInt32("reputationFlags2"),
                            ReputationFlags3 = reader.GetUInt32("reputationFlags3"),
                            ReputationFlags4 = reader.GetUInt32("reputationFlags4"),
                            Team = reader.GetUInt32("team"),
                            Name1 = reader.GetString("name1"),
                            Name2 = reader.GetString("name2"),
                            Name3 = reader.GetString("name3"),
                            Name4 = reader.GetString("name4"),
                            Name5 = reader.GetString("name5"),
                            Name6 = reader.GetString("name6"),
                            Name7 = reader.GetString("name7"),
                            Name8 = reader.GetString("name8"),
                            Description1 = reader.GetString("description1"),
                            Description2 = reader.GetString("description2"),
                            Description3 = reader.GetString("description3"),
                            Description4 = reader.GetString("description4"),
                            Description5 = reader.GetString("description5"),
                            Description6 = reader.GetString("description6"),
                            Description7 = reader.GetString("description7"),
                            Description8 = reader.GetString("description8")
                        };
                        factions.Add(faction);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factions;
        }
        public static List<FactionTemplate> GetFactionTemplates()
        {
            List<FactionTemplate> factionTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM faction_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        FactionTemplate factionTemplate = new()
                        {
                            Id = reader.GetUInt16("ID"),
                            FactionId = reader.GetUInt32("factionId"),
                            FactionFlags = reader.GetUInt32("factionFlags"),
                            OurMask = reader.GetUInt32("ourMask"),
                            FriendlyMask = reader.GetUInt32("friendlyMask"),
                            HostileMask = reader.GetUInt32("hostileMask"),
                            EnemyFaction1 = reader.GetUInt32("enemyFaction1"),
                            EnemyFaction2 = reader.GetUInt32("enemyFaction2"),
                            EnemyFaction3 = reader.GetUInt32("enemyFaction3"),
                            EnemyFaction4 = reader.GetUInt32("enemyFaction4"),
                            FriendFaction1 = reader.GetUInt32("friendFaction1"),
                            FriendFaction2 = reader.GetUInt32("friendFaction2"),
                            FriendFaction3 = reader.GetUInt32("friendFaction3"),
                            FriendFaction4 = reader.GetUInt32("friendFaction4")
                        };
                        factionTemplates.Add(factionTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factionTemplates;
        }
        public static List<FishingLootTemplate> GetFishingLootTemplates()
        {
            List<FishingLootTemplate> fishingLootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM fishing_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var fishingLootTemplate = new FishingLootTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            GroupId = reader.GetByte("groupid"),
                            MinCountOrRef = reader.GetUInt32("mincountOrRef"),
                            MaxCount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        fishingLootTemplates.Add(fishingLootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return fishingLootTemplates;
        }
        public static List<ForbiddenItem> GetForbiddenItems()
        {
            List<ForbiddenItem> forbiddenItems = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM forbidden_items";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var forbiddenItem = new ForbiddenItem
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            AfterOrBefore = reader.GetByte("AfterOrBefore")
                        };

                        forbiddenItems.Add(forbiddenItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return forbiddenItems;
        }
        public static List<GameObject> GetGameObjects()
        {
            List<GameObject> gameObjects = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameObject = new GameObject
                        {
                            Guid = reader.GetUInt32("guid"),
                            Id = reader.GetUInt32("id"),
                            Map = reader.GetUInt16("map"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Orientation = reader.GetFloat("orientation"),
                            Rotation0 = reader.GetFloat("rotation0"),
                            Rotation1 = reader.GetFloat("rotation1"),
                            Rotation2 = reader.GetFloat("rotation2"),
                            Rotation3 = reader.GetFloat("rotation3"),
                            Spawntimesecsmin = reader.GetInt32("spawntimesecsmin"),
                            Spawntimesecsmax = reader.GetInt32("spawntimesecsmax"),
                            Animprogress = reader.GetByte("animprogress"),
                            State = reader.GetByte("state"),
                            SpawnFlags = reader.GetUInt32("spawnFlags"),
                            Visibilitymod = reader.GetFloat("visibilitymod"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        gameObjects.Add(gameObject);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameObjects;
        }
        public static List<GameObjectBattleground> GetGameObjectBattlegrounds()
        {
            List<GameObjectBattleground> gameObjectBattlegrounds = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_battleground";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameObjectBattleground = new GameObjectBattleground
                        {
                            Guid = reader.GetUInt32("guid"),
                            Event1 = reader.GetByte("event1"),
                            Event2 = reader.GetByte("event2")
                        };

                        gameObjectBattlegrounds.Add(gameObjectBattleground);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameObjectBattlegrounds;
        }
        public static List<GameObjectInvolvedRelation> GetGameObjectInvolvedRelations()
        {
            List<GameObjectInvolvedRelation> relations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_involvedrelation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        GameObjectInvolvedRelation relation = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Quest = reader.GetUInt32("quest"),
                            Patch = reader.GetByte("patch")
                        };
                        relations.Add(relation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return relations;
        }
        public static List<GameObjectLootTemplate> GetGameObjectLootTemplates()
        {
            List<GameObjectLootTemplate> lootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var lootTemplate = new GameObjectLootTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            Groupid = reader.GetByte("groupid"),
                            MincountOrRef = reader.GetInt32("mincountOrRef"),
                            Maxcount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        lootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return lootTemplates;
        }
        public static List<GameObjectQuestRelation> GetGameObjectQuestRelations()
        {
            List<GameObjectQuestRelation> questRelations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_questrelation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var questRelation = new GameObjectQuestRelation
                        {
                            Id = reader.GetUInt32("id"),
                            Quest = reader.GetUInt32("quest"),
                            Patch = reader.GetByte("patch")
                        };

                        questRelations.Add(questRelation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return questRelations;
        }
        public static List<GameObjectRequirement> GetGameObjectRequirements()
        {
            List<GameObjectRequirement> requirements = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_requirement";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var requirement = new GameObjectRequirement
                        {
                            Guid = reader.GetUInt32("guid"),
                            ReqType = reader.GetUInt32("reqType"),
                            ReqGuid = reader.GetUInt32("reqGuid")
                        };

                        requirements.Add(requirement);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return requirements;
        }
        public static List<GameObjectScript> GetGameObjectScripts()
        {
            List<GameObjectScript> scripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var script = new GameObjectScript
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };

                        scripts.Add(script);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return scripts;
        }
        public static List<GameObjectTemplate> GetGameObjectTemplates()
        {
            List<GameObjectTemplate> gameObjectTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gameobject_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var template = new GameObjectTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            Type = reader.GetByte("type"),
                            DisplayId = reader.GetUInt32("displayId"),
                            Name = reader.GetString("name"),
                            Faction = reader.GetUInt16("faction"),
                            Flags = reader.GetUInt32("flags"),
                            Size = reader.GetFloat("size"),
                            Mingold = reader.GetUInt32("mingold"),
                            Maxgold = reader.GetUInt32("maxgold"),
                            ScriptName = reader.GetString("ScriptName")
                        };

                        template.Data.Add(reader.GetUInt32("data0"));
                        template.Data.Add(reader.GetUInt32("data1"));
                        template.Data.Add(reader.GetUInt32("data2"));
                        template.Data.Add(reader.GetUInt32("data3"));
                        template.Data.Add(reader.GetUInt32("data4"));
                        template.Data.Add(reader.GetUInt32("data5"));
                        template.Data.Add(reader.GetUInt32("data6"));
                        template.Data.Add(reader.GetUInt32("data7"));
                        template.Data.Add(reader.GetUInt32("data8"));
                        template.Data.Add(reader.GetUInt32("data9"));
                        template.Data.Add(reader.GetUInt32("data10"));
                        template.Data.Add(reader.GetUInt32("data11"));
                        template.Data.Add(reader.GetUInt32("data12"));
                        template.Data.Add(reader.GetUInt32("data13"));
                        template.Data.Add(reader.GetUInt32("data14"));
                        template.Data.Add(reader.GetUInt32("data15"));
                        template.Data.Add(reader.GetUInt32("data16"));
                        template.Data.Add(reader.GetUInt32("data17"));
                        template.Data.Add(reader.GetUInt32("data18"));
                        template.Data.Add(reader.GetUInt32("data19"));
                        template.Data.Add(reader.GetUInt32("data20"));
                        template.Data.Add(reader.GetUInt32("data21"));
                        template.Data.Add(reader.GetUInt32("data22"));
                        template.Data.Add(reader.GetUInt32("data23"));

                        gameObjectTemplates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameObjectTemplates;
        }
        public static List<GameEvent> GetGameEvents()
        {
            List<GameEvent> gameEvents = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_event";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameEvent = new GameEvent
                        {
                            Entry = reader.GetUInt32("entry"),
                            StartTime = GetTimestampSafe(reader, "start_time"),
                            EndTime = GetTimestampSafe(reader, "end_time"),
                            Occurrence = reader.GetUInt64("occurence"),
                            Length = reader.GetUInt64("length"),
                            Holiday = reader.GetUInt32("holiday"),
                            Description = reader.IsDBNull("description") ? string.Empty : reader.GetString("description"),
                            Hardcoded = reader.GetBoolean("hardcoded"),
                            Disabled = reader.GetBoolean("disabled"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        gameEvents.Add(gameEvent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameEvents;
        }
        public static List<GameEventCreature> GetGameEventCreatures()
        {
            List<GameEventCreature> gameEventCreatures = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_event_creature";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameEventCreature = new GameEventCreature
                        {
                            Guid = reader.GetUInt32("guid"),
                            Event = reader.GetInt16("event")
                        };

                        gameEventCreatures.Add(gameEventCreature);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameEventCreatures;
        }
        public static List<GameEventCreatureData> GetGameEventCreatureDatas()
        {
            List<GameEventCreatureData> gameEventCreatureDataList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_event_creature_data";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameEventCreatureData = new GameEventCreatureData
                        {
                            Guid = reader.GetUInt32("guid"),
                            EntryId = reader.GetUInt32("entry_id"),
                            Modelid = reader.GetUInt32("modelid"),
                            EquipmentId = reader.GetUInt32("equipment_id"),
                            SpellStart = reader.GetUInt32("spell_start"),
                            SpellEnd = reader.GetUInt32("spell_end"),
                            Event = reader.GetUInt16("event")
                        };

                        gameEventCreatureDataList.Add(gameEventCreatureData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameEventCreatureDataList;
        }
        public static List<GameEventGameObject> GetGameEventGameObjects()
        {
            List<GameEventGameObject> gameEventGameObjectList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_event_gameobject";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameEventGameObject = new GameEventGameObject
                        {
                            Guid = reader.GetUInt32("guid"),
                            Event = reader.GetInt16("event")
                        };

                        gameEventGameObjectList.Add(gameEventGameObject);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameEventGameObjectList;
        }
        public static List<GameEventQuest> GetGameEventQuests()
        {
            List<GameEventQuest> gameEventQuestList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_event_quest";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameEventQuest = new GameEventQuest
                        {
                            Quest = reader.GetUInt32("quest"),
                            Event = reader.GetUInt16("event"),
                            Patch = reader.GetByte("patch")
                        };

                        gameEventQuestList.Add(gameEventQuest);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameEventQuestList;
        }
        public static List<GameGraveyardZone> GetGameGraveyardZones()
        {
            List<GameGraveyardZone> gameGraveyardZones = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_graveyard_zone";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameGraveyardZone = new GameGraveyardZone
                        {
                            Id = reader.GetUInt32("id"),
                            GhostZone = reader.GetUInt32("ghost_zone"),
                            Faction = reader.GetUInt16("faction")
                        };

                        gameGraveyardZones.Add(gameGraveyardZone);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameGraveyardZones;
        }
        public static List<GameTele> GetGameTeles()
        {
            List<GameTele> gameTeles = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_tele";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameTele = new GameTele
                        {
                            Id = reader.GetUInt32("id"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Orientation = reader.GetFloat("orientation"),
                            Map = reader.GetUInt16("map"),
                            Name = reader.GetString("name")
                        };

                        gameTeles.Add(gameTele);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameTeles;
        }
        public static List<GameWeather> GetGameWeathers()
        {
            List<GameWeather> gameWeathers = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM game_weather";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        GameWeather gameWeather = new()
                        {
                            Zone = reader.GetUInt32("zone"),
                            SpringRainChance = reader.GetByte("spring_rain_chance"),
                            SpringSnowChance = reader.GetByte("spring_snow_chance"),
                            SpringStormChance = reader.GetByte("spring_storm_chance"),
                            SummerRainChance = reader.GetByte("summer_rain_chance"),
                            SummerSnowChance = reader.GetByte("summer_snow_chance"),
                            SummerStormChance = reader.GetByte("summer_storm_chance"),
                            FallRainChance = reader.GetByte("fall_rain_chance"),
                            FallSnowChance = reader.GetByte("fall_snow_chance"),
                            FallStormChance = reader.GetByte("fall_storm_chance"),
                            WinterRainChance = reader.GetByte("winter_rain_chance"),
                            WinterSnowChance = reader.GetByte("winter_snow_chance"),
                            WinterStormChance = reader.GetByte("winter_storm_chance")
                        };
                        gameWeathers.Add(gameWeather);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameWeathers;
        }
        public static List<GMSubSurvey> GetGMSubSurveys()
        {
            List<GMSubSurvey> gmSubSurveys = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gm_subsurveys";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gmSubSurvey = new GMSubSurvey
                        {
                            SurveyId = reader.GetUInt32("surveyId"),
                            SubsurveyId = reader.GetUInt32("subsurveyId"),
                            Rank = reader.GetUInt32("rank"),
                            Comment = reader.GetString("comment")
                        };

                        gmSubSurveys.Add(gmSubSurvey);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gmSubSurveys;
        }
        public static List<GMSurvey> GetGmSurveys()
        {
            List<GMSurvey> gmSurveys = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gm_surveys";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gmSurvey = new GMSurvey
                        {
                            SurveyId = reader.GetUInt32("surveyId"),
                            Guid = reader.GetUInt32("guid"),
                            MainSurvey = reader.GetUInt32("mainSurvey"),
                            OverallComment = reader.GetString("overallComment"),
                            CreateTime = reader.GetUInt32("createTime")
                        };

                        gmSurveys.Add(gmSurvey);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gmSurveys;
        }
        public static List<GMTicket> GetGmTickets()
        {
            List<GMTicket> gmTickets = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gm_tickets";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gmTicket = new GMTicket
                        {
                            TicketId = reader.GetUInt32("ticketId"),
                            Guid = reader.GetUInt32("guid"),
                            Name = reader.GetString("name"),
                            Message = reader.GetString("message"),
                            CreateTime = reader.GetUInt32("createTime"),
                            MapId = reader.GetUInt16("mapId"),
                            PosX = reader.GetFloat("posX"),
                            PosY = reader.GetFloat("posY"),
                            PosZ = reader.GetFloat("posZ"),
                            LastModifiedTime = reader.GetUInt32("lastModifiedTime"),
                            ClosedBy = reader.GetUInt32("closedBy"),
                            AssignedTo = reader.GetUInt32("assignedTo"),
                            Comment = reader.GetString("comment"),
                            Response = reader.GetString("response"),
                            Completed = reader.GetByte("completed"),
                            Escalated = reader.GetByte("escalated"),
                            Viewed = reader.GetByte("viewed"),
                            HaveTicket = reader.GetByte("haveTicket"),
                            TicketType = reader.GetByte("ticketType"),
                            SecurityNeeded = reader.GetByte("securityNeeded")
                        };

                        gmTickets.Add(gmTicket);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gmTickets;
        }
        public static List<GossipMenu> GetGossipMenus()
        {
            List<GossipMenu> gossipMenus = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gossip_menu";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gossipMenu = new GossipMenu
                        {
                            Entry = reader.GetUInt16("entry"),
                            TextId = reader.GetUInt32("text_id"),
                            ConditionId = reader.GetUInt32("condition_id")
                        };

                        gossipMenus.Add(gossipMenu);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gossipMenus;
        }
        public static List<GossipMenuOption> GetGossipMenuOptions()
        {
            List<GossipMenuOption> gossipMenuOptions = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gossip_menu_option";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gossipMenuOption = new GossipMenuOption
                        {
                            MenuId = reader.GetUInt16("menu_id"),
                            Id = reader.GetUInt16("id"),
                            OptionIcon = reader.GetUInt32("option_icon"),
                            OptionText = reader.IsDBNull("option_text") ? string.Empty : reader.GetString("option_text"),
                            OptionBroadcastTextId = reader.GetUInt32("OptionBroadcastTextID"),
                            OptionId = reader.GetByte("option_id"),
                            NpcOptionNpcflag = reader.GetUInt32("npc_option_npcflag"),
                            ActionMenuId = reader.GetInt32("action_menu_id"),
                            ActionPoiId = reader.GetUInt32("action_poi_id"),
                            ActionScriptId = reader.GetUInt32("action_script_id"),
                            BoxCoded = reader.GetByte("box_coded"),
                            BoxMoney = reader.GetUInt32("box_money"),
                            BoxText = reader.IsDBNull("box_text") ? string.Empty : reader.GetString("box_text"),
                            BoxBroadcastTextId = reader.GetUInt32("BoxBroadcastTextID"),
                            ConditionId = reader.GetUInt32("condition_id")
                        };

                        gossipMenuOptions.Add(gossipMenuOption);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gossipMenuOptions;
        }
        public static List<GossipScript> GetGossipScripts()
        {
            List<GossipScript> gossipScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM gossip_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gossipScript = new GossipScript
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.IsDBNull("comments") ? string.Empty : reader.GetString("comments")
                        };

                        gossipScripts.Add(gossipScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gossipScripts;
        }
        public static List<InstanceBuffRemoval> GetInstanceBuffRemovals()
        {
            List<InstanceBuffRemoval> instanceBuffRemovals = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM instance_buff_removal";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var instanceBuffRemoval = new InstanceBuffRemoval
                        {
                            MapId = reader.GetUInt32("mapId"),
                            AuraId = reader.GetUInt32("auraId"),
                            Enabled = reader.GetBoolean("enabled"),
                            Flags = reader.GetUInt32("flags"),
                            Comment = reader.IsDBNull("comment") ? string.Empty : reader.GetString("comment")
                        };

                        instanceBuffRemovals.Add(instanceBuffRemoval);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return instanceBuffRemovals;
        }
        public static List<InstanceCreatureKills> GetInstanceCreatureKills()
        {
            List<InstanceCreatureKills> instanceCreatureKills = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM instance_creature_kills";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var instanceCreatureKill = new InstanceCreatureKills
                        {
                            MapId = reader.GetUInt32("mapId"),
                            CreatureEntry = reader.GetUInt32("creatureEntry"),
                            SpellEntry = reader.GetUInt32("spellEntry"),
                            Count = reader.GetUInt32("count")
                        };

                        instanceCreatureKills.Add(instanceCreatureKill);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return instanceCreatureKills;
        }
        public static List<InstanceCustomCounter> GetInstanceCustomCounters()
        {
            List<InstanceCustomCounter> instanceCustomCounters = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM instance_custom_counters";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var instanceCustomCounter = new InstanceCustomCounter
                        {
                            Index = reader.GetUInt32("index"),
                            Count = reader.GetUInt32("count")
                        };

                        instanceCustomCounters.Add(instanceCustomCounter);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return instanceCustomCounters;
        }
        public static List<InstanceWipe> GetInstanceWipes()
        {
            List<InstanceWipe> instanceWipes = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM instance_wipes";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var instanceWipe = new InstanceWipe
                        {
                            MapId = reader.GetUInt32("mapId"),
                            CreatureEntry = reader.GetUInt32("creatureEntry"),
                            Count = reader.GetUInt32("count")
                        };

                        instanceWipes.Add(instanceWipe);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return instanceWipes;
        }
        public static List<ItemDisplayInfo> GetItemDisplayInfo()
        {
            List<ItemDisplayInfo> itemDisplayInfos = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM item_display_info";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var itemDisplayInfo = new ItemDisplayInfo
                        {
                            Field0 = reader.GetInt32("field0"),
                            Field5 = reader.IsDBNull(reader.GetOrdinal("field5")) ? string.Empty : reader.GetString("field5")
                        };

                        itemDisplayInfos.Add(itemDisplayInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return itemDisplayInfos;
        }
        public static List<ItemEnchantmentTemplate> GetItemEnchantmentTemplates()
        {
            List<ItemEnchantmentTemplate> enchantmentTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM item_enchantment_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var enchantmentTemplate = new ItemEnchantmentTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Ench = reader.GetUInt32("ench"),
                            Chance = reader.GetFloat("chance")
                        };

                        enchantmentTemplates.Add(enchantmentTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return enchantmentTemplates;
        }
        public static List<ItemLootTemplate> GetItemLootTemplates()
        {
            List<ItemLootTemplate> lootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM item_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var lootTemplate = new ItemLootTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            Groupid = reader.GetByte("groupid"),
                            MincountOrRef = reader.GetInt32("mincountOrRef"),
                            Maxcount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        lootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return lootTemplates;
        }
        public static List<ItemRequiredTarget> GetItemRequiredTargets()
        {
            List<ItemRequiredTarget> requiredTargets = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM item_required_target";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var requiredTarget = new ItemRequiredTarget
                        {
                            Entry = reader.GetUInt32("entry"),
                            Type = reader.GetByte("type"),
                            TargetEntry = reader.GetUInt32("targetEntry")
                        };

                        requiredTargets.Add(requiredTarget);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return requiredTargets;
        }
        public static List<ItemTemplate> GetItemTemplates()
        {
            List<ItemTemplate> itemTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM item_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var itemTemplate = new ItemTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            Class = reader.GetByte("class"),
                            Subclass = reader.GetByte("subclass"),
                            Name = reader.GetString("name"),
                            Displayid = reader.GetUInt32("displayid"),
                            Quality = reader.GetByte("Quality"),
                            Flags = reader.GetUInt32("Flags"),
                            BuyCount = reader.GetByte("BuyCount"),
                            BuyPrice = reader.GetUInt32("BuyPrice"),
                            SellPrice = reader.GetUInt32("SellPrice"),
                            InventoryType = reader.GetByte("InventoryType"),
                            AllowableClass = reader.GetInt32("AllowableClass"),
                            AllowableRace = reader.GetInt32("AllowableRace"),
                            ItemLevel = reader.GetByte("ItemLevel"),
                            RequiredLevel = reader.GetByte("RequiredLevel"),
                            RequiredSkill = reader.GetUInt16("RequiredSkill"),
                            RequiredSkillRank = reader.GetUInt16("RequiredSkillRank"),
                            RequiredSpell = reader.GetUInt32("requiredspell"),
                            RequiredHonorRank = reader.GetUInt32("requiredhonorrank"),
                            RequiredCityRank = reader.GetUInt32("RequiredCityRank"),
                            RequiredReputationFaction = reader.GetUInt16("RequiredReputationFaction"),
                            RequiredReputationRank = reader.GetUInt16("RequiredReputationRank"),
                            MaxCount = reader.GetUInt16("maxcount"),
                            Stackable = reader.GetUInt16("stackable"),
                            ContainerSlots = reader.GetByte("ContainerSlots"),
                            Armor = reader.GetUInt16("armor"),
                            Delay = reader.GetUInt16("delay"),
                            AmmoType = reader.GetByte("ammo_type"),
                            RangedModRange = reader.GetUInt32("RangedModRange"),
                            Bonding = reader.GetByte("bonding"),
                            Description = reader.GetString("description"),
                            PageText = reader.GetUInt32("PageText"),
                            LanguageID = reader.GetByte("LanguageID"),
                            PageMaterial = reader.GetByte("PageMaterial"),
                            StartQuest = reader.GetUInt32("startquest"),
                            LockID = reader.GetUInt32("lockid"),
                            Material = reader.GetSByte("Material"),  // Note: Material is signed, so GetSByte is used here
                            Sheath = reader.GetByte("sheath"),
                            RandomProperty = reader.GetUInt32("RandomProperty"),
                            Block = reader.GetUInt32("block"),
                            ItemSet = reader.GetUInt32("itemset"),
                            MaxDurability = reader.GetUInt16("MaxDurability"),
                            Area = reader.GetUInt32("area"),
                            Map = reader.GetUInt16("Map"),
                            BagFamily = reader.GetUInt32("BagFamily"),
                            ScriptName = reader.GetString("ScriptName"),
                            DisenchantID = reader.GetUInt32("DisenchantID"),
                            FoodType = reader.GetByte("FoodType"),
                            MinMoneyLoot = reader.GetUInt32("minMoneyLoot"),
                            MaxMoneyLoot = reader.GetUInt32("maxMoneyLoot"),
                            Duration = reader.GetUInt32("Duration"),
                            ExtraFlags = reader.GetByte("ExtraFlags"),
                            OtherTeamEntry = reader.GetUInt32("OtherTeamEntry")

                        };

                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type1"), Value = reader.GetInt32("stat_value1") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type2"), Value = reader.GetInt32("stat_value2") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type3"), Value = reader.GetInt32("stat_value3") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type4"), Value = reader.GetInt32("stat_value4") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type5"), Value = reader.GetInt32("stat_value5") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type6"), Value = reader.GetInt32("stat_value6") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type7"), Value = reader.GetInt32("stat_value7") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type8"), Value = reader.GetInt32("stat_value8") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type9"), Value = reader.GetInt32("stat_value9") });
                        itemTemplate.Stats.Add(new Stat() { Type = reader.GetUInt32("stat_type10"), Value = reader.GetInt32("stat_value10") });

                        itemTemplate.Damages.Add(new Damage() { Min = reader.GetFloat("dmg_min1"), Max = reader.GetFloat("dmg_max1"), Type = reader.GetByte("dmg_type1") });
                        itemTemplate.Damages.Add(new Damage() { Min = reader.GetFloat("dmg_min2"), Max = reader.GetFloat("dmg_max2"), Type = reader.GetByte("dmg_type2") });
                        itemTemplate.Damages.Add(new Damage() { Min = reader.GetFloat("dmg_min3"), Max = reader.GetFloat("dmg_max3"), Type = reader.GetByte("dmg_type3") });
                        itemTemplate.Damages.Add(new Damage() { Min = reader.GetFloat("dmg_min4"), Max = reader.GetFloat("dmg_max4"), Type = reader.GetByte("dmg_type4") });
                        itemTemplate.Damages.Add(new Damage() { Min = reader.GetFloat("dmg_min5"), Max = reader.GetFloat("dmg_max5"), Type = reader.GetByte("dmg_type5") });

                        itemTemplate.Resistances = new Resistance
                        {
                            Holy = reader.GetUInt16("holy_res"),
                            Fire = reader.GetUInt16("fire_res"),
                            Nature = reader.GetUInt16("nature_res"),
                            Frost = reader.GetUInt16("frost_res"),
                            Shadow = reader.GetUInt16("shadow_res"),
                            Arcane = reader.GetUInt16("arcane_res")
                        };

                        itemTemplate.Spells.Add(new Spell()
                        {
                            SpellID = reader.GetUInt32("spellid_1"),
                            Trigger = reader.GetByte("spelltrigger_1"),
                            Charges = reader.GetInt16("spellcharges_1"),
                            PpmRate = reader.GetFloat("spellppmRate_1"),
                            Cooldown = reader.GetInt32("spellcooldown_1"),
                            Category = reader.GetUInt16("spellcategory_1"),
                            CategoryCooldown = reader.GetInt32("spellcategorycooldown_1")
                        });
                        itemTemplate.Spells.Add(new Spell()
                        {
                            SpellID = reader.GetUInt32("spellid_2"),
                            Trigger = reader.GetByte("spelltrigger_2"),
                            Charges = reader.GetInt16("spellcharges_2"),
                            PpmRate = reader.GetFloat("spellppmRate_2"),
                            Cooldown = reader.GetInt32("spellcooldown_2"),
                            Category = reader.GetUInt16("spellcategory_2"),
                            CategoryCooldown = reader.GetInt32("spellcategorycooldown_2")
                        });
                        itemTemplate.Spells.Add(new Spell()
                        {
                            SpellID = reader.GetUInt32("spellid_3"),
                            Trigger = reader.GetByte("spelltrigger_3"),
                            Charges = reader.GetInt16("spellcharges_3"),
                            PpmRate = reader.GetFloat("spellppmRate_3"),
                            Cooldown = reader.GetInt32("spellcooldown_3"),
                            Category = reader.GetUInt16("spellcategory_3"),
                            CategoryCooldown = reader.GetInt32("spellcategorycooldown_3")
                        });
                        itemTemplate.Spells.Add(new Spell()
                        {
                            SpellID = reader.GetUInt32("spellid_4"),
                            Trigger = reader.GetByte("spelltrigger_4"),
                            Charges = reader.GetInt16("spellcharges_4"),
                            PpmRate = reader.GetFloat("spellppmRate_4"),
                            Cooldown = reader.GetInt32("spellcooldown_4"),
                            Category = reader.GetUInt16("spellcategory_4"),
                            CategoryCooldown = reader.GetInt32("spellcategorycooldown_4")
                        });
                        itemTemplate.Spells.Add(new Spell()
                        {
                            SpellID = reader.GetUInt32("spellid_5"),
                            Trigger = reader.GetByte("spelltrigger_5"),
                            Charges = reader.GetInt16("spellcharges_5"),
                            PpmRate = reader.GetFloat("spellppmRate_5"),
                            Cooldown = reader.GetInt32("spellcooldown_5"),
                            Category = reader.GetUInt16("spellcategory_5"),
                            CategoryCooldown = reader.GetInt32("spellcategorycooldown_5")
                        });

                        itemTemplates.Add(itemTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return itemTemplates;
        }
        public static List<LocalesArea> GetLocalesArea()
        {
            List<LocalesArea> localesAreas = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_area";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var localesArea = new LocalesArea
                        {
                            Entry = reader.GetUInt32("Entry"),
                            NameLoc1 = reader.GetString("NameLoc1"),
                            NameLoc2 = reader.GetString("NameLoc2"),
                            NameLoc3 = reader.GetString("NameLoc3"),
                            NameLoc4 = reader.GetString("NameLoc4"),
                            NameLoc5 = reader.GetString("NameLoc5"),
                            NameLoc6 = reader.GetString("NameLoc6"),
                            NameLoc7 = reader.GetString("NameLoc7"),
                            NameLoc8 = reader.GetString("NameLoc8")
                        };

                        localesAreas.Add(localesArea);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return localesAreas;
        }
        public static List<LocalesBroadcastText> GetLocalesBroadcastTexts()
        {
            List<LocalesBroadcastText> broadcastTexts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_broadcast_text";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var broadcastText = new LocalesBroadcastText
                        {
                            Id = reader.GetUInt32("ID"),
                            MaleTextLoc1 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc1")) ? string.Empty : reader.GetString("MaleText_loc1"),
                            MaleTextLoc2 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc2")) ? string.Empty : reader.GetString("MaleText_loc2"),
                            MaleTextLoc3 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc3")) ? string.Empty : reader.GetString("MaleText_loc3"),
                            MaleTextLoc4 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc4")) ? string.Empty : reader.GetString("MaleText_loc4"),
                            MaleTextLoc5 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc5")) ? string.Empty : reader.GetString("MaleText_loc5"),
                            MaleTextLoc6 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc6")) ? string.Empty : reader.GetString("MaleText_loc6"),
                            MaleTextLoc7 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc7")) ? string.Empty : reader.GetString("MaleText_loc7"),
                            MaleTextLoc8 = reader.IsDBNull(reader.GetOrdinal("MaleText_loc8")) ? string.Empty : reader.GetString("MaleText_loc8"),
                            FemaleTextLoc1 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc1")) ? string.Empty : reader.GetString("FemaleText_loc1"),
                            FemaleTextLoc2 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc2")) ? string.Empty : reader.GetString("FemaleText_loc2"),
                            FemaleTextLoc3 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc3")) ? string.Empty : reader.GetString("FemaleText_loc3"),
                            FemaleTextLoc4 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc4")) ? string.Empty : reader.GetString("FemaleText_loc4"),
                            FemaleTextLoc5 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc5")) ? string.Empty : reader.GetString("FemaleText_loc5"),
                            FemaleTextLoc6 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc6")) ? string.Empty : reader.GetString("FemaleText_loc6"),
                            FemaleTextLoc7 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc7")) ? string.Empty : reader.GetString("FemaleText_loc7"),
                            FemaleTextLoc8 = reader.IsDBNull(reader.GetOrdinal("FemaleText_loc8")) ? string.Empty : reader.GetString("FemaleText_loc8"),
                            VerifiedBuild = reader.GetInt16("VerifiedBuild")
                        };

                        broadcastTexts.Add(broadcastText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return broadcastTexts;
        }
        public static List<LocalesCreature> GetLocalesCreatures()
        {
            List<LocalesCreature> creatures = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_creature";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var creature = new LocalesCreature
                        {
                            Entry = reader.GetUInt32("entry"),
                            NameLoc1 = reader.GetString("name_loc1"),
                            NameLoc2 = reader.GetString("name_loc2"),
                            NameLoc3 = reader.GetString("name_loc3"),
                            NameLoc4 = reader.GetString("name_loc4"),
                            NameLoc5 = reader.GetString("name_loc5"),
                            NameLoc6 = reader.GetString("name_loc6"),
                            NameLoc7 = reader.GetString("name_loc7"),
                            NameLoc8 = reader.GetString("name_loc8"),
                            SubnameLoc1 = reader.IsDBNull(reader.GetOrdinal("subname_loc1")) ? string.Empty : reader.GetString("subname_loc1"),
                            SubnameLoc2 = reader.IsDBNull(reader.GetOrdinal("subname_loc2")) ? string.Empty : reader.GetString("subname_loc2"),
                            SubnameLoc3 = reader.IsDBNull(reader.GetOrdinal("subname_loc3")) ? string.Empty : reader.GetString("subname_loc3"),
                            SubnameLoc4 = reader.IsDBNull(reader.GetOrdinal("subname_loc4")) ? string.Empty : reader.GetString("subname_loc4"),
                            SubnameLoc5 = reader.IsDBNull(reader.GetOrdinal("subname_loc5")) ? string.Empty : reader.GetString("subname_loc5"),
                            SubnameLoc6 = reader.IsDBNull(reader.GetOrdinal("subname_loc6")) ? string.Empty : reader.GetString("subname_loc6"),
                            SubnameLoc7 = reader.IsDBNull(reader.GetOrdinal("subname_loc7")) ? string.Empty : reader.GetString("subname_loc7"),
                            SubnameLoc8 = reader.IsDBNull(reader.GetOrdinal("subname_loc8")) ? string.Empty : reader.GetString("subname_loc8")
                        };

                        creatures.Add(creature);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return creatures;
        }
        public static List<LocalesGameObject> GetLocalesGameObjects()
        {
            List<LocalesGameObject> gameObjects = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_gameobject";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gameObject = new LocalesGameObject
                        {
                            Entry = reader.GetUInt32("entry"),
                            NameLoc1 = reader.GetString("name_loc1"),
                            NameLoc2 = reader.GetString("name_loc2"),
                            NameLoc3 = reader.GetString("name_loc3"),
                            NameLoc4 = reader.GetString("name_loc4"),
                            NameLoc5 = reader.GetString("name_loc5"),
                            NameLoc6 = reader.GetString("name_loc6"),
                            NameLoc7 = reader.GetString("name_loc7"),
                            NameLoc8 = reader.GetString("name_loc8")
                        };

                        gameObjects.Add(gameObject);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gameObjects;
        }
        public static List<LocalesGossipMenuOption> GetLocalesGossipMenuOptions()
        {
            List<LocalesGossipMenuOption> gossipMenuOptions = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_gossip_menu_option";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var gossipMenuOption = new LocalesGossipMenuOption
                        {
                            MenuId = reader.GetUInt16("menu_id"),
                            Id = reader.GetUInt16("id"),
                            OptionTextLoc1 = reader.IsDBNull(reader.GetOrdinal("option_text_loc1")) ? string.Empty : reader.GetString("option_text_loc1"),
                            OptionTextLoc2 = reader.IsDBNull(reader.GetOrdinal("option_text_loc2")) ? string.Empty : reader.GetString("option_text_loc2"),
                            OptionTextLoc3 = reader.IsDBNull(reader.GetOrdinal("option_text_loc3")) ? string.Empty : reader.GetString("option_text_loc3"),
                            OptionTextLoc4 = reader.IsDBNull(reader.GetOrdinal("option_text_loc4")) ? string.Empty : reader.GetString("option_text_loc4"),
                            OptionTextLoc5 = reader.IsDBNull(reader.GetOrdinal("option_text_loc5")) ? string.Empty : reader.GetString("option_text_loc5"),
                            OptionTextLoc6 = reader.IsDBNull(reader.GetOrdinal("option_text_loc6")) ? string.Empty : reader.GetString("option_text_loc6"),
                            OptionTextLoc7 = reader.IsDBNull(reader.GetOrdinal("option_text_loc7")) ? string.Empty : reader.GetString("option_text_loc7"),
                            OptionTextLoc8 = reader.IsDBNull(reader.GetOrdinal("option_text_loc8")) ? string.Empty : reader.GetString("option_text_loc8"),
                            BoxTextLoc1 = reader.IsDBNull(reader.GetOrdinal("box_text_loc1")) ? string.Empty : reader.GetString("box_text_loc1"),
                            BoxTextLoc2 = reader.IsDBNull(reader.GetOrdinal("box_text_loc2")) ? string.Empty : reader.GetString("box_text_loc2"),
                            BoxTextLoc3 = reader.IsDBNull(reader.GetOrdinal("box_text_loc3")) ? string.Empty : reader.GetString("box_text_loc3"),
                            BoxTextLoc4 = reader.IsDBNull(reader.GetOrdinal("box_text_loc4")) ? string.Empty : reader.GetString("box_text_loc4"),
                            BoxTextLoc5 = reader.IsDBNull(reader.GetOrdinal("box_text_loc5")) ? string.Empty : reader.GetString("box_text_loc5"),
                            BoxTextLoc6 = reader.IsDBNull(reader.GetOrdinal("box_text_loc6")) ? string.Empty : reader.GetString("box_text_loc6"),
                            BoxTextLoc7 = reader.IsDBNull(reader.GetOrdinal("box_text_loc7")) ? string.Empty : reader.GetString("box_text_loc7"),
                            BoxTextLoc8 = reader.IsDBNull(reader.GetOrdinal("box_text_loc8")) ? string.Empty : reader.GetString("box_text_loc8")
                        };

                        gossipMenuOptions.Add(gossipMenuOption);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return gossipMenuOptions;
        }
        public static List<LocalesItem> GetLocalesItems()
        {
            List<LocalesItem> localesItems = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_item";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var localesItem = new LocalesItem
                        {
                            Entry = reader.GetUInt32("entry"),
                            NameLoc1 = reader.GetString("name_loc1"),
                            NameLoc2 = reader.GetString("name_loc2"),
                            NameLoc3 = reader.GetString("name_loc3"),
                            NameLoc4 = reader.GetString("name_loc4"),
                            NameLoc5 = reader.GetString("name_loc5"),
                            NameLoc6 = reader.GetString("name_loc6"),
                            NameLoc7 = reader.GetString("name_loc7"),
                            NameLoc8 = reader.GetString("name_loc8"),
                            DescriptionLoc1 = reader.IsDBNull(reader.GetOrdinal("description_loc1")) ? string.Empty : reader.GetString("description_loc1"),
                            DescriptionLoc2 = reader.IsDBNull(reader.GetOrdinal("description_loc2")) ? string.Empty : reader.GetString("description_loc2"),
                            DescriptionLoc3 = reader.IsDBNull(reader.GetOrdinal("description_loc3")) ? string.Empty : reader.GetString("description_loc3"),
                            DescriptionLoc4 = reader.IsDBNull(reader.GetOrdinal("description_loc4")) ? string.Empty : reader.GetString("description_loc4"),
                            DescriptionLoc5 = reader.IsDBNull(reader.GetOrdinal("description_loc5")) ? string.Empty : reader.GetString("description_loc5"),
                            DescriptionLoc6 = reader.IsDBNull(reader.GetOrdinal("description_loc6")) ? string.Empty : reader.GetString("description_loc6"),
                            DescriptionLoc7 = reader.IsDBNull(reader.GetOrdinal("description_loc7")) ? string.Empty : reader.GetString("description_loc7"),
                            DescriptionLoc8 = reader.IsDBNull(reader.GetOrdinal("description_loc8")) ? string.Empty : reader.GetString("description_loc8")
                        };

                        localesItems.Add(localesItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return localesItems;
        }
        public static List<LocalesPageText> GetLocalesPageTexts()
        {
            List<LocalesPageText> localesPageTexts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_page_text";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var localesPageText = new LocalesPageText
                        {
                            Entry = reader.GetUInt32("entry"),
                            TextLoc1 = reader.IsDBNull(reader.GetOrdinal("Text_loc1")) ? string.Empty : reader.GetString("Text_loc1"),
                            TextLoc2 = reader.IsDBNull(reader.GetOrdinal("Text_loc2")) ? string.Empty : reader.GetString("Text_loc2"),
                            TextLoc3 = reader.IsDBNull(reader.GetOrdinal("Text_loc3")) ? string.Empty : reader.GetString("Text_loc3"),
                            TextLoc4 = reader.IsDBNull(reader.GetOrdinal("Text_loc4")) ? string.Empty : reader.GetString("Text_loc4"),
                            TextLoc5 = reader.IsDBNull(reader.GetOrdinal("Text_loc5")) ? string.Empty : reader.GetString("Text_loc5"),
                            TextLoc6 = reader.IsDBNull(reader.GetOrdinal("Text_loc6")) ? string.Empty : reader.GetString("Text_loc6"),
                            TextLoc7 = reader.IsDBNull(reader.GetOrdinal("Text_loc7")) ? string.Empty : reader.GetString("Text_loc7"),
                            TextLoc8 = reader.IsDBNull(reader.GetOrdinal("Text_loc8")) ? string.Empty : reader.GetString("Text_loc8")
                        };

                        localesPageTexts.Add(localesPageText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return localesPageTexts;
        }
        public static List<LocalesPointsOfInterest> GetLocalesPointsOfInterest()
        {
            List<LocalesPointsOfInterest> pointsOfInterest = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_points_of_interest";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var pointOfInterest = new LocalesPointsOfInterest
                        {
                            Entry = reader.GetUInt32("entry"),
                            IconNameLoc1 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc1")) ? string.Empty : reader.GetString("icon_name_loc1"),
                            IconNameLoc2 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc2")) ? string.Empty : reader.GetString("icon_name_loc2"),
                            IconNameLoc3 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc3")) ? string.Empty : reader.GetString("icon_name_loc3"),
                            IconNameLoc4 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc4")) ? string.Empty : reader.GetString("icon_name_loc4"),
                            IconNameLoc5 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc5")) ? string.Empty : reader.GetString("icon_name_loc5"),
                            IconNameLoc6 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc6")) ? string.Empty : reader.GetString("icon_name_loc6"),
                            IconNameLoc7 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc7")) ? string.Empty : reader.GetString("icon_name_loc7"),
                            IconNameLoc8 = reader.IsDBNull(reader.GetOrdinal("icon_name_loc8")) ? string.Empty : reader.GetString("icon_name_loc8")
                        };

                        pointsOfInterest.Add(pointOfInterest);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return pointsOfInterest;
        }
        public static List<LocalesQuest> GetLocalesQuest()
        {
            List<LocalesQuest> quests = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM locales_quest";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var quest = new LocalesQuest
                        {
                            Entry = reader.GetUInt32("entry"),
                            TitleLoc1 = reader.IsDBNull(reader.GetOrdinal("Title_loc1")) ? string.Empty : reader.GetString("Title_loc1"),
                            TitleLoc2 = reader.IsDBNull(reader.GetOrdinal("Title_loc2")) ? string.Empty : reader.GetString("Title_loc2"),
                            TitleLoc3 = reader.IsDBNull(reader.GetOrdinal("Title_loc3")) ? string.Empty : reader.GetString("Title_loc3"),
                            TitleLoc4 = reader.IsDBNull(reader.GetOrdinal("Title_loc4")) ? string.Empty : reader.GetString("Title_loc4"),
                            TitleLoc5 = reader.IsDBNull(reader.GetOrdinal("Title_loc5")) ? string.Empty : reader.GetString("Title_loc5"),
                            TitleLoc6 = reader.IsDBNull(reader.GetOrdinal("Title_loc6")) ? string.Empty : reader.GetString("Title_loc6"),
                            TitleLoc7 = reader.IsDBNull(reader.GetOrdinal("Title_loc7")) ? string.Empty : reader.GetString("Title_loc7"),
                            TitleLoc8 = reader.IsDBNull(reader.GetOrdinal("Title_loc8")) ? string.Empty : reader.GetString("Title_loc8"),
                            DetailsLoc1 = reader.IsDBNull(reader.GetOrdinal("Details_loc1")) ? string.Empty : reader.GetString("Details_loc1"),
                            DetailsLoc2 = reader.IsDBNull(reader.GetOrdinal("Details_loc2")) ? string.Empty : reader.GetString("Details_loc2"),
                            DetailsLoc3 = reader.IsDBNull(reader.GetOrdinal("Details_loc3")) ? string.Empty : reader.GetString("Details_loc3"),
                            DetailsLoc4 = reader.IsDBNull(reader.GetOrdinal("Details_loc4")) ? string.Empty : reader.GetString("Details_loc4"),
                            DetailsLoc5 = reader.IsDBNull(reader.GetOrdinal("Details_loc5")) ? string.Empty : reader.GetString("Details_loc5"),
                            DetailsLoc6 = reader.IsDBNull(reader.GetOrdinal("Details_loc6")) ? string.Empty : reader.GetString("Details_loc6"),
                            DetailsLoc7 = reader.IsDBNull(reader.GetOrdinal("Details_loc7")) ? string.Empty : reader.GetString("Details_loc7"),
                            DetailsLoc8 = reader.IsDBNull(reader.GetOrdinal("Details_loc8")) ? string.Empty : reader.GetString("Details_loc8"),
                            ObjectivesLoc1 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc1")) ? string.Empty : reader.GetString("Objectives_loc1"),
                            ObjectivesLoc2 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc2")) ? string.Empty : reader.GetString("Objectives_loc2"),
                            ObjectivesLoc3 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc3")) ? string.Empty : reader.GetString("Objectives_loc3"),
                            ObjectivesLoc4 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc4")) ? string.Empty : reader.GetString("Objectives_loc4"),
                            ObjectivesLoc5 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc5")) ? string.Empty : reader.GetString("Objectives_loc5"),
                            ObjectivesLoc6 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc6")) ? string.Empty : reader.GetString("Objectives_loc6"),
                            ObjectivesLoc7 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc7")) ? string.Empty : reader.GetString("Objectives_loc7"),
                            ObjectivesLoc8 = reader.IsDBNull(reader.GetOrdinal("Objectives_loc8")) ? string.Empty : reader.GetString("Objectives_loc8"),
                            OfferRewardTextLoc1 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc1")) ? string.Empty : reader.GetString("OfferRewardText_loc1"),
                            OfferRewardTextLoc2 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc2")) ? string.Empty : reader.GetString("OfferRewardText_loc2"),
                            OfferRewardTextLoc3 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc3")) ? string.Empty : reader.GetString("OfferRewardText_loc3"),
                            OfferRewardTextLoc4 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc4")) ? string.Empty : reader.GetString("OfferRewardText_loc4"),
                            OfferRewardTextLoc5 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc5")) ? string.Empty : reader.GetString("OfferRewardText_loc5"),
                            OfferRewardTextLoc6 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc6")) ? string.Empty : reader.GetString("OfferRewardText_loc6"),
                            OfferRewardTextLoc7 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc7")) ? string.Empty : reader.GetString("OfferRewardText_loc7"),
                            OfferRewardTextLoc8 = reader.IsDBNull(reader.GetOrdinal("OfferRewardText_loc8")) ? string.Empty : reader.GetString("OfferRewardText_loc8"),
                            RequestItemsTextLoc1 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc1")) ? string.Empty : reader.GetString("RequestItemsText_loc1"),
                            RequestItemsTextLoc2 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc2")) ? string.Empty : reader.GetString("RequestItemsText_loc2"),
                            RequestItemsTextLoc3 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc3")) ? string.Empty : reader.GetString("RequestItemsText_loc3"),
                            RequestItemsTextLoc4 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc4")) ? string.Empty : reader.GetString("RequestItemsText_loc4"),
                            RequestItemsTextLoc5 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc5")) ? string.Empty : reader.GetString("RequestItemsText_loc5"),
                            RequestItemsTextLoc6 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc6")) ? string.Empty : reader.GetString("RequestItemsText_loc6"),
                            RequestItemsTextLoc7 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc7")) ? string.Empty : reader.GetString("RequestItemsText_loc7"),
                            RequestItemsTextLoc8 = reader.IsDBNull(reader.GetOrdinal("RequestItemsText_loc8")) ? string.Empty : reader.GetString("RequestItemsText_loc8"),
                            EndTextLoc1 = reader.IsDBNull(reader.GetOrdinal("EndText_loc1")) ? string.Empty : reader.GetString("EndText_loc1"),
                            EndTextLoc2 = reader.IsDBNull(reader.GetOrdinal("EndText_loc2")) ? string.Empty : reader.GetString("EndText_loc2"),
                            EndTextLoc3 = reader.IsDBNull(reader.GetOrdinal("EndText_loc3")) ? string.Empty : reader.GetString("EndText_loc3"),
                            EndTextLoc4 = reader.IsDBNull(reader.GetOrdinal("EndText_loc4")) ? string.Empty : reader.GetString("EndText_loc4"),
                            EndTextLoc5 = reader.IsDBNull(reader.GetOrdinal("EndText_loc5")) ? string.Empty : reader.GetString("EndText_loc5"),
                            EndTextLoc6 = reader.IsDBNull(reader.GetOrdinal("EndText_loc6")) ? string.Empty : reader.GetString("EndText_loc6"),
                            EndTextLoc7 = reader.IsDBNull(reader.GetOrdinal("EndText_loc7")) ? string.Empty : reader.GetString("EndText_loc7"),
                            EndTextLoc8 = reader.IsDBNull(reader.GetOrdinal("EndText_loc8")) ? string.Empty : reader.GetString("EndText_loc8"),
                            ObjectiveText1Loc1 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc1")) ? string.Empty : reader.GetString("ObjectiveText1_loc1"),
                            ObjectiveText1Loc2 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc2")) ? string.Empty : reader.GetString("ObjectiveText1_loc2"),
                            ObjectiveText1Loc3 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc3")) ? string.Empty : reader.GetString("ObjectiveText1_loc3"),
                            ObjectiveText1Loc4 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc4")) ? string.Empty : reader.GetString("ObjectiveText1_loc4"),
                            ObjectiveText1Loc5 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc5")) ? string.Empty : reader.GetString("ObjectiveText1_loc5"),
                            ObjectiveText1Loc6 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc6")) ? string.Empty : reader.GetString("ObjectiveText1_loc6"),
                            ObjectiveText1Loc7 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc7")) ? string.Empty : reader.GetString("ObjectiveText1_loc7"),
                            ObjectiveText1Loc8 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1_loc8")) ? string.Empty : reader.GetString("ObjectiveText1_loc8"),
                            ObjectiveText2Loc1 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc1")) ? string.Empty : reader.GetString("ObjectiveText2_loc1"),
                            ObjectiveText2Loc2 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc2")) ? string.Empty : reader.GetString("ObjectiveText2_loc2"),
                            ObjectiveText2Loc3 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc3")) ? string.Empty : reader.GetString("ObjectiveText2_loc3"),
                            ObjectiveText2Loc4 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc4")) ? string.Empty : reader.GetString("ObjectiveText2_loc4"),
                            ObjectiveText2Loc5 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc5")) ? string.Empty : reader.GetString("ObjectiveText2_loc5"),
                            ObjectiveText2Loc6 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc6")) ? string.Empty : reader.GetString("ObjectiveText2_loc6"),
                            ObjectiveText2Loc7 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc7")) ? string.Empty : reader.GetString("ObjectiveText2_loc7"),
                            ObjectiveText2Loc8 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2_loc8")) ? string.Empty : reader.GetString("ObjectiveText2_loc8"),
                            ObjectiveText3Loc1 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText3_loc1")) ? string.Empty : reader.GetString("ObjectiveText3_loc1"),
                            ObjectiveText3Loc2 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText3_loc2")) ? string.Empty : reader.GetString("ObjectiveText3_loc2"),
                            ObjectiveText3Loc3 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText3_loc3")) ? string.Empty : reader.GetString("ObjectiveText3_loc3"),
                            ObjectiveText3Loc4 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText3_loc4")) ? string.Empty : reader.GetString("ObjectiveText3_loc4"),
                            ObjectiveText4Loc1 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc1")) ? string.Empty : reader.GetString("ObjectiveText4_loc1"),
                            ObjectiveText4Loc2 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc2")) ? string.Empty : reader.GetString("ObjectiveText4_loc2"),
                            ObjectiveText4Loc3 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc3")) ? string.Empty : reader.GetString("ObjectiveText4_loc3"),
                            ObjectiveText4Loc4 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc4")) ? string.Empty : reader.GetString("ObjectiveText4_loc4"),
                            ObjectiveText4Loc5 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc5")) ? string.Empty : reader.GetString("ObjectiveText4_loc5"),
                            ObjectiveText4Loc6 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc6")) ? string.Empty : reader.GetString("ObjectiveText4_loc6"),
                            ObjectiveText4Loc7 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc7")) ? string.Empty : reader.GetString("ObjectiveText4_loc7"),
                            ObjectiveText4Loc8 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4_loc8")) ? string.Empty : reader.GetString("ObjectiveText4_loc8")
                        };

                        quests.Add(quest);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return quests;
        }
        public static List<MailLootTemplate> GetMailLootTemplates()
        {
            List<MailLootTemplate> lootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM mail_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var lootTemplate = new MailLootTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            Groupid = reader.GetByte("groupid"),
                            MincountOrRef = reader.GetInt32("mincountOrRef"),
                            Maxcount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        lootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return lootTemplates;
        }
        public static List<MangosString> GetMangosStrings()
        {
            List<MangosString> mangosStrings = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM mangos_string";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var mangosString = new MangosString
                        {
                            Entry = reader.GetUInt32("entry"),
                            ContentDefault = reader.GetString("content_default"),
                            ContentLoc1 = reader.IsDBNull(reader.GetOrdinal("content_loc1")) ? string.Empty : reader.GetString("content_loc1"),
                            ContentLoc2 = reader.IsDBNull(reader.GetOrdinal("content_loc2")) ? string.Empty : reader.GetString("content_loc2"),
                            ContentLoc3 = reader.IsDBNull(reader.GetOrdinal("content_loc3")) ? string.Empty : reader.GetString("content_loc3"),
                            ContentLoc4 = reader.IsDBNull(reader.GetOrdinal("content_loc4")) ? string.Empty : reader.GetString("content_loc4"),
                            ContentLoc5 = reader.IsDBNull(reader.GetOrdinal("content_loc5")) ? string.Empty : reader.GetString("content_loc5"),
                            ContentLoc6 = reader.IsDBNull(reader.GetOrdinal("content_loc6")) ? string.Empty : reader.GetString("content_loc6"),
                            ContentLoc7 = reader.IsDBNull(reader.GetOrdinal("content_loc7")) ? string.Empty : reader.GetString("content_loc7"),
                            ContentLoc8 = reader.IsDBNull(reader.GetOrdinal("content_loc8")) ? string.Empty : reader.GetString("content_loc8")
                        };

                        mangosStrings.Add(mangosString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return mangosStrings;
        }
        public static List<MapTemplate> GetMapTemplates()
        {
            List<MapTemplate> mapTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM map_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var mapTemplate = new MapTemplate
                        {
                            Entry = reader.GetUInt16("Entry"),
                            Patch = reader.GetByte("patch"),
                            Parent = reader.GetUInt32("Parent"),
                            MapType = reader.GetByte("MapType"),
                            LinkedZone = reader.GetUInt32("LinkedZone"),
                            LevelMin = reader.GetByte("LevelMin"),
                            LevelMax = reader.GetByte("LevelMax"),
                            MaxPlayers = reader.GetByte("MaxPlayers"),
                            ResetDelay = reader.GetUInt32("ResetDelay"),
                            GhostEntranceMap = reader.GetInt16("GhostEntranceMap"),
                            GhostEntranceX = reader.GetFloat("GhostEntranceX"),
                            GhostEntranceY = reader.GetFloat("GhostEntranceY"),
                            MapName = reader.GetString("MapName"),
                            ScriptName = reader.GetString("ScriptName")
                        };

                        mapTemplates.Add(mapTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return mapTemplates;
        }
        public static List<string> GetMigrations()
        {
            List<string> migrations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM migrations";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var migrationId = reader.GetString("id");
                        migrations.Add(migrationId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return migrations;
        }
        public static List<NpcGossip> GetNpcGossips()
        {
            List<NpcGossip> npcGossips = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM npc_gossip";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var npcGossip = new NpcGossip
                        {
                            NpcGuid = reader.GetUInt32("npc_guid"),
                            Textid = reader.GetUInt32("textid")
                        };

                        npcGossips.Add(npcGossip);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return npcGossips;
        }
        public static List<NpcText> GetNpcTexts()
        {
            List<NpcText> npcTexts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM npc_text";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var npcText = new NpcText
                        {
                            ID = reader.GetUInt32("ID"),
                            BroadcastTextID0 = reader.GetUInt32("BroadcastTextID0"),
                            Probability0 = reader.GetFloat("Probability0"),
                            BroadcastTextID1 = reader.GetUInt32("BroadcastTextID1"),
                            Probability1 = reader.GetFloat("Probability1"),
                            BroadcastTextID2 = reader.GetUInt32("BroadcastTextID2"),
                            Probability2 = reader.GetFloat("Probability2"),
                            BroadcastTextID3 = reader.GetUInt32("BroadcastTextID3"),
                            Probability3 = reader.GetFloat("Probability3"),
                            BroadcastTextID4 = reader.GetUInt32("BroadcastTextID4"),
                            Probability4 = reader.GetFloat("Probability4"),
                            BroadcastTextID5 = reader.GetUInt32("BroadcastTextID5"),
                            Probability5 = reader.GetFloat("Probability5"),
                            BroadcastTextID6 = reader.GetUInt32("BroadcastTextID6"),
                            Probability6 = reader.GetFloat("Probability6"),
                            BroadcastTextID7 = reader.GetUInt32("BroadcastTextID7"),
                            Probability7 = reader.GetFloat("Probability7")
                        };

                        npcTexts.Add(npcText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return npcTexts;
        }
        public static List<NpcTrainer> GetNpcTrainers()
        {
            List<NpcTrainer> npcTrainers = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM npc_trainer";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var npcTrainer = new NpcTrainer
                        {
                            Entry = reader.GetUInt32("entry"),
                            Spell = reader.GetUInt32("spell"),
                            Spellcost = reader.GetUInt32("spellcost"),
                            Reqskill = reader.GetUInt16("reqskill"),
                            Reqskillvalue = reader.GetUInt16("reqskillvalue"),
                            Reqlevel = reader.GetByte("reqlevel")
                        };

                        npcTrainers.Add(npcTrainer);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return npcTrainers;
        }
        public static List<NpcTrainerTemplate> GetNpcTrainerTemplates()
        {
            List<NpcTrainerTemplate> npcTrainerTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM npc_trainer_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var npcTrainerTemplate = new NpcTrainerTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Spell = reader.GetUInt32("spell"),
                            Spellcost = reader.GetUInt32("spellcost"),
                            Reqskill = reader.GetUInt16("reqskill"),
                            Reqskillvalue = reader.GetUInt16("reqskillvalue"),
                            Reqlevel = reader.GetByte("reqlevel")
                        };

                        npcTrainerTemplates.Add(npcTrainerTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return npcTrainerTemplates;
        }
        public static List<NpcVendor> GetNpcVendors()
        {
            List<NpcVendor> npcVendors = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM npc_vendor";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var npcVendor = new NpcVendor
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            Maxcount = reader.GetByte("maxcount"),
                            Incrtime = reader.GetUInt32("incrtime")
                        };

                        npcVendors.Add(npcVendor);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return npcVendors;
        }
        public static List<NpcVendorTemplate> GetNpcVendorTemplates()
        {
            List<NpcVendorTemplate> npcVendorTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM npc_vendor_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var npcVendorTemplate = new NpcVendorTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            Maxcount = reader.GetByte("maxcount"),
                            Incrtime = reader.GetUInt32("incrtime")
                        };

                        npcVendorTemplates.Add(npcVendorTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return npcVendorTemplates;
        }
        public static List<PageText> GetPageText()
        {
            List<PageText> pageTexts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM page_text";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var pageText = new PageText
                        {
                            Entry = reader.GetUInt32("entry"),
                            Text = reader.GetString("text"),
                            NextPage = reader.GetUInt32("next_page")
                        };

                        pageTexts.Add(pageText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return pageTexts;
        }
        public static List<PetCreateInfoSpell> GetPetCreateInfoSpells()
        {
            List<PetCreateInfoSpell> petCreateInfoSpells = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM petcreateinfo_spell";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var petCreateInfoSpell = new PetCreateInfoSpell
                        {
                            Entry = reader.GetUInt32("entry"),
                            Spell1 = reader.GetUInt32("Spell1"),
                            Spell2 = reader.GetUInt32("Spell2"),
                            Spell3 = reader.GetUInt32("Spell3"),
                            Spell4 = reader.GetUInt32("Spell4")
                        };

                        petCreateInfoSpells.Add(petCreateInfoSpell);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return petCreateInfoSpells;
        }
        public static List<PetLevelStats> GetPetLevelStats()
        {
            List<PetLevelStats> petLevelStats = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pet_levelstats";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var stats = new PetLevelStats
                        {
                            CreatureEntry = reader.GetUInt32("creature_entry"),
                            Level = reader.GetByte("level"),
                            Hp = reader.GetUInt16("hp"),
                            Mana = reader.GetUInt16("mana"),
                            Armor = reader.GetUInt32("armor"),
                            Str = reader.GetUInt16("str"),
                            Agi = reader.GetUInt16("agi"),
                            Sta = reader.GetUInt16("sta"),
                            Inte = reader.GetUInt16("inte"),
                            Spi = reader.GetUInt16("spi")
                        };

                        petLevelStats.Add(stats);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return petLevelStats;
        }
        public static List<PetNameGeneration> GetPetNameGenerations()
        {
            List<PetNameGeneration> petNames = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pet_name_generation";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var petName = new PetNameGeneration
                        {
                            Id = reader.GetUInt32("id"),
                            Word = reader.GetString("word"),
                            Entry = reader.GetUInt32("entry"),
                            Half = reader.GetUInt32("half")
                        };

                        petNames.Add(petName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return petNames;
        }
        public static List<PickpocketingLootTemplate> GetPickpocketingLoots()
        {
            List<PickpocketingLootTemplate> lootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pickpocketing_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var lootTemplate = new PickpocketingLootTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            Groupid = reader.GetByte("groupid"),
                            MincountOrRef = reader.GetUInt32("mincountOrRef"),
                            Maxcount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        lootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return lootTemplates;
        }
        public static List<PlayerBot> GetPlayerBots()
        {
            List<PlayerBot> playerBots = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM playerbot";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var playerBot = new PlayerBot
                        {
                            CharGuid = reader.GetUInt64("char_guid"),
                            Chance = reader.GetUInt32("chance"),
                            Comment = reader.IsDBNull(reader.GetOrdinal("comment")) ? string.Empty : reader.GetString("comment")
                        };

                        playerBots.Add(playerBot);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return playerBots;
        }
        public static List<PlayerCreateInfo> GetPlayerCreateInfo()
        {
            List<PlayerCreateInfo> playerCreateInfos = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM playercreateinfo";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var playerCreateInfo = new PlayerCreateInfo
                        {
                            Race = reader.GetByte("race"),
                            Class = reader.GetByte("class"),
                            Map = reader.GetUInt16("map"),
                            Zone = reader.GetUInt32("zone"),
                            PositionX = reader.GetFloat("position_x"),
                            PositionY = reader.GetFloat("position_y"),
                            PositionZ = reader.GetFloat("position_z"),
                            Orientation = reader.GetFloat("orientation")
                        };

                        playerCreateInfos.Add(playerCreateInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return playerCreateInfos;
        }
        public static List<PlayerCreateInfoAction> GetPlayerCreateInfoActions()
        {
            List<PlayerCreateInfoAction> playerCreateInfoActions = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM playercreateinfo_action";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var playerCreateInfoAction = new PlayerCreateInfoAction
                        {
                            Race = reader.GetByte("race"),
                            Class = reader.GetByte("class"),
                            Button = reader.GetUInt16("button"),
                            Action = reader.GetUInt32("action"),
                            Type = reader.GetUInt16("type")
                        };

                        playerCreateInfoActions.Add(playerCreateInfoAction);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return playerCreateInfoActions;
        }
        public static List<PlayerCreateInfoItem> GetPlayerCreateInfoItems()
        {
            List<PlayerCreateInfoItem> playerCreateInfoItems = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM playercreateinfo_item";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var playerCreateInfoItem = new PlayerCreateInfoItem
                        {
                            Race = reader.GetByte("race"),
                            Class = reader.GetByte("class"),
                            Itemid = reader.GetUInt32("itemid"),
                            Amount = reader.GetByte("amount")
                        };

                        playerCreateInfoItems.Add(playerCreateInfoItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return playerCreateInfoItems;
        }
        public static List<PlayerCreateInfoSpell> GetPlayerCreateInfoSpells()
        {
            List<PlayerCreateInfoSpell> playerCreateInfoSpells = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM playercreateinfo_spell";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var playerCreateInfoSpell = new PlayerCreateInfoSpell
                        {
                            Race = reader.GetByte("race"),
                            Class = reader.GetByte("class"),
                            Spell = reader.GetUInt32("Spell"),
                            Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? string.Empty : reader.GetString("Note")
                        };

                        playerCreateInfoSpells.Add(playerCreateInfoSpell);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return playerCreateInfoSpells;
        }
        public static List<PlayerClassLevelStats> GetPlayerClassLevelStats()
        {
            List<PlayerClassLevelStats> classLevelStats = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_classlevelstats";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var stats = new PlayerClassLevelStats
                        {
                            Class = reader.GetByte("class"),
                            Level = reader.GetByte("level"),
                            Basehp = reader.GetUInt16("basehp"),
                            Basemana = reader.GetUInt16("basemana")
                        };

                        classLevelStats.Add(stats);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return classLevelStats;
        }
        public static List<PlayerFactionChangeItems> GetPlayerFactionChangeItems()
        {
            List<PlayerFactionChangeItems> factionChangeItems = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_factionchange_items";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = new PlayerFactionChangeItems
                        {
                            AllianceId = reader.GetInt32("alliance_id"),
                            HordeId = reader.GetInt32("horde_id"),
                            Comment = reader.GetString("comment")
                        };

                        factionChangeItems.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factionChangeItems;
        }
        public static List<PlayerFactionChangeMounts> GetPlayerFactionChangeMounts()
        {
            List<PlayerFactionChangeMounts> factionChangeMounts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_factionchange_mounts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var mount = new PlayerFactionChangeMounts
                        {
                            RaceId = reader.GetInt32("RaceId"),
                            MountNum = reader.GetInt32("MountNum"),
                            ItemEntry = reader.GetInt32("ItemEntry"),
                            Comment = reader.GetString("Comment")
                        };

                        factionChangeMounts.Add(mount);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factionChangeMounts;
        }
        public static List<PlayerFactionChangeQuests> GetPlayerFactionChangeQuests()
        {
            List<PlayerFactionChangeQuests> factionChangeQuests = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_factionchange_quests";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var quest = new PlayerFactionChangeQuests
                        {
                            AllianceId = reader.GetInt32("alliance_id"),
                            HordeId = reader.GetInt32("horde_id"),
                            Comment = reader.GetString("comment")
                        };

                        factionChangeQuests.Add(quest);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factionChangeQuests;
        }
        public static List<PlayerFactionChangeReputations> GetPlayerFactionChangeReputations()
        {
            List<PlayerFactionChangeReputations> factionChangeReputations = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_factionchange_reputations";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var reputation = new PlayerFactionChangeReputations
                        {
                            AllianceId = reader.GetInt32("alliance_id"),
                            HordeId = reader.GetInt32("horde_id")
                        };

                        factionChangeReputations.Add(reputation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factionChangeReputations;
        }
        public static List<PlayerFactionChangeSpells> GetPlayerFactionChangeSpells()
        {
            List<PlayerFactionChangeSpells> factionChangeSpells = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_factionchange_spells";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var factionChangeSpell = new PlayerFactionChangeSpells
                        {
                            AllianceId = reader.GetInt32("alliance_id"),
                            HordeId = reader.GetInt32("horde_id"),
                            Comment = reader.GetString("comment")
                        };

                        factionChangeSpells.Add(factionChangeSpell);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return factionChangeSpells;
        }
        public static List<PlayerLevelStats> GetPlayerLevelStats()
        {
            List<PlayerLevelStats> levelStats = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_levelstats";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var levelStat = new PlayerLevelStats
                        {
                            Race = reader.GetByte("race"),
                            Class = reader.GetByte("class"),
                            Level = reader.GetByte("level"),
                            Str = reader.GetByte("str"),
                            Agi = reader.GetByte("agi"),
                            Sta = reader.GetByte("sta"),
                            Inte = reader.GetByte("inte"),
                            Spi = reader.GetByte("spi")
                        };

                        levelStats.Add(levelStat);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return levelStats;
        }
        public static List<PlayerXpForLevel> GetPlayerXpForLevel()
        {
            List<PlayerXpForLevel> xpForLevels = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM player_xp_for_level";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var xpForLevel = new PlayerXpForLevel
                        {
                            Lvl = reader.GetUInt32("lvl"),
                            XpForNextLevel = reader.GetUInt32("xp_for_next_level")
                        };

                        xpForLevels.Add(xpForLevel);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return xpForLevels;
        }
        public static List<PointsOfInterest> GetPointsOfInterest()
        {
            List<PointsOfInterest> pointsOfInterest = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM points_of_interest";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var pointOfInterest = new PointsOfInterest
                        {
                            Entry = reader.GetUInt32("entry"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Icon = reader.GetUInt32("icon"),
                            Flags = reader.GetUInt32("flags"),
                            Data = reader.GetUInt32("data"),
                            IconName = reader.GetString("icon_name")
                        };

                        pointsOfInterest.Add(pointOfInterest);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return pointsOfInterest;
        }
        public static List<PoolCreature> GetPoolCreatures()
        {
            List<PoolCreature> poolCreatures = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pool_creature";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var poolCreature = new PoolCreature
                        {
                            Guid = reader.GetUInt32("guid"),
                            PoolEntry = reader.GetUInt32("pool_entry"),
                            Chance = reader.GetFloat("chance"),
                            Description = reader.GetString("description"),
                            Flags = reader.GetUInt32("flags"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        poolCreatures.Add(poolCreature);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return poolCreatures;
        }
        public static List<PoolCreatureTemplate> GetPoolCreatureTemplates()
        {
            List<PoolCreatureTemplate> poolCreatureTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pool_creature_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var poolCreatureTemplate = new PoolCreatureTemplate
                        {
                            Id = reader.GetUInt32("id"),
                            PoolEntry = reader.GetUInt32("pool_entry"),
                            Chance = reader.GetFloat("chance"),
                            Description = reader.GetString("description"),
                            Flags = reader.GetUInt32("flags")
                        };

                        poolCreatureTemplates.Add(poolCreatureTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return poolCreatureTemplates;
        }
        public static List<PoolGameObject> GetPoolGameObjects()
        {
            List<PoolGameObject> poolGameObjects = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pool_gameobject";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var poolGameObject = new PoolGameObject
                        {
                            Guid = reader.GetUInt32("guid"),
                            PoolEntry = reader.GetUInt32("pool_entry"),
                            Chance = reader.GetFloat("chance"),
                            Description = reader.GetString("description"),
                            Flags = reader.GetUInt32("flags"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        poolGameObjects.Add(poolGameObject);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return poolGameObjects;
        }
        public static List<PoolGameObjectTemplate> GetPoolGameObjectTemplates()
        {
            List<PoolGameObjectTemplate> poolGameObjectTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pool_gameobject_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var poolGameObjectTemplate = new PoolGameObjectTemplate
                        {
                            Id = reader.GetUInt32("id"),
                            PoolEntry = reader.GetUInt32("pool_entry"),
                            Chance = reader.GetFloat("chance"),
                            Description = reader.GetString("description"),
                            Flags = reader.GetUInt32("flags")
                        };

                        poolGameObjectTemplates.Add(poolGameObjectTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return poolGameObjectTemplates;
        }
        public static List<PoolPool> GetPoolPools()
        {
            List<PoolPool> poolPools = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pool_pool";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var poolPool = new PoolPool
                        {
                            PoolId = reader.GetUInt32("pool_id"),
                            MotherPool = reader.GetUInt32("mother_pool"),
                            Chance = reader.GetFloat("chance"),
                            Description = reader.GetString("description"),
                            Flags = reader.GetUInt32("flags")
                        };

                        poolPools.Add(poolPool);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return poolPools;
        }
        public static List<PoolTemplate> GetPoolTemplates()
        {
            List<PoolTemplate> poolTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM pool_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var poolTemplate = new PoolTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            MaxLimit = reader.GetUInt32("max_limit"),
                            Description = reader.GetString("description"),
                            Flags = reader.GetUInt32("flags"),
                            Instance = reader.GetUInt32("instance"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        poolTemplates.Add(poolTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return poolTemplates;
        }
        public static List<QuestEndScripts> GetQuestEndScripts()
        {
            List<QuestEndScripts> questEndScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM quest_end_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var questEndScript = new QuestEndScripts
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };

                        questEndScripts.Add(questEndScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return questEndScripts;
        }
        public static List<QuestGreeting> GetQuestGreeting()
        {
            List<QuestGreeting> questGreetings = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM quest_greeting";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var questGreeting = new QuestGreeting
                        {
                            Entry = reader.GetUInt32("entry"),
                            Type = reader.GetByte("type"),
                            ContentDefault = reader.GetString("content_default"),
                            ContentLoc1 = reader.IsDBNull(reader.GetOrdinal("content_loc1")) ? string.Empty : reader.GetString("content_loc1"),
                            ContentLoc2 = reader.IsDBNull(reader.GetOrdinal("content_loc2")) ? string.Empty : reader.GetString("content_loc2"),
                            ContentLoc3 = reader.IsDBNull(reader.GetOrdinal("content_loc3")) ? string.Empty : reader.GetString("content_loc3"),
                            ContentLoc4 = reader.IsDBNull(reader.GetOrdinal("content_loc4")) ? string.Empty : reader.GetString("content_loc4"),
                            ContentLoc5 = reader.IsDBNull(reader.GetOrdinal("content_loc5")) ? string.Empty : reader.GetString("content_loc5"),
                            ContentLoc6 = reader.IsDBNull(reader.GetOrdinal("content_loc6")) ? string.Empty : reader.GetString("content_loc6"),
                            ContentLoc7 = reader.IsDBNull(reader.GetOrdinal("content_loc7")) ? string.Empty : reader.GetString("content_loc7"),
                            ContentLoc8 = reader.IsDBNull(reader.GetOrdinal("content_loc8")) ? string.Empty : reader.GetString("content_loc8"),
                            Emote = reader.GetUInt16("Emote"),
                            EmoteDelay = reader.GetUInt32("EmoteDelay")
                        };

                        questGreetings.Add(questGreeting);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return questGreetings;
        }
        public static List<QuestStartScripts> GetQuestStartScripts()
        {
            List<QuestStartScripts> questStartScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM quest_start_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var questStartScript = new QuestStartScripts
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };

                        questStartScripts.Add(questStartScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return questStartScripts;
        }
        public static List<QuestTemplate> GetQuestTemplates()
        {
            List<QuestTemplate> questTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM quest_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var questTemplate = new QuestTemplate
                        {
                            Entry = reader.GetUInt32("entry"),
                            Patch = reader.GetByte("patch"),
                            Method = reader.GetByte("Method"),
                            ZoneOrSort = reader.GetInt16("ZoneOrSort"),
                            MinLevel = reader.GetByte("MinLevel"),
                            MaxLevel = reader.GetByte("MaxLevel"),
                            QuestLevel = reader.GetByte("QuestLevel"),
                            Type = reader.GetUInt16("Type"),
                            RequiredClasses = reader.GetUInt16("RequiredClasses"),
                            RequiredRaces = reader.GetUInt16("RequiredRaces"),
                            RequiredSkill = reader.GetUInt16("RequiredSkill"),
                            RequiredSkillValue = reader.GetUInt16("RequiredSkillValue"),
                            RepObjectiveFaction = reader.GetUInt16("RepObjectiveFaction"),
                            RepObjectiveValue = reader.GetInt32("RepObjectiveValue"),
                            RequiredMinRepFaction = reader.GetUInt16("RequiredMinRepFaction"),
                            RequiredMinRepValue = reader.GetInt32("RequiredMinRepValue"),
                            RequiredMaxRepFaction = reader.GetUInt16("RequiredMaxRepFaction"),
                            RequiredMaxRepValue = reader.GetInt32("RequiredMaxRepValue"),
                            SuggestedPlayers = reader.GetByte("SuggestedPlayers"),
                            LimitTime = reader.GetUInt32("LimitTime"),
                            QuestFlags = reader.GetUInt16("QuestFlags"),
                            SpecialFlags = reader.GetByte("SpecialFlags"),
                            PrevQuestId = reader.GetInt32("PrevQuestId"),
                            NextQuestId = reader.GetInt32("NextQuestId"),
                            ExclusiveGroup = reader.GetInt32("ExclusiveGroup"),
                            NextQuestInChain = reader.GetUInt32("NextQuestInChain"),
                            SrcItemId = reader.GetUInt32("SrcItemId"),
                            SrcItemCount = reader.GetByte("SrcItemCount"),
                            SrcSpell = reader.GetUInt32("SrcSpell"),
                            Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? string.Empty : reader.GetString("Title"),
                            Details = reader.IsDBNull(reader.GetOrdinal("Details")) ? string.Empty : reader.GetString("Details"),
                            Objectives = reader.IsDBNull(reader.GetOrdinal("Objectives")) ? string.Empty : reader.GetString("Objectives"),
                            OfferRewardText = reader.IsDBNull(reader.GetOrdinal("OfferRewardText")) ? string.Empty : reader.GetString("OfferRewardText"),
                            RequestItemsText = reader.IsDBNull(reader.GetOrdinal("RequestItemsText")) ? string.Empty : reader.GetString("RequestItemsText"),
                            EndText = reader.IsDBNull(reader.GetOrdinal("EndText")) ? string.Empty : reader.GetString("EndText"),
                            ObjectiveText1 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText1")) ? string.Empty : reader.GetString("ObjectiveText1"),
                            ObjectiveText2 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText2")) ? string.Empty : reader.GetString("ObjectiveText2"),
                            ObjectiveText3 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText3")) ? string.Empty : reader.GetString("ObjectiveText3"),
                            ObjectiveText4 = reader.IsDBNull(reader.GetOrdinal("ObjectiveText4")) ? string.Empty : reader.GetString("ObjectiveText4"),
                            ReqItemId1 = reader.GetUInt32("ReqItemId1"),
                            ReqItemId2 = reader.GetUInt32("ReqItemId2"),
                            ReqItemId3 = reader.GetUInt32("ReqItemId3"),
                            ReqItemId4 = reader.GetUInt32("ReqItemId4"),
                            ReqItemCount1 = reader.GetUInt16("ReqItemCount1"),
                            ReqItemCount2 = reader.GetUInt16("ReqItemCount2"),
                            ReqItemCount3 = reader.GetUInt16("ReqItemCount3"),
                            ReqItemCount4 = reader.GetUInt16("ReqItemCount4"),
                            ReqSourceId1 = reader.GetUInt32("ReqSourceId1"),
                            ReqSourceId2 = reader.GetUInt32("ReqSourceId2"),
                            ReqSourceId3 = reader.GetUInt32("ReqSourceId3"),
                            ReqSourceId4 = reader.GetUInt32("ReqSourceId4"),
                            ReqSourceCount1 = reader.GetUInt16("ReqSourceCount1"),
                            ReqSourceCount2 = reader.GetUInt16("ReqSourceCount2"),
                            ReqSourceCount3 = reader.GetUInt16("ReqSourceCount3"),
                            ReqSourceCount4 = reader.GetUInt16("ReqSourceCount4"),
                            ReqCreatureOrGoId1 = reader.GetInt32("ReqCreatureOrGOId1"),
                            ReqCreatureOrGoId2 = reader.GetInt32("ReqCreatureOrGOId2"),
                            ReqCreatureOrGoId3 = reader.GetInt32("ReqCreatureOrGOId3"),
                            ReqCreatureOrGoId4 = reader.GetInt32("ReqCreatureOrGOId4"),
                            ReqCreatureOrGoCount1 = reader.GetUInt16("ReqCreatureOrGOCount1"),
                            ReqCreatureOrGoCount2 = reader.GetUInt16("ReqCreatureOrGOCount2"),
                            ReqCreatureOrGoCount3 = reader.GetUInt16("ReqCreatureOrGOCount3"),
                            ReqCreatureOrGoCount4 = reader.GetUInt16("ReqCreatureOrGOCount4"),
                            ReqSpellCast1 = reader.GetUInt32("ReqSpellCast1"),
                            ReqSpellCast2 = reader.GetUInt32("ReqSpellCast2"),
                            ReqSpellCast3 = reader.GetUInt32("ReqSpellCast3"),
                            ReqSpellCast4 = reader.GetUInt32("ReqSpellCast4"),
                            RewChoiceItemId1 = reader.GetUInt32("RewChoiceItemId1"),
                            RewChoiceItemId2 = reader.GetUInt32("RewChoiceItemId2"),
                            RewChoiceItemId3 = reader.GetUInt32("RewChoiceItemId3"),
                            RewChoiceItemId4 = reader.GetUInt32("RewChoiceItemId4"),
                            RewChoiceItemId5 = reader.GetUInt32("RewChoiceItemId5"),
                            RewChoiceItemId6 = reader.GetUInt32("RewChoiceItemId6"),
                            RewChoiceItemCount1 = reader.GetUInt16("RewChoiceItemCount1"),
                            RewChoiceItemCount2 = reader.GetUInt16("RewChoiceItemCount2"),
                            RewChoiceItemCount3 = reader.GetUInt16("RewChoiceItemCount3"),
                            RewChoiceItemCount4 = reader.GetUInt16("RewChoiceItemCount4"),
                            RewChoiceItemCount5 = reader.GetUInt16("RewChoiceItemCount5"),
                            RewChoiceItemCount6 = reader.GetUInt16("RewChoiceItemCount6"),
                            RewItemId1 = reader.GetUInt32("RewItemId1"),
                            RewItemId2 = reader.GetUInt32("RewItemId2"),
                            RewItemId3 = reader.GetUInt32("RewItemId3"),
                            RewItemId4 = reader.GetUInt32("RewItemId4"),
                            RewItemCount1 = reader.GetUInt16("RewItemCount1"),
                            RewItemCount2 = reader.GetUInt16("RewItemCount2"),
                            RewItemCount3 = reader.GetUInt16("RewItemCount3"),
                            RewItemCount4 = reader.GetUInt16("RewItemCount4"),
                            RewRepFaction1 = reader.GetUInt16("RewRepFaction1"),
                            RewRepFaction2 = reader.GetUInt16("RewRepFaction2"),
                            RewRepFaction3 = reader.GetUInt16("RewRepFaction3"),
                            RewRepFaction4 = reader.GetUInt16("RewRepFaction4"),
                            RewRepFaction5 = reader.GetUInt16("RewRepFaction5"),
                            RewRepValue1 = reader.GetInt32("RewRepValue1"),
                            RewRepValue2 = reader.GetInt32("RewRepValue2"),
                            RewRepValue3 = reader.GetInt32("RewRepValue3"),
                            RewRepValue4 = reader.GetInt32("RewRepValue4"),
                            RewRepValue5 = reader.GetInt32("RewRepValue5"),
                            RewOrReqMoney = reader.GetInt32("RewOrReqMoney"),
                            RewMoneyMaxLevel = reader.GetUInt32("RewMoneyMaxLevel"),
                            RewSpell = reader.GetUInt32("RewSpell"),
                            RewSpellCast = reader.GetUInt32("RewSpellCast"),
                            RewMailTemplateId = reader.GetUInt32("RewMailTemplateId"),
                            RewMailDelaySecs = reader.GetUInt32("RewMailDelaySecs"),
                            PointMapId = reader.GetUInt16("PointMapId"),
                            PointX = reader.GetFloat("PointX"),
                            PointY = reader.GetFloat("PointY"),
                            PointOpt = reader.GetUInt32("PointOpt"),
                            DetailsEmote1 = reader.GetUInt16("DetailsEmote1"),
                            DetailsEmote2 = reader.GetUInt16("DetailsEmote2"),
                            DetailsEmote3 = reader.GetUInt16("DetailsEmote3"),
                            DetailsEmote4 = reader.GetUInt16("DetailsEmote4"),
                            DetailsEmoteDelay1 = reader.GetUInt32("DetailsEmoteDelay1"),
                            DetailsEmoteDelay2 = reader.GetUInt32("DetailsEmoteDelay2"),
                            DetailsEmoteDelay3 = reader.GetUInt32("DetailsEmoteDelay3"),
                            DetailsEmoteDelay4 = reader.GetUInt32("DetailsEmoteDelay4"),
                            IncompleteEmote = reader.GetUInt16("IncompleteEmote"),
                            CompleteEmote = reader.GetUInt16("CompleteEmote"),
                            OfferRewardEmote1 = reader.GetUInt16("OfferRewardEmote1"),
                            OfferRewardEmote2 = reader.GetUInt16("OfferRewardEmote2"),
                            OfferRewardEmote3 = reader.GetUInt16("OfferRewardEmote3"),
                            OfferRewardEmote4 = reader.GetUInt16("OfferRewardEmote4"),
                            OfferRewardEmoteDelay1 = reader.GetUInt32("OfferRewardEmoteDelay1"),
                            OfferRewardEmoteDelay2 = reader.GetUInt32("OfferRewardEmoteDelay2"),
                            OfferRewardEmoteDelay3 = reader.GetUInt32("OfferRewardEmoteDelay3"),
                            OfferRewardEmoteDelay4 = reader.GetUInt32("OfferRewardEmoteDelay4"),
                            StartScript = reader.GetUInt32("StartScript"),
                            CompleteScript = reader.GetUInt32("CompleteScript")
                        };

                        questTemplates.Add(questTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return questTemplates;
        }
        public static List<ReferenceLootTemplate> GetReferenceLootTemplates()
        {
            List<ReferenceLootTemplate> lootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM reference_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ReferenceLootTemplate lootTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            GroupId = reader.GetUInt32("groupid"),
                            MinCountOrRef = reader.GetInt32("mincountOrRef"),
                            MaxCount = reader.GetUInt32("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetUInt32("patch_min"),
                            PatchMax = reader.GetUInt32("patch_max")
                        };

                        lootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return lootTemplates;
        }
        public static List<ReputationRewardRate> GetReputationRewardRates()
        {
            List<ReputationRewardRate> reputationRewardRates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM reputation_reward_rate";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ReputationRewardRate reputationRewardRate = new()
                        {
                            Faction = reader.GetUInt32("faction"),
                            QuestRate = reader.GetFloat("quest_rate"),
                            CreatureRate = reader.GetFloat("creature_rate"),
                            SpellRate = reader.GetFloat("spell_rate")
                        };
                        reputationRewardRates.Add(reputationRewardRate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return reputationRewardRates;
        }
        public static List<ReputationSpilloverTemplate> GetReputationSpilloverTemplate()
        {
            List<ReputationSpilloverTemplate> spilloverTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM reputation_spillover_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ReputationSpilloverTemplate spilloverTemplate = new()
                        {
                            Faction = reader.GetUInt16("faction"),
                            Faction1 = reader.GetUInt16("faction1"),
                            Rate1 = reader.GetFloat("rate_1"),
                            Rank1 = reader.GetByte("rank_1"),
                            Faction2 = reader.GetUInt16("faction2"),
                            Rate2 = reader.GetFloat("rate_2"),
                            Rank2 = reader.GetByte("rank_2"),
                            Faction3 = reader.GetUInt16("faction3"),
                            Rate3 = reader.GetFloat("rate_3"),
                            Rank3 = reader.GetByte("rank_3"),
                            Faction4 = reader.GetUInt16("faction4"),
                            Rate4 = reader.GetFloat("rate_4"),
                            Rank4 = reader.GetByte("rank_4")
                        };
                        spilloverTemplates.Add(spilloverTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spilloverTemplates;
        }
        public static List<string> GetReservedNames()
        {
            List<string> reservedNames = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT name FROM reserved_name";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        reservedNames.Add(reader.GetString("name"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return reservedNames;
        }
        public static List<ScriptedAreatrigger> GetScriptedAreaTriggers()
        {
            List<ScriptedAreatrigger> areaTriggers = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM scripted_areatrigger";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ScriptedAreatrigger trigger = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            ScriptName = reader.GetString("ScriptName")
                        };

                        areaTriggers.Add(trigger);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return areaTriggers;
        }
        public static List<ScriptedEventId> GetScriptedEvents()
        {
            List<ScriptedEventId> events = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM scripted_event_id";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ScriptedEventId scriptedEvent = new()
                        {
                            Id = reader.GetUInt32("id"),
                            ScriptName = reader.GetString("ScriptName")
                        };

                        events.Add(scriptedEvent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return events;
        }
        public static List<ScriptEscortData> GetScriptEscortData()
        {
            List<ScriptEscortData> escortDataList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM script_escort_data";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ScriptEscortData escortData = new()
                        {
                            CreatureId = reader.GetUInt32("creature_id"),
                            Quest = reader.GetUInt32("quest"),
                            EscortFaction = reader.GetUInt32("escort_faction")
                        };

                        escortDataList.Add(escortData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return escortDataList;
        }
        public static List<ScriptText> GetScriptTexts()
        {
            List<ScriptText> scriptTextList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM script_texts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ScriptText scriptText = new()
                        {
                            Entry = reader.GetInt32("entry"),
                            ContentDefault = reader.GetString("content_default"),
                            ContentLoc1 = reader.IsDBNull(reader.GetOrdinal("content_loc1")) ? string.Empty : reader.GetString("content_loc1"),
                            ContentLoc2 = reader.IsDBNull(reader.GetOrdinal("content_loc2")) ? string.Empty : reader.GetString("content_loc2"),
                            ContentLoc3 = reader.IsDBNull(reader.GetOrdinal("content_loc3")) ? string.Empty : reader.GetString("content_loc3"),
                            ContentLoc4 = reader.IsDBNull(reader.GetOrdinal("content_loc4")) ? string.Empty : reader.GetString("content_loc4"),
                            ContentLoc5 = reader.IsDBNull(reader.GetOrdinal("content_loc5")) ? string.Empty : reader.GetString("content_loc5"),
                            ContentLoc6 = reader.IsDBNull(reader.GetOrdinal("content_loc6")) ? string.Empty : reader.GetString("content_loc6"),
                            ContentLoc7 = reader.IsDBNull(reader.GetOrdinal("content_loc7")) ? string.Empty : reader.GetString("content_loc7"),
                            ContentLoc8 = reader.IsDBNull(reader.GetOrdinal("content_loc8")) ? string.Empty : reader.GetString("content_loc8"),
                            Sound = reader.GetUInt32("sound"),
                            Type = reader.GetUInt32("type"),
                            Language = reader.GetUInt32("language"),
                            Emote = reader.GetUInt32("emote"),
                            Comment = reader.IsDBNull(reader.GetOrdinal("comment")) ? string.Empty : reader.GetString("comment")
                        };

                        scriptTextList.Add(scriptText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return scriptTextList;
        }
        public static List<ScriptWaypoint> GetScriptWaypoints()
        {
            List<ScriptWaypoint> waypoints = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM script_waypoint";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ScriptWaypoint waypoint = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Pointid = reader.GetUInt32("pointid"),
                            LocationX = reader.GetFloat("location_x"),
                            LocationY = reader.GetFloat("location_y"),
                            LocationZ = reader.GetFloat("location_z"),
                            Waittime = reader.GetUInt32("waittime"),
                            PointComment = reader.IsDBNull(reader.GetOrdinal("point_comment")) ? string.Empty : reader.GetString("point_comment")
                        };

                        waypoints.Add(waypoint);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return waypoints;
        }
        public static List<SkillDiscoveryTemplate> GetSkillDiscoveryTemplates()
        {
            List<SkillDiscoveryTemplate> skillDiscoveries = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM skill_discovery_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SkillDiscoveryTemplate discovery = new()
                        {
                            SpellId = reader.GetUInt32("spellId"),
                            ReqSpell = reader.GetUInt32("reqSpell"),
                            Chance = reader.GetFloat("chance")
                        };

                        skillDiscoveries.Add(discovery);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return skillDiscoveries;
        }
        public static List<SkillExtraItemTemplate> GetSkillExtraItemTemplates()
        {
            List<SkillExtraItemTemplate> skillExtraItems = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM skill_extra_item_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SkillExtraItemTemplate extraItem = new()
                        {
                            SpellId = reader.GetUInt32("spellId"),
                            RequiredSpecialization = reader.GetUInt32("requiredSpecialization"),
                            AdditionalCreateChance = reader.GetFloat("additionalCreateChance"),
                            AdditionalMaxNum = reader.GetByte("additionalMaxNum")
                        };

                        skillExtraItems.Add(extraItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return skillExtraItems;
        }
        public static List<SkillFishingBaseLevel> GetSkillFishingBaseLevels()
        {
            List<SkillFishingBaseLevel> fishingBaseLevels = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM skill_fishing_base_level";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SkillFishingBaseLevel fishingBaseLevel = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Skill = reader.GetInt32("skill")
                        };
                        fishingBaseLevels.Add(fishingBaseLevel);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return fishingBaseLevels;
        }
        public static List<SkinningLootTemplate> GetSkinningLootTemplates()
        {
            List<SkinningLootTemplate> skinningLootTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM skinning_loot_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SkinningLootTemplate lootTemplate = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Item = reader.GetUInt32("item"),
                            ChanceOrQuestChance = reader.GetFloat("ChanceOrQuestChance"),
                            GroupId = reader.GetByte("groupid"),
                            MinCountOrRef = reader.GetInt32("mincountOrRef"),
                            MaxCount = reader.GetByte("maxcount"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            PatchMin = reader.GetByte("patch_min"),
                            PatchMax = reader.GetByte("patch_max")
                        };

                        skinningLootTemplates.Add(lootTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return skinningLootTemplates;
        }
        public static List<SoundEntries> GetSoundEntries()
        {
            List<SoundEntries> soundEntries = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM sound_entries";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SoundEntries soundEntry = new()
                        {
                            Id = reader.GetUInt32("ID"),
                            Name = reader.GetString("name")
                        };
                        soundEntries.Add(soundEntry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return soundEntries;
        }
        public static List<SpellAffect> GetSpellAffect()
        {
            List<SpellAffect> spellAffects = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_affect";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellAffect affect = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            EffectId = reader.GetByte("effectId"),
                            SpellFamilyMask = reader.GetUInt64("SpellFamilyMask")
                        };

                        spellAffects.Add(affect);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellAffects;
        }
        public static List<SpellArea> GetSpellAreas()
        {
            List<SpellArea> spellAreas = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_area";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellArea spellArea = new()
                        {
                            Spell = reader.GetUInt32("spell"),
                            Area = reader.GetUInt32("area"),
                            QuestStart = reader.GetUInt32("quest_start"),
                            QuestStartActive = reader.GetByte("quest_start_active"),
                            QuestEnd = reader.GetUInt32("quest_end"),
                            AuraSpell = reader.GetUInt32("aura_spell"),
                            Racemask = reader.GetUInt32("racemask"),
                            Gender = reader.GetByte("gender"),
                            Autocast = reader.GetByte("autocast")
                        };

                        spellAreas.Add(spellArea);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellAreas;
        }
        public static List<SpellBonusData> GetSpellBonusData()
        {
            List<SpellBonusData> spellBonusDataList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_bonus_data";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellBonusData spellBonusData = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            DirectBonus = reader.GetFloat("direct_bonus"),
                            DotBonus = reader.GetFloat("dot_bonus"),
                            ApBonus = reader.GetFloat("ap_bonus"),
                            ApDotBonus = reader.GetFloat("ap_dot_bonus"),
                            Comments = reader.IsDBNull("comments") ? string.Empty : reader.GetString("comments")
                        };

                        spellBonusDataList.Add(spellBonusData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellBonusDataList;
        }
        public static List<SpellChain> GetSpellChain()
        {
            List<SpellChain> spellChainList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_chain";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellChain spellChain = new()
                        {
                            SpellId = reader.GetUInt32("spell_id"),
                            PrevSpell = reader.GetUInt32("prev_spell"),
                            FirstSpell = reader.GetUInt32("first_spell"),
                            Rank = reader.GetByte("rank"),
                            ReqSpell = reader.GetUInt32("req_spell")
                        };

                        spellChainList.Add(spellChain);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellChainList;
        }
        public static List<SpellCheck> GetSpellCheck()
        {
            List<SpellCheck> spellCheckList = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_check";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellCheck spellCheck = new()
                        {
                            Spellid = reader.GetUInt32("spellid"),
                            SpellFamilyName = reader.GetInt32("SpellFamilyName"),
                            SpellFamilyMask = reader.GetInt64("SpellFamilyMask"),
                            SpellIcon = reader.GetInt32("SpellIcon"),
                            SpellVisual = reader.GetInt32("SpellVisual"),
                            SpellCategory = reader.GetInt32("SpellCategory"),
                            EffectType = reader.GetInt32("EffectType"),
                            EffectAura = reader.GetInt32("EffectAura"),
                            EffectIdx = reader.GetInt32("EffectIdx"),
                            Name = reader.GetString("Name"),
                            Code = reader.GetString("Code")
                        };

                        spellCheckList.Add(spellCheck);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellCheckList;
        }
        public static List<uint> GetDisabledSpells()
        {
            List<uint> disabledSpells = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_disabled";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        disabledSpells.Add(reader.GetUInt32("entry"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return disabledSpells;
        }
        public static List<SpellEffectMod> GetSpellEffectMods()
        {
            List<SpellEffectMod> spellEffectMods = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_effect_mod";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellEffectMod effectMod = new()
                        {
                            Id = reader.GetUInt32("Id"),
                            EffectIndex = reader.GetUInt32("EffectIndex"),
                            Effect = reader.GetInt32("Effect"),
                            EffectDieSides = reader.GetInt32("EffectDieSides"),
                            EffectBaseDice = reader.GetInt32("EffectBaseDice"),
                            EffectDicePerLevel = reader.GetFloat("EffectDicePerLevel"),
                            EffectRealPointsPerLevel = reader.GetFloat("EffectRealPointsPerLevel"),
                            EffectBasePoints = reader.GetInt32("EffectBasePoints"),
                            EffectAmplitude = reader.GetInt32("EffectAmplitude"),
                            EffectPointsPerComboPoint = reader.GetFloat("EffectPointsPerComboPoint"),
                            EffectChainTarget = reader.GetInt32("EffectChainTarget"),
                            EffectMultipleValue = reader.GetFloat("EffectMultipleValue"),
                            EffectMechanic = reader.GetInt32("EffectMechanic"),
                            EffectImplicitTargetA = reader.GetInt32("EffectImplicitTargetA"),
                            EffectImplicitTargetB = reader.GetInt32("EffectImplicitTargetB"),
                            EffectRadiusIndex = reader.GetInt32("EffectRadiusIndex"),
                            EffectApplyAuraName = reader.GetInt32("EffectApplyAuraName"),
                            EffectItemType = reader.GetInt32("EffectItemType"),
                            EffectMiscValue = reader.GetInt32("EffectMiscValue"),
                            EffectTriggerSpell = reader.GetInt32("EffectTriggerSpell"),
                            Comment = reader.IsDBNull("Comment") ? string.Empty : reader.GetString("Comment")
                        };

                        spellEffectMods.Add(effectMod);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellEffectMods;
        }
        public static List<SpellElixir> GetSpellElixirs()
        {
            List<SpellElixir> spellElixirs = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_elixir";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellElixir elixir = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Mask = reader.GetByte("mask")
                        };

                        spellElixirs.Add(elixir);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellElixirs;
        }
        public static List<SpellFacing> GetSpellFacings()
        {
            List<SpellFacing> spellFacings = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_facing";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellFacing facing = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Facingcasterflag = reader.GetByte("facingcasterflag")
                        };

                        spellFacings.Add(facing);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellFacings;
        }
        public static List<SpellGroup> GetSpellGroups()
        {
            List<SpellGroup> spellGroups = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_group";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellGroup group = new()
                        {
                            GroupId = reader.GetUInt32("group_id"),
                            GroupSpellId = reader.GetUInt32("group_spell_id"),
                            SpellId = reader.GetUInt32("spell_id")
                        };

                        spellGroups.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellGroups;
        }
        public static List<SpellGroupStackRules> GetSpellGroupStackRules()
        {
            List<SpellGroupStackRules> spellGroupStackRules = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_group_stack_rules";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellGroupStackRules rule = new()
                        {
                            GroupId = reader.GetUInt32("group_id"),
                            StackRule = reader.GetByte("stack_rule")
                        };

                        spellGroupStackRules.Add(rule);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellGroupStackRules;
        }
        public static List<SpellLearnSpell> GetSpellLearnSpells()
        {
            List<SpellLearnSpell> spellLearnSpells = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_learn_spell";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellLearnSpell learnSpell = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            SpellId = reader.GetUInt32("SpellID"),
                            Active = reader.GetByte("Active")
                        };

                        spellLearnSpells.Add(learnSpell);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellLearnSpells;
        }
        public static List<SpellMod> GetSpellMods()
        {
            List<SpellMod> spellMods = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_mod";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellMod spellMod = new()
                        {
                            Id = reader.GetUInt32("Id"),
                            ProcChance = reader.IsDBNull("procChance") ? -1 : reader.GetInt32("procChance"),
                            ProcFlags = reader.IsDBNull("procFlags") ? -1 : reader.GetInt32("procFlags"),
                            ProcCharges = reader.IsDBNull("procCharges") ? -1 : reader.GetInt32("procCharges"),
                            DurationIndex = reader.IsDBNull("DurationIndex") ? -1 : reader.GetInt32("DurationIndex"),
                            Category = reader.IsDBNull("Category") ? -1 : reader.GetInt32("Category"),
                            CastingTimeIndex = reader.IsDBNull("CastingTimeIndex") ? -1 : reader.GetInt32("CastingTimeIndex"),
                            StackAmount = reader.IsDBNull("StackAmount") ? -1 : reader.GetInt32("StackAmount"),
                            SpellIconId = reader.IsDBNull("SpellIconID") ? -1 : reader.GetInt32("SpellIconID"),
                            ActiveIconId = reader.IsDBNull("activeIconID") ? -1 : reader.GetInt32("activeIconID"),
                            ManaCost = reader.IsDBNull("manaCost") ? -1 : reader.GetInt32("manaCost"),
                            Attributes = reader.IsDBNull("Attributes") ? -1 : reader.GetInt32("Attributes"),
                            AttributesEx = reader.IsDBNull("AttributesEx") ? -1 : reader.GetInt32("AttributesEx"),
                            AttributesEx2 = reader.IsDBNull("AttributesEx2") ? -1 : reader.GetInt32("AttributesEx2"),
                            AttributesEx3 = reader.IsDBNull("AttributesEx3") ? -1 : reader.GetInt32("AttributesEx3"),
                            AttributesEx4 = reader.IsDBNull("AttributesEx4") ? -1 : reader.GetInt32("AttributesEx4"),
                            Custom = reader.GetInt32("Custom"),
                            InterruptFlags = reader.IsDBNull("InterruptFlags") ? -1 : reader.GetInt32("InterruptFlags"),
                            AuraInterruptFlags = reader.IsDBNull("AuraInterruptFlags") ? -1 : reader.GetInt32("AuraInterruptFlags"),
                            ChannelInterruptFlags = reader.IsDBNull("ChannelInterruptFlags") ? -1 : reader.GetInt32("ChannelInterruptFlags"),
                            Dispel = reader.GetInt32("Dispel"),
                            Stances = reader.IsDBNull("Stances") ? -1 : reader.GetInt32("Stances"),
                            StancesNot = reader.IsDBNull("StancesNot") ? -1 : reader.GetInt32("StancesNot"),
                            SpellVisual = reader.IsDBNull("SpellVisual") ? -1 : reader.GetInt32("SpellVisual"),
                            ManaCostPercentage = reader.IsDBNull("ManaCostPercentage") ? -1 : reader.GetInt32("ManaCostPercentage"),
                            StartRecoveryCategory = reader.IsDBNull("StartRecoveryCategory") ? -1 : reader.GetInt32("StartRecoveryCategory"),
                            StartRecoveryTime = reader.IsDBNull("StartRecoveryTime") ? -1 : reader.GetInt32("StartRecoveryTime"),
                            MaxAffectedTargets = reader.IsDBNull("MaxAffectedTargets") ? -1 : reader.GetInt32("MaxAffectedTargets"),
                            MaxTargetLevel = reader.IsDBNull("MaxTargetLevel") ? -1 : reader.GetInt32("MaxTargetLevel"),
                            DmgClass = reader.IsDBNull("DmgClass") ? -1 : reader.GetInt32("DmgClass"),
                            RangeIndex = reader.IsDBNull("rangeIndex") ? -1 : reader.GetInt32("rangeIndex"),
                            RecoveryTime = reader.GetInt32("RecoveryTime"),
                            CategoryRecoveryTime = reader.GetInt32("CategoryRecoveryTime"),
                            SpellFamilyName = reader.GetInt32("SpellFamilyName"),
                            SpellFamilyFlags = reader.GetUInt64("SpellFamilyFlags"),
                            Mechanic = reader.IsDBNull("Mechanic") ? -1 : reader.GetInt32("Mechanic"),
                            EquippedItemClass = reader.IsDBNull("EquippedItemClass") ? -1 : reader.GetInt32("EquippedItemClass"),
                            Comment = reader.IsDBNull("Comment") ? string.Empty : reader.GetString("Comment")
                        };

                        spellMods.Add(spellMod);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellMods;
        }
        public static List<SpellPetAura> GetSpellPetAuras()
        {
            List<SpellPetAura> spellPetAuras = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_pet_auras";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellPetAura spellPetAura = new()
                        {
                            Spell = reader.GetUInt32("spell"),
                            Pet = reader.GetUInt32("pet"),
                            Aura = reader.GetUInt32("aura")
                        };

                        spellPetAuras.Add(spellPetAura);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellPetAuras;
        }
        public static List<SpellProcEvent> GetSpellProcEvents()
        {
            List<SpellProcEvent> spellProcEvents = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_proc_event";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellProcEvent spellProcEvent = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            SchoolMask = reader.GetByte("SchoolMask"),
                            SpellFamilyName = reader.GetUInt16("SpellFamilyName"),
                            SpellFamilyMask0 = reader.GetUInt64("SpellFamilyMask0"),
                            SpellFamilyMask1 = reader.GetUInt64("SpellFamilyMask1"),
                            SpellFamilyMask2 = reader.GetUInt64("SpellFamilyMask2"),
                            ProcFlags = reader.GetUInt32("procFlags"),
                            ProcEx = reader.GetUInt32("procEx"),
                            PpmRate = reader.GetFloat("ppmRate"),
                            CustomChance = reader.GetFloat("CustomChance"),
                            Cooldown = reader.GetUInt32("Cooldown")
                        };

                        spellProcEvents.Add(spellProcEvent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellProcEvents;
        }
        public static List<SpellProcItemEnchant> GetSpellProcItemEnchants()
        {
            List<SpellProcItemEnchant> spellProcItemEnchants = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_proc_item_enchant";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellProcItemEnchant itemEnchant = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            PpmRate = reader.GetFloat("ppmRate")
                        };

                        spellProcItemEnchants.Add(itemEnchant);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellProcItemEnchants;
        }
        public static List<SpellScript> GetSpellScripts()
        {
            List<SpellScript> spellScripts = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_scripts";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellScript spellScript = new()
                        {
                            Id = reader.GetUInt32("id"),
                            Delay = reader.GetUInt32("delay"),
                            Command = reader.GetUInt32("command"),
                            Datalong = reader.GetUInt32("datalong"),
                            Datalong2 = reader.GetUInt32("datalong2"),
                            Datalong3 = reader.GetUInt32("datalong3"),
                            Datalong4 = reader.GetUInt32("datalong4"),
                            TargetParam1 = reader.GetUInt32("target_param1"),
                            TargetParam2 = reader.GetUInt32("target_param2"),
                            TargetType = reader.GetByte("target_type"),
                            DataFlags = reader.GetByte("data_flags"),
                            Dataint = reader.GetInt32("dataint"),
                            Dataint2 = reader.GetInt32("dataint2"),
                            Dataint3 = reader.GetInt32("dataint3"),
                            Dataint4 = reader.GetInt32("dataint4"),
                            X = reader.GetFloat("x"),
                            Y = reader.GetFloat("y"),
                            Z = reader.GetFloat("z"),
                            O = reader.GetFloat("o"),
                            ConditionId = reader.GetUInt32("condition_id"),
                            Comments = reader.GetString("comments")
                        };

                        spellScripts.Add(spellScript);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellScripts;
        }
        public static List<SpellScriptTarget> GetSpellScriptTargets()
        {
            List<SpellScriptTarget> spellScriptTargets = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_script_target";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellScriptTarget target = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Type = reader.GetByte("type"),
                            TargetEntry = reader.GetUInt32("targetEntry")
                        };

                        spellScriptTargets.Add(target);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellScriptTargets;
        }
        public static List<SpellTargetPosition> GetSpellTargetPositions()
        {
            List<SpellTargetPosition> spellTargetPositions = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_target_position";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellTargetPosition targetPosition = new()
                        {
                            Id = reader.GetUInt32("id"),
                            TargetMap = reader.GetUInt16("target_map"),
                            TargetPositionX = reader.GetFloat("target_position_x"),
                            TargetPositionY = reader.GetFloat("target_position_y"),
                            TargetPositionZ = reader.GetFloat("target_position_z"),
                            TargetOrientation = reader.GetFloat("target_orientation")
                        };

                        spellTargetPositions.Add(targetPosition);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellTargetPositions;
        }
        public static List<SpellTemplate> GetSpellTemplates()
        {
            List<SpellTemplate> spellTemplates = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_template";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellTemplate spellTemplate = new()
                        {
                            Id = reader.GetUInt32("ID"),
                            School = reader.GetUInt32("school"),
                            Category = reader.GetUInt32("category"),
                            CastUi = reader.GetUInt32("castUI"),
                            Dispel = reader.GetUInt32("dispel"),
                            Mechanic = reader.GetUInt32("mechanic"),
                            Attributes = reader.GetUInt64("attributes"),
                            AttributesEx = reader.GetUInt32("attributesEx"),
                            AttributesEx2 = reader.GetUInt32("attributesEx2"),
                            AttributesEx3 = reader.GetUInt32("attributesEx3"),
                            AttributesEx4 = reader.GetUInt32("attributesEx4"),
                            Stances = reader.GetUInt32("stances"),
                            StancesNot = reader.GetUInt32("stancesNot"),
                            Targets = reader.GetUInt32("targets"),
                            TargetCreatureType = reader.GetUInt32("targetCreatureType"),
                            RequiresSpellFocus = reader.GetUInt32("requiresSpellFocus"),
                            CasterAuraState = reader.GetUInt32("casterAuraState"),
                            TargetAuraState = reader.GetUInt32("targetAuraState"),
                            CastingTimeIndex = reader.GetUInt32("castingTimeIndex"),
                            RecoveryTime = reader.GetUInt32("recoveryTime"),
                            CategoryRecoveryTime = reader.GetUInt32("categoryRecoveryTime"),
                            InterruptFlags = reader.GetUInt32("interruptFlags"),
                            AuraInterruptFlags = reader.GetUInt32("auraInterruptFlags"),
                            ChannelInterruptFlags = reader.GetUInt32("channelInterruptFlags"),
                            ProcFlags = reader.GetUInt32("procFlags"),
                            ProcChance = reader.GetUInt32("procChance"),
                            ProcCharges = reader.GetUInt32("procCharges"),
                            MaxLevel = reader.GetUInt32("maxLevel"),
                            BaseLevel = reader.GetUInt32("baseLevel"),
                            SpellLevel = reader.GetUInt32("spellLevel"),
                            DurationIndex = reader.GetUInt32("durationIndex"),
                            PowerType = reader.GetUInt32("powerType"),
                            ManaCost = reader.GetUInt32("manaCost"),
                            ManaCostPerLevel = reader.GetUInt32("manCostPerLevel"),
                            ManaPerSecond = reader.GetUInt32("manaPerSecond"),
                            ManaPerSecondPerLevel = reader.GetUInt32("manaPerSecondPerLevel"),
                            RangeIndex = reader.GetUInt32("rangeIndex"),
                            Speed = reader.GetFloat("speed"),
                            ModelNextSpell = reader.GetUInt32("modelNextSpell"),
                            StackAmount = reader.GetUInt32("stackAmount"),
                            Totem1 = reader.GetUInt32("totem1"),
                            Totem2 = reader.GetUInt32("totem2"),
                            Reagent1 = reader.GetUInt32("reagent1"),
                            Reagent2 = reader.GetUInt32("reagent2"),
                            Reagent3 = reader.GetUInt32("reagent3"),
                            Reagent4 = reader.GetUInt32("reagent4"),
                            Reagent5 = reader.GetUInt32("reagent5"),
                            Reagent6 = reader.GetUInt32("reagent6"),
                            Reagent7 = reader.GetUInt32("reagent7"),
                            Reagent8 = reader.GetUInt32("reagent8"),
                            ReagentCount1 = reader.GetUInt32("reagentCount1"),
                            ReagentCount2 = reader.GetUInt32("reagentCount2"),
                            ReagentCount3 = reader.GetUInt32("reagentCount3"),
                            ReagentCount4 = reader.GetUInt32("reagentCount4"),
                            ReagentCount5 = reader.GetUInt32("reagentCount5"),
                            ReagentCount6 = reader.GetUInt32("reagentCount6"),
                            ReagentCount7 = reader.GetUInt32("reagentCount7"),
                            ReagentCount8 = reader.GetUInt32("reagentCount8"),
                            EquippedItemClass = reader.GetInt32("equippedItemClass"),
                            EquippedItemSubClassMask = reader.GetInt32("equippedItemSubClassMask"),
                            EquippedItemInventoryTypeMask = reader.GetInt32("equippedItemInventoryTypeMask"),
                            Effect1 = reader.GetUInt32("effect1"),
                            Effect2 = reader.GetUInt32("effect2"),
                            Effect3 = reader.GetUInt32("effect3"),
                            EffectDieSides1 = reader.GetInt32("effectDieSides1"),
                            EffectDieSides2 = reader.GetInt32("effectDieSides2"),
                            EffectDieSides3 = reader.GetInt32("effectDieSides3"),
                            EffectBaseDice1 = reader.GetInt32("effectBaseDice1"),
                            EffectBaseDice2 = reader.GetInt32("effectBaseDice2"),
                            EffectBaseDice3 = reader.GetInt32("effectBaseDice3"),
                            EffectDicePerLevel1 = reader.GetFloat("effectDicePerLevel1"),
                            EffectDicePerLevel2 = reader.GetFloat("effectDicePerLevel2"),
                            EffectDicePerLevel3 = reader.GetFloat("effectDicePerLevel3"),
                            EffectRealPointsPerLevel1 = reader.GetFloat("effectRealPointsPerLevel1"),
                            EffectRealPointsPerLevel2 = reader.GetFloat("effectRealPointsPerLevel2"),
                            EffectRealPointsPerLevel3 = reader.GetFloat("effectRealPointsPerLevel3"),
                            EffectBasePoints1 = reader.GetInt32("effectBasePoints1"),
                            EffectBasePoints2 = reader.GetInt32("effectBasePoints2"),
                            EffectBasePoints3 = reader.GetInt32("effectBasePoints3"),
                            EffectMechanic1 = reader.GetUInt32("effectMechanic1"),
                            EffectMechanic2 = reader.GetUInt32("effectMechanic2"),
                            EffectMechanic3 = reader.GetUInt32("effectMechanic3"),
                            EffectImplicitTargetA1 = reader.GetUInt32("effectImplicitTargetA1"),
                            EffectImplicitTargetA2 = reader.GetUInt32("effectImplicitTargetA2"),
                            EffectImplicitTargetA3 = reader.GetUInt32("effectImplicitTargetA3"),
                            EffectImplicitTargetB1 = reader.GetUInt32("effectImplicitTargetB1"),
                            EffectImplicitTargetB2 = reader.GetUInt32("effectImplicitTargetB2"),
                            EffectImplicitTargetB3 = reader.GetUInt32("effectImplicitTargetB3"),
                            EffectRadiusIndex1 = reader.GetUInt32("effectRadiusIndex1"),
                            EffectRadiusIndex2 = reader.GetUInt32("effectRadiusIndex2"),
                            EffectRadiusIndex3 = reader.GetUInt32("effectRadiusIndex3"),
                            EffectApplyAuraName1 = reader.GetUInt32("effectApplyAuraName1"),
                            EffectApplyAuraName2 = reader.GetUInt32("effectApplyAuraName2"),
                            EffectApplyAuraName3 = reader.GetUInt32("effectApplyAuraName3"),
                            EffectAmplitude1 = reader.GetUInt32("effectAmplitude1"),
                            EffectAmplitude2 = reader.GetUInt32("effectAmplitude2"),
                            EffectAmplitude3 = reader.GetUInt32("effectAmplitude3"),
                            EffectMultipleValue1 = reader.GetFloat("effectMultipleValue1"),
                            EffectMultipleValue2 = reader.GetFloat("effectMultipleValue2"),
                            EffectMultipleValue3 = reader.GetFloat("effectMultipleValue3"),
                            EffectChainTarget1 = reader.GetUInt32("effectChainTarget1"),
                            EffectChainTarget2 = reader.GetUInt32("effectChainTarget2"),
                            EffectChainTarget3 = reader.GetUInt32("effectChainTarget3"),
                            EffectItemType1 = reader.GetUInt32("effectItemType1"),
                            EffectItemType2 = reader.GetUInt32("effectItemType2"),
                            EffectItemType3 = reader.GetUInt32("effectItemType3"),
                            EffectMiscValue1 = reader.GetInt32("effectMiscValue1"),
                            EffectMiscValue2 = reader.GetInt32("effectMiscValue2"),
                            EffectMiscValue3 = reader.GetInt32("effectMiscValue3"),
                            EffectTriggerSpell1 = reader.GetUInt32("effectTriggerSpell1"),
                            EffectTriggerSpell2 = reader.GetUInt32("effectTriggerSpell2"),
                            EffectTriggerSpell3 = reader.GetUInt32("effectTriggerSpell3"),
                            EffectPointsPerComboPoint1 = reader.GetFloat("effectPointsPerComboPoint1"),
                            EffectPointsPerComboPoint2 = reader.GetFloat("effectPointsPerComboPoint2"),
                            EffectPointsPerComboPoint3 = reader.GetFloat("effectPointsPerComboPoint3"),
                            SpellVisual1 = reader.GetUInt32("spellVisual1"),
                            SpellVisual2 = reader.GetUInt32("spellVisual2"),
                            SpellIconId = reader.GetUInt32("spellIconId"),
                            ActiveIconId = reader.GetUInt32("activeIconId"),
                            SpellPriority = reader.GetUInt32("spellPriority"),
                            Name1 = reader.GetString("name1"),
                            Name2 = reader.GetString("name2"),
                            Name3 = reader.GetString("name3"),
                            Name4 = reader.GetString("name4"),
                            Name5 = reader.GetString("name5"),
                            Name6 = reader.GetString("name6"),
                            Name7 = reader.GetString("name7"),
                            Name8 = reader.GetString("name8"),
                            NameFlags = reader.GetUInt32("nameFlags"),
                            NameSubtext1 = reader.GetString("nameSubtext1"),
                            NameSubtext2 = reader.GetString("nameSubtext2"),
                            NameSubtext3 = reader.GetString("nameSubtext3"),
                            NameSubtext4 = reader.GetString("nameSubtext4"),
                            NameSubtext5 = reader.GetString("nameSubtext5"),
                            NameSubtext6 = reader.GetString("nameSubtext6"),
                            NameSubtext7 = reader.GetString("nameSubtext7"),
                            NameSubtext8 = reader.GetString("nameSubtext8"),
                            NameSubtextFlags = reader.GetUInt32("nameSubtextFlags"),
                            Description1 = reader.GetString("description1"),
                            Description2 = reader.GetString("description2"),
                            Description3 = reader.GetString("description3"),
                            Description4 = reader.GetString("description4"),
                            Description5 = reader.GetString("description5"),
                            Description6 = reader.GetString("description6"),
                            Description7 = reader.GetString("description7"),
                            Description8 = reader.GetString("description8"),
                            DescriptionFlags = reader.GetUInt32("descriptionFlags"),
                            AuraDescription1 = reader.GetString("auraDescription1"),
                            AuraDescription2 = reader.GetString("auraDescription2"),
                            AuraDescription3 = reader.GetString("auraDescription3"),
                            AuraDescription4 = reader.GetString("auraDescription4"),
                            AuraDescription5 = reader.GetString("auraDescription5"),
                            AuraDescription6 = reader.GetString("auraDescription6"),
                            AuraDescription7 = reader.GetString("auraDescription7"),
                            AuraDescription8 = reader.GetString("auraDescription8"),
                            AuraDescriptionFlags = reader.GetUInt32("auraDescriptionFlags"),
                            ManaCostPercentage = reader.GetUInt32("manaCostPercentage"),
                            StartRecoveryCategory = reader.GetUInt32("startRecoveryCategory"),
                            StartRecoveryTime = reader.GetUInt32("startRecoveryTime"),
                            MaxTargetLevel = reader.GetUInt32("maxTargetLevel"),
                            SpellFamilyName = reader.GetUInt32("spellFamilyName"),
                            SpellFamilyFlags = reader.GetUInt64("spellFamilyFlags"),
                            MaxAffectedTargets = reader.GetUInt32("maxAffectedTargets"),
                            DmgClass = reader.GetUInt32("dmgClass"),
                            PreventionType = reader.GetUInt32("preventionType"),
                            StanceBarOrder = reader.GetInt32("stanceBarOrder"),
                            DmgMultiplier1 = reader.GetFloat("dmgMultiplier1"),
                            DmgMultiplier2 = reader.GetFloat("dmgMultiplier2"),
                            DmgMultiplier3 = reader.GetFloat("dmgMultiplier3"),
                            MinFactionId = reader.GetUInt32("minFactionId"),
                            MinReputation = reader.GetUInt32("minReputation"),
                            RequiredAuraVision = reader.GetUInt32("requiredAuraVision")
                        };


                        spellTemplates.Add(spellTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellTemplates;
        }
        public static List<SpellThreat> GetSpellThreats()
        {
            List<SpellThreat> spellThreats = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM spell_threat";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        SpellThreat spellThreat = new()
                        {
                            Entry = reader.GetUInt32("entry"),
                            Threat = reader.GetInt32("Threat"),
                            Multiplier = reader.GetFloat("multiplier"),
                            ApBonus = reader.GetFloat("ap_bonus")
                        };

                        spellThreats.Add(spellThreat);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return spellThreats;
        }
        public static List<TaxiPathTransition> GetTaxiPathTransitions()
        {
            List<TaxiPathTransition> taxiPathTransitions = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM taxi_path_transitions";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        TaxiPathTransition transition = new()
                        {
                            InPath = reader.GetUInt32("inPath"),
                            OutPath = reader.GetUInt32("outPath"),
                            InNode = reader.GetUInt32("inNode"),
                            OutNode = reader.GetUInt32("outNode"),
                            Comment = reader.IsDBNull("comment") ? string.Empty : reader.GetString("comment")
                        };

                        taxiPathTransitions.Add(transition);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return taxiPathTransitions;
        }
        public static List<Transport> GetTransports()
        {
            List<Transport> transports = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM transports";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Transport transport = new()
                        {
                            Guid = reader.GetUInt32("guid"),
                            Entry = reader.GetUInt32("entry"),
                            Name = reader.IsDBNull("name") ? string.Empty : reader.GetString("name"),
                            Period = reader.GetUInt32("period")
                        };

                        transports.Add(transport);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return transports;
        }
        public static List<Variables> GetVariables()
        {
            List<Variables> variables = [];

            using (MySqlConnection connection = new(ConnectionString))
            {
                try
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM variables";

                    using MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Variables variable = new()
                        {
                            Index = reader.GetUInt32("index"),
                            Value = reader.GetUInt32("value")
                        };

                        variables.Add(variable);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message} {ex.StackTrace}");
                }
            }

            return variables;
        }

        private static Timestamp GetTimestampSafe(MySqlDataReader reader, string columnName)
        {
            MySqlDateTime mySqlDateTime = reader.GetMySqlDateTime(reader.GetOrdinal(columnName));

            // Check if the MySqlDateTime is a valid date or '0000-00-00 00:00:00'
            if (!mySqlDateTime.IsValidDateTime)
            {
                // Return a default value for '0000-00-00 00:00:00' or invalid dates
                return Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime());
            }

            // Convert to a valid DateTime and return the Timestamp
            return Timestamp.FromDateTime(mySqlDateTime.GetDateTime().ToUniversalTime());
        }
    }
}
