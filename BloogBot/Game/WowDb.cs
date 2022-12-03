using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public static class WowDb
    {
        public static readonly Dictionary<ClientDb, DbTable> Tables = new Dictionary<ClientDb, DbTable>();

        static WowDb()
        {
            for (var tableBase = (IntPtr)MemoryAddresses.WowDbTableBase;
                MemoryManager.ReadByte(tableBase) != 0xC3;
                tableBase += 0x11)
            {
                var index = MemoryManager.ReadUint(tableBase + 1);
                var tablePtr = new IntPtr(MemoryManager.ReadInt(tableBase + 0xB) + 0x18);
                Tables.Add((ClientDb)index, new DbTable(tablePtr));
            }
        }

        public class DbTable
        {
            readonly IntPtr pointer;
            
            public DbTable(IntPtr pointer)
            {
                this.pointer = pointer;
            }

            // For all DBs, we should use GetRow, except for Spells.db, which should use GetLocalizedRow
            public IntPtr GetRow(int index)
            {
                return Functions.GetRow(pointer, index);
            }

            // For all DBs, we should use GetRow, except for Spells.db, which should use GetLocalizedRow
            public IntPtr GetLocalizedRow(int index)
            {
                var rowPtr = Marshal.AllocHGlobal(4 * 4 * 256);
                var result = Functions.GetLocalizedRow(IntPtr.Subtract(pointer, 0x18), index, rowPtr);
                return rowPtr;
            }
        }
    }

    public enum ClientDb : uint
    {
        Achievement = 0x000000EB, // 0x00A73888
        Achievement_Criteria = 0x000000EC, // 0x00A738AC
        Achievement_Category = 0x000000ED, // 0x00A738D0
        AnimationData = 0x000000EE, // 0x00A738F4
        AreaGroup = 0x000000EF, // 0x00A73918
        AreaPOI = 0x000000F0, // 0x00A7393C
        AreaTable = 0x000000F1, // 0x00A73960
        AreaTrigger = 0x000000F2, // 0x00A73984
        AttackAnimKits = 0x000000F3, // 0x00A739A8
        AttackAnimTypes = 0x000000F4, // 0x00A739CC
        AuctionHouse = 0x000000F5, // 0x00A739F0
        BankBagSlotPrices = 0x000000F6, // 0x00A73A14
        BannedAddOns = 0x000000F7, // 0x00A73A38
        BarberShopStyle = 0x000000F8, // 0x00A73A5C
        BattlemasterList = 0x000000F9, // 0x00A73A80
        CameraShakes = 0x000000FA, // 0x00A73AA4
        Cfg_Categories = 0x000000FB, // 0x00A73AC8
        Cfg_Configs = 0x000000FC, // 0x00A73AEC
        CharBaseInfo = 0x000000FD, // 0x00A73B10
        CharHairGeosets = 0x000000FE, // 0x00A73B34
        CharSections = 0x000000FF, // 0x00A73B58
        CharStartOutfit = 0x00000100, // 0x00A73B7C
        CharTitles = 0x00000101, // 0x00A73BA0
        CharacterFacialHairStyles = 0x00000102, // 0x00A73BC4
        ChatChannels = 0x00000103, // 0x00A73BE8
        ChatProfanity = 0x00000104, // 0x00A73C0C
        ChrClasses = 0x00000105, // 0x00A73C30
        ChrRaces = 0x00000106, // 0x00A73C54
        CinematicCamera = 0x00000107, // 0x00A73C78
        CinematicSequences = 0x00000108, // 0x00A73C9C
        CreatureDisplayInfo = 0x00000109, // 0x00A73CE4
        CreatureDisplayInfoExtra = 0x0000010A, // 0x00A73CC0
        CreatureFamily = 0x0000010B, // 0x00A73D08
        CreatureModelData = 0x0000010C, // 0x00A73D2C
        CreatureMovementInfo = 0x0000010D, // 0x00A73D50
        CreatureSoundData = 0x0000010E, // 0x00A73D74
        CreatureSpellData = 0x0000010F, // 0x00A73D98
        CreatureType = 0x00000110, // 0x00A73DBC
        CurrencyTypes = 0x00000111, // 0x00A73DE0
        CurrencyCategory = 0x00000112, // 0x00A73E04
        DanceMoves = 0x00000113, // 0x00A73E28
        DeathThudLookups = 0x00000114, // 0x00A73E4C
        DestructibleModelData = 0x00000115, // 0x00A73EB8
        DungeonEncounter = 0x00000116, // 0x00A73EDC
        DungeonMap = 0x00000117, // 0x00A73F00
        DungeonMapChunk = 0x00000118, // 0x00A73F24
        DurabilityCosts = 0x00000119, // 0x00A73F48
        DurabilityQuality = 0x0000011A, // 0x00A73F6C
        Emotes = 0x0000011B, // 0x00A73F90
        EmotesText = 0x0000011C, // 0x00A73FFC
        EmotesTextData = 0x0000011D, // 0x00A73FB4
        EmotesTextSound = 0x0000011E, // 0x00A73FD8
        EnvironmentalDamage = 0x0000011F, // 0x00A74020
        Exhaustion = 0x00000120, // 0x00A74044
        Faction = 0x00000121, // 0x00A7408C
        FactionGroup = 0x00000122, // 0x00A74068
        FactionTemplate = 0x00000123, // 0x00A740B0
        FileData = 0x00000124, // 0x00A740D4
        FootprintTextures = 0x00000125, // 0x00A740F8
        FootstepTerrainLookup = 0x00000126, // 0x00A7411C
        GameObjectArtKit = 0x00000127, // 0x00A74140
        GameObjectDisplayInfo = 0x00000128, // 0x00A74164
        GameTables = 0x00000129, // 0x00A74188
        GameTips = 0x0000012A, // 0x00A741AC
        GemProperties = 0x0000012B, // 0x00A741D0
        GlyphProperties = 0x0000012C, // 0x00A741F4
        GlyphSlot = 0x0000012D, // 0x00A74218
        GMSurveyAnswers = 0x0000012E, // 0x00A7423C
        GMSurveyCurrentSurvey = 0x0000012F, // 0x00A74260
        GMSurveyQuestions = 0x00000130, // 0x00A74284
        GMSurveySurveys = 0x00000131, // 0x00A742A8
        GMTicketCategory = 0x00000132, // 0x00A742CC
        GroundEffectDoodad = 0x00000133, // 0x00A742F0
        GroundEffectTexture = 0x00000134, // 0x00A74314
        gtBarberShopCostBase = 0x00000135, // 0x00A74338
        gtCombatRatings = 0x00000136, // 0x00A7435C
        gtChanceToMeleeCrit = 0x00000137, // 0x00A74380
        gtChanceToMeleeCritBase = 0x00000138, // 0x00A743A4
        gtChanceToSpellCrit = 0x00000139, // 0x00A743C8
        gtChanceToSpellCritBase = 0x0000013A, // 0x00A743EC
        gtNPCManaCostScaler = 0x0000013B, // 0x00A74410
        gtOCTClassCombatRatingScalar = 0x0000013C, // 0x00A74434
        gtOCTRegenHP = 0x0000013D, // 0x00A74458
        gtOCTRegenMP = 0x0000013E, // 0x00A7447C
        gtRegenHPPerSpt = 0x0000013F, // 0x00A744A0
        gtRegenMPPerSpt = 0x00000140, // 0x00A744C4
        HelmetGeosetVisData = 0x00000141, // 0x00A744E8
        HolidayDescriptions = 0x00000142, // 0x00A7450C
        HolidayNames = 0x00000143, // 0x00A74530
        Holidays = 0x00000144, // 0x00A74554
        Item = 0x00000145, // 0x00A74578
        ItemBagFamily = 0x00000146, // 0x00A7459C
        ItemClass = 0x00000147, // 0x00A745C0
        ItemCondExtCosts = 0x00000148, // 0x00A745E4
        ItemDisplayInfo = 0x00000149, // 0x00A74608
        ItemExtendedCost = 0x0000014A, // 0x00A7462C
        ItemGroupSounds = 0x0000014B, // 0x00A74650
        ItemLimitCategory = 0x0000014C, // 0x00A74674
        ItemPetFood = 0x0000014D, // 0x00A74698
        ItemPurchaseGroup = 0x0000014E, // 0x00A746BC
        ItemRandomProperties = 0x0000014F, // 0x00A746E0
        ItemRandomSuffix = 0x00000150, // 0x00A74704
        ItemSet = 0x00000151, // 0x00A74728
        ItemSubClass = 0x00000152, // 0x00A74770
        ItemSubClassMask = 0x00000153, // 0x00A7474C
        ItemVisualEffects = 0x00000154, // 0x00A74794
        ItemVisuals = 0x00000155, // 0x00A747B8
        LanguageWords = 0x00000156, // 0x00A747DC
        Languages = 0x00000157, // 0x00A74800
        LfgDungeonExpansion = 0x00000158, // 0x00A74824
        LfgDungeonGroup = 0x00000159, // 0x00A74848
        LfgDungeons = 0x0000015A, // 0x00A7486C
        Light = 0x0000015B, // 0x00A96C08
        LightFloatBand = 0x0000015C, // 0x00A96BC0
        LightIntBand = 0x0000015D, // 0x00A96B9C
        LightParams = 0x0000015E, // 0x00A96BE4
        LightSkybox = 0x0000015F, // 0x00A96B78
        LiquidType = 0x00000160, // 0x00A74890
        LiquidMaterial = 0x00000161, // 0x00A748B4
        LoadingScreens = 0x00000162, // 0x00A748D8
        LoadingScreenTaxiSplines = 0x00000163, // 0x00A748FC
        Lock = 0x00000164, // 0x00A74920
        LockType = 0x00000165, // 0x00A74944
        MailTemplate = 0x00000166, // 0x00A74968
        Map = 0x00000167, // 0x00A7498C
        MapDifficulty = 0x00000168, // 0x00A749B0
        Material = 0x00000169, // 0x00A749D4
        Movie = 0x0000016A, // 0x00A749F8
        MovieFileData = 0x0000016B, // 0x00A74A1C
        MovieVariation = 0x0000016C, // 0x00A74A40
        NameGen = 0x0000016D, // 0x00A74A64
        NPCSounds = 0x0000016E, // 0x00A74A88
        NamesProfanity = 0x0000016F, // 0x00A74AAC
        NamesReserved = 0x00000170, // 0x00A74AD0
        OverrideSpellData = 0x00000171, // 0x00A74AF4
        Package = 0x00000172, // 0x00A74B18
        PageTextMaterial = 0x00000173, // 0x00A74B3C
        PaperDollItemFrame = 0x00000174, // 0x00A74B60
        ParticleColor = 0x00000175, // 0x00A74B84
        PetPersonality = 0x00000176, // 0x00A74BA8
        PowerDisplay = 0x00000177, // 0x00A74BCC
        PvpDifficulty = 0x00000178, // 0x00A74BF0
        QuestFactionReward = 0x00000179, // 0x00A74C14
        QuestInfo = 0x0000017A, // 0x00A74C38
        QuestSort = 0x0000017B, // 0x00A74C5C
        QuestXP = 0x0000017C, // 0x00A74C80
        Resistances = 0x0000017D, // 0x00A74CA4
        RandPropPoints = 0x0000017E, // 0x00A74CC8
        ScalingStatDistribution = 0x0000017F, // 0x00A74CEC
        ScalingStatValues = 0x00000180, // 0x00A74D10
        ScreenEffect = 0x00000181, // 0x00A74D34
        ServerMessages = 0x00000182, // 0x00A74D58
        SheatheSoundLookups = 0x00000183, // 0x00A74D7C
        SkillCostsData = 0x00000184, // 0x00A74DA0
        SkillLineAbility = 0x00000185, // 0x00A74DC4
        SkillLineCategory = 0x00000186, // 0x00A74DE8
        SkillLine = 0x00000187, // 0x00A74E0C
        SkillRaceClassInfo = 0x00000188, // 0x00A74E30
        SkillTiers = 0x00000189, // 0x00A74E54
        SoundAmbience = 0x0000018A, // 0x00A74E78
        SoundEmitters = 0x0000018B, // 0x00A74EC0
        SoundEntries = 0x0000018C, // 0x00A74E9C
        SoundProviderPreferences = 0x0000018D, // 0x00A74EE4
        SoundSamplePreferences = 0x0000018E, // 0x00A74F08
        SoundWaterType = 0x0000018F, // 0x00A74F2C
        SpamMessages = 0x00000190, // 0x00A74F50
        SpellCastTimes = 0x00000191, // 0x00A74F74
        SpellCategory = 0x00000192, // 0x00A74F98
        SpellChainEffects = 0x00000193, // 0x00A74FBC
        Spell = 0x00000194, // 0x00A751FC
        SpellDescriptionVariables = 0x00000195, // 0x00A74FE0
        SpellDifficulty = 0x00000196, // 0x00A75004
        SpellDispelType = 0x00000197, // 0x00A75028
        SpellDuration = 0x00000198, // 0x00A7504C
        SpellEffectCameraShakes = 0x00000199, // 0x00A75070
        SpellFocusObject = 0x0000019A, // 0x00A75094
        SpellIcon = 0x0000019B, // 0x00A750B8
        SpellItemEnchantment = 0x0000019C, // 0x00A750DC
        SpellItemEnchantmentCondition = 0x0000019D, // 0x00A75100
        SpellMechanic = 0x0000019E, // 0x00A75124
        SpellMissile = 0x0000019F, // 0x00A75148
        SpellMissileMotion = 0x000001A0, // 0x00A7516C
        SpellRadius = 0x000001A1, // 0x00A75190
        SpellRange = 0x000001A2, // 0x00A751B4
        SpellRuneCost = 0x000001A3, // 0x00A751D8
        SpellShapeshiftForm = 0x000001A4, // 0x00A75220
        SpellVisual = 0x000001A5, // 0x00A752D4
        SpellVisualEffectName = 0x000001A6, // 0x00A75244
        SpellVisualKit = 0x000001A7, // 0x00A75268
        SpellVisualKitAreaModel = 0x000001A8, // 0x00A7528C
        SpellVisualKitModelAttach = 0x000001A9, // 0x00A752B0
        StableSlotPrices = 0x000001AA, // 0x00A752F8
        Stationery = 0x000001AB, // 0x00A7531C
        StringLookups = 0x000001AC, // 0x00A75340
        SummonProperties = 0x000001AD, // 0x00A75364
        Talent = 0x000001AE, // 0x00A75388
        TalentTab = 0x000001AF, // 0x00A753AC
        TaxiNodes = 0x000001B0, // 0x00A753D0
        TaxiPath = 0x000001B1, // 0x00A75418
        TaxiPathNode = 0x000001B2, // 0x00A753F4
        TeamContributionPoints = 0x000001B3, // 0x00A7543C
        TerrainType = 0x000001B4, // 0x00A75460
        TerrainTypeSounds = 0x000001B5, // 0x00A75484
        TotemCategory = 0x000001B6, // 0x00A754A8
        TransportAnimation = 0x000001B7, // 0x00A754CC
        TransportPhysics = 0x000001B8, // 0x00A754F0
        TransportRotation = 0x000001B9, // 0x00A75514
        UISoundLookups = 0x000001BA, // 0x00A75538
        UnitBlood = 0x000001BB, // 0x00A75580
        UnitBloodLevels = 0x000001BC, // 0x00A7555C
        Vehicle = 0x000001BD, // 0x00A755A4
        VehicleSeat = 0x000001BE, // 0x00A755C8
        VehicleUIIndicator = 0x000001BF, // 0x00A755EC
        VehicleUIIndSeat = 0x000001C0, // 0x00A75610
        VocalUISounds = 0x000001C1, // 0x00A75634
        WMOAreaTable = 0x000001C2, // 0x00A75658
        WeaponImpactSounds = 0x000001C3, // 0x00A7567C
        WeaponSwingSounds2 = 0x000001C4, // 0x00A756A0
        Weather = 0x000001C5, // 0x00A756C4
        WorldMapArea = 0x000001C6, // 0x00A756E8
        WorldMapTransforms = 0x000001C7, // 0x00A75754
        WorldMapContinent = 0x000001C8, // 0x00A7570C
        WorldMapOverlay = 0x000001C9, // 0x00A75730
        WorldSafeLocs = 0x000001CA, // 0x00A75778
        WorldStateUI = 0x000001CB, // 0x00A7579C
        ZoneIntroMusicTable = 0x000001CC, // 0x00A757C0
        ZoneMusic = 0x000001CD, // 0x00A757E4
        WorldStateZoneSounds = 0x000001CE, // 0x00A75808
        WorldChunkSounds = 0x000001CF, // 0x00A7582C
        SoundEntriesAdvanced = 0x000001D0, // 0x00A75850
        ObjectEffect = 0x000001D1, // 0x00A75874
        ObjectEffectGroup = 0x000001D2, // 0x00A75898
        ObjectEffectModifier = 0x000001D3, // 0x00A758BC
        ObjectEffectPackage = 0x000001D4, // 0x00A758E0
        ObjectEffectPackageElem = 0x000001D5, // 0x00A75904
        SoundFilter = 0x000001D6, // 0x00A75928
        SoundFilterElem = 0x000001D7, // 0x00A7594C
    }
}