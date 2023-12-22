namespace RaidMemberBot.Mem
{
    public static class MemoryAddresses
    {
        // Functions
        public static int EnumerateVisibleObjectsFunPtr = 0x00468380;
        public static int GetObjectPtrFunPtr = 0x00464870;
        public static int GetPlayerGuidFunPtr = 0x00468550;
        public static int SetFacingFunPtr = 0x007C6F30;
        public static int SendMovementUpdateFunPtr = 0x00600A30;
        public static int SetControlBitFunPtr = 0x00515090;
        public static int SetControlBitDevicePtr = 0x00BE1148;
        public static int GetCreatureTypeFunPtr = 0x00605570;
        public static int GetCreatureRankFunPtr = 0x00605620;
        public static int GetUnitReactionFunPtr = 0x006061E0;
        public static int LuaCallFunPtr = 0x00704CD0;
        public static int GetTextFunPtr = 0x00703BF0;
        public static int IntersectFunPtr = 0x00672170; // https://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/409609-fixed-cworld-intersect-raycasting-1-12-a.html
        public static int SetTargetFunPtr = 0x00493540;
        public static int RetrieveCorpseFunPtr = 0x0048D260;
        public static int ReleaseCorpseFunPtr = 0x005E0AE0;
        public static int GetItemCacheEntryFunPtr = 0x0055BA30;
        public static int ItemCacheEntryBasePtr = 0x00C0E2A0;
        public static int IsSpellOnCooldownFunPtr = 0x006E13E0;
        public static int LootSlotFunPtr = 0x004C2790;
        public static int UseItemFunPtr = 0x005D8D00;
        public static int SellItemByGuidFunPtr = 0x005E1D50;
        public static int BuyVendorItemFunPtr = 0x005E1E90;
        public static int CastAtPositionFunPtr = 0x006E60F0;

        // Statics
        public static int ZoneTextPtr = 0x00B4B404;
        public static int MinimapZoneTextPtr = 0x00B4DA28;
        public static int MapId;
        public static int ServerName;
        public static int LocalPlayerCorpsePositionX = 0x00B4E284;
        public static int LocalPlayerCorpsePositionY = 0x00B4E288;
        public static int LocalPlayerCorpsePositionZ = 0x00B4E28C;
        public static int LocalPlayerFirstExtraBag = 0x00BDD060;
        public static int LocalPlayerClass = 0x00C27E81;
        public static int LastHardwareAction = 0x00CF0BC8;
        public static int LocalPlayerSpellsBase = 0x00B700F0;
        public static int SignalEventFunPtr = 0x00703F76;
        public static int SignalEventNoParamsFunPtr = 0x00703E72;
        public static int WardenLoadHook = 0x006CA22E;
        public static int WardenBase = 0x00CE8978;
        public static int WardenPageScanOffset = 0x00002B21;
        public static int WardenMemScanOffset = 0x00002A7F;
        public static int PartyLeaderGuid = 0x00BC75F8;
        public static int Party1Guid = 0x00BC6F48;
        public static int Party2Guid = 0x00BC6F50;
        public static int Party3Guid = 0x00BC6F58;
        public static int Party4Guid = 0x00BC6F60;

        // Frames
        public static int CoinCountPtr = 0x00B71BA0;
        public static int DialogFrameBase;
        public static int MerchantFrameItemsBasePtr = 0x00BDDFA8;
        public static int MerchantFrameItemPtr = 0x00BDD11C;
        public static int LootFrameItemsBasePtr = 0x00B7196C;
        public static int LootFrameItemOffset = 0x1C;
        public static int MerchantFrameItemOffset = 0x1C;

        // Descriptors
        public static int LocalPlayer_BackpackFirstItemOffset = 0x850;
        public static int WoWItem_ItemIdOffset = 0xC;
        public static int WoWItem_StackCountOffset = 0x38;
        public static int WoWItem_DurabilityOffset = 0xB8;
        public static int WoWItem_ContainerFirstItemOffset = 0xC0;
        public static int WoWPet_SpellsBase;
        public static int WoWUnit_SummonedByGuidOffset = 0x30;
        public static int WoWUnit_TargetGuidOffset = 0x40;
        public static int WoWUnit_HealthOffset = 0x58;
        public static int WoWUnit_ManaOffset = 0x5C;
        public static int WoWUnit_RageOffset = 0x60;
        public static int WoWUnit_EnergyOffset = 0x68;
        public static int WoWUnit_MaxHealthOffset = 0x70;
        public static int WoWUnit_MaxManaOffset = 0x74;
        public static int WoWUnit_LevelOffset = 0x88;
        public static int WoWUnit_FactionIdOffset = 0x8C;
        public static int WoWUnit_UnitFlagsOffset = 0xB8;
        public static int WoWUnit_DynamicFlagsOffset = 0x23C;
        public static int WoWUnit_CurrentChannelingOffset;

        // Offsets
        public static int LocalPlayer_SetFacingOffset = 0x9A8;
        public static int LocalPlayer_EquipmentFirstItemOffset = 0x2508;
        public static int WoWObject_DescriptorOffset = 0x8;
        public static int WoWObject_GetPositionFunOffset;
        public static int WoWObject_GetFacingFunOffset;
        public static int WoWObject_InteractFunOffset;
        public static int WoWObject_GetNameFunOffset;
        public static int WoWUnit_BuffsBaseOffset = 0xBC;
        public static int WoWUnit_DebuffsBaseOffset = 0x13C;
        public static int WoWUnit_MovementFlagsOffset = 0x9E8;
        public static int WoWUnit_CurrentSpellcastOffset = 0xC8C;
        public static int WoWItem_ContainerSlotsOffset = 0x6c8;
    }
}
