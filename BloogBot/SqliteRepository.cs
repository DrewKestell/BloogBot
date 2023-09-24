using BloogBot.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using WoWBot.Client.Models;

namespace BloogBot
{
    internal class SqliteRepository
    {
        private static readonly string ConnectionString = "Data Source=database.db";
        private static readonly string PreparedSql = @"database.sql";
        private static readonly List<string> TableNames = new List<string>()
        {
            "items",
            "creature",
            "creature_involvedrelation",
            "creature_template",
            "creature_loot_template",
            "gameobject",
            "gameobject_template",
            "gameobject_loot_template",
            "npc_vendor",
            "quest_poi_points",
            "quest_template"
        };

        public static void Initialize()
        {
            try
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    foreach (var table in TableNames)
                    {
                        command = connection.CreateCommand();
                        command.CommandText = $@"
                                                    SELECT *
                                                    FROM {table}
                                                    LIMIT 1;
                                                ";

                        command.ExecuteReader();
                    }
                }
            }
            catch (Exception)
            {
                //using (var connection = new SqliteConnection(ConnectionString))
                //{
                //    connection.Open();

                //    var command = new SqliteCommand(File.ReadAllText(PreparedSql), connection);

                //    command.ExecuteReader();
                //}
            }
        }
        public static Item GetItemById(ulong id)
        {
            Item item = null;

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM items
                                            WHERE ItemId = $id
                                        ";
                command.Parameters.AddWithValue("$id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        item = new Item()
                        {
                            ItemId = reader.GetInt32(reader.GetOrdinal("ItemId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Quality = reader.GetInt32(reader.GetOrdinal("Quality")),
                            Patch = reader.GetInt32(reader.GetOrdinal("Patch")),
                            Class = reader.GetInt32(reader.GetOrdinal("Class")),
                            Subclass = reader.GetInt32(reader.GetOrdinal("Subclass")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DisplayId = reader.GetInt32(reader.GetOrdinal("DisplayId")),
                            Flags = reader.GetInt32(reader.GetOrdinal("Flags")),
                            BuyCount = reader.GetInt32(reader.GetOrdinal("BuyCount")),
                            BuyPrice = reader.GetInt32(reader.GetOrdinal("BuyPrice")),
                            SellPrice = reader.GetInt32(reader.GetOrdinal("SellPrice")),
                            InventoryType = reader.GetInt32(reader.GetOrdinal("InventoryType")),
                            AllowableClass = reader.GetInt32(reader.GetOrdinal("AllowableClass")),
                            AllowableRace = reader.GetInt32(reader.GetOrdinal("AllowableRace")),
                            ItemLevel = reader.GetInt32(reader.GetOrdinal("ItemLevel")),
                            RequiredLevel = reader.GetInt32(reader.GetOrdinal("RequiredLevel")),
                            RequiredSkill = (short)reader.GetInt32(reader.GetOrdinal("RequiredSkill")),
                            RequiredSkillRank = (short)reader.GetInt32(reader.GetOrdinal("RequiredSkillRank")),
                            RequiredSpell = reader.GetInt32(reader.GetOrdinal("RequiredSpell")),
                            RequiredHonorRank = reader.GetInt32(reader.GetOrdinal("RequiredHonorRank")),
                            RequiredCityRank = reader.GetInt32(reader.GetOrdinal("RequiredCityRank")),
                            RequiredReputationFaction = reader.GetInt32(reader.GetOrdinal("RequiredReputationFaction")),
                            RequiredReputationRank = reader.GetInt32(reader.GetOrdinal("RequiredReputationRank")),
                            MaxCount = reader.GetInt32(reader.GetOrdinal("MaxCount")),
                            Stackable = reader.GetInt32(reader.GetOrdinal("Stackable")),
                            ContainerSlots = reader.GetInt32(reader.GetOrdinal("ContainerSlots")),
                            StatType1 = reader.GetInt32(reader.GetOrdinal("StatType1")),
                            StatValue1 = reader.GetInt32(reader.GetOrdinal("StatValue1")),
                            StatType2 = reader.GetInt32(reader.GetOrdinal("StatType2")),
                            StatValue2 = reader.GetInt32(reader.GetOrdinal("StatValue2")),
                            StatType3 = reader.GetInt32(reader.GetOrdinal("StatType3")),
                            StatValue3 = reader.GetInt32(reader.GetOrdinal("StatValue3")),
                            StatType4 = reader.GetInt32(reader.GetOrdinal("StatType4")),
                            StatValue4 = reader.GetInt32(reader.GetOrdinal("StatValue4")),
                            StatType5 = reader.GetInt32(reader.GetOrdinal("StatType5")),
                            StatValue5 = reader.GetInt32(reader.GetOrdinal("StatValue5")),
                            StatType6 = reader.GetInt32(reader.GetOrdinal("StatType6")),
                            StatValue6 = reader.GetInt32(reader.GetOrdinal("StatValue6")),
                            StatType7 = reader.GetInt32(reader.GetOrdinal("StatType7")),
                            StatValue7 = reader.GetInt32(reader.GetOrdinal("StatValue7")),
                            StatType8 = reader.GetInt32(reader.GetOrdinal("StatType8")),
                            StatValue8 = reader.GetInt32(reader.GetOrdinal("StatValue8")),
                            StatType9 = reader.GetInt32(reader.GetOrdinal("StatType9")),
                            StatValue9 = reader.GetInt32(reader.GetOrdinal("StatValue9")),
                            StatType10 = reader.GetInt32(reader.GetOrdinal("StatType10")),
                            StatValue10 = reader.GetInt32(reader.GetOrdinal("StatValue10")),
                            Delay = reader.GetInt32(reader.GetOrdinal("Delay")),
                            RangeMod = reader.GetFloat(reader.GetOrdinal("RangeMod")),
                            AmmoType = reader.GetInt32(reader.GetOrdinal("AmmoType")),
                            DmgMin1 = reader.GetFloat(reader.GetOrdinal("DmgMin1")),
                            DmgMax1 = reader.GetFloat(reader.GetOrdinal("DmgMax1")),
                            DmgType1 = reader.GetInt32(reader.GetOrdinal("DmgType1")),
                            DmgMin2 = reader.GetFloat(reader.GetOrdinal("DmgMin2")),
                            DmgMax2 = reader.GetFloat(reader.GetOrdinal("DmgMax2")),
                            DmgType2 = reader.GetInt32(reader.GetOrdinal("DmgType2")),
                            DmgMin3 = reader.GetFloat(reader.GetOrdinal("DmgMin3")),
                            DmgMax3 = reader.GetFloat(reader.GetOrdinal("DmgMax3")),
                            DmgType3 = reader.GetInt32(reader.GetOrdinal("DmgType3")),
                            DmgMin4 = reader.GetFloat(reader.GetOrdinal("DmgMin4")),
                            DmgMax4 = reader.GetFloat(reader.GetOrdinal("DmgMax4")),
                            DmgType4 = reader.GetInt32(reader.GetOrdinal("DmgType4")),
                            DmgMin5 = reader.GetFloat(reader.GetOrdinal("DmgMin5")),
                            DmgMax5 = reader.GetFloat(reader.GetOrdinal("DmgMax5")),
                            DmgType5 = reader.GetInt32(reader.GetOrdinal("DmgType5")),
                            Block = reader.GetInt32(reader.GetOrdinal("Block")),
                            Armor = reader.GetInt32(reader.GetOrdinal("Armor")),
                            HolyRes = reader.GetInt32(reader.GetOrdinal("HolyRes")),
                            FireRes = reader.GetInt32(reader.GetOrdinal("FireRes")),
                            NatureRes = reader.GetInt32(reader.GetOrdinal("NatureRes")),
                            FrostRes = reader.GetInt32(reader.GetOrdinal("FrostRes")),
                            ShadowRes = reader.GetInt32(reader.GetOrdinal("ShadowRes")),
                            ArcaneRes = reader.GetInt32(reader.GetOrdinal("ArcaneRes")),
                            SpellId1 = reader.GetInt32(reader.GetOrdinal("SpellId1")),
                            SpellTrigger1 = reader.GetInt32(reader.GetOrdinal("SpellTrigger1")),
                            SpellCharges1 = reader.GetInt32(reader.GetOrdinal("SpellCharges1")),
                            SpellPpmRate1 = reader.GetFloat(reader.GetOrdinal("SpellPpmRate1")),
                            SpellCooldown1 = reader.GetInt32(reader.GetOrdinal("SpellCooldown1")),
                            SpellCategory1 = reader.GetInt32(reader.GetOrdinal("SpellCategory1")),
                            SpellCategoryCooldown1 = reader.GetInt32(reader.GetOrdinal("SpellCategoryCooldown1")),
                            SpellId2 = reader.GetInt32(reader.GetOrdinal("SpellId2")),
                            SpellTrigger2 = reader.GetInt32(reader.GetOrdinal("SpellTrigger2")),
                            SpellCharges2 = reader.GetInt32(reader.GetOrdinal("SpellCharges2")),
                            SpellPpmRate2 = reader.GetFloat(reader.GetOrdinal("SpellPpmRate2")),
                            SpellCooldown2 = reader.GetInt32(reader.GetOrdinal("SpellCooldown2")),
                            SpellCategory2 = reader.GetInt32(reader.GetOrdinal("SpellCategory2")),
                            SpellCategoryCooldown2 = reader.GetInt32(reader.GetOrdinal("SpellCategoryCooldown2")),
                            SpellId3 = reader.GetInt32(reader.GetOrdinal("SpellId3")),
                            SpellTrigger3 = reader.GetInt32(reader.GetOrdinal("SpellTrigger3")),
                            SpellCharges3 = reader.GetInt32(reader.GetOrdinal("SpellCharges3")),
                            SpellPpmRate3 = reader.GetFloat(reader.GetOrdinal("SpellPpmRate3")),
                            SpellCooldown3 = reader.GetInt32(reader.GetOrdinal("SpellCooldown3")),
                            SpellCategory3 = reader.GetInt32(reader.GetOrdinal("SpellCategory3")),
                            SpellCategoryCooldown3 = reader.GetInt32(reader.GetOrdinal("SpellCategoryCooldown3")),
                            SpellId4 = reader.GetInt32(reader.GetOrdinal("SpellId4")),
                            SpellTrigger4 = reader.GetInt32(reader.GetOrdinal("SpellTrigger4")),
                            SpellCharges4 = reader.GetInt32(reader.GetOrdinal("SpellCharges4")),
                            SpellPpmRate4 = reader.GetFloat(reader.GetOrdinal("SpellPpmRate4")),
                            SpellCooldown4 = reader.GetInt32(reader.GetOrdinal("SpellCooldown4")),
                            SpellCategory4 = reader.GetInt32(reader.GetOrdinal("SpellCategory4")),
                            SpellCategoryCooldown4 = reader.GetInt32(reader.GetOrdinal("SpellCategoryCooldown4")),
                            SpellId5 = reader.GetInt32(reader.GetOrdinal("SpellId5")),
                            SpellTrigger5 = reader.GetInt32(reader.GetOrdinal("SpellTrigger5")),
                            SpellCharges5 = reader.GetInt32(reader.GetOrdinal("SpellCharges5")),
                            SpellPpmRate5 = reader.GetFloat(reader.GetOrdinal("SpellPpmRate5")),
                            SpellCooldown5 = reader.GetInt32(reader.GetOrdinal("SpellCooldown5")),
                            SpellCategory5 = reader.GetInt32(reader.GetOrdinal("SpellCategory5")),
                            SpellCategoryCooldown5 = reader.GetInt32(reader.GetOrdinal("SpellCategoryCooldown5")),
                            Bonding = reader.GetInt32(reader.GetOrdinal("Bonding")),
                            PageText = reader.GetInt32(reader.GetOrdinal("PageText")),
                            PageLanguage = reader.GetInt32(reader.GetOrdinal("PageLanguage")),
                            PageMaterial = reader.GetInt32(reader.GetOrdinal("PageMaterial")),
                            StartQuest = reader.GetInt32(reader.GetOrdinal("StartQuest")),
                            LockId = reader.GetInt32(reader.GetOrdinal("LockId")),
                            Material = reader.GetInt32(reader.GetOrdinal("Material")),
                            Sheath = reader.GetInt32(reader.GetOrdinal("Sheath")),
                            RandomProperty = reader.GetInt32(reader.GetOrdinal("RandomProperty")),
                            SetId = reader.GetInt32(reader.GetOrdinal("SetId")),
                            MaxDurability = reader.GetInt32(reader.GetOrdinal("MaxDurability")),
                            AreaBound = reader.GetInt32(reader.GetOrdinal("AreaBound")),
                            MapBound = reader.GetInt32(reader.GetOrdinal("MapBound")),
                            Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                            BagFamily = reader.GetInt32(reader.GetOrdinal("BagFamily")),
                            DisenchantId = reader.GetInt32(reader.GetOrdinal("DisenchantId")),
                            FoodType = reader.GetInt32(reader.GetOrdinal("FoodType")),
                            MinMoneyLoot = reader.GetInt32(reader.GetOrdinal("MinMoneyLoot")),
                            MaxMoneyLoot = reader.GetInt32(reader.GetOrdinal("MaxMoneyLoot")),
                            ExtraFlags = reader.GetInt32(reader.GetOrdinal("ExtraFlags")),
                            OtherTeamEntry = reader.GetInt32(reader.GetOrdinal("OtherTeamEntry"))
                        };
                    }
                }
            }
            return item;
        }
        public static QuestTemplate GetQuestTemplateByID(int id)
        {
            QuestTemplate questTemplate = null;
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM quest_template
                                            WHERE entry = $id
                                        ";
                command.Parameters.AddWithValue("$id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questTemplate = new QuestTemplate
                        {
                            Entry = Convert.ToInt32(reader["entry"]),
                            Method = Convert.ToByte(reader["Method"]),
                            ZoneOrSort = Convert.ToInt16(reader["ZoneOrSort"]),
                            MinLevel = Convert.ToByte(reader["MinLevel"]),
                            MaxLevel = Convert.ToByte(reader["MaxLevel"]),
                            QuestLevel = Convert.ToInt16(reader["QuestLevel"]),
                            Type = Convert.ToInt16(reader["Type"]),
                            RequiredClasses = Convert.ToInt16(reader["RequiredClasses"]),
                            RequiredRaces = Convert.ToInt16(reader["RequiredRaces"]),
                            RequiredSkill = Convert.ToInt16(reader["RequiredSkill"]),
                            RequiredSkillValue = Convert.ToInt16(reader["RequiredSkillValue"]),
                            RequiredCondition = Convert.ToInt32(reader["RequiredCondition"]),
                            RepObjectiveFaction = Convert.ToInt16(reader["RepObjectiveFaction"]),
                            RepObjectiveValue = Convert.ToInt32(reader["RepObjectiveValue"]),
                            RequiredMinRepFaction = Convert.ToInt16(reader["RequiredMinRepFaction"]),
                            RequiredMinRepValue = Convert.ToInt32(reader["RequiredMinRepValue"]),
                            RequiredMaxRepFaction = Convert.ToInt16(reader["RequiredMaxRepFaction"]),
                            RequiredMaxRepValue = Convert.ToInt32(reader["RequiredMaxRepValue"]),
                            SuggestedPlayers = Convert.ToByte(reader["SuggestedPlayers"]),
                            LimitTime = Convert.ToInt32(reader["LimitTime"]),
                            QuestFlags = Convert.ToInt16(reader["QuestFlags"]),
                            SpecialFlags = Convert.ToByte(reader["SpecialFlags"]),
                            PrevQuestId = Convert.ToInt32(reader["PrevQuestId"]),
                            NextQuestId = Convert.ToInt32(reader["NextQuestId"]),
                            ExclusiveGroup = Convert.ToInt32(reader["ExclusiveGroup"]),
                            BreadcrumbForQuestId = Convert.ToInt32(reader["BreadcrumbForQuestId"]),
                            NextQuestInChain = Convert.ToInt32(reader["NextQuestInChain"]),
                            SrcItemId = Convert.ToInt32(reader["SrcItemId"]),
                            SrcItemCount = Convert.ToByte(reader["SrcItemCount"]),
                            SrcSpell = Convert.ToInt32(reader["SrcSpell"]),
                            Title = reader["Title"].ToString(),
                            Details = reader["Details"].ToString(),
                            Objectives = reader["Objectives"].ToString(),
                            OfferRewardText = reader["OfferRewardText"].ToString(),
                            RequestItemsText = reader["RequestItemsText"].ToString(),
                            EndText = reader["EndText"].ToString(),
                            ObjectiveText1 = reader["ObjectiveText1"].ToString(),
                            ObjectiveText2 = reader["ObjectiveText2"].ToString(),
                            ObjectiveText3 = reader["ObjectiveText3"].ToString(),
                            ObjectiveText4 = reader["ObjectiveText4"].ToString(),
                            ReqItemId1 = Convert.ToInt32(reader["ReqItemId1"]),
                            ReqItemId2 = Convert.ToInt32(reader["ReqItemId2"]),
                            ReqItemId3 = Convert.ToInt32(reader["ReqItemId3"]),
                            ReqItemId4 = Convert.ToInt32(reader["ReqItemId4"]),
                            ReqItemCount1 = Convert.ToInt16(reader["ReqItemCount1"]),
                            ReqItemCount2 = Convert.ToInt16(reader["ReqItemCount2"]),
                            ReqItemCount3 = Convert.ToInt16(reader["ReqItemCount3"]),
                            ReqItemCount4 = Convert.ToInt16(reader["ReqItemCount4"]),
                            ReqSourceId1 = Convert.ToInt32(reader["ReqSourceId1"]),
                            ReqSourceId2 = Convert.ToInt32(reader["ReqSourceId2"]),
                            ReqSourceId3 = Convert.ToInt32(reader["ReqSourceId3"]),
                            ReqSourceId4 = Convert.ToInt32(reader["ReqSourceId4"]),
                            ReqSourceCount1 = Convert.ToInt16(reader["ReqSourceCount1"]),
                            ReqSourceCount2 = Convert.ToInt16(reader["ReqSourceCount2"]),
                            ReqSourceCount3 = Convert.ToInt16(reader["ReqSourceCount3"]),
                            ReqSourceCount4 = Convert.ToInt16(reader["ReqSourceCount4"]),
                            ReqCreatureOrGOId1 = Convert.ToInt32(reader["ReqCreatureOrGOId1"]),
                            ReqCreatureOrGOId2 = Convert.ToInt32(reader["ReqCreatureOrGOId2"]),
                            ReqCreatureOrGOId3 = Convert.ToInt32(reader["ReqCreatureOrGOId3"]),
                            ReqCreatureOrGOId4 = Convert.ToInt32(reader["ReqCreatureOrGOId4"]),
                            ReqCreatureOrGOCount1 = Convert.ToInt16(reader["ReqCreatureOrGOCount1"]),
                            ReqCreatureOrGOCount2 = Convert.ToInt16(reader["ReqCreatureOrGOCount2"]),
                            ReqCreatureOrGOCount3 = Convert.ToInt16(reader["ReqCreatureOrGOCount3"]),
                            ReqCreatureOrGOCount4 = Convert.ToInt16(reader["ReqCreatureOrGOCount4"]),
                            ReqSpellCast1 = Convert.ToInt32(reader["ReqSpellCast1"]),
                            ReqSpellCast2 = Convert.ToInt32(reader["ReqSpellCast2"]),
                            ReqSpellCast3 = Convert.ToInt32(reader["ReqSpellCast3"]),
                            ReqSpellCast4 = Convert.ToInt32(reader["ReqSpellCast4"]),
                            RewChoiceItemId1 = (ulong)Convert.ToInt32(reader["RewChoiceItemId1"]),
                            RewChoiceItemId2 = (ulong)Convert.ToInt32(reader["RewChoiceItemId2"]),
                            RewChoiceItemId3 = (ulong)Convert.ToInt32(reader["RewChoiceItemId3"]),
                            RewChoiceItemId4 = (ulong)Convert.ToInt32(reader["RewChoiceItemId4"]),
                            RewChoiceItemId5 = (ulong)Convert.ToInt32(reader["RewChoiceItemId5"]),
                            RewChoiceItemId6 = (ulong)Convert.ToInt32(reader["RewChoiceItemId6"]),
                            RewChoiceItemCount1 = Convert.ToInt16(reader["RewChoiceItemCount1"]),
                            RewChoiceItemCount2 = Convert.ToInt16(reader["RewChoiceItemCount2"]),
                            RewChoiceItemCount3 = Convert.ToInt16(reader["RewChoiceItemCount3"]),
                            RewChoiceItemCount4 = Convert.ToInt16(reader["RewChoiceItemCount4"]),
                            RewChoiceItemCount5 = Convert.ToInt16(reader["RewChoiceItemCount5"]),
                            RewChoiceItemCount6 = Convert.ToInt16(reader["RewChoiceItemCount6"]),
                            RewItemId1 = Convert.ToInt32(reader["RewItemId1"]),
                            RewItemId2 = Convert.ToInt32(reader["RewItemId2"]),
                            RewItemId3 = Convert.ToInt32(reader["RewItemId3"]),
                            RewItemId4 = Convert.ToInt32(reader["RewItemId4"]),
                            RewItemCount1 = Convert.ToInt16(reader["RewItemCount1"]),
                            RewItemCount2 = Convert.ToInt16(reader["RewItemCount2"]),
                            RewItemCount3 = Convert.ToInt16(reader["RewItemCount3"]),
                            RewItemCount4 = Convert.ToInt16(reader["RewItemCount4"]),
                            RewRepFaction1 = Convert.ToInt16(reader["RewRepFaction1"]),
                            RewRepFaction2 = Convert.ToInt16(reader["RewRepFaction2"]),
                            RewRepFaction3 = Convert.ToInt16(reader["RewRepFaction3"]),
                            RewRepFaction4 = Convert.ToInt16(reader["RewRepFaction4"]),
                            RewRepFaction5 = Convert.ToInt16(reader["RewRepFaction5"]),
                            RewRepValue1 = Convert.ToInt32(reader["RewRepValue1"]),
                            RewRepValue2 = Convert.ToInt32(reader["RewRepValue2"]),
                            RewRepValue3 = Convert.ToInt32(reader["RewRepValue3"]),
                            RewRepValue4 = Convert.ToInt32(reader["RewRepValue4"]),
                            RewRepValue5 = Convert.ToInt32(reader["RewRepValue5"]),
                            RewOrReqMoney = Convert.ToInt32(reader["RewOrReqMoney"]),
                            RewMoneyMaxLevel = Convert.ToUInt32(reader["RewMoneyMaxLevel"]),
                            RewSpell = Convert.ToInt32(reader["RewSpell"]),
                            RewSpellCast = Convert.ToInt32(reader["RewSpellCast"]),
                            RewMailTemplateId = Convert.ToInt32(reader["RewMailTemplateId"]),
                            RewMailDelaySecs = (uint)Convert.ToInt32(reader["RewMailDelaySecs"]),
                            PointMapId = (ushort)Convert.ToInt16(reader["PointMapId"]),
                            PointX = Convert.ToSingle(reader["PointX"]),
                            PointY = Convert.ToSingle(reader["PointY"]),
                            PointOpt = (uint)Convert.ToInt32(reader["PointOpt"]),
                            DetailsEmote1 = (ushort)Convert.ToInt16(reader["DetailsEmote1"]),
                            DetailsEmote2 = (ushort)Convert.ToInt16(reader["DetailsEmote2"]),
                            DetailsEmote3 = (ushort)Convert.ToInt16(reader["DetailsEmote3"]),
                            DetailsEmote4 = (ushort)Convert.ToInt16(reader["DetailsEmote4"]),
                            DetailsEmoteDelay1 = Convert.ToInt32(reader["DetailsEmoteDelay1"]),
                            DetailsEmoteDelay2 = Convert.ToInt32(reader["DetailsEmoteDelay2"]),
                            DetailsEmoteDelay3 = Convert.ToInt32(reader["DetailsEmoteDelay3"]),
                            DetailsEmoteDelay4 = Convert.ToInt32(reader["DetailsEmoteDelay4"]),
                            IncompleteEmote = (ushort)Convert.ToInt16(reader["IncompleteEmote"]),
                            IncompleteEmoteDelay = Convert.ToInt32(reader["IncompleteEmoteDelay"]),
                            CompleteEmote = (ushort)Convert.ToInt16(reader["CompleteEmote"]),
                            CompleteEmoteDelay = Convert.ToInt32(reader["CompleteEmoteDelay"]),
                            OfferRewardEmote1 = (ushort)Convert.ToInt16(reader["OfferRewardEmote1"]),
                            OfferRewardEmote2 = (ushort)Convert.ToInt16(reader["OfferRewardEmote2"]),
                            OfferRewardEmote3 = (ushort)Convert.ToInt16(reader["OfferRewardEmote3"]),
                            OfferRewardEmote4 = (ushort)Convert.ToInt16(reader["OfferRewardEmote4"]),
                            OfferRewardEmoteDelay1 = Convert.ToInt32(reader["OfferRewardEmoteDelay1"]),
                            OfferRewardEmoteDelay2 = Convert.ToInt32(reader["OfferRewardEmoteDelay2"]),
                            OfferRewardEmoteDelay3 = Convert.ToInt32(reader["OfferRewardEmoteDelay3"]),
                            OfferRewardEmoteDelay4 = Convert.ToInt32(reader["OfferRewardEmoteDelay4"]),
                            StartScript = Convert.ToInt32(reader["StartScript"]),
                            CompleteScript = Convert.ToInt32(reader["CompleteScript"])
                        };
                    }
                }
            }
            return questTemplate;
        }
        public static List<Creature> GetCreaturesById(ulong id)
        {
            List<Creature> creatures = new List<Creature>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature
                                            WHERE id = @id
                                        ";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Creature creature = new Creature
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            Id = Convert.ToInt32(reader["id"]),
                            Map = Convert.ToInt16(reader["map"]),
                            SpawnMask = Convert.ToByte(reader["spawnMask"]),
                            ModelId = Convert.ToInt32(reader["modelid"]),
                            EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                            PositionX = Convert.ToSingle(reader["position_x"]),
                            PositionY = Convert.ToSingle(reader["position_y"]),
                            PositionZ = Convert.ToSingle(reader["position_z"]),
                            Orientation = Convert.ToSingle(reader["orientation"]),
                            SpawnTimeSecsMin = Convert.ToInt32(reader["spawntimesecsmin"]),
                            SpawnTimeSecsMax = Convert.ToInt32(reader["spawntimesecsmax"]),
                            SpawnDist = Convert.ToSingle(reader["spawndist"]),
                            CurrentWaypoint = Convert.ToInt32(reader["currentwaypoint"]),
                            CurHealth = Convert.ToInt32(reader["curhealth"]),
                            CurMana = Convert.ToInt32(reader["curmana"]),
                            DeathState = Convert.ToByte(reader["DeathState"]),
                            MovementType = Convert.ToByte(reader["MovementType"])
                        };

                        creatures.Add(creature);
                    }
                }
            }
            return creatures;
        }
        public static CreatureTemplate GetCreatureTemplateById(ulong id)
        {
            CreatureTemplate creatureTemplate = null;
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature_template
                                            WHERE Entry = @id
                                        ";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        creatureTemplate = new CreatureTemplate()
                        {
                            Entry = reader.GetInt32(reader.GetOrdinal("Entry")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            SubName = reader.IsDBNull(reader.GetOrdinal("SubName")) ? string.Empty : reader.GetString(reader.GetOrdinal("SubName")),
                            MinLevel = reader.GetByte(reader.GetOrdinal("MinLevel")),
                            MaxLevel = reader.GetByte(reader.GetOrdinal("MaxLevel")),
                            ModelId1 = reader.GetInt32(reader.GetOrdinal("ModelId1")),
                            ModelId2 = reader.GetInt32(reader.GetOrdinal("ModelId2")),
                            ModelId3 = reader.GetInt32(reader.GetOrdinal("ModelId3")),
                            ModelId4 = reader.GetInt32(reader.GetOrdinal("ModelId4")),
                            Faction = reader.GetInt16(reader.GetOrdinal("Faction")),
                            Scale = reader.GetFloat(reader.GetOrdinal("Scale")),
                            Family = reader.GetByte(reader.GetOrdinal("Family")),
                            CreatureType = reader.GetByte(reader.GetOrdinal("CreatureType")),
                            InhabitType = reader.GetByte(reader.GetOrdinal("InhabitType")),
                            RegenerateStats = reader.GetByte(reader.GetOrdinal("RegenerateStats")),
                            RacialLeader = reader.GetByte(reader.GetOrdinal("RacialLeader")),
                            NpcFlags = reader.GetInt32(reader.GetOrdinal("NpcFlags")),
                            UnitFlags = reader.GetInt32(reader.GetOrdinal("UnitFlags")),
                            DynamicFlags = reader.GetInt32(reader.GetOrdinal("DynamicFlags")),
                            ExtraFlags = reader.GetInt32(reader.GetOrdinal("ExtraFlags")),
                            CreatureTypeFlags = reader.GetInt32(reader.GetOrdinal("CreatureTypeFlags")),
                            SpeedWalk = reader.GetFloat(reader.GetOrdinal("SpeedWalk")),
                            SpeedRun = reader.GetFloat(reader.GetOrdinal("SpeedRun")),
                            Detection = reader.GetInt32(reader.GetOrdinal("Detection")),
                            CallForHelp = reader.GetInt32(reader.GetOrdinal("CallForHelp")),
                            Pursuit = reader.GetInt32(reader.GetOrdinal("Pursuit")),
                            Leash = reader.GetInt32(reader.GetOrdinal("Leash")),
                            Timeout = reader.GetInt32(reader.GetOrdinal("Timeout")),
                            UnitClass = reader.GetByte(reader.GetOrdinal("UnitClass")),
                            Rank = reader.GetByte(reader.GetOrdinal("Rank")),
                            HealthMultiplier = reader.GetFloat(reader.GetOrdinal("HealthMultiplier")),
                            PowerMultiplier = reader.GetFloat(reader.GetOrdinal("PowerMultiplier")),
                            DamageMultiplier = reader.GetFloat(reader.GetOrdinal("DamageMultiplier")),
                            DamageVariance = reader.GetFloat(reader.GetOrdinal("DamageVariance")),
                            ArmorMultiplier = reader.GetFloat(reader.GetOrdinal("ArmorMultiplier")),
                            ExperienceMultiplier = reader.GetFloat(reader.GetOrdinal("ExperienceMultiplier")),
                            MinLevelHealth = reader.GetInt32(reader.GetOrdinal("MinLevelHealth")),
                            MaxLevelHealth = reader.GetInt32(reader.GetOrdinal("MaxLevelHealth")),
                            MinLevelMana = reader.GetInt32(reader.GetOrdinal("MinLevelMana")),
                            MaxLevelMana = reader.GetInt32(reader.GetOrdinal("MaxLevelMana")),
                            MinMeleeDmg = reader.GetFloat(reader.GetOrdinal("MinMeleeDmg")),
                            MaxMeleeDmg = reader.GetFloat(reader.GetOrdinal("MaxMeleeDmg")),
                            MinRangedDmg = reader.GetFloat(reader.GetOrdinal("MinRangedDmg")),
                            MaxRangedDmg = reader.GetFloat(reader.GetOrdinal("MaxRangedDmg")),
                            Armor = reader.GetInt32(reader.GetOrdinal("Armor")),
                            MeleeAttackPower = reader.GetInt32(reader.GetOrdinal("MeleeAttackPower")),
                            RangedAttackPower = reader.GetInt16(reader.GetOrdinal("RangedAttackPower")),
                            MeleeBaseAttackTime = reader.GetInt32(reader.GetOrdinal("MeleeBaseAttackTime")),
                            RangedBaseAttackTime = reader.GetInt32(reader.GetOrdinal("RangedBaseAttackTime")),
                            DamageSchool = reader.GetByte(reader.GetOrdinal("DamageSchool")),
                            MinLootGold = reader.GetInt32(reader.GetOrdinal("MinLootGold")),
                            MaxLootGold = reader.GetInt32(reader.GetOrdinal("MaxLootGold")),
                            LootId = reader.GetInt32(reader.GetOrdinal("LootId")),
                            PickpocketLootId = reader.GetInt32(reader.GetOrdinal("PickpocketLootId")),
                            SkinningLootId = reader.GetInt32(reader.GetOrdinal("SkinningLootId")),
                            KillCredit1 = reader.GetInt32(reader.GetOrdinal("KillCredit1")),
                            KillCredit2 = reader.GetInt32(reader.GetOrdinal("KillCredit2")),
                            MechanicImmuneMask = reader.GetInt32(reader.GetOrdinal("MechanicImmuneMask")),
                            SchoolImmuneMask = reader.GetInt32(reader.GetOrdinal("SchoolImmuneMask")),
                            ResistanceHoly = reader.GetByte(reader.GetOrdinal("ResistanceHoly")),
                            ResistanceFire = reader.GetByte(reader.GetOrdinal("ResistanceFire")),
                            ResistanceNature = reader.GetByte(reader.GetOrdinal("ResistanceNature")),
                            ResistanceFrost = reader.GetByte(reader.GetOrdinal("ResistanceFrost")),
                            ResistanceShadow = reader.GetByte(reader.GetOrdinal("ResistanceShadow")),
                            ResistanceArcane = reader.GetByte(reader.GetOrdinal("ResistanceArcane")),
                            PetSpellDataId = reader.GetInt32(reader.GetOrdinal("PetSpellDataId")),
                            MovementType = reader.GetByte(reader.GetOrdinal("MovementType")),
                            TrainerType = reader.GetByte(reader.GetOrdinal("TrainerType")),
                            TrainerSpell = reader.GetInt32(reader.GetOrdinal("TrainerSpell")),
                            TrainerClass = reader.GetByte(reader.GetOrdinal("TrainerClass")),
                            TrainerRace = reader.GetByte(reader.GetOrdinal("TrainerRace")),
                            TrainerTemplateId = reader.GetInt32(reader.GetOrdinal("TrainerTemplateId")),
                            VendorTemplateId = reader.GetInt32(reader.GetOrdinal("VendorTemplateId")),
                            GossipMenuId = reader.GetInt32(reader.GetOrdinal("GossipMenuId")),
                            InteractionPauseTimer = reader.GetInt32(reader.GetOrdinal("InteractionPauseTimer")),
                            VisibilityDistanceType = reader.GetByte(reader.GetOrdinal("visibilityDistanceType")),
                            CorpseDecay = reader.GetInt32(reader.GetOrdinal("CorpseDecay")),
                            SpellList = reader.GetInt32(reader.GetOrdinal("SpellList")),
                            EquipmentTemplateId = reader.GetInt32(reader.GetOrdinal("EquipmentTemplateId")),
                            Civilian = reader.GetByte(reader.GetOrdinal("Civilian")),
                            AIName = reader.GetString(reader.GetOrdinal("AIName")),
                            ScriptName = reader.GetString(reader.GetOrdinal("ScriptName")),
                        };
                    }
                }
            }
            return creatureTemplate;
        }
        public static List<int> GetQuestRelatedNPCsByQuestId(int id)
        {
            List<int> questNpcs = new List<int>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT A.*, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
                                            FROM creature_involvedrelation A
                                            WHERE A.quest = @id
                                            ORDER BY rn
                                        ";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questNpcs.Add(reader.GetInt16(reader.GetOrdinal("id")));
                    }
                }
            }
            return questNpcs;
        }
        public static List<GameObject> GetGameObjectsById(ulong id)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM gameobject
                                            WHERE id = @id
                                        ";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GameObject gameObject = new GameObject()
                        {
                            Guid = reader.GetInt32(reader.GetOrdinal("guid")),
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Map = reader.GetInt16(reader.GetOrdinal("map")),
                            SpawnMask = reader.GetByte(reader.GetOrdinal("spawnMask")),
                            PositionX = reader.GetFloat(reader.GetOrdinal("position_x")),
                            PositionY = reader.GetFloat(reader.GetOrdinal("position_y")),
                            PositionZ = reader.GetFloat(reader.GetOrdinal("position_z")),
                            Orientation = reader.GetFloat(reader.GetOrdinal("orientation")),
                            Rotation0 = reader.GetFloat(reader.GetOrdinal("rotation0")),
                            Rotation1 = reader.GetFloat(reader.GetOrdinal("rotation1")),
                            Rotation2 = reader.GetFloat(reader.GetOrdinal("rotation2")),
                            Rotation3 = reader.GetFloat(reader.GetOrdinal("rotation3")),
                            SpawnTimeSecsMin = reader.GetInt32(reader.GetOrdinal("spawntimesecsmin")),
                            SpawnTimeSecsMax = reader.GetInt32(reader.GetOrdinal("spawntimesecsmax")),
                            AnimProgress = reader.GetByte(reader.GetOrdinal("animprogress")),
                            State = reader.GetByte(reader.GetOrdinal("state")),
                        };

                        gameObjects.Add(gameObject);
                    }
                }
            }
            return gameObjects;
        }
        public static List<GameObject> GetGameObjectByLootableItemId(int itemId)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT DISTINCT gob.*
                                            FROM        gameobject                  gob
                                            LEFT JOIN   gameobject_template         got     ON  gob.id      = got.entry
                                            LEFT JOIN   gameobject_loot_template    golt    ON  golt.entry  = got.data1
                                            WHERE                                               golt.item   = @itemId
                                        ";
                command.Parameters.AddWithValue("@itemId", itemId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GameObject gameObject = new GameObject()
                        {
                            Guid = reader.GetInt32(reader.GetOrdinal("guid")),
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Map = reader.GetInt16(reader.GetOrdinal("map")),
                            SpawnMask = reader.GetByte(reader.GetOrdinal("spawnMask")),
                            PositionX = reader.GetFloat(reader.GetOrdinal("position_x")),
                            PositionY = reader.GetFloat(reader.GetOrdinal("position_y")),
                            PositionZ = reader.GetFloat(reader.GetOrdinal("position_z")),
                            Orientation = reader.GetFloat(reader.GetOrdinal("orientation")),
                            Rotation0 = reader.GetFloat(reader.GetOrdinal("rotation0")),
                            Rotation1 = reader.GetFloat(reader.GetOrdinal("rotation1")),
                            Rotation2 = reader.GetFloat(reader.GetOrdinal("rotation2")),
                            Rotation3 = reader.GetFloat(reader.GetOrdinal("rotation3")),
                            SpawnTimeSecsMin = reader.GetInt32(reader.GetOrdinal("spawntimesecsmin")),
                            SpawnTimeSecsMax = reader.GetInt32(reader.GetOrdinal("spawntimesecsmax")),
                            AnimProgress = reader.GetByte(reader.GetOrdinal("animprogress")),
                            State = reader.GetByte(reader.GetOrdinal("state")),
                        };

                        gameObjects.Add(gameObject);
                    }
                }
            }
            return gameObjects;
        }
        public static List<Creature> GetCreaturesByLootableItemId(int itemId)
        {
            List<Creature> creatures = new List<Creature>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      crt.*
                                            FROM        creature crt
                                            LEFT JOIN   creature_template       ct  ON  crt.id      = ct.entry
                                            LEFT JOIN   creature_loot_template  clt ON  ct.entry    = clt.entry
                                            WHERE                                       clt.item    = @itemId
                                        ";
                command.Parameters.AddWithValue("@itemId", itemId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Creature creature = new Creature
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            Id = Convert.ToInt32(reader["id"]),
                            Map = Convert.ToInt16(reader["map"]),
                            SpawnMask = Convert.ToByte(reader["spawnMask"]),
                            ModelId = Convert.ToInt32(reader["modelid"]),
                            EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                            PositionX = Convert.ToSingle(reader["position_x"]),
                            PositionY = Convert.ToSingle(reader["position_y"]),
                            PositionZ = Convert.ToSingle(reader["position_z"]),
                            Orientation = Convert.ToSingle(reader["orientation"]),
                            SpawnTimeSecsMin = Convert.ToInt32(reader["spawntimesecsmin"]),
                            SpawnTimeSecsMax = Convert.ToInt32(reader["spawntimesecsmax"]),
                            SpawnDist = Convert.ToSingle(reader["spawndist"]),
                            CurrentWaypoint = Convert.ToInt32(reader["currentwaypoint"]),
                            CurHealth = Convert.ToInt32(reader["curhealth"]),
                            CurMana = Convert.ToInt32(reader["curmana"]),
                            DeathState = Convert.ToByte(reader["DeathState"]),
                            MovementType = Convert.ToByte(reader["MovementType"])
                        };

                        creatures.Add(creature);
                    }
                }
            }
            return creatures;
        }
        public static List<CreatureTemplate> GetAllClassTrainers()
        {
            List<CreatureTemplate> creatureTemplates = new List<CreatureTemplate>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      ct.*
                                            FROM        creature_template   ct
                                            WHERE       ct.TrainerClass     != 0
                                            AND         ct.TrainerType      == 0
                                        ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CreatureTemplate creatureTemplate = new CreatureTemplate()
                        {
                            Entry = reader.GetInt32(reader.GetOrdinal("Entry")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            SubName = reader.IsDBNull(reader.GetOrdinal("SubName")) ? string.Empty : reader.GetString(reader.GetOrdinal("SubName")),
                            MinLevel = reader.GetByte(reader.GetOrdinal("MinLevel")),
                            MaxLevel = reader.GetByte(reader.GetOrdinal("MaxLevel")),
                            ModelId1 = reader.GetInt32(reader.GetOrdinal("ModelId1")),
                            ModelId2 = reader.GetInt32(reader.GetOrdinal("ModelId2")),
                            ModelId3 = reader.GetInt32(reader.GetOrdinal("ModelId3")),
                            ModelId4 = reader.GetInt32(reader.GetOrdinal("ModelId4")),
                            Faction = reader.GetInt16(reader.GetOrdinal("Faction")),
                            Scale = reader.GetFloat(reader.GetOrdinal("Scale")),
                            Family = reader.GetByte(reader.GetOrdinal("Family")),
                            CreatureType = reader.GetByte(reader.GetOrdinal("CreatureType")),
                            InhabitType = reader.GetByte(reader.GetOrdinal("InhabitType")),
                            RegenerateStats = reader.GetByte(reader.GetOrdinal("RegenerateStats")),
                            RacialLeader = reader.GetByte(reader.GetOrdinal("RacialLeader")),
                            NpcFlags = reader.GetInt32(reader.GetOrdinal("NpcFlags")),
                            UnitFlags = reader.GetInt32(reader.GetOrdinal("UnitFlags")),
                            DynamicFlags = reader.GetInt32(reader.GetOrdinal("DynamicFlags")),
                            ExtraFlags = reader.GetInt32(reader.GetOrdinal("ExtraFlags")),
                            CreatureTypeFlags = reader.GetInt32(reader.GetOrdinal("CreatureTypeFlags")),
                            SpeedWalk = reader.GetFloat(reader.GetOrdinal("SpeedWalk")),
                            SpeedRun = reader.GetFloat(reader.GetOrdinal("SpeedRun")),
                            Detection = reader.GetInt32(reader.GetOrdinal("Detection")),
                            CallForHelp = reader.GetInt32(reader.GetOrdinal("CallForHelp")),
                            Pursuit = reader.GetInt32(reader.GetOrdinal("Pursuit")),
                            Leash = reader.GetInt32(reader.GetOrdinal("Leash")),
                            Timeout = reader.GetInt32(reader.GetOrdinal("Timeout")),
                            UnitClass = reader.GetByte(reader.GetOrdinal("UnitClass")),
                            Rank = reader.GetByte(reader.GetOrdinal("Rank")),
                            HealthMultiplier = reader.GetFloat(reader.GetOrdinal("HealthMultiplier")),
                            PowerMultiplier = reader.GetFloat(reader.GetOrdinal("PowerMultiplier")),
                            DamageMultiplier = reader.GetFloat(reader.GetOrdinal("DamageMultiplier")),
                            DamageVariance = reader.GetFloat(reader.GetOrdinal("DamageVariance")),
                            ArmorMultiplier = reader.GetFloat(reader.GetOrdinal("ArmorMultiplier")),
                            ExperienceMultiplier = reader.GetFloat(reader.GetOrdinal("ExperienceMultiplier")),
                            MinLevelHealth = reader.GetInt32(reader.GetOrdinal("MinLevelHealth")),
                            MaxLevelHealth = reader.GetInt32(reader.GetOrdinal("MaxLevelHealth")),
                            MinLevelMana = reader.GetInt32(reader.GetOrdinal("MinLevelMana")),
                            MaxLevelMana = reader.GetInt32(reader.GetOrdinal("MaxLevelMana")),
                            MinMeleeDmg = reader.GetFloat(reader.GetOrdinal("MinMeleeDmg")),
                            MaxMeleeDmg = reader.GetFloat(reader.GetOrdinal("MaxMeleeDmg")),
                            MinRangedDmg = reader.GetFloat(reader.GetOrdinal("MinRangedDmg")),
                            MaxRangedDmg = reader.GetFloat(reader.GetOrdinal("MaxRangedDmg")),
                            Armor = reader.GetInt32(reader.GetOrdinal("Armor")),
                            MeleeAttackPower = reader.GetInt32(reader.GetOrdinal("MeleeAttackPower")),
                            RangedAttackPower = reader.GetInt16(reader.GetOrdinal("RangedAttackPower")),
                            MeleeBaseAttackTime = reader.GetInt32(reader.GetOrdinal("MeleeBaseAttackTime")),
                            RangedBaseAttackTime = reader.GetInt32(reader.GetOrdinal("RangedBaseAttackTime")),
                            DamageSchool = reader.GetByte(reader.GetOrdinal("DamageSchool")),
                            MinLootGold = reader.GetInt32(reader.GetOrdinal("MinLootGold")),
                            MaxLootGold = reader.GetInt32(reader.GetOrdinal("MaxLootGold")),
                            LootId = reader.GetInt32(reader.GetOrdinal("LootId")),
                            PickpocketLootId = reader.GetInt32(reader.GetOrdinal("PickpocketLootId")),
                            SkinningLootId = reader.GetInt32(reader.GetOrdinal("SkinningLootId")),
                            KillCredit1 = reader.GetInt32(reader.GetOrdinal("KillCredit1")),
                            KillCredit2 = reader.GetInt32(reader.GetOrdinal("KillCredit2")),
                            MechanicImmuneMask = reader.GetInt32(reader.GetOrdinal("MechanicImmuneMask")),
                            SchoolImmuneMask = reader.GetInt32(reader.GetOrdinal("SchoolImmuneMask")),
                            ResistanceHoly = reader.GetInt32(reader.GetOrdinal("ResistanceHoly")),
                            ResistanceFire = reader.GetInt32(reader.GetOrdinal("ResistanceFire")),
                            ResistanceNature = reader.GetInt32(reader.GetOrdinal("ResistanceNature")),
                            ResistanceFrost = reader.GetInt32(reader.GetOrdinal("ResistanceFrost")),
                            ResistanceShadow = reader.GetInt32(reader.GetOrdinal("ResistanceShadow")),
                            ResistanceArcane = reader.GetInt32(reader.GetOrdinal("ResistanceArcane")),
                            PetSpellDataId = reader.GetInt32(reader.GetOrdinal("PetSpellDataId")),
                            MovementType = reader.GetByte(reader.GetOrdinal("MovementType")),
                            TrainerType = reader.GetByte(reader.GetOrdinal("TrainerType")),
                            TrainerSpell = reader.GetInt32(reader.GetOrdinal("TrainerSpell")),
                            TrainerClass = reader.GetByte(reader.GetOrdinal("TrainerClass")),
                            TrainerRace = reader.GetByte(reader.GetOrdinal("TrainerRace")),
                            TrainerTemplateId = reader.GetInt32(reader.GetOrdinal("TrainerTemplateId")),
                            VendorTemplateId = reader.GetInt32(reader.GetOrdinal("VendorTemplateId")),
                            GossipMenuId = reader.GetInt32(reader.GetOrdinal("GossipMenuId")),
                            InteractionPauseTimer = reader.GetInt32(reader.GetOrdinal("InteractionPauseTimer")),
                            VisibilityDistanceType = reader.GetByte(reader.GetOrdinal("visibilityDistanceType")),
                            CorpseDecay = reader.GetInt32(reader.GetOrdinal("CorpseDecay")),
                            SpellList = reader.GetInt32(reader.GetOrdinal("SpellList")),
                            EquipmentTemplateId = reader.GetInt32(reader.GetOrdinal("EquipmentTemplateId")),
                            Civilian = reader.GetByte(reader.GetOrdinal("Civilian")),
                            AIName = reader.GetString(reader.GetOrdinal("AIName")),
                            ScriptName = reader.GetString(reader.GetOrdinal("ScriptName")),
                        };

                        creatureTemplates.Add(creatureTemplate);
                    }
                }
            }
            return creatureTemplates;
        }
        public static List<Creature> GetAllVendors()
        {
            List<Creature> creatures = new List<Creature>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      ct.*, crt.Name
                                            FROM        npc_vendor   npcv
                                            LEFT JOIN   creature_template   ct  ON  npcv.Entry  = ct.Entry
                                            LEFT JOIN   creature            crt ON  crt.id      = ct.entry
                                            GROUP BY    ct.Entry
                                        ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Creature creature = new Creature
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Id = Convert.ToInt32(reader["id"]),
                            Map = Convert.ToInt16(reader["map"]),
                            SpawnMask = Convert.ToByte(reader["spawnMask"]),
                            ModelId = Convert.ToInt32(reader["modelid"]),
                            EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                            PositionX = Convert.ToSingle(reader["position_x"]),
                            PositionY = Convert.ToSingle(reader["position_y"]),
                            PositionZ = Convert.ToSingle(reader["position_z"]),
                            Orientation = Convert.ToSingle(reader["orientation"]),
                            SpawnTimeSecsMin = Convert.ToInt32(reader["spawntimesecsmin"]),
                            SpawnTimeSecsMax = Convert.ToInt32(reader["spawntimesecsmax"]),
                            SpawnDist = Convert.ToSingle(reader["spawndist"]),
                            CurrentWaypoint = Convert.ToInt32(reader["currentwaypoint"]),
                            CurHealth = Convert.ToInt32(reader["curhealth"]),
                            CurMana = Convert.ToInt32(reader["curmana"]),
                            DeathState = Convert.ToByte(reader["DeathState"]),
                            MovementType = Convert.ToByte(reader["MovementType"])
                        };

                        creatures.Add(creature);
                    }
                }
            }
            return creatures;
        }
        public static List<NpcVendorEntry> GetAllItemsSoldByVendorByEntry(int entry)
        {
            List<NpcVendorEntry> npcVendorEntries = new List<NpcVendorEntry>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      npcv.*
                                            FROM        npc_vendor   npcv
                                            WHERE       npcv.Entry = @entry
                                        ";

                command.Parameters.AddWithValue("@entry", entry);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NpcVendorEntry npcVendorEntry = new NpcVendorEntry()
                        {
                            Entry = reader.GetInt32(reader.GetOrdinal("entry")),
                            Item = reader.GetInt32(reader.GetOrdinal("item")),
                            MaxCount = reader.GetInt32(reader.GetOrdinal("maxcount")),
                            IncrTime = reader.GetInt32(reader.GetOrdinal("incrtime")),
                            Slot = reader.GetInt32(reader.GetOrdinal("slot")),
                            ConditionId = reader.GetInt32(reader.GetOrdinal("condition_id")),
                            Comments = reader.GetString(reader.GetOrdinal("comments"))
                        };

                        npcVendorEntries.Add(npcVendorEntry);
                    }
                }
            }
            return npcVendorEntries;
        }
    }
}
