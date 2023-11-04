using RaidMemberBot.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Numerics;

namespace RaidMemberBot
{
    internal class SqliteRepository
    {
        private static readonly string ConnectionString = "Data Source=data.sqlite";
        private static readonly string PreparedSql = @"Repository\SqliteSchema.SQL";

        public static AreaTriggerTeleport GetAreaTriggerTeleportById(int id)
        {
            AreaTriggerTeleport teleport = null;

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = @"
                                            SELECT *
                                            FROM areatrigger_teleport
                                            WHERE id = $id
                                        ";
                    command.Parameters.AddWithValue("$id", id.ToString());
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
                }
                catch (Exception ex) { Console.WriteLine($"{ex.StackTrace}"); }
            }

            return teleport;
        }

        public static AreaTriggerTeleport GetAreaTriggerTeleportByMapId(int id)
        {
            AreaTriggerTeleport teleport = null;

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = @"
                                            SELECT *
                                            FROM areatrigger_teleport
                                            WHERE target_map = $id
                                        ";
                    command.Parameters.AddWithValue("$id", id.ToString());
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
                }
                catch (Exception ex) { Console.WriteLine($"{ex.StackTrace}"); }
            }

            return teleport;
        }
        public static Item GetItemById(ulong id)
        {
            Item item = null;

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM items
                                            WHERE itemId = $itemId
                                        ";

                command.Parameters.AddWithValue("$itemId", id.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        item = new Item()
                        {
                            ItemId = Convert.ToInt32(reader["ItemId"]),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Quality = Convert.ToInt32(reader["Quality"]),
                            Patch = Convert.ToInt32(reader["Patch"]),
                            Class = Convert.ToInt32(reader["Class"]),
                            Subclass = Convert.ToInt32(reader["Subclass"]),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DisplayId = Convert.ToInt32(reader["DisplayId"]),
                            Flags = Convert.ToInt32(reader["Flags"]),
                            BuyCount = Convert.ToInt32(reader["BuyCount"]),
                            BuyPrice = Convert.ToInt32(reader["BuyPrice"]),
                            SellPrice = Convert.ToInt32(reader["SellPrice"]),
                            InventoryType = Convert.ToInt32(reader["InventoryType"]),
                            AllowableClass = Convert.ToInt32(reader["AllowableClass"]),
                            AllowableRace = Convert.ToInt32(reader["AllowableRace"]),
                            ItemLevel = Convert.ToInt32(reader["ItemLevel"]),
                            RequiredLevel = Convert.ToInt32(reader["RequiredLevel"]),
                            RequiredSkill = (short)Convert.ToInt32(reader["RequiredSkill"]),
                            RequiredSkillRank = (short)Convert.ToInt32(reader["RequiredSkillRank"]),
                            RequiredSpell = Convert.ToInt32(reader["RequiredSpell"]),
                            RequiredHonorRank = Convert.ToInt32(reader["RequiredHonorRank"]),
                            RequiredCityRank = Convert.ToInt32(reader["RequiredCityRank"]),
                            RequiredReputationFaction = Convert.ToInt32(reader["RequiredReputationFaction"]),
                            RequiredReputationRank = Convert.ToInt32(reader["RequiredReputationRank"]),
                            MaxCount = Convert.ToInt32(reader["MaxCount"]),
                            Stackable = Convert.ToInt32(reader["Stackable"]),
                            ContainerSlots = Convert.ToInt32(reader["ContainerSlots"]),
                            StatType1 = Convert.ToInt32(reader["StatType1"]),
                            StatValue1 = Convert.ToInt32(reader["StatValue1"]),
                            StatType2 = Convert.ToInt32(reader["StatType2"]),
                            StatValue2 = Convert.ToInt32(reader["StatValue2"]),
                            StatType3 = Convert.ToInt32(reader["StatType3"]),
                            StatValue3 = Convert.ToInt32(reader["StatValue3"]),
                            StatType4 = Convert.ToInt32(reader["StatType4"]),
                            StatValue4 = Convert.ToInt32(reader["StatValue4"]),
                            StatType5 = Convert.ToInt32(reader["StatType5"]),
                            StatValue5 = Convert.ToInt32(reader["StatValue5"]),
                            StatType6 = Convert.ToInt32(reader["StatType6"]),
                            StatValue6 = Convert.ToInt32(reader["StatValue6"]),
                            StatType7 = Convert.ToInt32(reader["StatType7"]),
                            StatValue7 = Convert.ToInt32(reader["StatValue7"]),
                            StatType8 = Convert.ToInt32(reader["StatType8"]),
                            StatValue8 = Convert.ToInt32(reader["StatValue8"]),
                            StatType9 = Convert.ToInt32(reader["StatType9"]),
                            StatValue9 = Convert.ToInt32(reader["StatValue9"]),
                            StatType10 = Convert.ToInt32(reader["StatType10"]),
                            StatValue10 = Convert.ToInt32(reader["StatValue10"]),
                            Delay = Convert.ToInt32(reader["Delay"]),
                            RangeMod = Convert.ToSingle(reader["RangeMod"]),
                            AmmoType = Convert.ToInt32(reader["AmmoType"]),
                            DmgMin1 = Convert.ToSingle(reader["DmgMin1"]),
                            DmgMax1 = Convert.ToSingle(reader["DmgMax1"]),
                            DmgType1 = Convert.ToInt32(reader["DmgType1"]),
                            DmgMin2 = Convert.ToSingle(reader["DmgMin2"]),
                            DmgMax2 = Convert.ToSingle(reader["DmgMax2"]),
                            DmgType2 = Convert.ToInt32(reader["DmgType2"]),
                            DmgMin3 = Convert.ToSingle(reader["DmgMin3"]),
                            DmgMax3 = Convert.ToSingle(reader["DmgMax3"]),
                            DmgType3 = Convert.ToInt32(reader["DmgType3"]),
                            DmgMin4 = Convert.ToSingle(reader["DmgMin4"]),
                            DmgMax4 = Convert.ToSingle(reader["DmgMax4"]),
                            DmgType4 = Convert.ToInt32(reader["DmgType4"]),
                            DmgMin5 = Convert.ToSingle(reader["DmgMin5"]),
                            DmgMax5 = Convert.ToSingle(reader["DmgMax5"]),
                            DmgType5 = Convert.ToInt32(reader["DmgType5"]),
                            Block = Convert.ToInt32(reader["Block"]),
                            Armor = Convert.ToInt32(reader["Armor"]),
                            HolyRes = Convert.ToInt32(reader["HolyRes"]),
                            FireRes = Convert.ToInt32(reader["FireRes"]),
                            NatureRes = Convert.ToInt32(reader["NatureRes"]),
                            FrostRes = Convert.ToInt32(reader["FrostRes"]),
                            ShadowRes = Convert.ToInt32(reader["ShadowRes"]),
                            ArcaneRes = Convert.ToInt32(reader["ArcaneRes"]),
                            SpellId1 = Convert.ToInt32(reader["SpellId1"]),
                            SpellTrigger1 = Convert.ToInt32(reader["SpellTrigger1"]),
                            SpellCharges1 = Convert.ToInt32(reader["SpellCharges1"]),
                            SpellPpmRate1 = Convert.ToSingle(reader["SpellPpmRate1"]),
                            SpellCooldown1 = Convert.ToInt32(reader["SpellCooldown1"]),
                            SpellCategory1 = Convert.ToInt32(reader["SpellCategory1"]),
                            SpellCategoryCooldown1 = Convert.ToInt32(reader["SpellCategoryCooldown1"]),
                            SpellId2 = Convert.ToInt32(reader["SpellId2"]),
                            SpellTrigger2 = Convert.ToInt32(reader["SpellTrigger2"]),
                            SpellCharges2 = Convert.ToInt32(reader["SpellCharges2"]),
                            SpellPpmRate2 = Convert.ToSingle(reader["SpellPpmRate2"]),
                            SpellCooldown2 = Convert.ToInt32(reader["SpellCooldown2"]),
                            SpellCategory2 = Convert.ToInt32(reader["SpellCategory2"]),
                            SpellCategoryCooldown2 = Convert.ToInt32(reader["SpellCategoryCooldown2"]),
                            SpellId3 = Convert.ToInt32(reader["SpellId3"]),
                            SpellTrigger3 = Convert.ToInt32(reader["SpellTrigger3"]),
                            SpellCharges3 = Convert.ToInt32(reader["SpellCharges3"]),
                            SpellPpmRate3 = Convert.ToSingle(reader["SpellPpmRate3"]),
                            SpellCooldown3 = Convert.ToInt32(reader["SpellCooldown3"]),
                            SpellCategory3 = Convert.ToInt32(reader["SpellCategory3"]),
                            SpellCategoryCooldown3 = Convert.ToInt32(reader["SpellCategoryCooldown3"]),
                            SpellId4 = Convert.ToInt32(reader["SpellId4"]),
                            SpellTrigger4 = Convert.ToInt32(reader["SpellTrigger4"]),
                            SpellCharges4 = Convert.ToInt32(reader["SpellCharges4"]),
                            SpellPpmRate4 = Convert.ToSingle(reader["SpellPpmRate4"]),
                            SpellCooldown4 = Convert.ToInt32(reader["SpellCooldown4"]),
                            SpellCategory4 = Convert.ToInt32(reader["SpellCategory4"]),
                            SpellCategoryCooldown4 = Convert.ToInt32(reader["SpellCategoryCooldown4"]),
                            SpellId5 = Convert.ToInt32(reader["SpellId5"]),
                            SpellTrigger5 = Convert.ToInt32(reader["SpellTrigger5"]),
                            SpellCharges5 = Convert.ToInt32(reader["SpellCharges5"]),
                            SpellPpmRate5 = Convert.ToSingle(reader["SpellPpmRate5"]),
                            SpellCooldown5 = Convert.ToInt32(reader["SpellCooldown5"]),
                            SpellCategory5 = Convert.ToInt32(reader["SpellCategory5"]),
                            SpellCategoryCooldown5 = Convert.ToInt32(reader["SpellCategoryCooldown5"]),
                            Bonding = Convert.ToInt32(reader["Bonding"]),
                            PageText = Convert.ToInt32(reader["PageText"]),
                            PageLanguage = Convert.ToInt32(reader["PageLanguage"]),
                            PageMaterial = Convert.ToInt32(reader["PageMaterial"]),
                            StartQuest = Convert.ToInt32(reader["StartQuest"]),
                            LockId = Convert.ToInt32(reader["LockId"]),
                            Material = Convert.ToInt32(reader["Material"]),
                            Sheath = Convert.ToInt32(reader["Sheath"]),
                            RandomProperty = Convert.ToInt32(reader["RandomProperty"]),
                            SetId = Convert.ToInt32(reader["SetId"]),
                            MaxDurability = Convert.ToInt32(reader["MaxDurability"]),
                            AreaBound = Convert.ToInt32(reader["AreaBound"]),
                            MapBound = Convert.ToInt32(reader["MapBound"]),
                            Duration = Convert.ToInt32(reader["Duration"]),
                            BagFamily = Convert.ToInt32(reader["BagFamily"]),
                            DisenchantId = Convert.ToInt32(reader["DisenchantId"]),
                            FoodType = Convert.ToInt32(reader["FoodType"]),
                            MinMoneyLoot = Convert.ToInt32(reader["MinMoneyLoot"]),
                            MaxMoneyLoot = Convert.ToInt32(reader["MaxMoneyLoot"]),
                            ExtraFlags = Convert.ToInt32(reader["ExtraFlags"]),
                            OtherTeamEntry = Convert.ToInt32(reader["OtherTeamEntry"])
                        };
                    }
                }
            }
            return item;
        }
        public static QuestTemplate GetQuestTemplateByID(int id)
        {
            QuestTemplate questTemplate = null;
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM quest_template
                                            WHERE entry = $id
                                        ";
                command.Parameters.AddWithValue("$id", id);

                using (SQLiteDataReader reader = command.ExecuteReader())
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
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Details = reader.GetString(reader.GetOrdinal("Details")),
                            Objectives = reader.GetString(reader.GetOrdinal("Objectives")),
                            OfferRewardText = reader.GetString(reader.GetOrdinal("OfferRewardText")),
                            RequestItemsText = reader.GetString(reader.GetOrdinal("RequestItemsText")),
                            EndText = reader.GetString(reader.GetOrdinal("EndText")),
                            ObjectiveText1 = reader.GetString(reader.GetOrdinal("ObjectiveText1")),
                            ObjectiveText2 = reader.GetString(reader.GetOrdinal("ObjectiveText2")),
                            ObjectiveText3 = reader.GetString(reader.GetOrdinal("ObjectiveText3")),
                            ObjectiveText4 = reader.GetString(reader.GetOrdinal("ObjectiveText4")),
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
        public static List<Creature> GetCreaturesById(int id)
        {
            List<Creature> creatures = new List<Creature>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature
                                            WHERE id = $id
                                        ";
                command.Parameters.AddWithValue("$id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
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
                            LocationX = Convert.ToSingle(reader["position_x"]),
                            LocationY = Convert.ToSingle(reader["position_y"]),
                            LocationZ = Convert.ToSingle(reader["position_z"]),
                            SpawnLocation = new Vector3(Convert.ToSingle(reader["position_x"]), Convert.ToSingle(reader["position_y"]), Convert.ToSingle(reader["position_z"])),
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
        public static List<Creature> GetCreaturesByMapId(int mapId)
        {
            List<Creature> creatures = new List<Creature>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature
                                            WHERE map = $map
                                        ";

                command.Parameters.AddWithValue("$map", mapId.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
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
                            LocationX = Convert.ToSingle(reader["position_x"]),
                            LocationY = Convert.ToSingle(reader["position_y"]),
                            LocationZ = Convert.ToSingle(reader["position_z"]),
                            SpawnLocation = new Vector3(Convert.ToSingle(reader["position_x"]), Convert.ToSingle(reader["position_y"]), Convert.ToSingle(reader["position_z"])),
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
        public static List<Creature> GetCreaturesByLootableItemId(int itemId)
        {
            List<Creature> creatures = new List<Creature>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      crt.*
                                            FROM        creature crt
                                            LEFT JOIN   creature_template       ct  ON  crt.id      = ct.entry
                                            LEFT JOIN   creature_loot_template  clt ON  ct.entry    = clt.entry
                                            WHERE                                       clt.item    = @itemId
                                        ";
                command.Parameters.AddWithValue("@itemId", itemId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
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
                            LocationX = Convert.ToSingle(reader["position_x"]),
                            LocationY = Convert.ToSingle(reader["position_y"]),
                            LocationZ = Convert.ToSingle(reader["position_z"]),
                            SpawnLocation = new Vector3(Convert.ToSingle(reader["position_x"]), Convert.ToSingle(reader["position_y"]), Convert.ToSingle(reader["position_z"])),
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
        public static List<Creature> GetAllVendors()
        {
            List<Creature> creatures = new List<Creature>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      ct.Name name, crt.* 
                                            FROM        creature            crt
                                            LEFT JOIN   creature_template   ct  ON   crt.id = ct.Entry
                                            LEFT JOIN   npc_vendor          npcv ON  crt.id = npcv.Entry
                                            GROUP BY    npcv.Entry
                                        ";

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Creature creature = new Creature
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Id = Convert.ToInt32(reader["id"]),
                            Map = Convert.ToInt16(reader["map"]),
                            SpawnMask = Convert.ToByte(reader["spawnMask"]),
                            ModelId = Convert.ToInt32(reader["modelid"]),
                            EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                            LocationX = Convert.ToSingle(reader["position_x"]),
                            LocationY = Convert.ToSingle(reader["position_y"]),
                            LocationZ = Convert.ToSingle(reader["position_z"]),
                            SpawnLocation = new Vector3(Convert.ToSingle(reader["position_x"]), Convert.ToSingle(reader["position_y"]), Convert.ToSingle(reader["position_z"])),
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
        public static List<CreatureLinking> GetCreatureLinkedByGuid(int guid)
        {
            List<CreatureLinking> creatureLinkings = new List<CreatureLinking>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature_linking
                                            WHERE guid = $guid
                                            OR master_guid = $guid
                                        ";

                command.Parameters.AddWithValue("$guid", guid.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CreatureLinking linking = new CreatureLinking
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            MasterGuid = Convert.ToInt32(reader["master_guid"]),
                            Flag = Convert.ToInt32(reader["flag"])
                        };

                        if (linking.Guid == guid || linking.MasterGuid == guid)
                            creatureLinkings.Add(linking);
                    }
                }
            }
            return creatureLinkings;
        }
        public static CreatureEquipTemplate GetCreatureEquipTemplateById(int id)
        {
            CreatureEquipTemplate creatureEquipTemplate = new CreatureEquipTemplate();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature_equip_template
                                            WHERE entry = $entry
                                        ";

                command.Parameters.AddWithValue("$entry", id.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        creatureEquipTemplate = new CreatureEquipTemplate
                        {
                            Entry = Convert.ToInt32(reader["entry"]),
                            EquipEntry1 = Convert.ToInt32(reader["equipentry1"]),
                            EquipEntry2 = Convert.ToInt32(reader["equipentry2"]),
                            EquipEntry3 = Convert.ToInt32(reader["equipentry3"])
                        };
                    }
                }
            }
            return creatureEquipTemplate;
        }
        public static List<CreatureMovement> GetCreatureMovementById(int id)
        {
            List<CreatureMovement> creatureMovements = new List<CreatureMovement>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature_movement
                                            WHERE Id = $id
                                        ";

                command.Parameters.AddWithValue("id", id.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CreatureMovement movement = new CreatureMovement
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Point = Convert.ToInt32(reader["Point"]),
                            PositionX = Convert.ToSingle(reader["PositionX"]),
                            PositionY = Convert.ToSingle(reader["PositionY"]),
                            PositionZ = Convert.ToSingle(reader["PositionZ"]),
                            Orientation = Convert.ToSingle(reader["Orientation"]),
                            WaitTime = Convert.ToInt32(reader["WaitTime"]),
                            ScriptId = Convert.ToInt32(reader["ScriptId"]),
                            Comment = Convert.ToString(reader["Comment"])
                        };

                        creatureMovements.Add(movement);
                    }
                }
            }
            return creatureMovements;
        }
        public static CreatureTemplate GetCreatureTemplateById(int id)
        {
            CreatureTemplate creatureTemplate = null;
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM creature_template
                                            WHERE Entry = $Entry
                                        ";
                command.Parameters.AddWithValue("$Entry", id.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        creatureTemplate = new CreatureTemplate()
                        {
                            Entry = Convert.ToInt64(reader["Entry"]),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            SubName = reader["SubName"] == null ? string.Empty : reader.GetString(reader.GetOrdinal("SubName")),
                            MinLevel = Convert.ToByte(reader["MinLevel"]),
                            MaxLevel = Convert.ToByte(reader["MaxLevel"]),
                            ModelId1 = Convert.ToInt32(reader["ModelId1"]),
                            ModelId2 = Convert.ToInt32(reader["ModelId2"]),
                            ModelId3 = Convert.ToInt32(reader["ModelId3"]),
                            ModelId4 = Convert.ToInt32(reader["ModelId4"]),
                            Faction = Convert.ToInt16(reader["Faction"]),
                            Scale = Convert.ToSingle(reader["Scale"]),
                            Family = Convert.ToByte(reader["Family"]),
                            CreatureType = Convert.ToByte(reader["CreatureType"]),
                            InhabitType = Convert.ToByte(reader["InhabitType"]),
                            RegenerateStats = Convert.ToByte(reader["RegenerateStats"]),
                            RacialLeader = Convert.ToByte(reader["RacialLeader"]),
                            NpcFlags = Convert.ToInt32(reader["NpcFlags"]),
                            UnitFlags = Convert.ToInt32(reader["UnitFlags"]),
                            DynamicFlags = Convert.ToInt32(reader["DynamicFlags"]),
                            ExtraFlags = Convert.ToInt32(reader["ExtraFlags"]),
                            CreatureTypeFlags = Convert.ToInt32(reader["CreatureTypeFlags"]),
                            SpeedWalk = Convert.ToSingle(reader["SpeedWalk"]),
                            SpeedRun = Convert.ToSingle(reader["SpeedRun"]),
                            Detection = Convert.ToInt32(reader["Detection"]),
                            CallForHelp = Convert.ToInt32(reader["CallForHelp"]),
                            Pursuit = Convert.ToInt32(reader["Pursuit"]),
                            Leash = Convert.ToInt32(reader["Leash"]),
                            Timeout = Convert.ToInt32(reader["Timeout"]),
                            UnitClass = Convert.ToByte(reader["UnitClass"]),
                            Rank = Convert.ToByte(reader["Rank"]),
                            HealthMultiplier = Convert.ToSingle(reader["HealthMultiplier"]),
                            PowerMultiplier = Convert.ToSingle(reader["PowerMultiplier"]),
                            DamageMultiplier = Convert.ToSingle(reader["DamageMultiplier"]),
                            DamageVariance = Convert.ToSingle(reader["DamageVariance"]),
                            ArmorMultiplier = Convert.ToSingle(reader["ArmorMultiplier"]),
                            ExperienceMultiplier = Convert.ToSingle(reader["ExperienceMultiplier"]),
                            MinLevelHealth = Convert.ToInt32(reader["MinLevelHealth"]),
                            MaxLevelHealth = Convert.ToInt32(reader["MaxLevelHealth"]),
                            MinLevelMana = Convert.ToInt32(reader["MinLevelMana"]),
                            MaxLevelMana = Convert.ToInt32(reader["MaxLevelMana"]),
                            MinMeleeDmg = Convert.ToSingle(reader["MinMeleeDmg"]),
                            MaxMeleeDmg = Convert.ToSingle(reader["MaxMeleeDmg"]),
                            MinRangedDmg = Convert.ToSingle(reader["MinRangedDmg"]),
                            MaxRangedDmg = Convert.ToSingle(reader["MaxRangedDmg"]),
                            Armor = Convert.ToInt32(reader["Armor"]),
                            MeleeAttackPower = Convert.ToInt32(reader["MeleeAttackPower"]),
                            RangedAttackPower = Convert.ToInt16(reader["RangedAttackPower"]),
                            MeleeBaseAttackTime = Convert.ToInt32(reader["MeleeBaseAttackTime"]),
                            RangedBaseAttackTime = Convert.ToInt32(reader["RangedBaseAttackTime"]),
                            DamageSchool = Convert.ToByte(reader["DamageSchool"]),
                            MinLootGold = Convert.ToInt32(reader["MinLootGold"]),
                            MaxLootGold = Convert.ToInt32(reader["MaxLootGold"]),
                            LootId = Convert.ToInt32(reader["LootId"]),
                            PickpocketLootId = Convert.ToInt32(reader["PickpocketLootId"]),
                            SkinningLootId = Convert.ToInt32(reader["SkinningLootId"]),
                            KillCredit1 = Convert.ToInt32(reader["KillCredit1"]),
                            KillCredit2 = Convert.ToInt32(reader["KillCredit2"]),
                            MechanicImmuneMask = Convert.ToInt32(reader["MechanicImmuneMask"]),
                            SchoolImmuneMask = Convert.ToInt32(reader["SchoolImmuneMask"]),
                            ResistanceHoly = Convert.ToByte(reader["ResistanceHoly"]),
                            ResistanceFire = Convert.ToByte(reader["ResistanceFire"]),
                            ResistanceNature = Convert.ToByte(reader["ResistanceNature"]),
                            ResistanceFrost = Convert.ToByte(reader["ResistanceFrost"]),
                            ResistanceShadow = Convert.ToByte(reader["ResistanceShadow"]),
                            ResistanceArcane = Convert.ToByte(reader["ResistanceArcane"]),
                            PetSpellDataId = Convert.ToInt32(reader["PetSpellDataId"]),
                            MovementType = Convert.ToByte(reader["MovementType"]),
                            TrainerType = Convert.ToByte(reader["TrainerType"]),
                            TrainerSpell = Convert.ToInt32(reader["TrainerSpell"]),
                            TrainerClass = Convert.ToByte(reader["TrainerClass"]),
                            TrainerRace = Convert.ToByte(reader["TrainerRace"]),
                            TrainerTemplateId = Convert.ToInt32(reader["TrainerTemplateId"]),
                            VendorTemplateId = Convert.ToInt32(reader["VendorTemplateId"]),
                            GossipMenuId = Convert.ToInt32(reader["GossipMenuId"]),
                            InteractionPauseTimer = Convert.ToInt32(reader["InteractionPauseTimer"]),
                            VisibilityDistanceType = Convert.ToByte(reader["visibilityDistanceType"]),
                            CorpseDecay = Convert.ToInt32(reader["CorpseDecay"]),
                            SpellList = Convert.ToInt32(reader["SpellList"]),
                            EquipmentTemplateId = Convert.ToInt32(reader["EquipmentTemplateId"]),
                            Civilian = Convert.ToByte(reader["Civilian"]),
                            AIName = reader.GetString(reader.GetOrdinal("AIName")),
                            ScriptName = reader.GetString(reader.GetOrdinal("ScriptName")),
                        };
                    }
                }
            }
            return creatureTemplate;
        }
        public static List<CreatureTemplate> GetAllClassTrainers()
        {
            List<CreatureTemplate> creatureTemplates = new List<CreatureTemplate>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      ct.*
                                            FROM        creature_template   ct
                                            WHERE       ct.TrainerClass     != 0
                                            AND         ct.TrainerType      == 0
                                        ";

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CreatureTemplate creatureTemplate = new CreatureTemplate()
                        {
                            Entry = Convert.ToInt32(reader["Entry"]),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            SubName = reader["SubName"] == null ? string.Empty : reader.GetString(reader.GetOrdinal("SubName")),
                            MinLevel = Convert.ToByte(reader["MinLevel"]),
                            MaxLevel = Convert.ToByte(reader["MaxLevel"]),
                            ModelId1 = Convert.ToInt32(reader["ModelId1"]),
                            ModelId2 = Convert.ToInt32(reader["ModelId2"]),
                            ModelId3 = Convert.ToInt32(reader["ModelId3"]),
                            ModelId4 = Convert.ToInt32(reader["ModelId4"]),
                            Faction = Convert.ToInt16(reader["Faction"]),
                            Scale = Convert.ToSingle(reader["Scale"]),
                            Family = Convert.ToByte(reader["Family"]),
                            CreatureType = Convert.ToByte(reader["CreatureType"]),
                            InhabitType = Convert.ToByte(reader["InhabitType"]),
                            RegenerateStats = Convert.ToByte(reader["RegenerateStats"]),
                            RacialLeader = Convert.ToByte(reader["RacialLeader"]),
                            NpcFlags = Convert.ToInt32(reader["NpcFlags"]),
                            UnitFlags = Convert.ToInt32(reader["UnitFlags"]),
                            DynamicFlags = Convert.ToInt32(reader["DynamicFlags"]),
                            ExtraFlags = Convert.ToInt32(reader["ExtraFlags"]),
                            CreatureTypeFlags = Convert.ToInt32(reader["CreatureTypeFlags"]),
                            SpeedWalk = Convert.ToSingle(reader["SpeedWalk"]),
                            SpeedRun = Convert.ToSingle(reader["SpeedRun"]),
                            Detection = Convert.ToInt32(reader["Detection"]),
                            CallForHelp = Convert.ToInt32(reader["CallForHelp"]),
                            Pursuit = Convert.ToInt32(reader["Pursuit"]),
                            Leash = Convert.ToInt32(reader["Leash"]),
                            Timeout = Convert.ToInt32(reader["Timeout"]),
                            UnitClass = Convert.ToByte(reader["UnitClass"]),
                            Rank = Convert.ToByte(reader["Rank"]),
                            HealthMultiplier = Convert.ToSingle(reader["HealthMultiplier"]),
                            PowerMultiplier = Convert.ToSingle(reader["PowerMultiplier"]),
                            DamageMultiplier = Convert.ToSingle(reader["DamageMultiplier"]),
                            DamageVariance = Convert.ToSingle(reader["DamageVariance"]),
                            ArmorMultiplier = Convert.ToSingle(reader["ArmorMultiplier"]),
                            ExperienceMultiplier = Convert.ToSingle(reader["ExperienceMultiplier"]),
                            MinLevelHealth = Convert.ToInt32(reader["MinLevelHealth"]),
                            MaxLevelHealth = Convert.ToInt32(reader["MaxLevelHealth"]),
                            MinLevelMana = Convert.ToInt32(reader["MinLevelMana"]),
                            MaxLevelMana = Convert.ToInt32(reader["MaxLevelMana"]),
                            MinMeleeDmg = Convert.ToSingle(reader["MinMeleeDmg"]),
                            MaxMeleeDmg = Convert.ToSingle(reader["MaxMeleeDmg"]),
                            MinRangedDmg = Convert.ToSingle(reader["MinRangedDmg"]),
                            MaxRangedDmg = Convert.ToSingle(reader["MaxRangedDmg"]),
                            Armor = Convert.ToInt32(reader["Armor"]),
                            MeleeAttackPower = Convert.ToInt32(reader["MeleeAttackPower"]),
                            RangedAttackPower = Convert.ToInt16(reader["RangedAttackPower"]),
                            MeleeBaseAttackTime = Convert.ToInt32(reader["MeleeBaseAttackTime"]),
                            RangedBaseAttackTime = Convert.ToInt32(reader["RangedBaseAttackTime"]),
                            DamageSchool = Convert.ToByte(reader["DamageSchool"]),
                            MinLootGold = Convert.ToInt32(reader["MinLootGold"]),
                            MaxLootGold = Convert.ToInt32(reader["MaxLootGold"]),
                            LootId = Convert.ToInt32(reader["LootId"]),
                            PickpocketLootId = Convert.ToInt32(reader["PickpocketLootId"]),
                            SkinningLootId = Convert.ToInt32(reader["SkinningLootId"]),
                            KillCredit1 = Convert.ToInt32(reader["KillCredit1"]),
                            KillCredit2 = Convert.ToInt32(reader["KillCredit2"]),
                            MechanicImmuneMask = Convert.ToInt32(reader["MechanicImmuneMask"]),
                            SchoolImmuneMask = Convert.ToInt32(reader["SchoolImmuneMask"]),
                            ResistanceHoly = Convert.ToInt32(reader["ResistanceHoly"]),
                            ResistanceFire = Convert.ToInt32(reader["ResistanceFire"]),
                            ResistanceNature = Convert.ToInt32(reader["ResistanceNature"]),
                            ResistanceFrost = Convert.ToInt32(reader["ResistanceFrost"]),
                            ResistanceShadow = Convert.ToInt32(reader["ResistanceShadow"]),
                            ResistanceArcane = Convert.ToInt32(reader["ResistanceArcane"]),
                            PetSpellDataId = Convert.ToInt32(reader["PetSpellDataId"]),
                            MovementType = Convert.ToByte(reader["MovementType"]),
                            TrainerType = Convert.ToByte(reader["TrainerType"]),
                            TrainerSpell = Convert.ToInt32(reader["TrainerSpell"]),
                            TrainerClass = Convert.ToByte(reader["TrainerClass"]),
                            TrainerRace = Convert.ToByte(reader["TrainerRace"]),
                            TrainerTemplateId = Convert.ToInt32(reader["TrainerTemplateId"]),
                            VendorTemplateId = Convert.ToInt32(reader["VendorTemplateId"]),
                            GossipMenuId = Convert.ToInt32(reader["GossipMenuId"]),
                            InteractionPauseTimer = Convert.ToInt32(reader["InteractionPauseTimer"]),
                            VisibilityDistanceType = Convert.ToByte(reader["visibilityDistanceType"]),
                            CorpseDecay = Convert.ToInt32(reader["CorpseDecay"]),
                            SpellList = Convert.ToInt32(reader["SpellList"]),
                            EquipmentTemplateId = Convert.ToInt32(reader["EquipmentTemplateId"]),
                            Civilian = Convert.ToByte(reader["Civilian"]),
                            AIName = reader.GetString(reader.GetOrdinal("AIName")),
                            ScriptName = reader.GetString(reader.GetOrdinal("ScriptName")),
                        };

                        creatureTemplates.Add(creatureTemplate);
                    }
                }
            }
            return creatureTemplates;
        }
        public static List<int> GetQuestRelatedNPCsByQuestId(int id)
        {
            List<int> questNpcs = new List<int>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT A.*, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
                                            FROM creature_involvedrelation A
                                            WHERE A.quest = @id
                                            ORDER BY rn
                                        ";
                command.Parameters.AddWithValue("@id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questNpcs.Add(Convert.ToInt16(reader["id"]));
                    }
                }
            }
            return questNpcs;
        }
        public static List<GameObject> GetGameObjectsById(int id)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT *
                                            FROM gameobject
                                            WHERE id = @id
                                        ";
                command.Parameters.AddWithValue("@id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GameObject gameObject = new GameObject()
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            Id = Convert.ToInt32(reader["id"]),
                            Map = Convert.ToInt16(reader["map"]),
                            SpawnMask = Convert.ToByte(reader["spawnMask"]),
                            LocationX = Convert.ToSingle(reader["position_x"]),
                            LocationY = Convert.ToSingle(reader["position_y"]),
                            LocationZ = Convert.ToSingle(reader["position_z"]),
                            Orientation = Convert.ToSingle(reader["orientation"]),
                            Rotation0 = Convert.ToSingle(reader["rotation0"]),
                            Rotation1 = Convert.ToSingle(reader["rotation1"]),
                            Rotation2 = Convert.ToSingle(reader["rotation2"]),
                            Rotation3 = Convert.ToSingle(reader["rotation3"]),
                            SpawnTimeSecsMin = Convert.ToInt32(reader["spawntimesecsmin"]),
                            SpawnTimeSecsMax = Convert.ToInt32(reader["spawntimesecsmax"]),
                            AnimProgress = Convert.ToByte(reader["animprogress"]),
                            State = Convert.ToByte(reader["state"]),
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
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT DISTINCT gob.*
                                            FROM        gameobject                  gob
                                            LEFT JOIN   gameobject_template         got     ON  gob.id      = got.entry
                                            LEFT JOIN   gameobject_loot_template    golt    ON  golt.entry  = got.data1
                                            WHERE                                               golt.item   = @itemId
                                        ";
                command.Parameters.AddWithValue("@itemId", itemId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GameObject gameObject = new GameObject()
                        {
                            Guid = Convert.ToInt32(reader["guid"]),
                            Id = Convert.ToInt32(reader["id"]),
                            Map = Convert.ToInt16(reader["map"]),
                            SpawnMask = Convert.ToByte(reader["spawnMask"]),
                            LocationX = Convert.ToSingle(reader["position_x"]),
                            LocationY = Convert.ToSingle(reader["position_y"]),
                            LocationZ = Convert.ToSingle(reader["position_z"]),
                            Orientation = Convert.ToSingle(reader["orientation"]),
                            Rotation0 = Convert.ToSingle(reader["rotation0"]),
                            Rotation1 = Convert.ToSingle(reader["rotation1"]),
                            Rotation2 = Convert.ToSingle(reader["rotation2"]),
                            Rotation3 = Convert.ToSingle(reader["rotation3"]),
                            SpawnTimeSecsMin = Convert.ToInt32(reader["spawntimesecsmin"]),
                            SpawnTimeSecsMax = Convert.ToInt32(reader["spawntimesecsmax"]),
                            AnimProgress = Convert.ToByte(reader["animprogress"]),
                            State = Convert.ToByte(reader["state"]),
                        };

                        gameObjects.Add(gameObject);
                    }
                }
            }
            return gameObjects;
        }
        public static List<NpcVendorEntry> GetAllItemsSoldByVendorByEntry(int entry)
        {
            List<NpcVendorEntry> npcVendorEntries = new List<NpcVendorEntry>();
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = @"
                                            SELECT      npcv.*
                                            FROM        npc_vendor   npcv
                                            WHERE       npcv.Entry = @entry
                                        ";

                command.Parameters.AddWithValue("@entry", entry.ToString());
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NpcVendorEntry npcVendorEntry = new NpcVendorEntry()
                        {
                            Entry = Convert.ToInt32(reader["entry"]),
                            Item = Convert.ToInt32(reader["item"]),
                            MaxCount = Convert.ToInt32(reader["maxcount"]),
                            IncrTime = Convert.ToInt32(reader["incrtime"]),
                            Slot = Convert.ToInt32(reader["slot"]),
                            ConditionId = Convert.ToInt32(reader["condition_id"]),
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
