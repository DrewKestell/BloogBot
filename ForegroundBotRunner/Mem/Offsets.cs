namespace ForegroundBotRunner.Mem
{
    public static class Offsets
    {
        /// <summary>
        ///     Offsets for WoW, no commenting needed
        /// </summary>
        public static class Player
        {
            public static nint Class = 0xC27E81;
            public static nint IsIngame = 0xB4B424;
            public static nint IsGhost = 0x835A48;
            public static nint Name = 0x827D88;
            public static nint TargetGuid = 0x74E2D8;
            public static nint LastHostileTargetGuid = 0x74E2E8;
            public static nint IsChannelingDescriptor = 0x240;
            public static nint IsCasting = 0xCECA88;
            public static int ComboPoints1 = 0xE68;
            public static int ComboPoints2 = 0x1029;
            public static nint CharacterCount = 0x00B42140;

            public static nint QuestLog = 0x318;

            public static nint CorpsePositionX = 0x00B4E284;
            public static nint CorpsePositionY = 0x00B4E288;
            public static nint CorpsePositionZ = 0x00B4E28C;

            public static nint CtmX = 0xC4D890;
            public static nint CtmY = 0xC4D894;
            public static nint CtmZ = 0xC4D898;
            public static nint CtmState = 0xC4D888;

            public static int MovementStruct = 0x9A8;

            public static nint IsLooting = 0xB71B48;
            public static nint IsInCC = 0xB4B3E4;

            public static int RealZoneText = 0xB4B404;
            public static nint ContinentText = 0x00C961A0;
            public static nint MinimapZoneText = 0xB4DA28;
        }

        public static class Warden
        {
            public static nint WardenPtr1 = 0x0CE8978;
            public static nint WardenMemScanStart = 0x2A7F;
            public static nint WardenPageScan = 0x2B21;
        }

        public static class CharacterScreen
        {
            public static nint Pointer = 0xB42144;
            public static nint Size = 0x120;
            public static nint NumCharacters = nint.Subtract(Pointer, 0x4);
            public static nint LoginState = 0xB41478;
        }

        public static class Misc
        {
            public static nint LuaState = 0x00CEEF74;
            public static nint GameVersion = 0x00837C04;
            public static nint MapId = 0x84C498;
            public static nint AntiDc = 0x00B41D98;
            public static nint LoginState = 0xB41478;
            public static nint RealmName = 0x00C27FC1;
            public static nint CGInputControlActive = 0xBE1148;
        }

        public static class Hooks
        {
            public static nint ChatMessage = 0x0049A7C9;
            public static nint ErrorEnum = 0x00496807;
            public static nint UpdateSpells = 0x004B302A;
            public static nint SignalEvent_0 = 0x00703E72;
            public static nint SignalEvent = 0x00703F76;
        }

        public static class Functions
        {
            public static nint SelectCharacter = 0x472740;
            public static nint CastAtPos = 0x6E60F0;
            public static nint EnterWorld = 0x0046B500;
            public static nint GetCreatureRank = 0x00605620;
            public static nint GetCreatureType = 0x00605570;
            public static nint CastSpellByName = 0x004B4AB0;

            public static nint RetrieveCorpse = 0x0048D260;
            public static nint RepopMe = 0x005E0AE0;

            public static nint LootSlotAt = 0x004C2790;
            public static nint IsTransport = 0x007C61C0;
            public static nint ConfirmBindLoot = 0x0048DD00;

            public static nint CanCompleteQuest = 0x4DF580;
            public static nint CanUseItem = 0x005EA930;

            public static nint UnitReaction = 0x006061E0;
            public static nint SetTarget = 0x493540;
            public static nint LastHardwareAction = 0x00CF0BC8;
            public static nint AutoLoot = 0x4C1FA0;
            public static nint ClickToMove = 0x00611130;
            public static nint AcceptQuest = 0x005EAC10;
            public static nint CompleteQuest = 0x005EACA0;
            public static nint GetText = 0x703BF0;
            public static nint DoString = 0x00704CD0;
            public static nint GetEndscene = 0x5A17B6;
            public static nint IsLooting = 0x006126B0;
            public static nint GetLootSlots = 0x004C2260;
            public static nint OnRightClickObject = 0x005F8660;
            public static nint OnRightClickUnit = 0x60BEA0;
            public static nint SetFacing = 0x007C6F30;
            public static nint SendMovementPacket = 0x00600A30;
            public static nint PerformDefaultAction = 0x00481F60;
            public static nint CGInputControl__GetActive = 0x005143E0;
            public static nint CGInputControl__SetControlBit = 0x00515090;
            public static nint EnumVisibleObjects = 0x00468380;
            public static nint BuyVendorItem = 0x005E1E90;
            public static nint GetPtrForGuid = 0x464870;
            public static nint ClntObjMgrGetActivePlayer = 0x00468550;
            public static nint ClntObjMgrGetMapId = 0x00468580;
            public static nint NetClientSend = 0x005379A0;
            public static nint GetSpellCooldown = 0x006E13E0;
            public static nint GetSpellCooldownPtr1 = 0x00CECAEC;
            public static nint UseItem = 0x005D8D00;
            public static nint DbQueryCreatureCache = 0x00556AA0;
            public static nint DbQueryCreatureCachePtr1 = 0x00C0E354;
            public static nint ClientConnection = 0x005AB490;

            public static nint SellItem = 0x005E1D50;

            public static nint LuaGetArgCount = 0x006F3070;
            public static nint HandleSpellTerrainClick = 0x6E60F0;

            public static nint DefaultServerLogin = 0x0046D160;

            public static nint IsSceneEnd = 0x005A17A0;
            public static nint EndScenePtr1 = 0x38a8;
            public static nint EndScenePtr2 = 0xa8;

            public static nint ItemCacheGetRow = 0x0055BA30;
            public static nint ItemCacheBasePtr = 0x00C0E2A0;

            public static nint QuestCacheGetRow = 0x00562A40;
            public static nint QuestCacheBasePtr = 0x00C0E1B0;

            public static nint LuaRegisterFunc = 0x00704120;
            public static nint LuaUnregFunc = 0x00704160;
            public static nint LuaIsString = 0x006F3510;
            public static nint LuaIsNumber = 0x006F34D0;
            public static nint LuaToString = 0x006F3690;
            public static nint LuaToNumber = 0x006F3620;

            public static nint Intersect = 0x6aa160;

            public static nint CastSpell = 0x6E5A90;
            public static nint AbandonQuest = 0x5EAF40;
            public static nint GetGameObjectPosition = 0x005F9F50;
        }

        public enum Party : uint
        {
            leaderGuid = 0x00BC75F8,
            party1Guid = 0x00BC6F48,
            party2Guid = 0x00BC6F50,
            party3Guid = 0x00BC6F58,
            party4Guid = 0x00BC6F60
        }

        public enum RaidIcon : uint
        {
            Star = 0x00771368,
            Circle = 0x00771370,
            Diamond = 0x00771378,
            Triangle = 0x00771380,
            Moon = 0x00771388,
            Square = 0x00771390,
            Cross = 0x00771398,
            Skull = 0x007713A0
        }

        public static class ObjectManager
        {
            public static nint CurObjGuid = 0x30;
            public static nint ManagerBase = 0x00B41414;
            public static nint PlayerGuid = 0xc0;
            public static nint FirstObj = 0xac;
            public static nint NextObj = 0x3c;
            public static nint ObjType = 0x14;
            public static int DescriptorOffset = 0x8;
        }

        public static class PlayerObject
        {
            public static nint NameBase = 0xC0E230;
            public static int NameBaseNextGuid = 0xc;
        }

        public static class Unit
        {
            public static int PosX = 0x9B8;
            public static int PosY = 0x9BC;
            public static int PosZ = 0x9BC + 4;
            public static int AuraBase = 0xBC;
            public static int DebuffBase = 0x13C;

            public static int NameBase = 0xB30;
            public static int IsCritterOffset = 24;
        }

        public static class GameObject
        {
            public static int PosX = 0x3C;
            public static int PosY = 0x3C + 0x4;
            public static int PosZ = 0x3C + 0x8;

            public static int NameBase = 0x214;
            public static int NameBasePtr1 = 0x8;
        }

        public static class Item
        {
            public static int UseItemPtr1 = 0xABE8;
            public static int ItemCachePtrQuality = 0x1C;
            public static int ItemCachePtrName = 0x8;

            public static int ItemSlots = 0x6c8;
        }

        public static class Descriptors
        {
            public static int GotLoot = 0xB4;

            public static int SummonedByGuid = 0x30;

            public static int NpcId = 0xE74;

            public static int DynamicFlags = 0x23C;
            public static int Flags = 0xB8;

            public static int ChannelingId = 0x240;
            public static int CreatedByGuid = 0x38;
            public static int GameObjectCreatedByGuid = 0x18;

            public static int MovementFlags = 0x9E8;

            public static int Health = 0x58;
            public static int MaxHealth = 0x70;
            public static int FactionId = 0x8C;
            public static int Mana = 0x5C;
            public static int MaxMana = 0x74;
            public static int Rage = 0x60;
            public static int Energy = 0x68;
            public static int TargetGuid = 0x40;
            public static int CorpseOwnedBy = 0x18;

            public static int ItemId = 0xC;
            public static int ItemDurability = 0xB8;
            public static int ItemMaxDurability = 0xBC;
            public static int ItemStackCount = 0x38;

            public static int Level = 0x88;

            public static int MountDisplayId = 0x214;

            public static int ContainerTotalSlots = 0x6c8;
            public static int CorpseX = 0x24;
            public static int CorpseY = 0x28;
            public static int CorpseZ = 0x2c;

            public static int NextLevelXp = 0xB34;
            public static int CurrentXp = 0xB30;
        }

        public static class Buffs
        {
            public static nint FirstBuff = 0xBC;
            public static nint FirstDebuff = 0x13C;
            public static nint NextBuff = 0x4;
        }

        public static class Hacks
        {
            public static nint DisableCollision = 0x6ABC5A;
            public static nint CtmPatch = 0x860A90;
            public static nint Wallclimb = 0x0080DFFC;
            public static nint Collision3 = 0x006ABF13;
            public static nint LootPatch = 0x004C21C0;
            public static nint LootPatch2 = 0x004C28FF;
            public static nint LuaUnlock = 0x494A50;
        }
    }
}
