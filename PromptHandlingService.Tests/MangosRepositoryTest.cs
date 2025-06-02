using Database;
using DecisionEngineService.Repository;
using Google.Protobuf.WellKnownTypes;

namespace PromptHandlingService.Tests
{
    public class MangosRepositoryTest
    {

        public static void TestGetAreaTriggerBgEntrances()
        {
            int count = MangosRepository.GetRowCountForTable("areatrigger_bg_entrance");
            List<AreaTriggerBgEntrance> areaTriggerBgEntrances = MangosRepository.GetAreaTriggerBgEntrances();

            Assert.Equal(count, areaTriggerBgEntrances.Count);

            Assert.Equal(2412u, areaTriggerBgEntrances[0].Id);
            Assert.Equal("Alterac Valley (Alliance)", areaTriggerBgEntrances[0].Name);
            Assert.Equal(469u, areaTriggerBgEntrances[0].Team);
            Assert.Equal(1u, areaTriggerBgEntrances[0].BgTemplate);
            Assert.Equal(0u, areaTriggerBgEntrances[0].ExitMap);
            Assert.Equal(102.150f, areaTriggerBgEntrances[0].ExitPositionX);
            Assert.Equal(-188.887f, areaTriggerBgEntrances[0].ExitPositionY);
            Assert.Equal(126.932f, areaTriggerBgEntrances[0].ExitPositionZ);
            Assert.Equal(4.813f, areaTriggerBgEntrances[0].ExitOrientation);
        }


        public static void TestGetAreaTriggerInvolvedRelations()
        {
            int count = MangosRepository.GetRowCountForTable("areatrigger_involvedrelation");
            List<AreaTriggerInvolvedRelation> areaTriggerInvolvedRelations = MangosRepository.GetAreaTriggerInvolvedRelations();

            Assert.Equal(count, areaTriggerInvolvedRelations.Count);

            Assert.Equal(2946u, areaTriggerInvolvedRelations[0].Id);
            Assert.Equal(6421u, areaTriggerInvolvedRelations[0].Quest);
        }


        public static void TestGetAreaTriggerTaverns()
        {
            int count = MangosRepository.GetRowCountForTable("areatrigger_tavern");
            List<AreaTriggerTavern> areaTriggerTaverns = MangosRepository.GetAreaTriggerTaverns();

            Assert.Equal(count, areaTriggerTaverns.Count);

            Assert.Equal(71u, areaTriggerTaverns[0].Id);
            Assert.Equal("Westfall - Sentinel Hill Inn", areaTriggerTaverns[0].Name);
        }


        public static void TestGetAreaTriggerTeleports()
        {
            int count = MangosRepository.GetRowCountForTable("areatrigger_teleport");
            List<AreaTriggerTeleport> areaTriggerTeleports = MangosRepository.GetAreaTriggerTeleports();

            Assert.Equal(count, areaTriggerTeleports.Count);

            Assert.Equal(45u, areaTriggerTeleports[0].Id);
            Assert.Equal(0u, areaTriggerTeleports[0].Patch);
            Assert.Equal("Scarlet Monastery - Graveyard (Entrance)", areaTriggerTeleports[0].Name);
            Assert.Equal(20u, areaTriggerTeleports[0].RequiredLevel);
            Assert.Equal(0u, areaTriggerTeleports[0].RequiredItem);
            Assert.Equal(0u, areaTriggerTeleports[0].RequiredItem2);
            Assert.Equal(0u, areaTriggerTeleports[0].RequiredQuestDone);
            Assert.Equal(0, areaTriggerTeleports[0].RequiredEvent);
            Assert.Equal(0u, areaTriggerTeleports[0].RequiredPvpRank);
            Assert.Equal(0u, areaTriggerTeleports[0].RequiredTeam);
            Assert.Equal("", areaTriggerTeleports[0].RequiredFailedText);
            Assert.Equal(189u, areaTriggerTeleports[0].TargetMap);
            Assert.Equal(1688.99f, areaTriggerTeleports[0].TargetPositionX);
            Assert.Equal(1053.47998f, areaTriggerTeleports[0].TargetPositionY);
            Assert.Equal(18.6775f, areaTriggerTeleports[0].TargetPositionZ);
            Assert.Equal(0.00117f, areaTriggerTeleports[0].TargetOrientation);
        }


        public static void TestGetAreaTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("area_template");
            List<AreaTemplate> areaTemplates = MangosRepository.GetAreaTemplates();

            Assert.Equal(count, areaTemplates.Count);

            Assert.Equal(1u, areaTemplates[0].Entry);
            Assert.Equal(0u, areaTemplates[0].MapId);
            Assert.Equal(0u, areaTemplates[0].ZoneId);
            Assert.Equal(119u, areaTemplates[0].ExploreFlag);
            Assert.Equal(65u, areaTemplates[0].Flags);
            Assert.Equal(0, areaTemplates[0].AreaLevel);
            Assert.Equal("Dun Morogh", areaTemplates[0].Name);
            Assert.Equal(2u, areaTemplates[0].Team);
            Assert.Equal(0u, areaTemplates[0].LiquidTypeId);
        }


        public static void TestGetAuctionHouseBots()
        {
            int count = MangosRepository.GetRowCountForTable("auctionhousebot");
            List<AuctionHouseBot> autoHouseBot = MangosRepository.GetAuctionHouseBots();

            Assert.Equal(count, autoHouseBot.Count);

            Assert.Equal(4687, autoHouseBot[0].Item);
            Assert.Equal(1, autoHouseBot[0].Stack);
            Assert.Equal(1500, autoHouseBot[0].Bid);
            Assert.Equal(5000, autoHouseBot[0].Buyout);
        }


        public static void TestGetAutoBroadcasts()
        {
            int count = MangosRepository.GetRowCountForTable("autobroadcast");
            List<AutoBroadcast> autoBroadcasts = MangosRepository.GetAutoBroadcasts();

            Assert.Equal(count, autoBroadcasts.Count);
        }


        public static void TestGetBattlegroundEvents()
        {
            int count = MangosRepository.GetRowCountForTable("battleground_events");
            List<BattlegroundEvent> battlegroundEvent = MangosRepository.GetBattlegroundEvents();

            Assert.Equal(count, battlegroundEvent.Count);

            Assert.Equal(489, battlegroundEvent[0].Map);
            Assert.Equal(0u, battlegroundEvent[0].Event1);
            Assert.Equal(0u, battlegroundEvent[0].Event2);
            Assert.Equal("Alliance Flag", battlegroundEvent[0].Description);
        }


        public static void TestGetBattlegroundTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("battleground_template");
            List<BattlegroundTemplate> battlegroundTemplates = MangosRepository.GetBattlegroundTemplates();

            Assert.Equal(count, battlegroundTemplates.Count);

            Assert.Equal(1u, battlegroundTemplates[0].Id);
            Assert.Equal(0u, battlegroundTemplates[0].Patch);
            Assert.Equal(20u, battlegroundTemplates[0].MinPlayersPerTeam);
            Assert.Equal(40u, battlegroundTemplates[0].MaxPlayersPerTeam);
            Assert.Equal(61u, battlegroundTemplates[0].MinLvl);
            Assert.Equal(61u, battlegroundTemplates[0].MaxLvl);
            Assert.Equal(0u, battlegroundTemplates[0].AllianceWinSpell);
            Assert.Equal(0u, battlegroundTemplates[0].AllianceLoseSpell);
            Assert.Equal(0u, battlegroundTemplates[0].HordeWinSpell);
            Assert.Equal(0u, battlegroundTemplates[0].HordeLoseSpell);
            Assert.Equal(611u, battlegroundTemplates[0].AllianceStartLoc);
            Assert.Equal(2.72532f, battlegroundTemplates[0].AllianceStartO);
            Assert.Equal(610u, battlegroundTemplates[0].HordeStartLoc);
            Assert.Equal(2.27452f, (double)battlegroundTemplates[0].HordeStartO);
        }


        public static void TestGetBattlemasterEntries()
        {
            int count = MangosRepository.GetRowCountForTable("battlemaster_entry");
            List<BattlemasterEntry> battlemasterEntries = MangosRepository.GetBattlemasterEntries();

            Assert.Equal(count, battlemasterEntries.Count);

            Assert.Equal(347u, battlemasterEntries[0].Entry);
            Assert.Equal(1u, battlemasterEntries[0].BgTemplate);
        }


        public static void TestGetBroadcastTexts()
        {
            int count = MangosRepository.GetRowCountForTable("broadcast_text");
            List<BroadcastText> broadcastTexts = MangosRepository.GetBroadcastTexts();

            Assert.Equal(count, broadcastTexts.Count);

            Assert.Equal(1u, broadcastTexts[0].Id);
            Assert.Equal("Help help!  I'm being repressed!", broadcastTexts[0].MaleText);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleText);
            Assert.Equal(0u, broadcastTexts[0].Sound);
            Assert.Equal(0u, broadcastTexts[0].Type);
            Assert.Equal(0u, broadcastTexts[0].Language);
            Assert.Equal(0u, broadcastTexts[0].EmoteId0);
            Assert.Equal(0u, broadcastTexts[0].EmoteId1);
            Assert.Equal(0u, broadcastTexts[0].EmoteId2);
            Assert.Equal(0u, broadcastTexts[0].EmoteDelay0);
            Assert.Equal(0u, broadcastTexts[0].EmoteDelay1);
            Assert.Equal(0u, broadcastTexts[0].EmoteDelay2);
        }


        public static void TestGetCinematicWaypoints()
        {
            int count = MangosRepository.GetRowCountForTable("cinematic_waypoints");
            List<CinematicWaypoint> cinematicWaypoints = MangosRepository.GetCinematicWaypoints();

            Assert.Equal(count, cinematicWaypoints.Count);

            Assert.Equal(81, cinematicWaypoints[0].Cinematic);
            Assert.Equal(0, cinematicWaypoints[0].Timer);
            Assert.Equal(-8960u, cinematicWaypoints[0].Posx);
            Assert.Equal(517u, cinematicWaypoints[0].Posy);
            Assert.Equal(86u, cinematicWaypoints[0].Posz);
            Assert.Equal("Humains start", cinematicWaypoints[0].Comment);
        }

        public static void TestGetCommands()
        {
            int count = MangosRepository.GetRowCountForTable("command");
            List<Command> commands = MangosRepository.GetCommands();

            Assert.Equal(count, commands.Count);

            Assert.Equal("wareffortget", commands[0].Name);
            Assert.Equal(6u, commands[0].Security);
            Assert.Equal("Syntax: .wareffortget \"[ResourceName]\"", commands[0].Help);
        }

        public static void TestGetConditions()
        {
            int count = MangosRepository.GetRowCountForTable("conditions");
            List<Condition> conditions = MangosRepository.GetConditions();

            Assert.Equal(count, conditions.Count);

            Assert.Equal(1u, conditions[0].ConditionEntry);
            Assert.Equal(2, conditions[0].Type);
            Assert.Equal(11511u, conditions[0].Value1);
            Assert.Equal(1u, conditions[0].Value2);
            Assert.Equal(0u, conditions[0].Flags);
        }

        public static void TestGetCreatures()
        {
            int count = MangosRepository.GetRowCountForTable("creature");
            List<Creature> creatures = MangosRepository.GetCreatures();

            Assert.Equal(count, creatures.Count);

            Assert.Equal(6732u, creatures[0].Guid);
            Assert.Equal(721u, creatures[0].Id);
            Assert.Equal(0u, creatures[0].Map);
            Assert.Equal(0u, creatures[0].Modelid);
            Assert.Equal(0u, creatures[0].EquipmentId);
            Assert.Equal(-9638.01953f, creatures[0].PositionX);
            Assert.Equal(-3164.06f, creatures[0].PositionY);
            Assert.Equal(49.1948f, creatures[0].PositionZ);
            Assert.Equal(3.11049f, creatures[0].Orientation);
            Assert.Equal(300u, creatures[0].Spawntimesecsmin);
            Assert.Equal(300u, creatures[0].Spawntimesecsmax);
            Assert.Equal(5u, creatures[0].Spawndist);
            Assert.Equal(0u, creatures[0].Currentwaypoint);
            Assert.Equal(1u, creatures[0].Curhealth);
            Assert.Equal(0u, creatures[0].Curmana);
            Assert.Equal(0u, creatures[0].DeathState);
            Assert.Equal(1u, creatures[0].MovementType);
            Assert.Equal(0u, creatures[0].SpawnFlags);
            Assert.Equal(0u, creatures[0].Visibilitymod);
            Assert.Equal(0u, creatures[0].PatchMin);
            Assert.Equal(10u, creatures[0].PatchMax);
        }

        public static void TestGetCreatureAddons()
        {
            int count = MangosRepository.GetRowCountForTable("creature_addon");
            List<CreatureAddon> creatureAddons = MangosRepository.GetCreatureAddons();

            Assert.Equal(count, creatureAddons.Count);

            Assert.Equal(46u, creatureAddons[0].Guid);
            Assert.Equal(0u, creatureAddons[0].Patch);
            Assert.Equal(0u, creatureAddons[0].Mount);
            Assert.Equal(0u, creatureAddons[0].Bytes1);
            Assert.Equal(1u, creatureAddons[0].B20Sheath);
            Assert.Equal(16u, creatureAddons[0].B21Flags);
            Assert.Equal(0u, creatureAddons[0].Emote);
            Assert.Equal(0u, creatureAddons[0].Moveflags);
            Assert.Equal("1244", creatureAddons[0].Auras);
        }

        public static void TestGetCreatureAIEvents()
        {
            int count = MangosRepository.GetRowCountForTable("creature_ai_events");
            List<CreatureAIEvent> creatureAIEvents = MangosRepository.GetCreatureAIEvents();

            Assert.Equal(count, creatureAIEvents.Count);

            Assert.Equal(224001u, creatureAIEvents[0].Id);
            Assert.Equal(2240u, creatureAIEvents[0].CreatureId);
            Assert.Equal(0u, creatureAIEvents[0].ConditionId);
            Assert.Equal(2u, creatureAIEvents[0].EventType);
            Assert.Equal(0u, creatureAIEvents[0].EventInversePhaseMask);
            Assert.Equal(100u, creatureAIEvents[0].EventChance);
            Assert.Equal(0u, creatureAIEvents[0].EventFlags);
            Assert.Equal(15, creatureAIEvents[0].EventParam1);
            Assert.Equal(0, creatureAIEvents[0].EventParam2);
            Assert.Equal(0, creatureAIEvents[0].EventParam3);
            Assert.Equal(0, creatureAIEvents[0].EventParam4);
            Assert.Equal(224001u, creatureAIEvents[0].Action1Script);
            Assert.Equal(0u, creatureAIEvents[0].Action2Script);
            Assert.Equal(0u, creatureAIEvents[0].Action3Script);
            Assert.Equal("Syndicate Footpad - Flee at 15% HP", creatureAIEvents[0].Comment);
        }

        public static void TestGetCreatureBattlegrounds()
        {
            int count = MangosRepository.GetRowCountForTable("creature_battleground");
            List<CreatureBattleground> creatureBattlegrounds = MangosRepository.GetCreatureBattlegrounds();

            Assert.Equal(count, creatureBattlegrounds.Count);

            Assert.Equal(150000u, creatureBattlegrounds[0].Guid);
            Assert.Equal(2u, creatureBattlegrounds[0].Event1);
            Assert.Equal(0u, creatureBattlegrounds[0].Event2);
        }

        public static void TestGetCreatureEquipTemplate()
        {
            int count = MangosRepository.GetRowCountForTable("creature_equip_template");
            List<CreatureEquipTemplate> creatureEquipTemplates = MangosRepository.GetCreatureEquipTemplate();

            Assert.Equal(count, creatureEquipTemplates.Count);

            Assert.Equal(104291u, creatureEquipTemplates[0].Entry);
            Assert.Equal(0u, creatureEquipTemplates[0].Patch);
            Assert.Equal(12583u, creatureEquipTemplates[0].Equipentry1);
            Assert.Equal(0u, creatureEquipTemplates[0].Equipentry2);
            Assert.Equal(0u, creatureEquipTemplates[0].Equipentry3);
        }

        public static void TestGetCreatureEquipTemplateRaws()
        {
            int count = MangosRepository.GetRowCountForTable("creature_equip_template_raw");
            List<CreatureEquipTemplateRaw> creatureEquipTemplateRaws = MangosRepository.GetCreatureEquipTemplateRaws();

            Assert.Equal(count, creatureEquipTemplateRaws.Count);

            Assert.Equal(32u, creatureEquipTemplateRaws[0].Entry);
            Assert.Equal(0u, creatureEquipTemplateRaws[0].Patch);
            Assert.Equal(0u, creatureEquipTemplateRaws[0].Equipmodel1);
            Assert.Equal(0u, creatureEquipTemplateRaws[0].Equipmodel2);
            Assert.Equal(31210u, creatureEquipTemplateRaws[0].Equipmodel3);
            Assert.Equal(0u, creatureEquipTemplateRaws[0].Equipinfo1);
            Assert.Equal(0u, creatureEquipTemplateRaws[0].Equipinfo2);
            Assert.Equal(33489666u, creatureEquipTemplateRaws[0].Equipinfo3);
            Assert.Equal(0, creatureEquipTemplateRaws[0].Equipslot1);
            Assert.Equal(0, creatureEquipTemplateRaws[0].Equipslot2);
            Assert.Equal(26, creatureEquipTemplateRaws[0].Equipslot3);
        }

        public static void TestGetCreatureAIScripts()
        {
            int count = MangosRepository.GetRowCountForTable("creature_ai_scripts");
            List<CreatureAIScript> creatureAIScripts = MangosRepository.GetCreatureAIScripts();

            Assert.Equal(count, creatureAIScripts.Count);

            Assert.Equal(601u, creatureAIScripts[0].Id);
            Assert.Equal(0u, creatureAIScripts[0].Delay);
            Assert.Equal(0u, creatureAIScripts[0].Command);
            Assert.Equal(0u, creatureAIScripts[0].Datalong);
            Assert.Equal(0u, creatureAIScripts[0].Datalong2);
            Assert.Equal(0u, creatureAIScripts[0].Datalong3);
            Assert.Equal(0u, creatureAIScripts[0].Datalong4);
            Assert.Equal(0u, creatureAIScripts[0].TargetParam1);
            Assert.Equal(0u, creatureAIScripts[0].TargetParam2);
            Assert.Equal(0u, creatureAIScripts[0].TargetType);
            Assert.Equal(0u, creatureAIScripts[0].DataFlags);
            Assert.Equal(1868, creatureAIScripts[0].Dataint);
            Assert.Equal(1864, creatureAIScripts[0].Dataint2);
            Assert.Equal(0, creatureAIScripts[0].Dataint3);
            Assert.Equal(0u, creatureAIScripts[0].X);
            Assert.Equal(0u, creatureAIScripts[0].Y);
            Assert.Equal(0u, creatureAIScripts[0].Z);
            Assert.Equal(0u, creatureAIScripts[0].O);
            Assert.Equal(0u, creatureAIScripts[0].ConditionId);
            Assert.Equal("Kobold Vermin - Say Text", creatureAIScripts[0].Comments);
        }

        public static void TestGetCreatureInvolvedRelations()
        {
            int count = MangosRepository.GetRowCountForTable("creature_involvedrelation");
            List<CreatureInvolvedRelation> creatureInvolvedRelations = MangosRepository.GetCreatureInvolvedRelations();

            Assert.Equal(count, creatureInvolvedRelations.Count);

            Assert.Equal(196u, creatureInvolvedRelations[0].Id);
            Assert.Equal(0u, creatureInvolvedRelations[0].Patch);
            Assert.Equal(33u, creatureInvolvedRelations[0].Quest);
        }

        public static void TestGetCreatureLinkings()
        {
            int count = MangosRepository.GetRowCountForTable("creature_linking");
            List<CreatureLinking> creatureLinkings = MangosRepository.GetCreatureLinkings();

            Assert.Equal(count, creatureLinkings.Count);

            Assert.Equal(88453u, creatureLinkings[0].Guid);
            Assert.Equal(88460u, creatureLinkings[0].MasterGuid);
            Assert.Equal(3073u, creatureLinkings[0].Flag);
        }

        public static void TestGetCreatureLinkingTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("creature_linking_template");
            List<CreatureLinkingTemplate> creatureLinkingTemplates = MangosRepository.GetCreatureLinkingTemplates();

            Assert.Equal(count, creatureLinkingTemplates.Count);

            Assert.Equal(16056u, creatureLinkingTemplates[0].Entry);
            Assert.Equal(16011u, creatureLinkingTemplates[0].MasterEntry);
            Assert.Equal(533u, creatureLinkingTemplates[0].Map);
            Assert.Equal(3072u, creatureLinkingTemplates[0].Flag);
            Assert.Equal(0u, creatureLinkingTemplates[0].SearchRange);
        }


        public static void TestGetCreatureLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("creature_loot_template");
            List<CreatureLootTemplate> creatureLootTemplates = MangosRepository.GetCreatureLootTemplates();

            Assert.Equal(count, creatureLootTemplates.Count);

            Assert.Equal(3u, creatureLootTemplates[0].Entry);
            Assert.Equal(30016u, creatureLootTemplates[0].Item);
            Assert.Equal(0.01f, creatureLootTemplates[0].ChanceOrQuestChance);
            Assert.Equal(0u, creatureLootTemplates[0].Groupid);
            Assert.Equal(-30016u, creatureLootTemplates[0].MincountOrRef);
            Assert.Equal(1u, creatureLootTemplates[0].Maxcount);
            Assert.Equal(0u, creatureLootTemplates[0].ConditionId);
            Assert.Equal(0u, creatureLootTemplates[0].PatchMin);
            Assert.Equal(10u, creatureLootTemplates[0].PatchMax);
        }


        public static void TestGetCreatureModelInfos()
        {
            int count = MangosRepository.GetRowCountForTable("creature_model_info");
            List<CreatureModelInfo> creatureModelInfos = MangosRepository.GetCreatureModelInfos();

            Assert.Equal(count, creatureModelInfos.Count);

            Assert.Equal(4u, creatureModelInfos[0].Modelid);
            Assert.Equal(2.0f, creatureModelInfos[0].BoundingRadius);
            Assert.Equal(3.0f, creatureModelInfos[0].CombatReach);
            Assert.Equal(2u, creatureModelInfos[0].Gender);
            Assert.Equal(0u, creatureModelInfos[0].ModelidOtherGender);
            Assert.Equal(0u, creatureModelInfos[0].ModelidOtherTeam);
        }


        public static void TestGetCreatureMovements()
        {
            int count = MangosRepository.GetRowCountForTable("creature_movement");
            // Act: Retrieve the list of creature movements
            List<CreatureMovement> creatureMovements = MangosRepository.GetCreatureMovements();

            Assert.Equal(count, creatureMovements.Count);

            // Assert: Check values of the first creature movement
            Assert.Equal(51u, creatureMovements[0].Id);
            Assert.Equal(7u, creatureMovements[0].Point);
            Assert.Equal(-4947.67f, creatureMovements[0].PositionX);
            Assert.Equal(-1204.41f, creatureMovements[0].PositionY);
            Assert.Equal(501.658f, creatureMovements[0].PositionZ);
            Assert.Equal(0u, creatureMovements[0].Waittime);
            Assert.Equal(0u, creatureMovements[0].ScriptId);
            Assert.Equal(0, creatureMovements[0].Textid1);
            Assert.Equal(0, creatureMovements[0].Textid2);
            Assert.Equal(0, creatureMovements[0].Textid3);
            Assert.Equal(0, creatureMovements[0].Textid4);
            Assert.Equal(0, creatureMovements[0].Textid5);
            Assert.Equal(0u, creatureMovements[0].Emote);
            Assert.Equal(0u, creatureMovements[0].Spell);
            Assert.Equal(0.00f, creatureMovements[0].Orientation); // This represents 90 degrees in radians
            Assert.Equal(0u, creatureMovements[0].Model1);
            Assert.Equal(0u, creatureMovements[0].Model2);
        }


        public static void TestGetCreatureMovementScripts()
        {
            int count = MangosRepository.GetRowCountForTable("creature_movement_scripts");
            List<CreatureMovementScript> creatureMovementScripts = MangosRepository.GetCreatureMovementScripts();

            Assert.Equal(count, creatureMovementScripts.Count);

            Assert.Equal(1u, creatureMovementScripts[0].Id);
            Assert.Equal(0u, creatureMovementScripts[0].Delay);
            Assert.Equal(25u, creatureMovementScripts[0].Command);
            Assert.Equal(1u, creatureMovementScripts[0].Datalong);
            Assert.Equal(0u, creatureMovementScripts[0].Datalong2);
            Assert.Equal(0u, creatureMovementScripts[0].Datalong3);
            Assert.Equal(0u, creatureMovementScripts[0].Datalong4);
            Assert.Equal(14386u, creatureMovementScripts[0].TargetParam1);
            Assert.Equal(20u, creatureMovementScripts[0].TargetParam2);
            Assert.Equal(8u, creatureMovementScripts[0].TargetType);
            Assert.Equal(3u, creatureMovementScripts[0].DataFlags);
            Assert.Equal(0, creatureMovementScripts[0].Dataint);
            Assert.Equal(0, creatureMovementScripts[0].Dataint2);
            Assert.Equal(0, creatureMovementScripts[0].Dataint3);
            Assert.Equal(0, creatureMovementScripts[0].Dataint4);
            Assert.Equal(0.0f, creatureMovementScripts[0].X);
            Assert.Equal(0.0f, creatureMovementScripts[0].Y);
            Assert.Equal(0.0f, creatureMovementScripts[0].Z);
            Assert.Equal(0.0f, creatureMovementScripts[0].O); // 90 degrees in radians
            Assert.Equal(0u, creatureMovementScripts[0].ConditionId);
            Assert.Equal(" set run Wandering Eye of Kilrogg", creatureMovementScripts[0].Comments);
        }


        public static void TestGetCreatureMovementSpecials()
        {
            int count = MangosRepository.GetRowCountForTable("creature_movement_special");
            List<CreatureMovementSpecial> creatureMovementSpecials = MangosRepository.GetCreatureMovementSpecials();

            Assert.Equal(count, creatureMovementSpecials.Count);
        }


        public static void TestGetCreatureMovementTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("creature_movement_template");
            List<CreatureMovementTemplate> creatureMovementTemplates = MangosRepository.GetCreatureMovementTemplates();

            Assert.Equal(count, creatureMovementTemplates.Count);

            Assert.Equal(6090u, creatureMovementTemplates[0].Entry);
            Assert.Equal(1u, creatureMovementTemplates[0].Point);
            Assert.Equal(-8604.02f, creatureMovementTemplates[0].PositionX);
            Assert.Equal(389.82f, creatureMovementTemplates[0].PositionY);
            Assert.Equal(102.924f, creatureMovementTemplates[0].PositionZ);
            Assert.Equal(30000u, creatureMovementTemplates[0].Waittime);
            Assert.Equal(0u, creatureMovementTemplates[0].ScriptId);
            Assert.Equal(0u, creatureMovementTemplates[0].Textid1);
            Assert.Equal(0u, creatureMovementTemplates[0].Textid2);
            Assert.Equal(0u, creatureMovementTemplates[0].Textid3);
            Assert.Equal(0u, creatureMovementTemplates[0].Textid4);
            Assert.Equal(0u, creatureMovementTemplates[0].Textid5);
            Assert.Equal(7u, creatureMovementTemplates[0].Emote);
            Assert.Equal(0u, creatureMovementTemplates[0].Spell);
            Assert.Equal(5.57619f, creatureMovementTemplates[0].Orientation); // Represents 180 degrees in radians
            Assert.Equal(0u, creatureMovementTemplates[0].Model1);
            Assert.Equal(0u, creatureMovementTemplates[0].Model2);
        }


        public static void TestGetCreatureOnKillReputations()
        {
            int count = MangosRepository.GetRowCountForTable("creature_onkill_reputation");
            List<CreatureOnKillReputation> creatureOnKillReputations = MangosRepository.GetCreatureOnKillReputations();

            Assert.Equal(count, creatureOnKillReputations.Count);

            Assert.Equal(674u, creatureOnKillReputations[0].CreatureId);
            Assert.Equal(21, creatureOnKillReputations[0].RewOnKillRepFaction1);
            Assert.Equal(0, creatureOnKillReputations[0].RewOnKillRepFaction2);
            Assert.Equal(5, creatureOnKillReputations[0].MaxStanding1);
            Assert.False(creatureOnKillReputations[0].IsTeamAward1);
            Assert.Equal(25, creatureOnKillReputations[0].RewOnKillRepValue1);
            Assert.Equal(0, creatureOnKillReputations[0].MaxStanding2);
            Assert.False(creatureOnKillReputations[0].IsTeamAward2);
            Assert.Equal(0, creatureOnKillReputations[0].RewOnKillRepValue2);
            Assert.False(creatureOnKillReputations[0].TeamDependent);
        }


        public static void TestGetCreatureQuestRelations()
        {
            int count = MangosRepository.GetRowCountForTable("creature_questrelation");
            List<CreatureQuestRelation> creatureQuestRelations = MangosRepository.GetCreatureQuestRelations();

            Assert.Equal(count, creatureQuestRelations.Count);

            Assert.Equal(196u, creatureQuestRelations[0].Id);
            Assert.Equal(33u, creatureQuestRelations[0].Quest);
            Assert.Equal(0u, creatureQuestRelations[0].Patch);
        }


        public static void TestGetCreatureSpells()
        {
            int count = MangosRepository.GetRowCountForTable("creature_spells");
            List<CreatureSpell> creatureSpells = MangosRepository.GetCreatureSpells();

            Assert.Equal(count, creatureSpells.Count);

            Assert.Equal(24250u, creatureSpells[0].Entry);
            Assert.Equal("Undercity - Varimathras", creatureSpells[0].Name);

            // Spell 1
            Assert.Equal(20743u, creatureSpells[0].SpellId1);
            Assert.Equal(100u, creatureSpells[0].Probability1);
            Assert.Equal(1u, creatureSpells[0].CastTarget1);
            Assert.Equal(0u, creatureSpells[0].TargetParam11);
            Assert.Equal(0u, creatureSpells[0].TargetParam21);
            Assert.Equal(2u, creatureSpells[0].CastFlags1);
            Assert.Equal(15u, creatureSpells[0].DelayInitialMin1);
            Assert.Equal(15u, creatureSpells[0].DelayInitialMax1);
            Assert.Equal(20u, creatureSpells[0].DelayRepeatMin1);
            Assert.Equal(20u, creatureSpells[0].DelayRepeatMax1);
            Assert.Equal(0u, creatureSpells[0].ScriptId1);

            // Spell 2
            Assert.Equal(20741u, creatureSpells[0].SpellId2);
            Assert.Equal(100u, creatureSpells[0].Probability2);
            Assert.Equal(0u, creatureSpells[0].CastTarget2);
            Assert.Equal(0u, creatureSpells[0].TargetParam12);
            Assert.Equal(0u, creatureSpells[0].TargetParam22);
            Assert.Equal(0u, creatureSpells[0].CastFlags2);
            Assert.Equal(25u, creatureSpells[0].DelayInitialMin2);
            Assert.Equal(25u, creatureSpells[0].DelayInitialMax2);
            Assert.Equal(20u, creatureSpells[0].DelayRepeatMin2);
            Assert.Equal(20u, creatureSpells[0].DelayRepeatMax2);
            Assert.Equal(0u, creatureSpells[0].ScriptId2);

            // Spell 3
            Assert.Equal(20740u, creatureSpells[0].SpellId3);
            Assert.Equal(100u, creatureSpells[0].Probability3);
            Assert.Equal(5u, creatureSpells[0].CastTarget3);
            Assert.Equal(0u, creatureSpells[0].TargetParam13);
            Assert.Equal(0u, creatureSpells[0].TargetParam23);
            Assert.Equal(0u, creatureSpells[0].CastFlags3);
            Assert.Equal(20u, creatureSpells[0].DelayInitialMin3);
            Assert.Equal(20u, creatureSpells[0].DelayInitialMax3);
            Assert.Equal(15u, creatureSpells[0].DelayRepeatMin3);
            Assert.Equal(25u, creatureSpells[0].DelayRepeatMax3);
            Assert.Equal(0u, creatureSpells[0].ScriptId3);

            // Spell 4
            Assert.Equal(0u, creatureSpells[0].SpellId4);
            Assert.Equal(100u, creatureSpells[0].Probability4);
            Assert.Equal(1u, creatureSpells[0].CastTarget4);
            Assert.Equal(0u, creatureSpells[0].TargetParam14);
            Assert.Equal(0u, creatureSpells[0].TargetParam24);
            Assert.Equal(0u, creatureSpells[0].CastFlags4);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMin4);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMax4);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMin4);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMax4);
            Assert.Equal(0u, creatureSpells[0].ScriptId4);

            // Spell 5
            Assert.Equal(0u, creatureSpells[0].SpellId5);
            Assert.Equal(100u, creatureSpells[0].Probability5);
            Assert.Equal(1u, creatureSpells[0].CastTarget5);
            Assert.Equal(0u, creatureSpells[0].TargetParam15);
            Assert.Equal(0u, creatureSpells[0].TargetParam25);
            Assert.Equal(0u, creatureSpells[0].CastFlags5);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMin5);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMax5);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMin5);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMax5);
            Assert.Equal(0u, creatureSpells[0].ScriptId5);

            // Spell 6
            Assert.Equal(0u, creatureSpells[0].SpellId6);
            Assert.Equal(100u, creatureSpells[0].Probability6);
            Assert.Equal(1u, creatureSpells[0].CastTarget6);
            Assert.Equal(0u, creatureSpells[0].TargetParam16);
            Assert.Equal(0u, creatureSpells[0].TargetParam26);
            Assert.Equal(0u, creatureSpells[0].CastFlags6);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMin6);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMax6);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMin6);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMax6);
            Assert.Equal(0u, creatureSpells[0].ScriptId6);

            // Spell 7
            Assert.Equal(0u, creatureSpells[0].SpellId7);
            Assert.Equal(100u, creatureSpells[0].Probability7);
            Assert.Equal(1u, creatureSpells[0].CastTarget7);
            Assert.Equal(0u, creatureSpells[0].TargetParam17);
            Assert.Equal(0u, creatureSpells[0].TargetParam27);
            Assert.Equal(0u, creatureSpells[0].CastFlags7);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMin7);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMax7);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMin7);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMax7);
            Assert.Equal(0u, creatureSpells[0].ScriptId7);

            // Spell 8
            Assert.Equal(0u, creatureSpells[0].SpellId8);
            Assert.Equal(100u, creatureSpells[0].Probability8);
            Assert.Equal(1u, creatureSpells[0].CastTarget8);
            Assert.Equal(0u, creatureSpells[0].TargetParam18);
            Assert.Equal(0u, creatureSpells[0].TargetParam28);
            Assert.Equal(0u, creatureSpells[0].CastFlags8);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMin8);
            Assert.Equal(0u, creatureSpells[0].DelayInitialMax8);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMin8);
            Assert.Equal(0u, creatureSpells[0].DelayRepeatMax8);
            Assert.Equal(0u, creatureSpells[0].ScriptId8);
        }


        public static void TestGetCreatureSpellsScripts()
        {
            int count = MangosRepository.GetRowCountForTable("creature_spells_scripts");
            List<CreatureSpellScript> creatureSpellsScripts = MangosRepository.GetCreatureSpellsScripts();

            Assert.Equal(count, creatureSpellsScripts.Count);

            Assert.Equal(21147u, creatureSpellsScripts[0].Id);
            Assert.Equal(0u, creatureSpellsScripts[0].Delay);
            Assert.Equal(0u, creatureSpellsScripts[0].Command);
            Assert.Equal(0u, creatureSpellsScripts[0].Datalong);
            Assert.Equal(0u, creatureSpellsScripts[0].Datalong2);
            Assert.Equal(0u, creatureSpellsScripts[0].Datalong3);
            Assert.Equal(0u, creatureSpellsScripts[0].Datalong4);
            Assert.Equal(0u, creatureSpellsScripts[0].TargetParam1);
            Assert.Equal(0u, creatureSpellsScripts[0].TargetParam2);
            Assert.Equal(0u, creatureSpellsScripts[0].TargetType);
            Assert.Equal(0u, creatureSpellsScripts[0].DataFlags);
            Assert.Equal(9071, creatureSpellsScripts[0].Dataint);
            Assert.Equal(0, creatureSpellsScripts[0].Dataint2);
            Assert.Equal(0, creatureSpellsScripts[0].Dataint3);
            Assert.Equal(0, creatureSpellsScripts[0].Dataint4);
            Assert.Equal(0.0f, creatureSpellsScripts[0].X);
            Assert.Equal(0.0f, creatureSpellsScripts[0].Y);
            Assert.Equal(0.0f, creatureSpellsScripts[0].Z);
            Assert.Equal(0.0f, creatureSpellsScripts[0].O); // 90 degrees in radians
            Assert.Equal(0u, creatureSpellsScripts[0].ConditionId);
            Assert.Equal("Azuregos - Arcane Vacuum - Say Text", creatureSpellsScripts[0].Comments);
        }


        public static void TestGetCreatureTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("creature_template");
            List<CreatureTemplate> creatureTemplates = MangosRepository.GetCreatureTemplates();

            Assert.Equal(count, creatureTemplates.Count);

            Assert.Equal(2u, creatureTemplates[0].Entry);
            Assert.Equal(0u, creatureTemplates[0].Patch);
            Assert.Equal(0u, creatureTemplates[0].KillCredit1);
            Assert.Equal(0u, creatureTemplates[0].KillCredit2);
            Assert.Equal(262u, creatureTemplates[0].ModelId1);
            Assert.Equal(0u, creatureTemplates[0].ModelId2);
            Assert.Equal(0u, creatureTemplates[0].ModelId3);
            Assert.Equal(0u, creatureTemplates[0].ModelId4);
            Assert.Equal("Spawn Point (Only GM can see it)", creatureTemplates[0].Name);
            Assert.Equal(string.Empty, creatureTemplates[0].Subname);
            Assert.Equal(0u, creatureTemplates[0].GossipMenuId);
            Assert.Equal(63u, creatureTemplates[0].MinLevel);
            Assert.Equal(63u, creatureTemplates[0].MaxLevel);
            Assert.Equal(9999u, creatureTemplates[0].MinHealth);
            Assert.Equal(9999u, creatureTemplates[0].MaxHealth);
            Assert.Equal(0u, creatureTemplates[0].MinMana);
            Assert.Equal(0u, creatureTemplates[0].MaxMana);
            Assert.Equal(68u, creatureTemplates[0].Armor);
            Assert.Equal(35u, creatureTemplates[0].FactionA);
            Assert.Equal(35u, creatureTemplates[0].FactionH);
            Assert.Equal(0u, creatureTemplates[0].NpcFlag);
            Assert.Equal(0.0f, creatureTemplates[0].SpeedWalk);
            Assert.Equal(1.14286f, creatureTemplates[0].SpeedRun);
            Assert.Equal(0.0f, creatureTemplates[0].Scale);
            Assert.Equal(3u, creatureTemplates[0].Rank);
            Assert.Equal(11.0f, creatureTemplates[0].MinDmg);
            Assert.Equal(11.0f, creatureTemplates[0].MaxDmg);
            Assert.Equal(0u, creatureTemplates[0].DmgSchool);
            Assert.Equal(290u, creatureTemplates[0].AttackPower);
            Assert.Equal(1.0f, creatureTemplates[0].DmgMultiplier);
            Assert.Equal(1800u, creatureTemplates[0].BaseAttackTime);
            Assert.Equal(1900u, creatureTemplates[0].RangeAttackTime);
            Assert.Equal(0u, creatureTemplates[0].UnitClass);
            Assert.Equal(0u, creatureTemplates[0].UnitFlags);
            Assert.Equal(0u, creatureTemplates[0].DynamicFlags);
            Assert.Equal(0u, creatureTemplates[0].Family);
            Assert.Equal(0u, creatureTemplates[0].TrainerType);
            Assert.Equal(0u, creatureTemplates[0].TrainerSpell);
            Assert.Equal(0u, creatureTemplates[0].TrainerClass);
            Assert.Equal(0u, creatureTemplates[0].TrainerRace);
            Assert.Equal(387.6f, creatureTemplates[0].MinRangedDmg);
            Assert.Equal(532.95f, creatureTemplates[0].MaxRangedDmg);
            Assert.Equal(100u, creatureTemplates[0].RangedAttackPower);
            Assert.Equal(1u, creatureTemplates[0].Type);
            Assert.Equal(8u, creatureTemplates[0].TypeFlags);
            Assert.Equal(0u, creatureTemplates[0].LootId);
            Assert.Equal(0u, creatureTemplates[0].PickpocketLoot);
            Assert.Equal(0u, creatureTemplates[0].SkinLoot);
            Assert.Equal(0, creatureTemplates[0].Resistance1);
            Assert.Equal(15, creatureTemplates[0].Resistance2);
            Assert.Equal(15, creatureTemplates[0].Resistance3);
            Assert.Equal(15, creatureTemplates[0].Resistance4);
            Assert.Equal(15, creatureTemplates[0].Resistance5);
            Assert.Equal(15, creatureTemplates[0].Resistance6);
            Assert.Equal(0u, creatureTemplates[0].Spell1);
            Assert.Equal(0u, creatureTemplates[0].Spell2);
            Assert.Equal(0u, creatureTemplates[0].Spell3);
            Assert.Equal(0u, creatureTemplates[0].Spell4);
            Assert.Equal(0u, creatureTemplates[0].SpellsTemplate);
            Assert.Equal(0u, creatureTemplates[0].PetSpellDataId);
            Assert.Equal(222u, creatureTemplates[0].MinGold);
            Assert.Equal(1110u, creatureTemplates[0].MaxGold);
            Assert.Equal(string.Empty, creatureTemplates[0].AiName);
            Assert.Equal(0u, creatureTemplates[0].MovementType);
            Assert.Equal(3u, creatureTemplates[0].InhabitType);
            Assert.Equal(0u, creatureTemplates[0].Civilian);
            Assert.Equal(0u, creatureTemplates[0].RacialLeader);
            Assert.Equal(1u, creatureTemplates[0].RegenHealth);
            Assert.Equal(0u, creatureTemplates[0].EquipmentId);
            Assert.Equal(0u, creatureTemplates[0].TrainerId);
            Assert.Equal(0u, creatureTemplates[0].VendorId);
            Assert.Equal(2147483648u, creatureTemplates[0].MechanicImmuneMask);
            Assert.Equal(0u, creatureTemplates[0].SchoolImmuneMask);
            Assert.Equal(32898u, creatureTemplates[0].FlagsExtra);
            Assert.Equal(string.Empty, creatureTemplates[0].ScriptName);
        }

        public static void TestGetCreatureTemplateAddons()
        {
            int count = MangosRepository.GetRowCountForTable("creature_template_addon");
            List<CreatureTemplateAddon> creatureTemplateAddons = MangosRepository.GetCreatureTemplateAddons();

            Assert.Equal(count, creatureTemplateAddons.Count);

            Assert.Equal(1356u, creatureTemplateAddons[0].Entry);
            Assert.Equal(0u, creatureTemplateAddons[0].Patch);
            Assert.Equal(0u, creatureTemplateAddons[0].Mount);
            Assert.Equal(0u, creatureTemplateAddons[0].Bytes1);
            Assert.Equal(1u, creatureTemplateAddons[0].B20Sheath);
            Assert.Equal(16u, creatureTemplateAddons[0].B21Flags);
            Assert.Equal(0u, creatureTemplateAddons[0].Emote);
            Assert.Equal(0u, creatureTemplateAddons[0].Moveflags);
            Assert.Equal(string.Empty, creatureTemplateAddons[0].Auras); // Example auras, adjust as needed
        }

        public static void TestGetCustomTexts()
        {
            int count = MangosRepository.GetRowCountForTable("custom_texts");
            List<CustomText> customTexts = MangosRepository.GetCustomTexts();

            Assert.Equal(count, customTexts.Count);
        }


        public static void TestGetDisenchantLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("disenchant_loot_template");
            List<DisenchantLootTemplate> disenchantLootTemplates = MangosRepository.GetDisenchantLootTemplates();

            Assert.Equal(count, disenchantLootTemplates.Count);

            Assert.Equal(65u, disenchantLootTemplates[0].Entry);
            Assert.Equal(20725u, disenchantLootTemplates[0].Item);
            Assert.Equal(100.0f, disenchantLootTemplates[0].ChanceOrQuestChance);
            Assert.Equal(0u, disenchantLootTemplates[0].Groupid);
            Assert.Equal(1u, disenchantLootTemplates[0].MincountOrRef);
            Assert.Equal(2u, disenchantLootTemplates[0].Maxcount);
            Assert.Equal(0u, disenchantLootTemplates[0].ConditionId);
            Assert.Equal(0u, disenchantLootTemplates[0].PatchMin);
            Assert.Equal(10u, disenchantLootTemplates[0].PatchMax);
        }

        public static void TestGetEventScripts()
        {
            int count = MangosRepository.GetRowCountForTable("event_scripts");
            List<EventScript> eventScripts = MangosRepository.GetEventScripts();

            Assert.Equal(count, eventScripts.Count);

            Assert.Equal(364u, eventScripts[0].Id);
            Assert.Equal(5u, eventScripts[0].Delay);
            Assert.Equal(10u, eventScripts[0].Command);
            Assert.Equal(2624u, eventScripts[0].Datalong);
            Assert.Equal(900000u, eventScripts[0].Datalong2);
            Assert.Equal(0u, eventScripts[0].Datalong3);
            Assert.Equal(0u, eventScripts[0].Datalong4);
            Assert.Equal(0u, eventScripts[0].TargetParam1);
            Assert.Equal(0u, eventScripts[0].TargetParam2);
            Assert.Equal(0u, eventScripts[0].TargetType);
            Assert.Equal(0u, eventScripts[0].DataFlags);
            Assert.Equal(0u, eventScripts[0].Dataint);
            Assert.Equal(0u, eventScripts[0].Dataint2);
            Assert.Equal(6u, eventScripts[0].Dataint3);
            Assert.Equal(1u, eventScripts[0].Dataint4);
            Assert.Equal(-12179.4f, eventScripts[0].X);
            Assert.Equal(644.22f, eventScripts[0].Y);
            Assert.Equal(-67.1f, eventScripts[0].Z);
            Assert.Equal(5.18f, eventScripts[0].O); // 90 degrees in radians
            Assert.Equal(0u, eventScripts[0].ConditionId);
            Assert.Equal(string.Empty, eventScripts[0].Comments);
        }

        public static void TestGetExplorationBaseXPs()
        {
            int count = MangosRepository.GetRowCountForTable("exploration_basexp");
            List<ExplorationBaseXP> explorationBaseXPs = MangosRepository.GetExplorationBaseXPs();

            Assert.Equal(count, explorationBaseXPs.Count);

            Assert.Equal(0u, explorationBaseXPs[0].Level);
            Assert.Equal(0u, explorationBaseXPs[0].Basexp);

            Assert.Equal(59u, explorationBaseXPs[59].Level); // Assuming levels go up to 60
            Assert.Equal(640u, explorationBaseXPs[59].Basexp); // Example XP for level 60
        }

        public static void TestGetFactions()
        {
            int count = MangosRepository.GetRowCountForTable("faction");
            List<Faction> factions = MangosRepository.GetFactions();

            Assert.Equal(count, factions.Count);

            Assert.Equal(1u, factions[0].Id);
            Assert.Equal(-1, factions[0].ReputationListID);
            Assert.Equal(0u, factions[0].BaseRepRaceMask1);
            Assert.Equal(0u, factions[0].BaseRepRaceMask2);
            Assert.Equal(0u, factions[0].BaseRepRaceMask3);
            Assert.Equal(0u, factions[0].BaseRepRaceMask4);
            Assert.Equal(0u, factions[0].BaseRepClassMask1);
            Assert.Equal(0u, factions[0].BaseRepClassMask2);
            Assert.Equal(0u, factions[0].BaseRepClassMask3);
            Assert.Equal(0u, factions[0].BaseRepClassMask4);
            Assert.Equal(0, factions[0].BaseRepValue1);
            Assert.Equal(0, factions[0].BaseRepValue2);
            Assert.Equal(0, factions[0].BaseRepValue3);
            Assert.Equal(0, factions[0].BaseRepValue4);
            Assert.Equal(0u, factions[0].ReputationFlags1);
            Assert.Equal(0u, factions[0].ReputationFlags2);
            Assert.Equal(0u, factions[0].ReputationFlags3);
            Assert.Equal(0u, factions[0].ReputationFlags4);
            Assert.Equal(0u, factions[0].Team);
            Assert.Equal("PLAYER, Human", factions[0].Name1);
            Assert.Equal("플레이어 - 인간", factions[0].Name2);
            Assert.Equal("JOUEUR, Humain", factions[0].Name3);
            Assert.Equal("SPIELER, Mensch", factions[0].Name4);
            Assert.Equal("人类(玩家)", factions[0].Name5);
            Assert.Equal(string.Empty, factions[0].Name6);
            Assert.Equal("JUGADOR, humano", factions[0].Name7);
            Assert.Equal(string.Empty, factions[0].Name8);
            Assert.Equal(string.Empty, factions[0].Description1);
            Assert.Equal(string.Empty, factions[0].Description2);
            Assert.Equal(string.Empty, factions[0].Description3);
            Assert.Equal(string.Empty, factions[0].Description4);
            Assert.Equal(string.Empty, factions[0].Description5);
            Assert.Equal(string.Empty, factions[0].Description6);
            Assert.Equal(string.Empty, factions[0].Description7);
            Assert.Equal(string.Empty, factions[0].Description8);
        }

        public static void TestGetFactionTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("faction_template");
            List<FactionTemplate> factionTemplates = MangosRepository.GetFactionTemplates();

            Assert.Equal(count, factionTemplates.Count);

            Assert.Equal(1u, factionTemplates[0].Id);
            Assert.Equal(1u, factionTemplates[0].FactionId);
            Assert.Equal(72u, factionTemplates[0].FactionFlags);
            Assert.Equal(3u, factionTemplates[0].OurMask);
            Assert.Equal(2u, factionTemplates[0].FriendlyMask);
            Assert.Equal(12u, factionTemplates[0].HostileMask);
            Assert.Equal(0u, factionTemplates[0].EnemyFaction1);
            Assert.Equal(0u, factionTemplates[0].EnemyFaction2);
            Assert.Equal(0u, factionTemplates[0].EnemyFaction3);
            Assert.Equal(0u, factionTemplates[0].EnemyFaction4);
            Assert.Equal(0u, factionTemplates[0].FriendFaction1);
            Assert.Equal(0u, factionTemplates[0].FriendFaction2);
            Assert.Equal(0u, factionTemplates[0].FriendFaction3);
            Assert.Equal(0u, factionTemplates[0].FriendFaction4);
        }


        public static void TestGetFishingLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("fishing_loot_template");
            List<FishingLootTemplate> fishingLootTemplates = MangosRepository.GetFishingLootTemplates();

            Assert.Equal(count, fishingLootTemplates.Count);

            Assert.Equal(382u, fishingLootTemplates[0].Entry);
            Assert.Equal(6651u, fishingLootTemplates[0].Item);
            Assert.Equal(0.1f, fishingLootTemplates[0].ChanceOrQuestChance);
            Assert.Equal(0u, fishingLootTemplates[0].GroupId);
            Assert.Equal(1u, fishingLootTemplates[0].MinCountOrRef);
            Assert.Equal(1u, fishingLootTemplates[0].MaxCount);
            Assert.Equal(0u, fishingLootTemplates[0].ConditionId);
            Assert.Equal(0u, fishingLootTemplates[0].PatchMin);
            Assert.Equal(10u, fishingLootTemplates[0].PatchMax);
        }


        public static void TestGetForbiddenItems()
        {
            int count = MangosRepository.GetRowCountForTable("forbidden_items");
            List<ForbiddenItem> forbiddenItems = MangosRepository.GetForbiddenItems();

            Assert.Equal(count, forbiddenItems.Count);

            Assert.Equal(20725u, forbiddenItems[0].Entry);
            Assert.Equal(6u, forbiddenItems[0].Patch);
            Assert.Equal(1u, forbiddenItems[0].AfterOrBefore);
        }

        public static void TestGetGameObjects()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject");
            List<GameObject> gameObjects = MangosRepository.GetGameObjects();

            Assert.Equal(count, gameObjects.Count);

            Assert.Equal(99863u, gameObjects[0].Guid);
            Assert.Equal(300147u, gameObjects[0].Id);
            Assert.Equal(0u, gameObjects[0].Map);
            Assert.Equal(-43.4367f, gameObjects[0].PositionX);
            Assert.Equal(-923.198f, gameObjects[0].PositionY);
            Assert.Equal(55.8714f, gameObjects[0].PositionZ);
            Assert.Equal(5.75401f, gameObjects[0].Orientation); // 90 degrees in radians
            Assert.Equal(0.0f, gameObjects[0].Rotation0);
            Assert.Equal(0.0f, gameObjects[0].Rotation1);
            Assert.Equal(0.261511f, gameObjects[0].Rotation2);
            Assert.Equal(-0.965201f, gameObjects[0].Rotation3);
            Assert.Equal(25, gameObjects[0].Spawntimesecsmin);
            Assert.Equal(25, gameObjects[0].Spawntimesecsmax);
            Assert.Equal(0u, gameObjects[0].Animprogress);
            Assert.Equal(1u, gameObjects[0].State);
            Assert.Equal(0u, gameObjects[0].SpawnFlags);
            Assert.Equal(0.0f, gameObjects[0].Visibilitymod);
            Assert.Equal(0u, gameObjects[0].PatchMin);
            Assert.Equal(10u, gameObjects[0].PatchMax);
        }

        public static void TestGetGameObjectBattlegrounds()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_battleground");
            List<GameObjectBattleground> gameObjectBattlegrounds = MangosRepository.GetGameObjectBattlegrounds();

            Assert.Equal(count, gameObjectBattlegrounds.Count);

            Assert.Equal(53682u, gameObjectBattlegrounds[0].Guid);
            Assert.Equal(96u, gameObjectBattlegrounds[0].Event1);
            Assert.Equal(0u, gameObjectBattlegrounds[0].Event2);
        }

        public static void TestGetGameObjectInvolvedRelations()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_involvedrelation");
            List<GameObjectInvolvedRelation> relations = MangosRepository.GetGameObjectInvolvedRelations();

            Assert.Equal(count, relations.Count);

            Assert.Equal(31u, relations[0].Id);
            Assert.Equal(94u, relations[0].Quest);
            Assert.Equal(0u, relations[0].Patch);
        }

        public static void TestGetGameObjectLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_loot_template");
            List<GameObjectLootTemplate> lootTemplates = MangosRepository.GetGameObjectLootTemplates();

            Assert.Equal(count, lootTemplates.Count);

            Assert.Equal(1683u, lootTemplates[0].Entry);
            Assert.Equal(1309u, lootTemplates[0].Item);
            Assert.Equal(-100.0f, lootTemplates[0].ChanceOrQuestChance);
            Assert.Equal(0u, lootTemplates[0].Groupid);
            Assert.Equal(1, lootTemplates[0].MincountOrRef);
            Assert.Equal(1u, lootTemplates[0].Maxcount);
            Assert.Equal(0u, lootTemplates[0].ConditionId);
            Assert.Equal(0u, lootTemplates[0].PatchMin);
            Assert.Equal(10u, lootTemplates[0].PatchMax);
        }

        public static void TestGetGameObjectQuestRelations()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_questrelation");
            List<GameObjectQuestRelation> questRelations = MangosRepository.GetGameObjectQuestRelations();

            Assert.Equal(count, questRelations.Count);

            Assert.Equal(31u, questRelations[0].Id);
            Assert.Equal(248u, questRelations[0].Quest);
            Assert.Equal(0u, questRelations[0].Patch);
        }

        public static void TestGetGameObjectRequirements()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_requirement");
            List<GameObjectRequirement> requirements = MangosRepository.GetGameObjectRequirements();

            Assert.Equal(count, requirements.Count);

            Assert.Equal(43121u, requirements[0].Guid);
            Assert.Equal(1u, requirements[0].ReqType);
            Assert.Equal(15536u, requirements[0].ReqGuid);
        }

        public static void TestGetGameObjectScripts()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_scripts");
            List<GameObjectScript> scripts = MangosRepository.GetGameObjectScripts();

            Assert.Equal(count, scripts.Count);

            Assert.Equal(34006u, scripts[0].Id);
            Assert.Equal(0u, scripts[0].Delay);
            Assert.Equal(11u, scripts[0].Command);
            Assert.Equal(33219u, scripts[0].Datalong);
            Assert.Equal(15u, scripts[0].Datalong2);
            Assert.Equal(0u, scripts[0].Datalong3);
            Assert.Equal(0u, scripts[0].Datalong4);
            Assert.Equal(0u, scripts[0].TargetParam1);
            Assert.Equal(0u, scripts[0].TargetParam2);
            Assert.Equal(0u, scripts[0].TargetType);
            Assert.Equal(0u, scripts[0].DataFlags);
            Assert.Equal(0, scripts[0].Dataint);
            Assert.Equal(0, scripts[0].Dataint2);
            Assert.Equal(0, scripts[0].Dataint3);
            Assert.Equal(0, scripts[0].Dataint4);
            Assert.Equal(0.0f, scripts[0].X);
            Assert.Equal(0.0f, scripts[0].Y);
            Assert.Equal(0.0f, scripts[0].Z);
            Assert.Equal(0.0f, scripts[0].O); // 90 degrees in radians
            Assert.Equal(0u, scripts[0].ConditionId);
            Assert.Equal("Ouverture Grille Ombrecroc cellule ashcrombe", scripts[0].Comments);
        }

        public static void TestGetGameObjectTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("gameobject_template");
            List<GameObjectTemplate> gameObjectTemplates = MangosRepository.GetGameObjectTemplates();

            Assert.Equal(count, gameObjectTemplates.Count);

            Assert.Equal(300148u, gameObjectTemplates[0].Entry);
            Assert.Equal(0u, gameObjectTemplates[0].Patch);
            Assert.Equal(8u, gameObjectTemplates[0].Type);
            Assert.Equal(1287u, gameObjectTemplates[0].DisplayId);
            Assert.Equal("TEMP Ruins of Stardust Fountain", gameObjectTemplates[0].Name);
            Assert.Equal(0u, gameObjectTemplates[0].Faction);
            Assert.Equal(0u, gameObjectTemplates[0].Flags);
            Assert.Equal(1.0f, gameObjectTemplates[0].Size);
            Assert.Equal(0u, gameObjectTemplates[0].Mingold);
            Assert.Equal(0u, gameObjectTemplates[0].Maxgold);
            Assert.Equal(string.Empty, gameObjectTemplates[0].ScriptName);

            Assert.Equal(223u, gameObjectTemplates[0].Data[0]);
            Assert.Equal(5u, gameObjectTemplates[0].Data[1]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[2]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[3]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[4]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[5]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[6]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[7]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[8]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[9]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[10]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[11]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[12]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[13]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[14]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[15]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[16]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[17]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[18]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[19]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[20]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[21]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[22]);
            Assert.Equal(0u, gameObjectTemplates[0].Data[23]);
        }

        public static void TestGetGameEvents()
        {
            int count = MangosRepository.GetRowCountForTable("game_event");
            List<GameEvent> gameEvents = MangosRepository.GetGameEvents();

            Assert.Equal(count, gameEvents.Count);

            Assert.Equal(1u, gameEvents[0].Entry);
            Assert.Equal(Timestamp.FromDateTimeOffset(DateTime.Parse("2007-06-21 01:00:00Z")), gameEvents[0].StartTime);
            Assert.Equal(Timestamp.FromDateTimeOffset(DateTime.Parse("2020-12-31 03:00:00Z")), gameEvents[0].EndTime);
            Assert.Equal(525600u, gameEvents[0].Occurrence); // Example occurrence in seconds
            Assert.Equal(20160u, gameEvents[0].Length); // Example length in seconds
            Assert.Equal(341u, gameEvents[0].Holiday);
            Assert.Equal("Midsummer Fire Festival", gameEvents[0].Description);
            Assert.False(gameEvents[0].Hardcoded);
            Assert.False(gameEvents[0].Disabled);
            Assert.Equal(9u, gameEvents[0].PatchMin);
            Assert.Equal(10u, gameEvents[0].PatchMax);
        }

        public static void TestGetGameEventCreatures()
        {
            int count = MangosRepository.GetRowCountForTable("game_event_creature");
            List<GameEventCreature> gameEventCreatures = MangosRepository.GetGameEventCreatures();

            Assert.Equal(count, gameEventCreatures.Count);

            Assert.Equal(37u, gameEventCreatures[0].Guid);
            Assert.Equal(159, gameEventCreatures[0].Event);
        }

        public static void TestGetGameEventCreatureDatas()
        {
            int count = MangosRepository.GetRowCountForTable("game_event_creature_data");
            List<GameEventCreatureData> gameEventCreatureDataList = MangosRepository.GetGameEventCreatureDatas();

            Assert.Equal(count, gameEventCreatureDataList.Count);

            Assert.Equal(12088u, gameEventCreatureDataList[0].Guid);
            Assert.Equal(0u, gameEventCreatureDataList[0].EntryId);
            Assert.Equal(0u, gameEventCreatureDataList[0].Modelid);
            Assert.Equal(504u, gameEventCreatureDataList[0].EquipmentId);
            Assert.Equal(0u, gameEventCreatureDataList[0].SpellStart);
            Assert.Equal(0u, gameEventCreatureDataList[0].SpellEnd);
            Assert.Equal(27u, gameEventCreatureDataList[0].Event);
        }

        public static void TestGetGameEventGameObjects()
        {
            int count = MangosRepository.GetRowCountForTable("game_event_gameobject");
            List<GameEventGameObject> gameEventGameObjectList = MangosRepository.GetGameEventGameObjects();

            Assert.Equal(count, gameEventGameObjectList.Count);

            Assert.Equal(1u, gameEventGameObjectList[0].Guid);
            Assert.Equal(1, gameEventGameObjectList[0].Event);
        }

        public static void TestGetGameEventQuests()
        {
            int count = MangosRepository.GetRowCountForTable("game_event_quest");
            List<GameEventQuest> gameEventQuestList = MangosRepository.GetGameEventQuests();

            Assert.Equal(count, gameEventQuestList.Count);

            Assert.Equal(172u, gameEventQuestList[0].Quest);
            Assert.Equal(10u, gameEventQuestList[0].Event);
            Assert.Equal(2u, gameEventQuestList[0].Patch);
        }

        public static void TestGetGameGraveyardZones()
        {
            int count = MangosRepository.GetRowCountForTable("game_graveyard_zone");
            List<GameGraveyardZone> gameGraveyardZones = MangosRepository.GetGameGraveyardZones();

            Assert.Equal(count, gameGraveyardZones.Count);

            Assert.Equal(100u, gameGraveyardZones[0].Id);
            Assert.Equal(1u, gameGraveyardZones[0].GhostZone);
            Assert.Equal(469u, gameGraveyardZones[0].Faction);
        }

        public static void TestGetGameTeles()
        {
            int count = MangosRepository.GetRowCountForTable("game_tele");
            List<GameTele> gameTeles = MangosRepository.GetGameTeles();

            Assert.Equal(count, gameTeles.Count);

            Assert.Equal(1u, gameTeles[0].Id);
            Assert.Equal(1400.61f, gameTeles[0].PositionX);
            Assert.Equal(-1493.87f, gameTeles[0].PositionY);
            Assert.Equal(54.7844f, gameTeles[0].PositionZ);
            Assert.Equal(4.08661f, gameTeles[0].Orientation); // 90 degrees in radians
            Assert.Equal(0u, gameTeles[0].Map);
            Assert.Equal("RuinsOfAndorhal", gameTeles[0].Name);
        }

        public static void TestGetGameWeathers()
        {
            int count = MangosRepository.GetRowCountForTable("game_weather");
            List<GameWeather> gameWeathers = MangosRepository.GetGameWeathers();

            Assert.Equal(count, gameWeathers.Count);
        }

        public static void TestGetGMSubSurveys()
        {
            int count = MangosRepository.GetRowCountForTable("gm_subsurveys");
            List<GMSubSurvey> gmSubSurveys = MangosRepository.GetGMSubSurveys();

            Assert.Equal(count, gmSubSurveys.Count);
        }

        public static void TestGetGMSurveys()
        {
            int count = MangosRepository.GetRowCountForTable("gm_surveys");
            List<GMSurvey> gmSurveys = MangosRepository.GetGmSurveys();

            Assert.Equal(count, gmSurveys.Count);
        }

        public static void TestGetGMTickets()
        {
            int count = MangosRepository.GetRowCountForTable("gm_tickets");
            List<GMTicket> gmTickets = MangosRepository.GetGmTickets();

            Assert.Equal(count, gmTickets.Count);
        }

        public static void TestGetGossipMenus()
        {
            int count = MangosRepository.GetRowCountForTable("gossip_menu");
            List<GossipMenu> gossipMenus = MangosRepository.GetGossipMenus();

            Assert.Equal(count, gossipMenus.Count);

            Assert.Equal(64u, gossipMenus[0].Entry);
            Assert.Equal(564u, gossipMenus[0].TextId);
            Assert.Equal(90u, gossipMenus[0].ConditionId);
        }

        public static void TestGetGossipMenuOptions()
        {
            int count = MangosRepository.GetRowCountForTable("gossip_menu_option");
            List<GossipMenuOption> gossipMenuOptions = MangosRepository.GetGossipMenuOptions();

            Assert.Equal(count, gossipMenuOptions.Count);

            Assert.Equal(3331u, gossipMenuOptions[0].MenuId);
            Assert.Equal(3u, gossipMenuOptions[0].Id);
            Assert.Equal(0u, gossipMenuOptions[0].OptionIcon);
            Assert.Equal("Stable Master", gossipMenuOptions[0].OptionText);
            Assert.Equal(8511u, gossipMenuOptions[0].OptionBroadcastTextId);
            Assert.Equal(1u, gossipMenuOptions[0].OptionId);
            Assert.Equal(1u, gossipMenuOptions[0].NpcOptionNpcflag);
            Assert.Equal(4903, gossipMenuOptions[0].ActionMenuId);
            Assert.Equal(420u, gossipMenuOptions[0].ActionPoiId);
            Assert.Equal(0u, gossipMenuOptions[0].ActionScriptId);
            Assert.Equal(0u, gossipMenuOptions[0].BoxCoded);
            Assert.Equal(0u, gossipMenuOptions[0].BoxMoney);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxText);
            Assert.Equal(0u, gossipMenuOptions[0].BoxBroadcastTextId);
            Assert.Equal(0u, gossipMenuOptions[0].ConditionId);
        }

        public static void TestGetGossipScripts()
        {
            int count = MangosRepository.GetRowCountForTable("gossip_scripts");
            List<GossipScript> gossipScripts = MangosRepository.GetGossipScripts();

            Assert.Equal(count, gossipScripts.Count);

            Assert.Equal(7u, gossipScripts[0].Id);
            Assert.Equal(0u, gossipScripts[0].Delay);
            Assert.Equal(22u, gossipScripts[0].Command);
            Assert.Equal(168u, gossipScripts[0].Datalong);
            Assert.Equal(2u, gossipScripts[0].Datalong2);
            Assert.Equal(0u, gossipScripts[0].Datalong3);
            Assert.Equal(0u, gossipScripts[0].Datalong4);
            Assert.Equal(0u, gossipScripts[0].TargetParam1);
            Assert.Equal(0u, gossipScripts[0].TargetParam2);
            Assert.Equal(0u, gossipScripts[0].TargetType);
            Assert.Equal(0u, gossipScripts[0].DataFlags);
            Assert.Equal(0, gossipScripts[0].Dataint);
            Assert.Equal(0, gossipScripts[0].Dataint2);
            Assert.Equal(0, gossipScripts[0].Dataint3);
            Assert.Equal(0, gossipScripts[0].Dataint4);
            Assert.Equal(0.0f, gossipScripts[0].X);
            Assert.Equal(0.0f, gossipScripts[0].Y);
            Assert.Equal(0.0f, gossipScripts[0].Z);
            Assert.Equal(0.0f, gossipScripts[0].O); // 90 degrees in radians
            Assert.Equal(0u, gossipScripts[0].ConditionId);
            Assert.Equal("Azuregos - Set Faction to Enemy", gossipScripts[0].Comments);
        }

        public static void TestGetInstanceBuffRemovals()
        {
            int count = MangosRepository.GetRowCountForTable("instance_buff_removal");
            List<InstanceBuffRemoval> instanceBuffRemovals = MangosRepository.GetInstanceBuffRemovals();

            Assert.Equal(count, instanceBuffRemovals.Count);
        }

        public static void TestGetInstanceCreatureKills()
        {
            int count = MangosRepository.GetRowCountForTable("instance_creature_kills");
            List<InstanceCreatureKills> instanceCreatureKills = MangosRepository.GetInstanceCreatureKills();

            Assert.Equal(count, instanceCreatureKills.Count);
        }

        public static void TestGetInstanceCustomCounters()
        {
            int count = MangosRepository.GetRowCountForTable("instance_custom_counters");
            List<InstanceCustomCounter> instanceCustomCounters = MangosRepository.GetInstanceCustomCounters();

            Assert.Equal(count, instanceCustomCounters.Count);
        }

        public static void TestGetInstanceWipes()
        {
            int count = MangosRepository.GetRowCountForTable("instance_wipes");
            List<InstanceWipe> instanceWipes = MangosRepository.GetInstanceWipes();

            Assert.Equal(count, instanceWipes.Count);
        }

        public static void TestGetItemDisplayInfo()
        {
            int count = MangosRepository.GetRowCountForTable("item_display_info");
            List<ItemDisplayInfo> itemDisplayInfos = MangosRepository.GetItemDisplayInfo();

            Assert.Equal(count, itemDisplayInfos.Count);

            Assert.Equal(220, itemDisplayInfos[0].Field0);
            Assert.Equal("INV_Robe_02", itemDisplayInfos[0].Field5); // Example value
        }

        public static void TestGetItemEnchantmentTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("item_enchantment_template");
            List<ItemEnchantmentTemplate> enchantmentTemplates = MangosRepository.GetItemEnchantmentTemplates();

            Assert.Equal(count, enchantmentTemplates.Count);

            Assert.Equal(454u, enchantmentTemplates[0].Entry);
            Assert.Equal(5u, enchantmentTemplates[0].Ench);
            Assert.Equal(4.53f, enchantmentTemplates[0].Chance);
        }

        public static void TestGetItemLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("item_loot_template");
            List<ItemLootTemplate> lootTemplates = MangosRepository.GetItemLootTemplates();

            Assert.Equal(count, lootTemplates.Count);

            Assert.Equal(4632u, lootTemplates[0].Entry);
            Assert.Equal(2003u, lootTemplates[0].Item);
            Assert.Equal(100.0f, lootTemplates[0].ChanceOrQuestChance);
            Assert.Equal(1u, lootTemplates[0].Groupid);
            Assert.Equal(-2003, lootTemplates[0].MincountOrRef);
            Assert.Equal(1u, lootTemplates[0].Maxcount);
            Assert.Equal(0u, lootTemplates[0].ConditionId);
            Assert.Equal(0u, lootTemplates[0].PatchMin);
            Assert.Equal(10u, lootTemplates[0].PatchMax);
        }

        public static void TestGetItemRequiredTargets()
        {
            int count = MangosRepository.GetRowCountForTable("item_required_target");
            List<ItemRequiredTarget> requiredTargets = MangosRepository.GetItemRequiredTargets();

            Assert.Equal(count, requiredTargets.Count);

            Assert.Equal(3912u, requiredTargets[0].Entry);
            Assert.Equal(1u, requiredTargets[0].Type);
            Assert.Equal(2530u, requiredTargets[0].TargetEntry);
        }

        public static void TestGetItemTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("item_template");
            List<ItemTemplate> itemTemplates = MangosRepository.GetItemTemplates();

            Assert.Equal(count, itemTemplates.Count);

            Assert.Equal(25u, itemTemplates[0].Entry);
            Assert.Equal(0u, itemTemplates[0].Patch);
            Assert.Equal(2u, itemTemplates[0].Class);
            Assert.Equal(7u, itemTemplates[0].Subclass);
            Assert.Equal("Worn Shortsword", itemTemplates[0].Name);
            Assert.Equal(1542u, itemTemplates[0].Displayid);
            Assert.Equal(1u, itemTemplates[0].Quality);
            Assert.Equal(0u, itemTemplates[0].Flags);
            Assert.Equal(1u, itemTemplates[0].BuyCount);
            Assert.Equal(35u, itemTemplates[0].BuyPrice);
            Assert.Equal(7u, itemTemplates[0].SellPrice);
            Assert.Equal(21u, itemTemplates[0].InventoryType);
            Assert.Equal(-1, itemTemplates[0].AllowableClass);
            Assert.Equal(-1, itemTemplates[0].AllowableRace);
            Assert.Equal(2u, itemTemplates[0].ItemLevel);
            Assert.Equal(1u, itemTemplates[0].RequiredLevel);
            Assert.Equal(0u, itemTemplates[0].RequiredSkill);
            Assert.Equal(0u, itemTemplates[0].RequiredSkillRank);
            Assert.Equal(0u, itemTemplates[0].RequiredSpell);
            Assert.Equal(0u, itemTemplates[0].RequiredHonorRank);
            Assert.Equal(0u, itemTemplates[0].RequiredCityRank);
            Assert.Equal(0u, itemTemplates[0].RequiredReputationFaction);
            Assert.Equal(0u, itemTemplates[0].RequiredReputationRank);
            Assert.Equal(0u, itemTemplates[0].MaxCount);
            Assert.Equal(1u, itemTemplates[0].Stackable);
            Assert.Equal(0u, itemTemplates[0].ContainerSlots);
            Assert.Equal(0u, itemTemplates[0].Armor);
            Assert.Equal(1900u, itemTemplates[0].Delay);
            Assert.Equal(0u, itemTemplates[0].AmmoType);
            Assert.Equal(0u, itemTemplates[0].Bonding);
            Assert.Equal(string.Empty, itemTemplates[0].Description);
            Assert.Equal(0u, itemTemplates[0].PageText);
            Assert.Equal(1u, itemTemplates[0].LanguageID);
            Assert.Equal(0u, itemTemplates[0].PageMaterial);
            Assert.Equal(0u, itemTemplates[0].StartQuest);
            Assert.Equal(0u, itemTemplates[0].LockID);
            Assert.Equal(1, itemTemplates[0].Material); // Example signed value for Material
            Assert.Equal(3u, itemTemplates[0].Sheath);
            Assert.Equal(0u, itemTemplates[0].RandomProperty);
            Assert.Equal(0u, itemTemplates[0].Block);
            Assert.Equal(0u, itemTemplates[0].ItemSet);
            Assert.Equal(20u, itemTemplates[0].MaxDurability);
            Assert.Equal(0u, itemTemplates[0].Area);
            Assert.Equal(0u, itemTemplates[0].Map);
            Assert.Equal(0u, itemTemplates[0].BagFamily);
            Assert.Equal(string.Empty, itemTemplates[0].ScriptName);
            Assert.Equal(0u, itemTemplates[0].DisenchantID);
            Assert.Equal(0u, itemTemplates[0].FoodType);
            Assert.Equal(0u, itemTemplates[0].MinMoneyLoot);
            Assert.Equal(0u, itemTemplates[0].MaxMoneyLoot);
            Assert.Equal(0u, itemTemplates[0].Duration); // Example duration in seconds
            Assert.Equal(0u, itemTemplates[0].ExtraFlags);
            Assert.Equal(1u, itemTemplates[0].OtherTeamEntry);

            // Assert the stats
            Assert.Equal(0u, itemTemplates[0].Stats[0].Type);
            Assert.Equal(0, itemTemplates[0].Stats[0].Value);
            Assert.Equal(0u, itemTemplates[1].Stats[1].Type);
            Assert.Equal(0, itemTemplates[1].Stats[1].Value);
            Assert.Equal(0u, itemTemplates[2].Stats[2].Type);
            Assert.Equal(0, itemTemplates[2].Stats[2].Value);
            Assert.Equal(0u, itemTemplates[3].Stats[3].Type);
            Assert.Equal(0, itemTemplates[3].Stats[3].Value);
            Assert.Equal(0u, itemTemplates[4].Stats[4].Type);
            Assert.Equal(0, itemTemplates[4].Stats[4].Value);
            Assert.Equal(0u, itemTemplates[5].Stats[5].Type);
            Assert.Equal(0, itemTemplates[5].Stats[5].Value);
            Assert.Equal(0u, itemTemplates[6].Stats[6].Type);
            Assert.Equal(0, itemTemplates[6].Stats[6].Value);
            Assert.Equal(0u, itemTemplates[7].Stats[7].Type);
            Assert.Equal(0, itemTemplates[7].Stats[7].Value);
            Assert.Equal(0u, itemTemplates[8].Stats[8].Type);
            Assert.Equal(0, itemTemplates[8].Stats[8].Value);
            Assert.Equal(0u, itemTemplates[9].Stats[9].Type);
            Assert.Equal(0, itemTemplates[9].Stats[9].Value);

            // Assert the damages
            Assert.Equal(1.0f, itemTemplates[0].Damages[0].Min);
            Assert.Equal(3.0f, itemTemplates[0].Damages[0].Max);
            Assert.Equal(0u, itemTemplates[0].Damages[0].Type);
            Assert.Equal(0.0f, itemTemplates[0].Damages[1].Min);
            Assert.Equal(0.0f, itemTemplates[0].Damages[1].Max);
            Assert.Equal(0u, itemTemplates[0].Damages[1].Type);
            Assert.Equal(0.0f, itemTemplates[0].Damages[2].Min);
            Assert.Equal(0.0f, itemTemplates[0].Damages[2].Max);
            Assert.Equal(0u, itemTemplates[0].Damages[2].Type);
            Assert.Equal(0.0f, itemTemplates[0].Damages[3].Min);
            Assert.Equal(0.0f, itemTemplates[0].Damages[3].Max);
            Assert.Equal(0u, itemTemplates[0].Damages[3].Type);
            Assert.Equal(0.0f, itemTemplates[0].Damages[4].Min);
            Assert.Equal(0.0f, itemTemplates[0].Damages[4].Max);
            Assert.Equal(0u, itemTemplates[0].Damages[4].Type);

            // Assert the resistances
            Assert.Equal(0u, itemTemplates[0].Resistances.Holy);
            Assert.Equal(0u, itemTemplates[0].Resistances.Fire);
            Assert.Equal(0u, itemTemplates[0].Resistances.Nature);
            Assert.Equal(0u, itemTemplates[0].Resistances.Frost);
            Assert.Equal(0u, itemTemplates[0].Resistances.Shadow);
            Assert.Equal(0u, itemTemplates[0].Resistances.Arcane);

            // Assert the spells
            Assert.Equal(0u, itemTemplates[0].Spells[0].SpellID);
            Assert.Equal(0u, itemTemplates[0].Spells[0].Trigger);
            Assert.Equal(0, itemTemplates[0].Spells[0].Charges);
            Assert.Equal(0.0f, itemTemplates[0].Spells[0].PpmRate);
            Assert.Equal(-1, itemTemplates[0].Spells[0].Cooldown);
            Assert.Equal(0u, itemTemplates[1].Spells[1].SpellID);
            Assert.Equal(0u, itemTemplates[1].Spells[1].Trigger);
            Assert.Equal(0, itemTemplates[1].Spells[1].Charges);
            Assert.Equal(0.0f, itemTemplates[1].Spells[1].PpmRate);
            Assert.Equal(-1, itemTemplates[1].Spells[1].Cooldown);
            Assert.Equal(0u, itemTemplates[2].Spells[2].SpellID);
            Assert.Equal(0u, itemTemplates[2].Spells[2].Trigger);
            Assert.Equal(0, itemTemplates[2].Spells[2].Charges);
            Assert.Equal(0.0f, itemTemplates[2].Spells[2].PpmRate);
            Assert.Equal(-1, itemTemplates[2].Spells[2].Cooldown);
            Assert.Equal(0u, itemTemplates[3].Spells[3].SpellID);
            Assert.Equal(0u, itemTemplates[3].Spells[3].Trigger);
            Assert.Equal(0, itemTemplates[3].Spells[3].Charges);
            Assert.Equal(0.0f, itemTemplates[3].Spells[3].PpmRate);
            Assert.Equal(-1, itemTemplates[3].Spells[3].Cooldown);
            Assert.Equal(0u, itemTemplates[4].Spells[4].SpellID);
            Assert.Equal(0u, itemTemplates[4].Spells[4].Trigger);
            Assert.Equal(0, itemTemplates[4].Spells[4].Charges);
            Assert.Equal(0.0f, itemTemplates[4].Spells[4].PpmRate);
            Assert.Equal(0, itemTemplates[4].Spells[4].Cooldown);
        }

        public static void TestGetLocalesArea()
        {
            int count = MangosRepository.GetRowCountForTable("locales_area");
            List<LocalesArea> localesAreas = MangosRepository.GetLocalesArea();

            Assert.Equal(count, localesAreas.Count);

            Assert.Equal(1u, localesAreas[0].Entry);
            Assert.Equal("던 모로", localesAreas[0].NameLoc1);
            Assert.Equal("Dun Morogh", localesAreas[0].NameLoc2);
            Assert.Equal("Dun Morogh", localesAreas[0].NameLoc3);
            Assert.Equal("丹莫罗", localesAreas[0].NameLoc4);
            Assert.Equal(string.Empty, localesAreas[0].NameLoc5);
            Assert.Equal(string.Empty, localesAreas[0].NameLoc6);
            Assert.Equal(string.Empty, localesAreas[0].NameLoc7);
            Assert.Equal("Дун Морог", localesAreas[0].NameLoc8);
        }

        public static void TestGetLocalesBroadcastTexts()
        {
            int count = MangosRepository.GetRowCountForTable("locales_broadcast_text");
            List<LocalesBroadcastText> broadcastTexts = MangosRepository.GetLocalesBroadcastTexts();

            Assert.Equal(count, broadcastTexts.Count);

            Assert.Equal(1u, broadcastTexts[0].Id);
            Assert.Equal("제발, 제발 좀 도와주시오! 난 여기 갇혀 있소!", broadcastTexts[0].MaleTextLoc1);
            Assert.Equal("Au secours, au secours ! On m’opprime !", broadcastTexts[0].MaleTextLoc2);
            Assert.Equal("Hilfe, Hilfe! Ich werde bedrängt!", broadcastTexts[0].MaleTextLoc3);
            Assert.Equal("救命啊！我动不了了！", broadcastTexts[0].MaleTextLoc4);
            Assert.Equal("救命啊!我動不了了!", broadcastTexts[0].MaleTextLoc5);
            Assert.Equal("¡Socorro! ¡Socorro! ¡Soy víctima de represión!", broadcastTexts[0].MaleTextLoc6);
            Assert.Equal("¡Socorro! ¡Socorro! ¡Soy víctima de represión!", broadcastTexts[0].MaleTextLoc7);
            Assert.Equal("Помогите! Хулиганы зрения лишают!", broadcastTexts[0].MaleTextLoc8);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc1);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc2);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc3);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc4);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc5);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc6);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc7);
            Assert.Equal(string.Empty, broadcastTexts[0].FemaleTextLoc8);
            Assert.Equal(18019, broadcastTexts[0].VerifiedBuild);
        }

        public static void TestGetLocalesCreatures()
        {
            int count = MangosRepository.GetRowCountForTable("locales_creature");
            List<LocalesCreature> creatures = MangosRepository.GetLocalesCreatures();

            Assert.Equal(count, creatures.Count);

            Assert.Equal(10290u, creatures[0].Entry);
            Assert.Equal("사로잡은 악령숲 수액괴물", creatures[0].NameLoc1);
            Assert.Equal("Limon de Gangrebois capturé", creatures[0].NameLoc2);
            Assert.Equal("Gefangener Teufelswaldschlamm", creatures[0].NameLoc3);
            Assert.Equal("被捕获的费伍德软泥怪", creatures[0].NameLoc4);
            Assert.Equal("被捕獲的費伍德軟泥怪", creatures[0].NameLoc5);
            Assert.Equal("Moco de Frondavil capturado", creatures[0].NameLoc6);
            Assert.Equal("Moco de Frondavil capturado", creatures[0].NameLoc7);
            Assert.Equal("Пойманный слизнюк Оскверненного леса", creatures[0].NameLoc8);

            Assert.Equal(string.Empty, creatures[0].SubnameLoc1);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc2);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc3);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc4);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc5);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc6);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc7);
            Assert.Equal(string.Empty, creatures[0].SubnameLoc8);
        }

        public static void TestGetLocalesGameObjects()
        {
            int count = MangosRepository.GetRowCountForTable("locales_gameobject");
            List<LocalesGameObject> gameObjects = MangosRepository.GetLocalesGameObjects();

            Assert.Equal(count, gameObjects.Count);

            Assert.Equal(31u, gameObjects[0].Entry);
            Assert.Equal("오래된 사자상", gameObjects[0].NameLoc1);
            Assert.Equal("Statue du vieux lion", gameObjects[0].NameLoc2);
            Assert.Equal("Alte Löwenstatue", gameObjects[0].NameLoc3);
            Assert.Equal("石狮子", gameObjects[0].NameLoc4);
            Assert.Equal("老舊獅子雕像", gameObjects[0].NameLoc5);
            Assert.Equal("Estatua de león antigua", gameObjects[0].NameLoc6);
            Assert.Equal("Estatua de león antigua", gameObjects[0].NameLoc7);
            Assert.Equal("Статуя старого льва", gameObjects[0].NameLoc8);
        }

        public static void TestGetLocalesGossipMenuOptions()
        {
            int count = MangosRepository.GetRowCountForTable("locales_gossip_menu_option");
            List<LocalesGossipMenuOption> gossipMenuOptions = MangosRepository.GetLocalesGossipMenuOptions();

            Assert.Equal(count, gossipMenuOptions.Count);

            Assert.Equal(3506u, gossipMenuOptions[0].MenuId);
            Assert.Equal(0u, gossipMenuOptions[0].Id);
            Assert.Equal(string.Empty, gossipMenuOptions[0].OptionTextLoc1);
            Assert.Equal("Banque", gossipMenuOptions[0].OptionTextLoc2);
            Assert.Equal("Bank", gossipMenuOptions[0].OptionTextLoc3);
            Assert.Equal("银行", gossipMenuOptions[0].OptionTextLoc4);
            Assert.Equal(string.Empty, gossipMenuOptions[0].OptionTextLoc5);
            Assert.Equal(string.Empty, gossipMenuOptions[0].OptionTextLoc6);
            Assert.Equal(string.Empty, gossipMenuOptions[0].OptionTextLoc7);
            Assert.Equal("Банк", gossipMenuOptions[0].OptionTextLoc8);

            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc1);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc2);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc3);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc4);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc5);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc6);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc7);
            Assert.Equal(string.Empty, gossipMenuOptions[0].BoxTextLoc8);
        }

        public static void TestGetLocalesItems()
        {
            int count = MangosRepository.GetRowCountForTable("locales_item");
            List<LocalesItem> localesItems = MangosRepository.GetLocalesItems();

            Assert.Equal(count, localesItems.Count);

            Assert.Equal(25u, localesItems[0].Entry);
            Assert.Equal("낡은 쇼트소드", localesItems[0].NameLoc1);
            Assert.Equal("Epée courte usée", localesItems[0].NameLoc2);
            Assert.Equal("Abgenutztes Kurzschwert", localesItems[0].NameLoc3);
            Assert.Equal("破损的短剑", localesItems[0].NameLoc4);
            Assert.Equal("破損的短劍", localesItems[0].NameLoc5);
            Assert.Equal("Espada corta desgastada", localesItems[0].NameLoc6);
            Assert.Equal("Espada corta desgastada", localesItems[0].NameLoc7);
            Assert.Equal("Иссеченный короткий меч", localesItems[0].NameLoc8);

            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc1);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc2);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc3);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc4);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc5);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc6);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc7);
            Assert.Equal(string.Empty, localesItems[0].DescriptionLoc8);
        }

        public static void TestGetLocalesPageTexts()
        {
            int count = MangosRepository.GetRowCountForTable("locales_page_text");
            List<LocalesPageText> localesPageTexts = MangosRepository.GetLocalesPageTexts();

            Assert.Equal(count, localesPageTexts.Count);

            Assert.Equal(15u, localesPageTexts[0].Entry);
            Assert.Equal(string.Empty, localesPageTexts[0].TextLoc1);
            Assert.Equal("Bonjour Morgan,\n\nLes affaires sont florissantes à Comté-de-l'or, au point que je n'ai pas eu le temps de vous envoyer la moindre cargaison ! \n\nJ'ai demandé au porteur de ce message de vous remettre un grand paquet de bougies (vous savez, celles que les kobolds aiment porter sur le sommet du crâne ?) \n\nRemerciez ce messager et donnez-lui le paiement qui lui revient.", localesPageTexts[0].TextLoc2);
            Assert.Equal("Mein lieber Morgan,$B$Bdie Gesch?fte hier in Goldhain gehen gut - so gut, dass ich bislang noch nicht einmal dazugekommen bin, dir eine Lieferung zu schicken!$B$BDie Person, die dir diese Notiz übergibt, hat gleichzeitig auch den Auftrag von mir, dir ein gro?es Paket Wachskerzen zu übergeben - du wei?t schon, diese Dinger, wie sie Kobolde gern auf dem Kopf tragen.$B$BBitte zeig dich dankbar und entlohn die Person angemessen.", localesPageTexts[0].TextLoc3);
            Assert.Equal("你好，摩根。$B $B 闪金镇的交易非常繁忙，所以我根本抽不出时间来帮你送货！$B $B 我已委托帮我送这封信的人给你带去一包大蜡烛（你知道，可笑的狗头人总喜欢把这些东西戴在头上）。$B $B 请向这位送货的人表示我们的感谢，并给予他合理的报酬。 ", localesPageTexts[0].TextLoc4);
            Assert.Equal(string.Empty, localesPageTexts[0].TextLoc5);
            Assert.Equal(string.Empty, localesPageTexts[0].TextLoc6);
            Assert.Equal(string.Empty, localesPageTexts[0].TextLoc7);
            Assert.Equal("Здравствуй, Морган!\r\n\r\nДела в Златоземье идут бойко, так бойко, что у меня не было времени отправить тебе товар! \r\n\r\nЯ нанял человека, который принесет эту записку, доставить тебе пачку больших восковых свечей (тех, что кобольды обычно носят на голове). \r\n\r\nБудь добр, отблагодари этого человека и заплати ему как следует.", localesPageTexts[0].TextLoc8);
        }

        public static void TestGetLocalesPointsOfInterest()
        {
            int count = MangosRepository.GetRowCountForTable("locales_points_of_interest");
            List<LocalesPointsOfInterest> pointsOfInterest = MangosRepository.GetLocalesPointsOfInterest();

            Assert.Equal(count, pointsOfInterest.Count);

            Assert.Equal(1u, pointsOfInterest[0].Entry);
            Assert.Equal(string.Empty, pointsOfInterest[0].IconNameLoc1);
            Assert.Equal("L'auberge de la Fierté du Lion", pointsOfInterest[0].IconNameLoc2);
            Assert.Equal("Gasthaus Zur H?hle des L?wen", pointsOfInterest[0].IconNameLoc3);
            Assert.Equal("狮王之傲旅店", pointsOfInterest[0].IconNameLoc4);
            Assert.Equal(string.Empty, pointsOfInterest[0].IconNameLoc5);
            Assert.Equal(string.Empty, pointsOfInterest[0].IconNameLoc6);
            Assert.Equal(string.Empty, pointsOfInterest[0].IconNameLoc7);
            Assert.Equal("Таверна \"Гордость льва\"", pointsOfInterest[0].IconNameLoc8);
        }

        public static void TestGetLocalesQuest()
        {
            int count = MangosRepository.GetRowCountForTable("locales_quest");
            List<LocalesQuest> localesQuests = MangosRepository.GetLocalesQuest();

            Assert.Equal(count, localesQuests.Count);

            Assert.Equal(2u, localesQuests[0].Entry);
            Assert.Equal("뾰족발톱의 발톱", localesQuests[0].TitleLoc1);
            Assert.Equal("La griffe de Serres-tranchantes", localesQuests[0].TitleLoc2);
            Assert.Equal("Klaue von Scharfkralle", localesQuests[0].TitleLoc3);
            Assert.Equal("沙普塔隆的爪子", localesQuests[0].TitleLoc4);
            Assert.Equal(string.Empty, localesQuests[0].TitleLoc5);
            Assert.Equal("La garfa de Garfafilada", localesQuests[0].TitleLoc6);
            Assert.Equal("La garfa de Garfafilada", localesQuests[0].TitleLoc7);
            Assert.Equal("Коготь гиппогрифа Острокогтя", localesQuests[0].TitleLoc8);
            Assert.Equal(string.Empty, localesQuests[0].DetailsLoc1);
            Assert.Equal("Le grand hippogriffe Serres-tranchantes a été tué, et la griffe arrachée à son cadavre témoigne de votre victoire. Senani Cœur-de-tonnerre, au poste de Bois-brisé, sera sans doute intéressée de voir ce trophée qui est la preuve de votre exploit.", localesQuests[0].DetailsLoc2);
            Assert.Equal("Der mächtige Hippogryph Scharfkralle wurde getötet und die Klaue der erschlagenen Bestie dient als Beweis für Euren Sieg.$b$bSenani Donnerherz in der Silberwindzuflucht wird zum Beweis Eurer Tat sicher gern diese Trophäe sehen wollen.", localesQuests[0].DetailsLoc3);
            Assert.Equal("强大的角鹰兽沙普塔隆已经被你杀死了，它的爪子将成为你胜利的象征。碎木哨岗的塞娜尼·雷心一定会对你的战利品感兴趣的。", localesQuests[0].DetailsLoc4);
            Assert.Equal(string.Empty, localesQuests[0].DetailsLoc5);
            Assert.Equal("El poderoso hipogrifo Garfafilada ha sido ejecutado, con la garfa de la bestia derribada como testimonio de tu victoria. Seguro que Senani Corazón Atronador, del Refugio Brisa de Plata, estará interesado en ver este trofeo que prueba tus actos.", localesQuests[0].DetailsLoc6);
            Assert.Equal("El poderoso hipogrifo Garfafilada ha sido ejecutado, con la garfa de la bestia derribada como testimonio de tu victoria. Seguro que Senani Corazón Atronador, del Refugio Brisa de Plata, estará interesado en ver este trofeo que prueba tus actos.", localesQuests[0].DetailsLoc7);
            Assert.Equal("Могучий гиппогриф Острокоготь был убит, и коготь этой свирепой твари – свидетельство вашей победы. Сенани Громосерд на заставе Расщепленного Дерева несомненно пожелает увидеть этот трофей – доказательство ваших деяний.", localesQuests[0].DetailsLoc8);
            Assert.Equal(string.Empty, localesQuests[0].ObjectivesLoc1);
            Assert.Equal("Apporter la griffe de Serres-tranchantes à Senani Cœur-de-tonnerre, au poste de Bois-brisé, en Orneval.", localesQuests[0].ObjectivesLoc2);
            Assert.Equal("Bringt Senani Donnerherz in der Silberwindzuflucht die Klaue von Scharfkralle.", localesQuests[0].ObjectivesLoc3);
            Assert.Equal("将沙普塔隆的爪子交给灰谷碎木哨岗的塞娜尼·雷心。", localesQuests[0].ObjectivesLoc4);
            Assert.Equal(string.Empty, localesQuests[0].ObjectivesLoc5);
            Assert.Equal("Llévale la garfa de Garfafilada a Senani Corazón Atronador en el Refugio Brisa de Plata, Vallefresno.", localesQuests[0].ObjectivesLoc6);
            Assert.Equal("Llévale la garfa de Garfafilada a Senani Corazón Atronador en el Refugio Brisa de Plata, Vallefresno.", localesQuests[0].ObjectivesLoc7);
            Assert.Equal("Принесите коготь гиппогрифа Острокогтя Сенани Громосерд на заставу Расщепленного Дерева в Ясеневом лесу.", localesQuests[0].ObjectivesLoc8);
            Assert.Equal(string.Empty, localesQuests[0].OfferRewardTextLoc1);
            Assert.Equal("Très impressionnant, $N... la griffe de Serres-tranchantes n'a pas dû être facile à se procurer ! La Chasse d'Orneval se déroule bien pour vous !$B$BSerres-tranchantes terrorisait depuis longtemps les péons du camp de bûcherons qui essayaient de voyager entre ici et le poste de Bois-brisé. Dès que les gens sauront que c'est vous qui avez défait cette bête, ils composeront sans doute des chants sur votre bravoure, et ils seront chantés dans tous les camps de bûcherons et toutes les scieries d'Orneval !", localesQuests[0].OfferRewardTextLoc2);
            Assert.Equal(string.Empty, localesQuests[0].OfferRewardTextLoc3);
            Assert.Equal("真不错，$N……沙普塔隆的爪子可不是那麽容易可以得到的!看来你的梣谷狩猎之行很顺利啊!$B$B伐木场的工人总是会受到沙普塔隆的袭击。毫无疑问的，当这头猛兽被你杀死的消息传出去之後，很多歌颂你英勇事蹟的歌谣肯定会在梣谷营地及伐木场流传开来的!$B", localesQuests[0].OfferRewardTextLoc4);
            Assert.Equal(string.Empty, localesQuests[0].OfferRewardTextLoc5);
            Assert.Equal(string.Empty, localesQuests[0].OfferRewardTextLoc6);
            Assert.Equal(string.Empty, localesQuests[0].OfferRewardTextLoc7);
            Assert.Equal("Очень впечатляет, $N... коготь Острокогтя непросто добыть! Тебе везет в охоте!$B$BОстрокоготь давно донимал батраков с лесозаготовок, когда они пытались добраться сюда, на заставу Расщепленного Дерева. Несомненно, скоро разойдется весть, что именно ты $gприкончил:прикончила; чудовище, и множество песен в твою честь будет распеваться у костров и на лесопильнях по всему Ясеневому лесу!", localesQuests[0].OfferRewardTextLoc8);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc1);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc2);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc3);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc4);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc5);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc6);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc7);
            Assert.Equal(string.Empty, localesQuests[0].EndTextLoc8);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc1);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc2);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc3);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc4);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc5);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc6);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc7);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText1Loc8);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc1);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc2);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc3);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc4);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc5);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc6);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc7);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText2Loc8);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc1);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc2);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc3);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc4);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc5);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc6);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc7);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText3Loc8);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc1);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc2);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc3);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc4);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc5);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc6);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc7);
            Assert.Equal(string.Empty, localesQuests[0].ObjectiveText4Loc8);
        }

        public static void TestGetMailLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("mail_loot_template");
            List<MailLootTemplate> lootTemplates = MangosRepository.GetMailLootTemplates();

            Assert.Equal(count, lootTemplates.Count);

            Assert.Equal(103u, lootTemplates[0].Entry);
            Assert.Equal(11422u, lootTemplates[0].Item);
            Assert.Equal(100.0f, lootTemplates[0].ChanceOrQuestChance);
            Assert.Equal(0u, lootTemplates[0].Groupid);
            Assert.Equal(1, lootTemplates[0].MincountOrRef);
            Assert.Equal(1u, lootTemplates[0].Maxcount);
            Assert.Equal(0u, lootTemplates[0].ConditionId);
            Assert.Equal(0u, lootTemplates[0].PatchMin);
            Assert.Equal(10u, lootTemplates[0].PatchMax);
        }

        public static void TestGetMangosStrings()
        {
            int count = MangosRepository.GetRowCountForTable("mangos_string");
            List<MangosString> mangosStrings = MangosRepository.GetMangosStrings();

            Assert.Equal(count, mangosStrings.Count);

            Assert.Equal(1u, mangosStrings[0].Entry);
            Assert.Equal("You should select a character or a creature.", mangosStrings[0].ContentDefault);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc1);
            Assert.Equal("Vous devez sélectionner un personnage ou une créature.", mangosStrings[0].ContentLoc2);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc3);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc4);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc5);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc6);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc7);
            Assert.Equal(string.Empty, mangosStrings[0].ContentLoc8);
        }

        public static void TestGetMapTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("map_template");
            List<MapTemplate> mapTemplates = MangosRepository.GetMapTemplates();

            Assert.Equal(count, mapTemplates.Count);

            Assert.Equal(0u, mapTemplates[0].Entry);
            Assert.Equal(0u, mapTemplates[0].Patch);
            Assert.Equal(0u, mapTemplates[0].Parent);
            Assert.Equal(0u, mapTemplates[0].MapType);
            Assert.Equal(0u, mapTemplates[0].LinkedZone);
            Assert.Equal(0u, mapTemplates[0].LevelMin);
            Assert.Equal(0u, mapTemplates[0].LevelMax);
            Assert.Equal(0u, mapTemplates[0].MaxPlayers);
            Assert.Equal(0u, mapTemplates[0].ResetDelay);
            Assert.Equal(-1, mapTemplates[0].GhostEntranceMap);
            Assert.Equal(0.0f, mapTemplates[0].GhostEntranceX);
            Assert.Equal(0.0f, mapTemplates[0].GhostEntranceY);
            Assert.Equal("Eastern Kingdoms", mapTemplates[0].MapName);
            Assert.Equal(string.Empty, mapTemplates[0].ScriptName);
        }

        public static void TestGetMigrations()
        {
            int count = MangosRepository.GetRowCountForTable("migrations");
            List<string> migrations = MangosRepository.GetMigrations();

            Assert.Equal(count, migrations.Count);

            Assert.Equal("20180129213805", migrations[0]);
            Assert.Equal("20180201170354", migrations[1]);
        }

        public static void TestGetNpcGossips()
        {
            int count = MangosRepository.GetRowCountForTable("npc_gossip");
            List<NpcGossip> npcGossips = MangosRepository.GetNpcGossips();

            Assert.Equal(count, npcGossips.Count);

            Assert.Equal(29u, npcGossips[0].NpcGuid);  // Adjust this value based on your NPC data
            Assert.Equal(5054u, npcGossips[0].Textid);    // Adjust this value based on your NPC data
        }

        public static void TestGetNpcTexts()
        {
            int count = MangosRepository.GetRowCountForTable("npc_text");
            List<NpcText> npcTexts = MangosRepository.GetNpcTexts();

            Assert.Equal(count, npcTexts.Count);

            // Test the first NPC text entry
            Assert.Equal(1u, npcTexts[0].ID);  // Adjust this value based on your expected data
            Assert.Equal(50429u, npcTexts[0].BroadcastTextID0);
            Assert.Equal(0.0f, npcTexts[0].Probability0);
            Assert.Equal(0u, npcTexts[0].BroadcastTextID1);
            Assert.Equal(0.0f, npcTexts[0].Probability1);
        }

        public static void TestGetNpcTrainers()
        {
            int count = MangosRepository.GetRowCountForTable("npc_trainer");
            List<NpcTrainer> npcTrainers = MangosRepository.GetNpcTrainers();

            Assert.Equal(count, npcTrainers.Count);

            // Test the first NPC trainer entry
            Assert.Equal(2131u, npcTrainers[0].Entry);  // Adjust this value based on your expected data
            Assert.Equal(11559u, npcTrainers[0].Spell);
            Assert.Equal(56000u, npcTrainers[0].Spellcost);
            Assert.Equal(0u, npcTrainers[0].Reqskill);
            Assert.Equal(0u, npcTrainers[0].Reqskillvalue);
            Assert.Equal(54u, npcTrainers[0].Reqlevel);
        }

        public static void TestGetNpcTrainerTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("npc_trainer_template");
            List<NpcTrainerTemplate> npcTrainerTemplates = MangosRepository.GetNpcTrainerTemplates();

            Assert.Equal(count, npcTrainerTemplates.Count);

            // Test the first NPC trainer template entry
            Assert.Equal(1u, npcTrainerTemplates[0].Entry);  // Adjust this value based on your expected data
            Assert.Equal(33389u, npcTrainerTemplates[0].Spell);
            Assert.Equal(900000u, npcTrainerTemplates[0].Spellcost);
            Assert.Equal(0u, npcTrainerTemplates[0].Reqskill);
            Assert.Equal(0u, npcTrainerTemplates[0].Reqskillvalue);
            Assert.Equal(40u, npcTrainerTemplates[0].Reqlevel);
        }

        public static void TestGetNpcVendors()
        {
            int count = MangosRepository.GetRowCountForTable("npc_vendor");
            List<NpcVendor> npcVendors = MangosRepository.GetNpcVendors();

            Assert.Equal(count, npcVendors.Count);

            // Test the first NPC vendor entry
            Assert.Equal(54u, npcVendors[0].Entry);  // Adjust this value based on your expected data
            Assert.Equal(2488u, npcVendors[0].Item);
            Assert.Equal(0u, npcVendors[0].Maxcount);
            Assert.Equal(0u, npcVendors[0].Incrtime);
        }

        public static void TestGetNpcVendorTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("npc_vendor_template");
            List<NpcVendorTemplate> npcVendorTemplates = MangosRepository.GetNpcVendorTemplates();

            Assert.Equal(count, npcVendorTemplates.Count);

            // Test the first NPC vendor template entry
            Assert.Equal(1279501u, npcVendorTemplates[0].Entry);  // Adjust this value based on your expected data
            Assert.Equal(5565u, npcVendorTemplates[0].Item);
            Assert.Equal(0u, npcVendorTemplates[0].Maxcount);
            Assert.Equal(0u, npcVendorTemplates[0].Incrtime);
        }

        public static void TestGetPageText()
        {
            int count = MangosRepository.GetRowCountForTable("page_text");
            List<PageText> pageTexts = MangosRepository.GetPageText();

            Assert.Equal(count, pageTexts.Count);

            // Test the first page text entry
            Assert.Equal(15u, pageTexts[0].Entry);  // Adjust this value based on your expected data
            Assert.Equal("Hello Morgan,\r\n\r\nBusiness in Goldshire is brisk, so brisk that I haven't had time to send you any shipments!  \r\n\r\nI commissioned the person bearing this note to bring you a package of large wax candles (you know, the ones the Kobolds like to wear on their heads?). \r\n\r\nPlease give this person our thanks, and fair payment.", pageTexts[0].Text);  // Example text, adjust as needed
        }

        public static void TestGetPetCreateInfoSpells()
        {
            int count = MangosRepository.GetRowCountForTable("petcreateinfo_spell");
            List<PetCreateInfoSpell> petCreateInfoSpells = MangosRepository.GetPetCreateInfoSpells();

            Assert.Equal(count, petCreateInfoSpells.Count);

            // Test the first pet create info spell entry
            Assert.Equal(416u, petCreateInfoSpells[0].Entry);  // Adjust based on your expected data
            Assert.Equal(3110u, petCreateInfoSpells[0].Spell1);  // Example spell ID, adjust as needed
            Assert.Equal(0u, petCreateInfoSpells[0].Spell2);  // Example spell ID, adjust as needed
            Assert.Equal(0u, petCreateInfoSpells[0].Spell3);  // Example spell ID, adjust as needed
            Assert.Equal(0u, petCreateInfoSpells[0].Spell4);  // Example spell ID, adjust as needed
        }

        public static void TestGetPetLevelStats()
        {
            int count = MangosRepository.GetRowCountForTable("pet_levelstats");
            List<PetLevelStats> petLevelStats = MangosRepository.GetPetLevelStats();

            Assert.Equal(count, petLevelStats.Count);

            // Test the first pet level stats entry
            Assert.Equal(1u, petLevelStats[0].CreatureEntry); // Example creature entry, adjust as needed
            Assert.Equal(1u, petLevelStats[0].Level); // Example level, adjust as needed
            Assert.Equal(42u, petLevelStats[0].Hp); // Example HP, adjust as needed
            Assert.Equal(1u, petLevelStats[0].Mana); // Example Mana, adjust as needed
            Assert.Equal(20u, petLevelStats[0].Armor); // Example Armor, adjust as needed
            Assert.Equal(22u, petLevelStats[0].Str); // Example Strength, adjust as needed
            Assert.Equal(20u, petLevelStats[0].Agi); // Example Agility, adjust as needed
            Assert.Equal(22u, petLevelStats[0].Sta); // Example Stamina, adjust as needed
            Assert.Equal(20u, petLevelStats[0].Inte); // Example Intellect, adjust as needed
            Assert.Equal(20u, petLevelStats[0].Spi); // Example Spirit, adjust as needed
        }

        public static void TestGetPetNameGenerations()
        {
            int count = MangosRepository.GetRowCountForTable("pet_name_generation");
            List<PetNameGeneration> petNames = MangosRepository.GetPetNameGenerations();

            Assert.Equal(count, petNames.Count);

            // Test the first pet name generation entry
            Assert.Equal(1u, petNames[0].Id); // Example ID, adjust as needed
            Assert.Equal("Aba", petNames[0].Word); // Example word, adjust as needed
            Assert.Equal(416u, petNames[0].Entry); // Example entry, adjust as needed
            Assert.Equal(0u, petNames[0].Half); // Example half value, adjust as needed

            // Test the second pet name generation entry
            Assert.Equal(2u, petNames[1].Id); // Example ID, adjust as needed
            Assert.Equal("Az", petNames[1].Word); // Example word, adjust as needed
            Assert.Equal(416u, petNames[1].Entry); // Example entry, adjust as needed
            Assert.Equal(0u, petNames[1].Half); // Example half value, adjust as needed

            // Continue testing other entries based on your data
        }

        public static void TestGetPickpocketingLoots()
        {
            int count = MangosRepository.GetRowCountForTable("pickpocketing_loot_template");
            List<PickpocketingLootTemplate> lootTemplates = MangosRepository.GetPickpocketingLoots();

            Assert.Equal(count, lootTemplates.Count);

            // Test the first pickpocketing loot template entry
            Assert.Equal(3u, lootTemplates[0].Entry); // Example entry ID, adjust as needed
            Assert.Equal(929u, lootTemplates[0].Item); // Example item ID, adjust as needed
            Assert.Equal(2.449f, lootTemplates[0].ChanceOrQuestChance); // Example chance value, adjust as needed
            Assert.Equal(0u, lootTemplates[0].Groupid); // Example groupid, adjust as needed
            Assert.Equal(1u, lootTemplates[0].MincountOrRef); // Example mincountOrRef, adjust as needed
            Assert.Equal(1u, lootTemplates[0].Maxcount); // Example maxcount, adjust as needed
            Assert.Equal(0u, lootTemplates[0].ConditionId); // Example condition ID, adjust as needed
            Assert.Equal(0u, lootTemplates[0].PatchMin); // Example patch_min, adjust as needed
            Assert.Equal(10u, lootTemplates[0].PatchMax); // Example patch_max, adjust as needed

            // Continue testing other entries based on your data
        }

        public static void TestGetPlayerBots()
        {
            int count = MangosRepository.GetRowCountForTable("playerbot");
            List<PlayerBot> playerBots = MangosRepository.GetPlayerBots();

            Assert.Equal(count, playerBots.Count);
        }

        public static void TestGetPlayerCreateInfo()
        {
            int count = MangosRepository.GetRowCountForTable("playercreateinfo");
            List<PlayerCreateInfo> playerCreateInfos = MangosRepository.GetPlayerCreateInfo();

            Assert.Equal(count, playerCreateInfos.Count);

            // Test the first player create info entry
            Assert.Equal(1u, playerCreateInfos[0].Race); // Example race, adjust as needed
            Assert.Equal(1u, playerCreateInfos[0].Class); // Example class, adjust as needed
            Assert.Equal(0u, playerCreateInfos[0].Map); // Example map ID, adjust as needed
            Assert.Equal(12u, playerCreateInfos[0].Zone); // Example zone ID, adjust as needed
            Assert.Equal(-8949.95f, playerCreateInfos[0].PositionX, 2); // Example position X, adjust as needed
            Assert.Equal(-132.49f, playerCreateInfos[0].PositionY, 2); // Example position Y, adjust as needed
            Assert.Equal(83.53f, playerCreateInfos[0].PositionZ, 2); // Example position Z, adjust as needed
            Assert.Equal(0.00f, playerCreateInfos[0].Orientation, 2); // Example orientation, adjust as needed
        }

        public static void TestGetPlayerCreateInfoActions()
        {
            int count = MangosRepository.GetRowCountForTable("playercreateinfo_action");
            List<PlayerCreateInfoAction> playerCreateInfoActions = MangosRepository.GetPlayerCreateInfoActions();

            Assert.Equal(count, playerCreateInfoActions.Count);

            // Test the first player create info action entry
            Assert.Equal(8u, playerCreateInfoActions[0].Race); // Example race, adjust as needed
            Assert.Equal(8u, playerCreateInfoActions[0].Class); // Example class, adjust as needed
            Assert.Equal(11u, playerCreateInfoActions[0].Button); // Example button, adjust as needed
            Assert.Equal(117u, playerCreateInfoActions[0].Action); // Example action, adjust as needed
            Assert.Equal(128u, playerCreateInfoActions[0].Type); // Example type, adjust as needed
        }

        public static void TestGetPlayerCreateInfoItems()
        {
            int count = MangosRepository.GetRowCountForTable("playercreateinfo_item");
            List<PlayerCreateInfoItem> playerCreateInfoItems = MangosRepository.GetPlayerCreateInfoItems();

            Assert.Equal(count, playerCreateInfoItems.Count);
        }

        public static void TestGetPlayerCreateInfoSpells()
        {
            int count = MangosRepository.GetRowCountForTable("playercreateinfo_spell");
            List<PlayerCreateInfoSpell> playerCreateInfoSpells = MangosRepository.GetPlayerCreateInfoSpells();

            Assert.Equal(count, playerCreateInfoSpells.Count);

            // Test the first player create info spell entry
            Assert.Equal(1u, playerCreateInfoSpells[0].Race); // Example race, adjust as needed
            Assert.Equal(1u, playerCreateInfoSpells[0].Class); // Example class, adjust as needed
            Assert.Equal(78u, playerCreateInfoSpells[0].Spell); // Example spell ID, adjust as needed
            Assert.Equal("Heroic Strike", playerCreateInfoSpells[0].Note); // Example note, adjust as needed
        }

        public static void TestGetPlayerClassLevelStats()
        {
            int count = MangosRepository.GetRowCountForTable("player_classlevelstats");
            List<PlayerClassLevelStats> classLevelStats = MangosRepository.GetPlayerClassLevelStats();

            Assert.Equal(count, classLevelStats.Count);

            // Test the first entry in the list
            Assert.Equal(1u, classLevelStats[0].Class);  // Example class, adjust as needed
            Assert.Equal(1u, classLevelStats[0].Level);  // Example level, adjust as needed
            Assert.Equal(20u, classLevelStats[0].Basehp);  // Example base HP, adjust as needed
            Assert.Equal(0u, classLevelStats[0].Basemana);  // Example base mana, adjust as needed
        }

        public static void TestGetPlayerFactionChangeItems()
        {
            int count = MangosRepository.GetRowCountForTable("player_factionchange_items");
            List<PlayerFactionChangeItems> factionChangeItems = MangosRepository.GetPlayerFactionChangeItems();

            Assert.Equal(count, factionChangeItems.Count);

            // Test the first entry in the list
            Assert.Equal(15196, factionChangeItems[0].AllianceId);  // Example Alliance ID, adjust as needed
            Assert.Equal(15197, factionChangeItems[0].HordeId);  // Example Horde ID, adjust as needed
            Assert.Equal("Scout's Tabard - Private's Tabard", factionChangeItems[0].Comment);  // Example comment, adjust as needed
        }

        public static void TestGetPlayerFactionChangeMounts()
        {
            int count = MangosRepository.GetRowCountForTable("player_factionchange_mounts");
            // Call the method to retrieve faction change mounts
            List<PlayerFactionChangeMounts> factionChangeMounts = MangosRepository.GetPlayerFactionChangeMounts();

            Assert.Equal(count, factionChangeMounts.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(1, factionChangeMounts[0].RaceId);  // Example RaceId, adjust as needed
            Assert.Equal(0, factionChangeMounts[0].MountNum);  // Example MountNum, adjust as needed
            Assert.Equal(2414, factionChangeMounts[0].ItemEntry);  // Example ItemEntry, adjust as needed
            Assert.Equal("Pinto", factionChangeMounts[0].Comment);  // Example Comment, adjust as needed
        }

        public static void TestGetPlayerFactionChangeQuests()
        {
            int count = MangosRepository.GetRowCountForTable("player_factionchange_quests");
            // Call the method to retrieve faction change quests
            List<PlayerFactionChangeQuests> factionChangeQuests = MangosRepository.GetPlayerFactionChangeQuests();

            Assert.Equal(count, factionChangeQuests.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(26, factionChangeQuests[0].AllianceId);  // Example AllianceId, adjust as needed
            Assert.Equal(27, factionChangeQuests[0].HordeId);  // Example HordeId, adjust as needed
            Assert.Equal("A Lesson to Learn", factionChangeQuests[0].Comment);  // Example Comment, adjust as needed
        }

        public static void TestGetPlayerFactionChangeReputations()
        {
            int count = MangosRepository.GetRowCountForTable("player_factionchange_reputations");
            // Call the method to retrieve faction change quests
            List<PlayerFactionChangeReputations> factionChangeReputations = MangosRepository.GetPlayerFactionChangeReputations();

            Assert.Equal(count, factionChangeReputations.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(47, factionChangeReputations[0].AllianceId);  // Example AllianceId, adjust as needed
            Assert.Equal(530, factionChangeReputations[0].HordeId);  // Example HordeId, adjust as needed
        }

        public static void TestGetPlayerFactionChangeSpells()
        {
            int count = MangosRepository.GetRowCountForTable("player_factionchange_spells");
            // Call the method to retrieve faction change spells
            List<PlayerFactionChangeSpells> factionChangeSpells = MangosRepository.GetPlayerFactionChangeSpells();

            Assert.Equal(count, factionChangeSpells.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(458, factionChangeSpells[0].AllianceId);  // Example AllianceId, adjust as needed
            Assert.Equal(6654, factionChangeSpells[0].HordeId);  // Example HordeId, adjust as needed
            Assert.Equal(string.Empty, factionChangeSpells[0].Comment);  // Example comment, adjust as needed
        }

        public static void TestGetPlayerLevelStats()
        {
            int count = MangosRepository.GetRowCountForTable("player_levelstats");
            // Call the method to retrieve player level stats
            List<PlayerLevelStats> levelStats = MangosRepository.GetPlayerLevelStats();

            Assert.Equal(count, levelStats.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(1u, levelStats[0].Race);  // Example Race, adjust as needed
            Assert.Equal(1u, levelStats[0].Class);  // Example Class, adjust as needed
            Assert.Equal(1u, levelStats[0].Level);  // Example Level, adjust as needed
            Assert.Equal(23u, levelStats[0].Str);  // Example Strength, adjust as needed
            Assert.Equal(20u, levelStats[0].Agi);  // Example Agility, adjust as needed
            Assert.Equal(22u, levelStats[0].Sta);  // Example Stamina, adjust as needed
            Assert.Equal(20u, levelStats[0].Inte);  // Example Intellect, adjust as needed
            Assert.Equal(21u, levelStats[0].Spi);  // Example Spirit, adjust as needed
        }

        public static void TestGetPlayerXpForLevel()
        {
            int count = MangosRepository.GetRowCountForTable("player_xp_for_level");
            // Call the method to retrieve player XP for levels
            List<PlayerXpForLevel> xpForLevels = MangosRepository.GetPlayerXpForLevel();

            Assert.Equal(count, xpForLevels.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(1u, xpForLevels[0].Lvl);  // Example level, adjust as needed
            Assert.Equal(400u, xpForLevels[0].XpForNextLevel);  // Example XP for next level, adjust as needed
        }

        public static void TestGetPointsOfInterest()
        {
            int count = MangosRepository.GetRowCountForTable("points_of_interest");
            // Call the method to retrieve points of interest
            List<PointsOfInterest> pointsOfInterest = MangosRepository.GetPointsOfInterest();

            Assert.Equal(count, pointsOfInterest.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(1u, pointsOfInterest[0].Entry);  // Example entry value, adjust as needed
            Assert.Equal(-9459f, pointsOfInterest[0].X);  // Example X coordinate, adjust as needed
            Assert.Equal(42.0805f, pointsOfInterest[0].Y);  // Example Y coordinate, adjust as needed
            Assert.Equal(6u, pointsOfInterest[0].Icon);  // Example icon, adjust as needed
            Assert.Equal(99u, pointsOfInterest[0].Flags);  // Example flags, adjust as needed
            Assert.Equal(0u, pointsOfInterest[0].Data);  // Example data, adjust as needed
            Assert.Equal("Lion's Pride Inn", pointsOfInterest[0].IconName);  // Example icon name, adjust as needed
        }

        public static void TestGetPoolCreatures()
        {
            int count = MangosRepository.GetRowCountForTable("pool_creature");
            // Call the method to retrieve pool creatures
            List<PoolCreature> poolCreatures = MangosRepository.GetPoolCreatures();

            Assert.Equal(count, poolCreatures.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(1068500u, poolCreatures[0].Guid);  // Example GUID value, adjust as needed
            Assert.Equal(32901u, poolCreatures[0].PoolEntry);  // Example pool entry, adjust as needed
            Assert.Equal(15f, poolCreatures[0].Chance);  // Example chance value, adjust as needed
            Assert.Equal("Skul", poolCreatures[0].Description);  // Example description, adjust as needed
            Assert.Equal(0u, poolCreatures[0].Flags);  // Example flags, adjust as needed
            Assert.Equal(0u, poolCreatures[0].PatchMin);  // Example patch min, adjust as needed
            Assert.Equal(10u, poolCreatures[0].PatchMax);  // Example patch max, adjust as needed
        }

        public static void TestGetPoolCreatureTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("pool_creature_template");
            // Call the method to retrieve pool creature templates
            List<PoolCreatureTemplate> poolCreatureTemplates = MangosRepository.GetPoolCreatureTemplates();

            Assert.Equal(count, poolCreatureTemplates.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(15466u, poolCreatureTemplates[0].Id);  // Example ID value, adjust as needed
            Assert.Equal(14006u, poolCreatureTemplates[0].PoolEntry);  // Example pool entry, adjust as needed
            Assert.Equal(0.0f, poolCreatureTemplates[0].Chance);  // Example chance value, adjust as needed
            Assert.Equal("Minion of Omen", poolCreatureTemplates[0].Description);  // Example description, adjust as needed
            Assert.Equal(0u, poolCreatureTemplates[0].Flags);  // Example flags, adjust as needed
        }

        public static void TestGetPoolGameObjects()
        {
            int count = MangosRepository.GetRowCountForTable("pool_gameobject");
            // Call the method to retrieve pool game objects
            List<PoolGameObject> poolGameObjects = MangosRepository.GetPoolGameObjects();

            Assert.Equal(count, poolGameObjects.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(96u, poolGameObjects[0].Guid);  // Example GUID value, adjust as needed
            Assert.Equal(1u, poolGameObjects[0].PoolEntry);  // Example pool entry, adjust as needed
            Assert.Equal(0.0f, poolGameObjects[0].Chance);  // Example chance value, adjust as needed
            Assert.Equal("Firefin Snapper School", poolGameObjects[0].Description);  // Example description, adjust as needed
            Assert.Equal(0u, poolGameObjects[0].Flags);  // Example flags value, adjust as needed
            Assert.Equal(0u, poolGameObjects[0].PatchMin);  // Example patch min, adjust as needed
            Assert.Equal(10u, poolGameObjects[0].PatchMax);  // Example patch max, adjust as needed
        }

        public static void TestGetPoolGameObjectTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("pool_gameobject_template");
            // Call the method to retrieve pool game object templates
            List<PoolGameObjectTemplate> poolGameObjectTemplates = MangosRepository.GetPoolGameObjectTemplates();

            Assert.Equal(count, poolGameObjectTemplates.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(180753u, poolGameObjectTemplates[0].Id);  // Example ID value, adjust as needed
            Assert.Equal(42905u, poolGameObjectTemplates[0].PoolEntry);  // Example pool entry, adjust as needed
            Assert.Equal(0.0f, poolGameObjectTemplates[0].Chance);  // Example chance value, adjust as needed
            Assert.Equal("Patch of Elemental Water", poolGameObjectTemplates[0].Description);  // Example description, adjust as needed
            Assert.Equal(0u, poolGameObjectTemplates[0].Flags);  // Example flags value, adjust as needed
        }

        public static void TestGetPoolPools()
        {
            int count = MangosRepository.GetRowCountForTable("pool_pool");
            // Call the method to retrieve pool pools
            List<PoolPool> poolPools = MangosRepository.GetPoolPools();

            Assert.Equal(count, poolPools.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(42907u, poolPools[0].PoolId);  // Example Pool ID, adjust as needed
            Assert.Equal(42906u, poolPools[0].MotherPool);  // Example Mother Pool ID, adjust as needed
            Assert.Equal(50, poolPools[0].Chance);  // Example Chance value, adjust as needed
            Assert.Equal("BRD : Warder Stilgiss", poolPools[0].Description);  // Example Description, adjust as needed
            Assert.Equal(0u, poolPools[0].Flags);  // Example Flags value, adjust as needed
        }

        public static void TestGetPoolTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("pool_template");
            // Call the method to retrieve pool templates
            List<PoolTemplate> poolTemplates = MangosRepository.GetPoolTemplates();

            Assert.Equal(count, poolTemplates.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(1u, poolTemplates[0].Entry);  // Example Entry ID, adjust as needed
            Assert.Equal(1u, poolTemplates[0].MaxLimit);  // Example MaxLimit, adjust as needed
            Assert.Equal("Fish Node", poolTemplates[0].Description);  // Example Description, adjust as needed
            Assert.Equal(0u, poolTemplates[0].Flags);  // Example Flags value, adjust as needed
            Assert.Equal(0u, poolTemplates[0].Instance);  // Example Instance, adjust as needed
            Assert.Equal(0u, poolTemplates[0].PatchMin);  // Example PatchMin value, adjust as needed
            Assert.Equal(10u, poolTemplates[0].PatchMax);  // Example PatchMax value, adjust as needed
        }

        public static void TestGetQuestEndScripts()
        {
            int count = MangosRepository.GetRowCountForTable("quest_end_scripts");
            // Call the method to retrieve quest end scripts
            List<QuestEndScripts> questEndScripts = MangosRepository.GetQuestEndScripts();

            Assert.Equal(count, questEndScripts.Count);

            // Test the first entry in the list (Adjust expected values to match actual data)
            Assert.Equal(3364u, questEndScripts[0].Id);  // Example ID, adjust as needed
            Assert.Equal(8u, questEndScripts[0].Delay);  // Example Delay, adjust as needed
            Assert.Equal(0u, questEndScripts[0].Command);  // Example Command, adjust as needed
            Assert.Equal(0u, questEndScripts[0].Datalong);  // Example Datalong, adjust as needed
            Assert.Equal(0u, questEndScripts[0].TargetType);  // Example TargetType, adjust as needed
            Assert.Equal(string.Empty, questEndScripts[0].Comments);  // Example Comment, adjust as needed
        }

        public static void TestGetQuestGreeting()
        {
            int count = MangosRepository.GetRowCountForTable("quest_greeting");
            List<QuestGreeting> questGreetings = MangosRepository.GetQuestGreeting();

            Assert.Equal(count, questGreetings.Count);

            var firstQuestGreeting = questGreetings[0];

            Assert.Equal(823u, firstQuestGreeting.Entry);
            Assert.Equal(0u, firstQuestGreeting.Type);
            Assert.Equal("Hello there, $c.  Normally I'd be out on the beat looking after the folk of Stormwind, but a lot of the Stormwind guards are fighting in the other lands.  So here I am, deputized and offering bounties when I'd rather be on patrol...", firstQuestGreeting.ContentDefault);
            Assert.Equal(string.Empty, firstQuestGreeting.ContentLoc1);
            Assert.Equal(string.Empty, firstQuestGreeting.ContentLoc2);
            Assert.Equal("Guten Tag, $C. Normalerweise würde ich jetzt meine Runde machen und die Leute von Sturmwind beschützen, doch viele der Wachen von Sturmwind kämpfen in fremden Landen. Daher mache ich jetzt hier Vertretung und setze Kopfgelder aus, wo ich doch eigentlich lieber auf Patrouille sein würde...", firstQuestGreeting.ContentLoc3);
            Assert.Equal(1u, firstQuestGreeting.Emote);
            Assert.Equal(0u, firstQuestGreeting.EmoteDelay);
        }

        public static void TestGetQuestStartScripts()
        {
            int count = MangosRepository.GetRowCountForTable("quest_start_scripts");
            List<QuestStartScripts> questStartScripts = MangosRepository.GetQuestStartScripts();

            Assert.Equal(count, questStartScripts.Count);

            var firstQuestStartScript = questStartScripts[0];

            Assert.Equal(947u, firstQuestStartScript.Id);
            Assert.Equal(4u, firstQuestStartScript.Delay);
            Assert.Equal(0u, firstQuestStartScript.Command);
            Assert.Equal(0u, firstQuestStartScript.Datalong);
            Assert.Equal(0u, firstQuestStartScript.Datalong2);
            Assert.Equal(0u, firstQuestStartScript.Datalong3);
            Assert.Equal(0u, firstQuestStartScript.Datalong4);
            Assert.Equal(0u, firstQuestStartScript.TargetParam1);
            Assert.Equal(0u, firstQuestStartScript.TargetParam2);
            Assert.Equal(0u, firstQuestStartScript.TargetType);
            Assert.Equal(0u, firstQuestStartScript.DataFlags);
            Assert.Equal(1210, firstQuestStartScript.Dataint);
            Assert.Equal(0, firstQuestStartScript.Dataint2);
            Assert.Equal(0, firstQuestStartScript.Dataint3);
            Assert.Equal(0, firstQuestStartScript.Dataint4);
            Assert.Equal(0.0f, firstQuestStartScript.X);
            Assert.Equal(0.0f, firstQuestStartScript.Y);
            Assert.Equal(0.0f, firstQuestStartScript.Z);
            Assert.Equal(0.0f, firstQuestStartScript.O);
            Assert.Equal(0u, firstQuestStartScript.ConditionId);
            Assert.Equal("Quest Cave Mushrooms, Say 2nd Line", firstQuestStartScript.Comments);
        }

        public static void TestGetQuestTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("quest_template");
            List<QuestTemplate> questTemplates = MangosRepository.GetQuestTemplates();

            Assert.Equal(count, questTemplates.Count);

            var firstQuestTemplate = questTemplates[0];

            Assert.Equal(5u, firstQuestTemplate.Entry);
            Assert.Equal(0u, firstQuestTemplate.Patch);
            Assert.Equal(2u, firstQuestTemplate.Method);
            Assert.Equal(10, firstQuestTemplate.ZoneOrSort);
            Assert.Equal(17u, firstQuestTemplate.MinLevel);
            Assert.Equal(0u, firstQuestTemplate.MaxLevel);
            Assert.Equal(20u, firstQuestTemplate.QuestLevel);
            Assert.Equal(0u, firstQuestTemplate.Type);
            Assert.Equal(0u, firstQuestTemplate.RequiredClasses);
            Assert.Equal(77u, firstQuestTemplate.RequiredRaces);
            Assert.Equal(0u, firstQuestTemplate.RequiredSkill);
            Assert.Equal(0u, firstQuestTemplate.RequiredSkillValue);
            Assert.Equal(0u, firstQuestTemplate.RepObjectiveFaction);
            Assert.Equal(0, firstQuestTemplate.RepObjectiveValue);
            Assert.Equal(0u, firstQuestTemplate.RequiredMinRepFaction);
            Assert.Equal(0, firstQuestTemplate.RequiredMinRepValue);
            Assert.Equal(0u, firstQuestTemplate.RequiredMaxRepFaction);
            Assert.Equal(0, firstQuestTemplate.RequiredMaxRepValue);
            Assert.Equal(0u, firstQuestTemplate.SuggestedPlayers);
            Assert.Equal(0u, firstQuestTemplate.LimitTime);
            Assert.Equal(8u, firstQuestTemplate.QuestFlags);
            Assert.Equal(0u, firstQuestTemplate.SpecialFlags);
            Assert.Equal(163, firstQuestTemplate.PrevQuestId);
            Assert.Equal(0, firstQuestTemplate.NextQuestId);
            Assert.Equal(0, firstQuestTemplate.ExclusiveGroup);
            Assert.Equal(93u, firstQuestTemplate.NextQuestInChain);
            Assert.Equal(0u, firstQuestTemplate.SrcItemId);
            Assert.Equal(0u, firstQuestTemplate.SrcItemCount);
            Assert.Equal(0u, firstQuestTemplate.SrcSpell);
            Assert.Equal("Jitters' Growling Gut", firstQuestTemplate.Title);
            Assert.Equal("I've been stuck hiding in this ghost town for weeks, and I haven't eaten anything but grubs and weeds!  I need some decent food, and I'm willing to trade well for it.$B$BBring me a feast and I'll pay you handsomely.$B$BI heard Chef Grual at the Scarlet Raven Tavern in Darkshire makes very good Dusky Crab Cakes...", firstQuestTemplate.Details);
            Assert.Equal("Speak with Chef Grual.", firstQuestTemplate.Objectives);
            Assert.Equal("You need some Crab Cakes, do you? Well I might be able to cook some up for you...", firstQuestTemplate.OfferRewardText);
            Assert.Equal("Oh, a shipment from my brother? Splendid! Fortune truly shines on me today!", firstQuestTemplate.RequestItemsText);
            Assert.Equal(string.Empty, firstQuestTemplate.EndText);
            Assert.Equal(string.Empty, firstQuestTemplate.ObjectiveText1);
            Assert.Equal(string.Empty, firstQuestTemplate.ObjectiveText2);
            Assert.Equal(string.Empty, firstQuestTemplate.ObjectiveText3);
            Assert.Equal(string.Empty, firstQuestTemplate.ObjectiveText4);
            Assert.Equal(0u, firstQuestTemplate.ReqItemId1);
            Assert.Equal(0u, firstQuestTemplate.ReqItemId2);
            Assert.Equal(0u, firstQuestTemplate.ReqItemId3);
            Assert.Equal(0u, firstQuestTemplate.ReqItemId4);
            Assert.Equal(0u, firstQuestTemplate.ReqItemCount1);
            Assert.Equal(0u, firstQuestTemplate.ReqItemCount2);
            Assert.Equal(0u, firstQuestTemplate.ReqItemCount3);
            Assert.Equal(0u, firstQuestTemplate.ReqItemCount4);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceId1);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceId2);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceId3);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceId4);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceCount1);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceCount2);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceCount3);
            Assert.Equal(0u, firstQuestTemplate.ReqSourceCount4);
            Assert.Equal(0, firstQuestTemplate.ReqCreatureOrGoId1);
            Assert.Equal(0, firstQuestTemplate.ReqCreatureOrGoId2);
            Assert.Equal(0, firstQuestTemplate.ReqCreatureOrGoId3);
            Assert.Equal(0, firstQuestTemplate.ReqCreatureOrGoId4);
            Assert.Equal(0u, firstQuestTemplate.ReqCreatureOrGoCount1);
            Assert.Equal(0u, firstQuestTemplate.ReqCreatureOrGoCount2);
            Assert.Equal(0u, firstQuestTemplate.ReqCreatureOrGoCount3);
            Assert.Equal(0u, firstQuestTemplate.ReqCreatureOrGoCount4);
            Assert.Equal(0u, firstQuestTemplate.ReqSpellCast1);
            Assert.Equal(0u, firstQuestTemplate.ReqSpellCast2);
            Assert.Equal(0u, firstQuestTemplate.ReqSpellCast3);
            Assert.Equal(0u, firstQuestTemplate.ReqSpellCast4);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemId1);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemId2);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemId3);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemId4);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemId5);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemId6);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemCount1);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemCount2);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemCount3);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemCount4);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemCount5);
            Assert.Equal(0u, firstQuestTemplate.RewChoiceItemCount6);
            Assert.Equal(0u, firstQuestTemplate.RewItemId1);
            Assert.Equal(0u, firstQuestTemplate.RewItemId2);
            Assert.Equal(0u, firstQuestTemplate.RewItemId3);
            Assert.Equal(0u, firstQuestTemplate.RewItemId4);
            Assert.Equal(0u, firstQuestTemplate.RewItemCount1);
            Assert.Equal(0u, firstQuestTemplate.RewItemCount2);
            Assert.Equal(0u, firstQuestTemplate.RewItemCount3);
            Assert.Equal(0u, firstQuestTemplate.RewItemCount4);
            Assert.Equal(72u, firstQuestTemplate.RewRepFaction1);
            Assert.Equal(0u, firstQuestTemplate.RewRepFaction2);
            Assert.Equal(0u, firstQuestTemplate.RewRepFaction3);
            Assert.Equal(0u, firstQuestTemplate.RewRepFaction4);
            Assert.Equal(0u, firstQuestTemplate.RewRepFaction5);
            Assert.Equal(25, firstQuestTemplate.RewRepValue1);
            Assert.Equal(0, firstQuestTemplate.RewRepValue2);
            Assert.Equal(0, firstQuestTemplate.RewRepValue3);
            Assert.Equal(0, firstQuestTemplate.RewRepValue4);
            Assert.Equal(0, firstQuestTemplate.RewRepValue5);
            Assert.Equal(0, firstQuestTemplate.RewOrReqMoney);
            Assert.Equal(234u, firstQuestTemplate.RewMoneyMaxLevel);
            Assert.Equal(0u, firstQuestTemplate.RewSpell);
            Assert.Equal(0u, firstQuestTemplate.RewSpellCast);
            Assert.Equal(0u, firstQuestTemplate.RewMailTemplateId);
            Assert.Equal(0u, firstQuestTemplate.RewMailDelaySecs);
            Assert.Equal(0u, firstQuestTemplate.PointMapId);
            Assert.Equal(0.0f, firstQuestTemplate.PointX);
            Assert.Equal(0.0f, firstQuestTemplate.PointY);
            Assert.Equal(0u, firstQuestTemplate.PointOpt);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmote1);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmote2);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmote3);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmote4);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmoteDelay1);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmoteDelay2);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmoteDelay3);
            Assert.Equal(0u, firstQuestTemplate.DetailsEmoteDelay4);
            Assert.Equal(0u, firstQuestTemplate.IncompleteEmote);
            Assert.Equal(1u, firstQuestTemplate.CompleteEmote);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmote1);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmote2);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmote3);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmote4);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmoteDelay1);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmoteDelay2);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmoteDelay3);
            Assert.Equal(0u, firstQuestTemplate.OfferRewardEmoteDelay4);
            Assert.Equal(0u, firstQuestTemplate.StartScript);
            Assert.Equal(0u, firstQuestTemplate.CompleteScript);
        }


        public static void TestGetReferenceLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("reference_loot_template");
            // Act
            List<ReferenceLootTemplate> lootTemplates = MangosRepository.GetReferenceLootTemplates();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, lootTemplates.Count); // Replace with the expected count of loot templates

            // Validate the first reference loot template (Adjust values according to your expected data)
            var firstLootTemplate = lootTemplates[0];

            Assert.Equal(12002u, firstLootTemplate.Entry);              // Replace with expected entry value
            Assert.Equal(22351u, firstLootTemplate.Item);               // Replace with expected item value
            Assert.Equal(0.0f, firstLootTemplate.ChanceOrQuestChance); // Replace with expected ChanceOrQuestChance value
            Assert.Equal(1u, firstLootTemplate.GroupId);              // Replace with expected groupid value
            Assert.Equal(1, firstLootTemplate.MinCountOrRef);         // Replace with expected mincountOrRef value
            Assert.Equal(1u, firstLootTemplate.MaxCount);             // Replace with expected maxcount value
            Assert.Equal(0u, firstLootTemplate.ConditionId);          // Replace with expected condition_id value
            Assert.Equal(0u, firstLootTemplate.PatchMin);             // Replace with expected patch_min value
            Assert.Equal(10u, firstLootTemplate.PatchMax);             // Replace with expected patch_max value
        }

        public static void TestGetReputationRewardRates()
        {
            int count = MangosRepository.GetRowCountForTable("reputation_reward_rate");
            // Act
            List<ReputationRewardRate> reputationRewardRates = MangosRepository.GetReputationRewardRates();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, reputationRewardRates.Count); // Replace with the expected number of spillover templates
        }

        public static void TestGetReputationSpilloverTemplate()
        {
            int count = MangosRepository.GetRowCountForTable("reputation_spillover_template");
            // Act
            List<ReputationSpilloverTemplate> spilloverTemplates = MangosRepository.GetReputationSpilloverTemplate();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, spilloverTemplates.Count); // Replace with the expected number of spillover templates

            // Validate the first reputation spillover template (Adjust values according to your expected data)
            var firstSpilloverTemplate = spilloverTemplates[0];

            Assert.Equal(169u, firstSpilloverTemplate.Faction);        // Replace with expected faction value
            Assert.Equal(21u, firstSpilloverTemplate.Faction1);       // Replace with expected faction1 value
            Assert.Equal(1.0f, firstSpilloverTemplate.Rate1);         // Replace with expected rate_1 value
            Assert.Equal(7u, firstSpilloverTemplate.Rank1);           // Replace with expected rank_1 value
            Assert.Equal(369u, firstSpilloverTemplate.Faction2);       // Replace with expected faction2 value
            Assert.Equal(1.0f, firstSpilloverTemplate.Rate2);         // Replace with expected rate_2 value
            Assert.Equal(7u, firstSpilloverTemplate.Rank2);           // Replace with expected rank_2 value
            Assert.Equal(470u, firstSpilloverTemplate.Faction3);       // Replace with expected faction3 value
            Assert.Equal(1.0f, firstSpilloverTemplate.Rate3);         // Replace with expected rate_3 value
            Assert.Equal(7u, firstSpilloverTemplate.Rank3);           // Replace with expected rank_3 value
            Assert.Equal(577u, firstSpilloverTemplate.Faction4);       // Replace with expected faction4 value
            Assert.Equal(1.0f, firstSpilloverTemplate.Rate4);         // Replace with expected rate_4 value
            Assert.Equal(7u, firstSpilloverTemplate.Rank4);           // Replace with expected rank_4 value
        }

        public static void TestGetReservedNames()
        {
            int count = MangosRepository.GetRowCountForTable("reserved_name");
            // Act
            List<string> reservedNames = MangosRepository.GetReservedNames();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, reservedNames.Count); // Replace with the expected number of reserved names

            // Validate the first reserved name (Adjust values according to your expected data)
            Assert.Equal("Admin", reservedNames[0]);  // Replace with the actual expected name
            Assert.Equal("Administrato", reservedNames[1]);  // Replace with the actual expected name
            Assert.Equal("Blizzard", reservedNames[2]); // Replace with the actual expected name
        }

        public static void TestGetScriptedAreaTriggers()
        {
            int count = MangosRepository.GetRowCountForTable("scripted_areatrigger");
            // Act
            List<ScriptedAreatrigger> areaTriggers = MangosRepository.GetScriptedAreaTriggers();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, areaTriggers.Count); // Replace with the expected number of area triggers

            // Validate the first area trigger (Adjust values according to your expected data)
            var firstTrigger = areaTriggers[0];

            Assert.Equal(522u, firstTrigger.Entry);              // Replace with expected entry value
            Assert.Equal("at_twiggy_flathead", firstTrigger.ScriptName); // Replace with expected script name
        }

        public static void TestGetScriptedEvents()
        {
            int count = MangosRepository.GetRowCountForTable("scripted_event_id");
            // Act
            List<ScriptedEventId> events = MangosRepository.GetScriptedEvents();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, events.Count); // Replace with the expected number of scripted events

            // Validate the first scripted event (Adjust values according to your expected data)
            var firstEvent = events[0];

            Assert.Equal(8420u, firstEvent.Id);                    // Replace with expected id value
            Assert.Equal("event_dreadsteed_ritual_start", firstEvent.ScriptName);   // Replace with expected script name
        }

        public static void TestGetScriptEscortData()
        {
            int count = MangosRepository.GetRowCountForTable("script_escort_data");
            // Act
            List<ScriptEscortData> escortDataList = MangosRepository.GetScriptEscortData();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, escortDataList.Count); // Replace with the expected number of script texts

            // Validate the first escort data entry (Adjust values according to your expected data)
            var firstEscortData = escortDataList[0];

            Assert.Equal(9023u, firstEscortData.CreatureId);       // Replace with expected creature_id value
            Assert.Equal(4322u, firstEscortData.Quest);            // Replace with expected quest value
            Assert.Equal(11u, firstEscortData.EscortFaction);    // Replace with expected escort_faction value
        }

        public static void TestGetScriptTexts()
        {
            int count = MangosRepository.GetRowCountForTable("script_texts");
            // Act
            List<ScriptText> scriptTextList = MangosRepository.GetScriptTexts();

            // Assert - Adjust this value based on your expected data size
            Assert.Equal(count, scriptTextList.Count); // Replace with the expected number of script texts

            // Validate the first script text (Adjust values according to your expected data)
            var firstScriptText = scriptTextList[0];

            Assert.Equal(-1000000, firstScriptText.Entry);                               // Replace with expected entry value
            Assert.Equal("<ScriptDev2 Text Entry Missing!>", firstScriptText.ContentDefault);   // Replace with expected content_default value
            Assert.Equal(string.Empty, firstScriptText.ContentLoc1);       // Replace with expected content_loc1 value
            Assert.Equal("<Texte ScriptDev2 introuvable !>", firstScriptText.ContentLoc2);             // Adjust based on expected data
            Assert.Equal(string.Empty, firstScriptText.ContentLoc3);       // Replace with expected content_loc3 value
            Assert.Equal(string.Empty, firstScriptText.ContentLoc4);             // Adjust based on expected data
            Assert.Equal(string.Empty, firstScriptText.ContentLoc5);       // Replace with expected content_loc5 value
            Assert.Equal(string.Empty, firstScriptText.ContentLoc6);             // Adjust based on expected data
            Assert.Equal(string.Empty, firstScriptText.ContentLoc7);       // Replace with expected content_loc7 value
            Assert.Equal(string.Empty, firstScriptText.ContentLoc8);             // Adjust based on expected data
            Assert.Equal(0u, firstScriptText.Sound);                           // Replace with expected sound value
            Assert.Equal(0u, firstScriptText.Type);                              // Replace with expected type value
            Assert.Equal(0u, firstScriptText.Language);                          // Replace with expected language value
            Assert.Equal(0u, firstScriptText.Emote);                             // Replace with expected emote value
        }

        public static void TestGetScriptWaypoints()
        {
            int count = MangosRepository.GetRowCountForTable("script_waypoint");
            List<ScriptWaypoint> waypoints = MangosRepository.GetScriptWaypoints();

            Assert.Equal(count, waypoints.Count);

            var firstWaypoint = waypoints[0];

            Assert.Equal(467u, firstWaypoint.Entry);
            Assert.Equal(0u, firstWaypoint.Pointid);
            Assert.Equal(-10508.4f, firstWaypoint.LocationX);
            Assert.Equal(1068.0f, firstWaypoint.LocationY);
            Assert.Equal(55.21f, firstWaypoint.LocationZ);
            Assert.Equal(0u, firstWaypoint.Waittime);
            Assert.Equal(string.Empty, firstWaypoint.PointComment);
        }

        public static void TestGetSkillDiscoveryTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("skill_discovery_template");
            List<SkillDiscoveryTemplate> skillDiscoveries = MangosRepository.GetSkillDiscoveryTemplates();

            Assert.Equal(count, skillDiscoveries.Count);
        }

        public static void TestGetSkillExtraItemTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("skill_extra_item_template");
            List<SkillExtraItemTemplate> skillExtraItems = MangosRepository.GetSkillExtraItemTemplates();

            Assert.Equal(count, skillExtraItems.Count);
        }

        public static void TestGetSkillFishingBaseLevels()
        {
            int count = MangosRepository.GetRowCountForTable("skill_fishing_base_level");
            List<SkillFishingBaseLevel> fishingBaseLevels = MangosRepository.GetSkillFishingBaseLevels();

            Assert.Equal(count, fishingBaseLevels.Count);

            var firstFishingBaseLevel = fishingBaseLevels[0];

            Assert.Equal(1u, firstFishingBaseLevel.Entry);
            Assert.Equal(-70, firstFishingBaseLevel.Skill);
        }

        public static void TestGetSkinningLootTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("skinning_loot_template");
            List<SkinningLootTemplate> skinningLootTemplates = MangosRepository.GetSkinningLootTemplates();

            Assert.Equal(count, skinningLootTemplates.Count);

            var firstLootTemplate = skinningLootTemplates[0];

            Assert.Equal(113u, firstLootTemplate.Entry);
            Assert.Equal(2318u, firstLootTemplate.Item);
            Assert.Equal(39.2962f, firstLootTemplate.ChanceOrQuestChance);
            Assert.Equal(1u, firstLootTemplate.GroupId);
            Assert.Equal(1, firstLootTemplate.MinCountOrRef);
            Assert.Equal(1u, firstLootTemplate.MaxCount);
            Assert.Equal(0u, firstLootTemplate.ConditionId);
            Assert.Equal(0u, firstLootTemplate.PatchMin);
            Assert.Equal(10u, firstLootTemplate.PatchMax);
        }

        public static void TestGetSoundEntries()
        {
            int count = MangosRepository.GetRowCountForTable("sound_entries");
            List<SoundEntries> soundEntries = MangosRepository.GetSoundEntries();

            Assert.Equal(count, soundEntries.Count);

            var firstSoundEntry = soundEntries[0];

            Assert.Equal(3u, firstSoundEntry.Id);
            Assert.Equal("Invisibility Impact", firstSoundEntry.Name);
        }

        public static void TestGetSpellAffect()
        {
            int count = MangosRepository.GetRowCountForTable("spell_affect");
            List<SpellAffect> spellAffects = MangosRepository.GetSpellAffect();

            Assert.Equal(count, spellAffects.Count);

            var firstSpellAffect = spellAffects[0];

            Assert.Equal(11083u, firstSpellAffect.Entry);
            Assert.Equal(0u, firstSpellAffect.EffectId);
            Assert.Equal(12714007uL, firstSpellAffect.SpellFamilyMask);
        }

        public static void TestGetSpellAreas()
        {
            int count = MangosRepository.GetRowCountForTable("spell_area");
            List<SpellArea> spellAreas = MangosRepository.GetSpellAreas();

            Assert.Equal(count, spellAreas.Count);

            var firstSpellArea = spellAreas[0];

            Assert.Equal(24414u, firstSpellArea.Spell);
            Assert.Equal(3358u, firstSpellArea.Area);
            Assert.Equal(0u, firstSpellArea.QuestStart);
            Assert.Equal(0u, firstSpellArea.QuestStartActive);
            Assert.Equal(0u, firstSpellArea.QuestEnd);
            Assert.Equal(0u, firstSpellArea.AuraSpell);
            Assert.Equal(0u, firstSpellArea.Racemask);
            Assert.Equal(2u, firstSpellArea.Gender);
            Assert.Equal(0u, firstSpellArea.Autocast);
        }

        public static void TestGetSpellBonusData()
        {
            int count = MangosRepository.GetRowCountForTable("spell_bonus_data");
            List<SpellBonusData> spellBonusDataList = MangosRepository.GetSpellBonusData();

            Assert.Equal(count, spellBonusDataList.Count);

            var firstSpellBonusData = spellBonusDataList[0];

            Assert.Equal(10u, firstSpellBonusData.Entry);
            Assert.Equal(0.0f, firstSpellBonusData.DirectBonus);
            Assert.Equal(0.0416f, firstSpellBonusData.DotBonus);
            Assert.Equal(0.0f, firstSpellBonusData.ApBonus);
            Assert.Equal(0.0f, firstSpellBonusData.ApDotBonus);
            Assert.Equal("Mage - Blizzard", firstSpellBonusData.Comments);
        }

        public static void TestGetSpellChain()
        {
            int count = MangosRepository.GetRowCountForTable("spell_chain");
            List<SpellChain> spellChainList = MangosRepository.GetSpellChain();

            Assert.Equal(count, spellChainList.Count);

            var firstSpellChain = spellChainList[0];

            Assert.Equal(10u, firstSpellChain.SpellId);
            Assert.Equal(0u, firstSpellChain.PrevSpell);
            Assert.Equal(10u, firstSpellChain.FirstSpell);
            Assert.Equal(1, firstSpellChain.Rank);
            Assert.Equal(0u, firstSpellChain.ReqSpell);
        }

        public static void TestGetSpellCheck()
        {
            int count = MangosRepository.GetRowCountForTable("spell_check");
            List<SpellCheck> spellCheckList = MangosRepository.GetSpellCheck();

            Assert.Equal(count, spellCheckList.Count);

            var firstSpellCheck = spellCheckList[0];

            Assert.Equal(18788u, firstSpellCheck.Spellid);
            Assert.Equal(-1, firstSpellCheck.SpellFamilyName);
            Assert.Equal(-1L, firstSpellCheck.SpellFamilyMask);
            Assert.Equal(-1, firstSpellCheck.SpellIcon);
            Assert.Equal(-1, firstSpellCheck.SpellVisual);
            Assert.Equal(-1, firstSpellCheck.SpellCategory);
            Assert.Equal(1, firstSpellCheck.EffectType);
            Assert.Equal(-1, firstSpellCheck.EffectAura);
            Assert.Equal(-1, firstSpellCheck.EffectIdx);
            Assert.Equal("Demonic Sacrifice", firstSpellCheck.Name);
            Assert.Equal("Spell::EffectInstaKill", firstSpellCheck.Code);
        }

        public static void TestGetDisabledSpells()
        {
            int count = MangosRepository.GetRowCountForTable("spell_disabled");
            List<uint> disabledSpells = MangosRepository.GetDisabledSpells();

            Assert.Equal(count, disabledSpells.Count);

            Assert.Equal(3602u, disabledSpells[0]);
            Assert.Equal(21563u, disabledSpells[1]);
            Assert.Equal(24417u, disabledSpells[2]);
            Assert.Equal(27661u, disabledSpells[3]);
            Assert.Equal(30003u, disabledSpells[4]);
        }

        public static void TestGetSpellEffectMods()
        {
            int count = MangosRepository.GetRowCountForTable("spell_effect_mod");
            List<SpellEffectMod> spellEffectMods = MangosRepository.GetSpellEffectMods();

            Assert.Equal(count, spellEffectMods.Count);

            var firstEffectMod = spellEffectMods[0];

            Assert.Equal(2u, firstEffectMod.Id);
            Assert.Equal(0u, firstEffectMod.EffectIndex);
            Assert.Equal(10, firstEffectMod.Effect);
            Assert.Equal(-1, firstEffectMod.EffectDieSides);
            Assert.Equal(-1, firstEffectMod.EffectBaseDice);
            Assert.Equal(-1, firstEffectMod.EffectDicePerLevel);
            Assert.Equal(-1.0f, firstEffectMod.EffectRealPointsPerLevel);
            Assert.Equal(100, firstEffectMod.EffectBasePoints);
            Assert.Equal(-1, firstEffectMod.EffectAmplitude);
            Assert.Equal(-1, firstEffectMod.EffectPointsPerComboPoint);
            Assert.Equal(-1, firstEffectMod.EffectChainTarget);
            Assert.Equal(-1.0f, firstEffectMod.EffectMultipleValue);
            Assert.Equal(-1, firstEffectMod.EffectMechanic);
            Assert.Equal(1, firstEffectMod.EffectImplicitTargetA);
            Assert.Equal(1, firstEffectMod.EffectImplicitTargetB);
            Assert.Equal(-1, firstEffectMod.EffectRadiusIndex);
            Assert.Equal(-1, firstEffectMod.EffectApplyAuraName);
            Assert.Equal(-1, firstEffectMod.EffectItemType);
            Assert.Equal(0, firstEffectMod.EffectMiscValue);
            Assert.Equal(-1, firstEffectMod.EffectTriggerSpell);
            Assert.Equal(string.Empty, firstEffectMod.Comment);
        }

        public static void TestGetSpellElixirs()
        {
            int count = MangosRepository.GetRowCountForTable("spell_elixir");
            List<SpellElixir> spellElixirs = MangosRepository.GetSpellElixirs();

            Assert.Equal(count, spellElixirs.Count);

            var firstElixir = spellElixirs[0];

            Assert.Equal(17624u, firstElixir.Entry);
            Assert.Equal(0u, firstElixir.Mask);
        }

        public static void TestGetSpellFacings()
        {
            int count = MangosRepository.GetRowCountForTable("spell_facing");
            List<SpellFacing> spellFacings = MangosRepository.GetSpellFacings();

            Assert.Equal(count, spellFacings.Count);

            var firstFacing = spellFacings[0];

            Assert.Equal(53u, firstFacing.Entry);
            Assert.Equal(1u, firstFacing.Facingcasterflag);
        }

        public static void TestGetSpellGroups()
        {
            int count = MangosRepository.GetRowCountForTable("spell_group");
            List<SpellGroup> spellGroups = MangosRepository.GetSpellGroups();

            Assert.Equal(count, spellGroups.Count);

            var firstGroup = spellGroups[0];

            Assert.Equal(1001u, firstGroup.GroupId);
            Assert.Equal(0u, firstGroup.GroupSpellId);
            Assert.Equal(18125u, firstGroup.SpellId);
        }

        public static void TestGetSpellGroupStackRules()
        {
            int count = MangosRepository.GetRowCountForTable("spell_group_stack_rules");
            List<SpellGroupStackRules> spellGroupStackRules = MangosRepository.GetSpellGroupStackRules();

            Assert.Equal(count, spellGroupStackRules.Count);

            var firstRule = spellGroupStackRules[0];

            Assert.Equal(1001u, firstRule.GroupId);
            Assert.Equal(1u, firstRule.StackRule);
        }

        public static void TestGetSpellLearnSpells()
        {
            int count = MangosRepository.GetRowCountForTable("spell_learn_spell");
            List<SpellLearnSpell> spellLearnSpells = MangosRepository.GetSpellLearnSpells();

            Assert.Equal(count, spellLearnSpells.Count);

            var firstLearnSpell = spellLearnSpells[0];

            Assert.Equal(2842u, firstLearnSpell.Entry);
            Assert.Equal(8681u, firstLearnSpell.SpellId);
            Assert.Equal(1u, firstLearnSpell.Active);
        }

        public static void TestGetSpellMods()
        {
            int count = MangosRepository.GetRowCountForTable("spell_mod");
            List<SpellMod> spellMods = MangosRepository.GetSpellMods();

            Assert.Equal(count, spellMods.Count);

            var firstSpellMod = spellMods[0];

            Assert.Equal(1u, firstSpellMod.Id);
            Assert.Equal(-1, firstSpellMod.ProcChance);
            Assert.Equal(-1, firstSpellMod.ProcFlags);
            Assert.Equal(-1, firstSpellMod.ProcCharges);
            Assert.Equal(-1, firstSpellMod.DurationIndex);
            Assert.Equal(-1, firstSpellMod.Category);
            Assert.Equal(-1, firstSpellMod.CastingTimeIndex);
            Assert.Equal(-1, firstSpellMod.StackAmount);
            Assert.Equal(-1, firstSpellMod.SpellIconId);
            Assert.Equal(-1, firstSpellMod.ActiveIconId);
            Assert.Equal(-1, firstSpellMod.ManaCost);
            Assert.Equal(-1, firstSpellMod.Attributes);
            Assert.Equal(-1, firstSpellMod.AttributesEx);
            Assert.Equal(-1, firstSpellMod.AttributesEx2);
            Assert.Equal(-1, firstSpellMod.AttributesEx3);
            Assert.Equal(-1, firstSpellMod.AttributesEx4);
            Assert.Equal(0, firstSpellMod.Custom);
            Assert.Equal(-1, firstSpellMod.InterruptFlags);
            Assert.Equal(-1, firstSpellMod.AuraInterruptFlags);
            Assert.Equal(-1, firstSpellMod.ChannelInterruptFlags);
            Assert.Equal(-1, firstSpellMod.Dispel);
            Assert.Equal(-1, firstSpellMod.Stances);
            Assert.Equal(-1, firstSpellMod.StancesNot);
            Assert.Equal(-1, firstSpellMod.SpellVisual);
            Assert.Equal(-1, firstSpellMod.ManaCostPercentage);
            Assert.Equal(-1, firstSpellMod.StartRecoveryCategory);
            Assert.Equal(-1, firstSpellMod.StartRecoveryTime);
            Assert.Equal(-1, firstSpellMod.MaxAffectedTargets);
            Assert.Equal(-1, firstSpellMod.MaxTargetLevel);
            Assert.Equal(-1, firstSpellMod.DmgClass);
            Assert.Equal(-1, firstSpellMod.RangeIndex);
            Assert.Equal(-1, firstSpellMod.RecoveryTime);
            Assert.Equal(-1, firstSpellMod.CategoryRecoveryTime);
            Assert.Equal(-1, firstSpellMod.SpellFamilyName);
            Assert.Equal(0uL, firstSpellMod.SpellFamilyFlags);
            Assert.Equal(-1, firstSpellMod.Mechanic);
            Assert.Equal(-1, firstSpellMod.EquippedItemClass);
            Assert.Equal(string.Empty, firstSpellMod.Comment);
        }

        public static void TestGetSpellPetAuras()
        {
            int count = MangosRepository.GetRowCountForTable("spell_pet_auras");
            List<SpellPetAura> spellPetAuras = MangosRepository.GetSpellPetAuras();

            Assert.Equal(count, spellPetAuras.Count);

            var firstSpellPetAura = spellPetAuras[0];

            Assert.Equal(19028u, firstSpellPetAura.Spell);
            Assert.Equal(0u, firstSpellPetAura.Pet);
            Assert.Equal(25228u, firstSpellPetAura.Aura);
        }

        public static void TestGetSpellProcEvents()
        {
            int count = MangosRepository.GetRowCountForTable("spell_proc_event");
            List<SpellProcEvent> spellProcEvents = MangosRepository.GetSpellProcEvents();

            Assert.Equal(count, spellProcEvents.Count);

            var firstSpellProcEvent = spellProcEvents[0];

            Assert.Equal(324u, firstSpellProcEvent.Entry);
            Assert.Equal(0u, firstSpellProcEvent.SchoolMask);
            Assert.Equal(0u, firstSpellProcEvent.SpellFamilyName);
            Assert.Equal(0uL, firstSpellProcEvent.SpellFamilyMask0);
            Assert.Equal(0uL, firstSpellProcEvent.SpellFamilyMask1);
            Assert.Equal(0uL, firstSpellProcEvent.SpellFamilyMask2);
            Assert.Equal(0u, firstSpellProcEvent.ProcFlags);
            Assert.Equal(0u, firstSpellProcEvent.ProcEx);
            Assert.Equal(0.0f, firstSpellProcEvent.PpmRate);
            Assert.Equal(0.0f, firstSpellProcEvent.CustomChance);
            Assert.Equal(3u, firstSpellProcEvent.Cooldown);
        }

        public static void TestGetSpellProcItemEnchants()
        {
            int count = MangosRepository.GetRowCountForTable("spell_proc_item_enchant");
            List<SpellProcItemEnchant> spellProcItemEnchants = MangosRepository.GetSpellProcItemEnchants();

            Assert.Equal(count, spellProcItemEnchants.Count);

            var firstItemEnchant = spellProcItemEnchants[0];

            Assert.Equal(8034u, firstItemEnchant.Entry);
            Assert.Equal(9.0f, firstItemEnchant.PpmRate);
        }

        public static void TestGetSpellScripts()
        {
            int count = MangosRepository.GetRowCountForTable("spell_scripts");
            List<SpellScript> spellScripts = MangosRepository.GetSpellScripts();

            Assert.Equal(count, spellScripts.Count);

            var firstSpellScript = spellScripts[0];

            Assert.Equal(13982u, firstSpellScript.Id);
            Assert.Equal(0u, firstSpellScript.Delay);
            Assert.Equal(17u, firstSpellScript.Command);
            Assert.Equal(11230u, firstSpellScript.Datalong);
            Assert.Equal(1u, firstSpellScript.Datalong2);
            Assert.Equal(0u, firstSpellScript.Datalong3);
            Assert.Equal(0u, firstSpellScript.Datalong4);
            Assert.Equal(0u, firstSpellScript.TargetParam1);
            Assert.Equal(0u, firstSpellScript.TargetParam2);
            Assert.Equal(0u, firstSpellScript.TargetType);
            Assert.Equal(0u, firstSpellScript.DataFlags);
            Assert.Equal(0, firstSpellScript.Dataint);
            Assert.Equal(0, firstSpellScript.Dataint2);
            Assert.Equal(0, firstSpellScript.Dataint3);
            Assert.Equal(0, firstSpellScript.Dataint4);
            Assert.Equal(0.0f, firstSpellScript.X);
            Assert.Equal(0.0f, firstSpellScript.Y);
            Assert.Equal(0.0f, firstSpellScript.Z);
            Assert.Equal(0.0f, firstSpellScript.O);
            Assert.Equal(0u, firstSpellScript.ConditionId);
            Assert.Equal("Create object : Essence flamboyante enchâssée", firstSpellScript.Comments);
        }

        public static void TestGetSpellScriptTargets()
        {
            int count = MangosRepository.GetRowCountForTable("spell_script_target");
            List<SpellScriptTarget> spellScriptTargets = MangosRepository.GetSpellScriptTargets();

            Assert.Equal(count, spellScriptTargets.Count);

            var firstTarget = spellScriptTargets[0];

            Assert.Equal(3730u, firstTarget.Entry);
            Assert.Equal(1u, firstTarget.Type);
            Assert.Equal(15263u, firstTarget.TargetEntry);
        }

        public static void TestGetSpellTargetPositions()
        {
            int count = MangosRepository.GetRowCountForTable("spell_target_position");
            List<SpellTargetPosition> spellTargetPositions = MangosRepository.GetSpellTargetPositions();

            Assert.Equal(count, spellTargetPositions.Count);

            var firstTargetPosition = spellTargetPositions[0];

            Assert.Equal(442u, firstTargetPosition.Id);
            Assert.Equal(129u, firstTargetPosition.TargetMap);
            Assert.Equal(2592.55f, firstTargetPosition.TargetPositionX, 2);
            Assert.Equal(1107.50f, firstTargetPosition.TargetPositionY);
            Assert.Equal(51.29f, firstTargetPosition.TargetPositionZ);
            Assert.Equal(4.74f, firstTargetPosition.TargetOrientation);
        }

        public static void TestGetSpellTemplates()
        {
            int count = MangosRepository.GetRowCountForTable("spell_template");
            List<SpellTemplate> spellTemplates = MangosRepository.GetSpellTemplates();

            Assert.Equal(count, spellTemplates.Count);

            var firstSpellTemplate = spellTemplates[0];

            Assert.Equal(1u, firstSpellTemplate.Id);
            Assert.Equal(3u, firstSpellTemplate.School);
            Assert.Equal(0u, firstSpellTemplate.Category);
            Assert.Equal(0u, firstSpellTemplate.CastUi);
            Assert.Equal(0u, firstSpellTemplate.Dispel);
            Assert.Equal(0u, firstSpellTemplate.Mechanic);
            Assert.Equal(0u, firstSpellTemplate.Attributes);
            Assert.Equal(0u, firstSpellTemplate.AttributesEx);
            Assert.Equal(0u, firstSpellTemplate.AttributesEx2);
            Assert.Equal(0u, firstSpellTemplate.AttributesEx3);
            Assert.Equal(0u, firstSpellTemplate.AttributesEx4);
            Assert.Equal(0u, firstSpellTemplate.Stances);
            Assert.Equal(0u, firstSpellTemplate.StancesNot);
            Assert.Equal(0u, firstSpellTemplate.Targets);
            Assert.Equal(0u, firstSpellTemplate.TargetCreatureType);
            Assert.Equal(0u, firstSpellTemplate.RequiresSpellFocus);
            Assert.Equal(0u, firstSpellTemplate.CasterAuraState);
            Assert.Equal(0u, firstSpellTemplate.TargetAuraState);
            Assert.Equal(7u, firstSpellTemplate.CastingTimeIndex);
            Assert.Equal(0u, firstSpellTemplate.RecoveryTime);
            Assert.Equal(0u, firstSpellTemplate.CategoryRecoveryTime);
            Assert.Equal(7u, firstSpellTemplate.InterruptFlags);
            Assert.Equal(0u, firstSpellTemplate.AuraInterruptFlags);
            Assert.Equal(0u, firstSpellTemplate.ChannelInterruptFlags);
            Assert.Equal(0u, firstSpellTemplate.ProcFlags);
            Assert.Equal(101u, firstSpellTemplate.ProcChance);
            Assert.Equal(0u, firstSpellTemplate.ProcCharges);
            Assert.Equal(0u, firstSpellTemplate.MaxLevel);
            Assert.Equal(0u, firstSpellTemplate.BaseLevel);
            Assert.Equal(0u, firstSpellTemplate.SpellLevel);
            Assert.Equal(0u, firstSpellTemplate.DurationIndex);
            Assert.Equal(0u, firstSpellTemplate.PowerType);
            Assert.Equal(10u, firstSpellTemplate.ManaCost);
            Assert.Equal(0u, firstSpellTemplate.ManaCostPerLevel);
            Assert.Equal(0u, firstSpellTemplate.ManaPerSecond);
            Assert.Equal(0u, firstSpellTemplate.ManaPerSecondPerLevel);
            Assert.Equal(1u, firstSpellTemplate.RangeIndex);
            Assert.Equal(0.0f, firstSpellTemplate.Speed);
            Assert.Equal(0u, firstSpellTemplate.ModelNextSpell);
            Assert.Equal(0u, firstSpellTemplate.StackAmount);
            Assert.Equal(0u, firstSpellTemplate.Totem1);
            Assert.Equal(0u, firstSpellTemplate.Totem2);
            Assert.Equal(0u, firstSpellTemplate.Reagent1);
            Assert.Equal(0u, firstSpellTemplate.Reagent2);
            Assert.Equal(0u, firstSpellTemplate.Reagent3);
            Assert.Equal(0u, firstSpellTemplate.Reagent4);
            Assert.Equal(0u, firstSpellTemplate.Reagent5);
            Assert.Equal(0u, firstSpellTemplate.Reagent6);
            Assert.Equal(0u, firstSpellTemplate.Reagent7);
            Assert.Equal(0u, firstSpellTemplate.Reagent8);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount1);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount2);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount3);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount4);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount5);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount6);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount7);
            Assert.Equal(0u, firstSpellTemplate.ReagentCount8);
            Assert.Equal(-1, firstSpellTemplate.EquippedItemClass);
            Assert.Equal(0, firstSpellTemplate.EquippedItemSubClassMask);
            Assert.Equal(0, firstSpellTemplate.EquippedItemInventoryTypeMask);
            Assert.Equal(5u, firstSpellTemplate.Effect1);
            Assert.Equal(0u, firstSpellTemplate.Effect2);
            Assert.Equal(0u, firstSpellTemplate.Effect3);
            Assert.Equal(6, firstSpellTemplate.EffectDieSides1);
            Assert.Equal(0, firstSpellTemplate.EffectDieSides2);
            Assert.Equal(0, firstSpellTemplate.EffectDieSides3);
            Assert.Equal(0, firstSpellTemplate.EffectBaseDice1);
            Assert.Equal(0, firstSpellTemplate.EffectBaseDice2);
            Assert.Equal(0, firstSpellTemplate.EffectBaseDice3);
            Assert.Equal(0.0f, firstSpellTemplate.EffectDicePerLevel1);
            Assert.Equal(0.0f, firstSpellTemplate.EffectDicePerLevel2);
            Assert.Equal(0.0f, firstSpellTemplate.EffectDicePerLevel3);
            Assert.Equal(0.0f, firstSpellTemplate.EffectRealPointsPerLevel1);
            Assert.Equal(0.0f, firstSpellTemplate.EffectRealPointsPerLevel2);
            Assert.Equal(0.0f, firstSpellTemplate.EffectRealPointsPerLevel3);
            Assert.Equal(0, firstSpellTemplate.EffectBasePoints1);
            Assert.Equal(0, firstSpellTemplate.EffectBasePoints2);
            Assert.Equal(0, firstSpellTemplate.EffectBasePoints3);
            Assert.Equal(0u, firstSpellTemplate.EffectMechanic1);
            Assert.Equal(0u, firstSpellTemplate.EffectMechanic2);
            Assert.Equal(0u, firstSpellTemplate.EffectMechanic3);
            Assert.Equal(1u, firstSpellTemplate.EffectImplicitTargetA1);
            Assert.Equal(0u, firstSpellTemplate.EffectImplicitTargetA2);
            Assert.Equal(0u, firstSpellTemplate.EffectImplicitTargetA3);
            Assert.Equal(9u, firstSpellTemplate.EffectImplicitTargetB1);
            Assert.Equal(0u, firstSpellTemplate.EffectImplicitTargetB2);
            Assert.Equal(0u, firstSpellTemplate.EffectImplicitTargetB3);
            Assert.Equal(0u, firstSpellTemplate.EffectRadiusIndex1);
            Assert.Equal(0u, firstSpellTemplate.EffectRadiusIndex2);
            Assert.Equal(0u, firstSpellTemplate.EffectRadiusIndex3);
            Assert.Equal(0u, firstSpellTemplate.EffectApplyAuraName1);
            Assert.Equal(0u, firstSpellTemplate.EffectApplyAuraName2);
            Assert.Equal(0u, firstSpellTemplate.EffectApplyAuraName3);
            Assert.Equal(0u, firstSpellTemplate.EffectAmplitude1);
            Assert.Equal(0u, firstSpellTemplate.EffectAmplitude2);
            Assert.Equal(0u, firstSpellTemplate.EffectAmplitude3);
            Assert.Equal(1.0f, firstSpellTemplate.EffectMultipleValue1);
            Assert.Equal(0.0f, firstSpellTemplate.EffectMultipleValue2);
            Assert.Equal(0.0f, firstSpellTemplate.EffectMultipleValue3);
            Assert.Equal(0u, firstSpellTemplate.EffectChainTarget1);
            Assert.Equal(0u, firstSpellTemplate.EffectChainTarget2);
            Assert.Equal(0u, firstSpellTemplate.EffectChainTarget3);
            Assert.Equal(0u, firstSpellTemplate.EffectItemType1);
            Assert.Equal(0u, firstSpellTemplate.EffectItemType2);
            Assert.Equal(0u, firstSpellTemplate.EffectItemType3);
            Assert.Equal(0, firstSpellTemplate.EffectMiscValue1);
            Assert.Equal(0, firstSpellTemplate.EffectMiscValue2);
            Assert.Equal(0, firstSpellTemplate.EffectMiscValue3);
            Assert.Equal(0u, firstSpellTemplate.EffectTriggerSpell1);
            Assert.Equal(0u, firstSpellTemplate.EffectTriggerSpell2);
            Assert.Equal(0u, firstSpellTemplate.EffectTriggerSpell3);
            Assert.Equal(0.0f, firstSpellTemplate.EffectPointsPerComboPoint1);
            Assert.Equal(0.0f, firstSpellTemplate.EffectPointsPerComboPoint2);
            Assert.Equal(0.0f, firstSpellTemplate.EffectPointsPerComboPoint3);
            Assert.Equal(0u, firstSpellTemplate.SpellVisual1);
            Assert.Equal(0u, firstSpellTemplate.SpellVisual2);
            Assert.Equal(1u, firstSpellTemplate.SpellIconId);
            Assert.Equal(0u, firstSpellTemplate.ActiveIconId);
            Assert.Equal(50u, firstSpellTemplate.SpellPriority);
            Assert.Equal("Word of Recall (OLD)", firstSpellTemplate.Name1);
            Assert.Equal("Word of Recall (OLD)", firstSpellTemplate.Name2);
            Assert.Equal("Mot de rappel (OLD)", firstSpellTemplate.Name3);
            Assert.Equal("Word of Recall (OLD)", firstSpellTemplate.Name4);
            Assert.Equal("Word of Recall (OLD)", firstSpellTemplate.Name5);
            Assert.Equal(string.Empty, firstSpellTemplate.Name6);
            Assert.Equal("Palabra de memoria (OLD)", firstSpellTemplate.Name7);
            Assert.Equal(string.Empty, firstSpellTemplate.Name8);
            Assert.Equal(8323198u, firstSpellTemplate.NameFlags);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext1);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext2);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext3);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext4);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext5);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext6);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext7);
            Assert.Equal(string.Empty, firstSpellTemplate.NameSubtext8);
            Assert.Equal(8323196u, firstSpellTemplate.NameSubtextFlags);
            Assert.Equal(string.Empty, firstSpellTemplate.Description1);
            Assert.Equal(string.Empty, firstSpellTemplate.Description2);
            Assert.Equal(string.Empty, firstSpellTemplate.Description3);
            Assert.Equal(string.Empty, firstSpellTemplate.Description4);
            Assert.Equal(string.Empty, firstSpellTemplate.Description5);
            Assert.Equal(string.Empty, firstSpellTemplate.Description6);
            Assert.Equal(string.Empty, firstSpellTemplate.Description7);
            Assert.Equal(string.Empty, firstSpellTemplate.Description8);
            Assert.Equal(8323196u, firstSpellTemplate.DescriptionFlags);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription1);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription2);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription3);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription4);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription5);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription6);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription7);
            Assert.Equal(string.Empty, firstSpellTemplate.AuraDescription8);
            Assert.Equal(4128892u, firstSpellTemplate.AuraDescriptionFlags);
            Assert.Equal(0u, firstSpellTemplate.ManaCostPercentage);
            Assert.Equal(0u, firstSpellTemplate.StartRecoveryCategory);
            Assert.Equal(0u, firstSpellTemplate.StartRecoveryTime);
            Assert.Equal(0u, firstSpellTemplate.MaxTargetLevel);
            Assert.Equal(0u, firstSpellTemplate.SpellFamilyName);
            Assert.Equal(0uL, firstSpellTemplate.SpellFamilyFlags);
            Assert.Equal(0u, firstSpellTemplate.MaxAffectedTargets);
            Assert.Equal(1u, firstSpellTemplate.DmgClass);
            Assert.Equal(1u, firstSpellTemplate.PreventionType);
            Assert.Equal(-1, firstSpellTemplate.StanceBarOrder);
            Assert.Equal(1.0f, firstSpellTemplate.DmgMultiplier1);
            Assert.Equal(1.0f, firstSpellTemplate.DmgMultiplier2);
            Assert.Equal(1.0f, firstSpellTemplate.DmgMultiplier3);
            Assert.Equal(0u, firstSpellTemplate.MinFactionId);
            Assert.Equal(0u, firstSpellTemplate.MinReputation);
            Assert.Equal(0u, firstSpellTemplate.RequiredAuraVision);
        }

        public static void TestGetSpellThreats()
        {
            int count = MangosRepository.GetRowCountForTable("spell_threat");
            List<SpellThreat> spellThreats = MangosRepository.GetSpellThreats();

            Assert.Equal(count, spellThreats.Count);

            var firstSpellThreat = spellThreats[0];

            Assert.Equal(78u, firstSpellThreat.Entry);
            Assert.Equal(20, firstSpellThreat.Threat);
            Assert.Equal(1.0f, firstSpellThreat.Multiplier);
            Assert.Equal(0.0f, firstSpellThreat.ApBonus);
        }

        public static void TestGetTaxiPathTransitions()
        {
            int count = MangosRepository.GetRowCountForTable("taxi_path_transitions");
            List<TaxiPathTransition> taxiPathTransitions = MangosRepository.GetTaxiPathTransitions();

            Assert.Equal(count, taxiPathTransitions.Count);

            var firstTransition = taxiPathTransitions[0];

            Assert.Equal(499u, firstTransition.InPath);
            Assert.Equal(482u, firstTransition.OutPath);
            Assert.Equal(20u, firstTransition.InNode);
            Assert.Equal(1u, firstTransition.OutNode);
            Assert.Equal("Everlook, Winterspring -> Valormok, Azshara -> Splintertree Post, Ashenvale", firstTransition.Comment);
        }

        public static void TestGetTransports()
        {
            int count = MangosRepository.GetRowCountForTable("transports");
            List<Transport> transports = MangosRepository.GetTransports();

            Assert.Equal(count, transports.Count);

            var firstTransport = transports[0];

            Assert.Equal(1u, firstTransport.Guid);
            Assert.Equal(20808u, firstTransport.Entry);
            Assert.Equal("Ratchet and Booty Bay", firstTransport.Name);
            Assert.Equal(350818u, firstTransport.Period);
        }

        public static void TestGetVariables()
        {
            int count = MangosRepository.GetRowCountForTable("variables");
            List<Variables> variables = MangosRepository.GetVariables();

            Assert.Equal(count, variables.Count);

            var firstVariable = variables[0];

            Assert.Equal(30000u, firstVariable.Index);
            Assert.Equal(4u, firstVariable.Value);
        }

    }
}