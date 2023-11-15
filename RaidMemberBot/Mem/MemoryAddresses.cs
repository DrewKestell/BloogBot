namespace RaidMemberBot.Mem
{
    public static class MemoryAddresses
    {
        static MemoryAddresses()
        {
            EnumerateVisibleObjectsFunPtr = 0x00468380;
            GetObjectPtrFunPtr = 0x00464870;
            GetPlayerGuidFunPtr = 0x00468550;
            SetFacingFunPtr = 0x007C6F30;
            SendMovementUpdateFunPtr = 0x00600A30;
            SetControlBitFunPtr = 0x00515090;
            SetControlBitDevicePtr = 0x00BE1148;
            GetCreatureTypeFunPtr = 0x00605570;
            GetCreatureRankFunPtr = 0x00605620;
            GetUnitReactionFunPtr = 0x006061E0;
            LuaCallFunPtr = 0x00704CD0;
            GetTextFunPtr = 0x00703BF0;
            IntersectFunPtr = 0x00672170; // https://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/409609-fixed-cworld-intersect-raycasting-1-12-a.html
            SetTargetFunPtr = 0x00493540;
            RetrieveCorpseFunPtr = 0x0048D260;
            ReleaseCorpseFunPtr = 0x005E0AE0;
            GetItemCacheEntryFunPtr = 0x0055BA30;
            ItemCacheEntryBasePtr = 0x00C0E2A0;
            IsSpellOnCooldownFunPtr = 0x006E13E0;
            LootSlotFunPtr = 0x004C2790;
            UseItemFunPtr = 0x005D8D00;
            SellItemByGuidFunPtr = 0x005E1D50;
            BuyVendorItemFunPtr = 0x005E1E90;
            CastAtPositionFunPtr = 0x006E60F0;
            ZoneTextPtr = 0x00B4B404;
            MinimapZoneTextPtr = 0x00B4DA28;
            LootFrameItemsBasePtr = 0x00B7196C;
            CoinCountPtr = 0x00B71BA0;
            MerchantFrameItemsBasePtr = 0x00BDDFA8;
            MerchantFrameItemPtr = 0x00BDD11C;
            LootFrameItemOffset = 0x1C;
            MerchantFrameItemOffset = 0x1C;
            LocalPlayer_SetFacingOffset = 0x9A8;
            LocalPlayerCorpsePositionX = 0x00B4E284;
            LocalPlayerCorpsePositionY = 0x00B4E288;
            LocalPlayerCorpsePositionZ = 0x00B4E28C;
            LastHardwareAction = 0x00CF0BC8;
            LocalPlayerSpellsBase = 0x00B700F0;
            LocalPlayerClass = 0x00C27E81;
            LocalPlayer_BackpackFirstItemOffset = 0x850;
            LocalPlayer_EquipmentFirstItemOffset = 0x2508;
            WoWItem_ItemIdOffset = 0xC;
            WoWItem_StackCountOffset = 0x38;
            WoWItem_DurabilityOffset = 0xB8;
            WoWItem_ContainerFirstItemOffset = 0xC0;
            WoWItem_ContainerSlotsOffset = 0x6c8;
            WoWObject_DescriptorOffset = 0x8;
            WoWUnit_SummonedByGuidOffset = 0x30;
            WoWUnit_TargetGuidOffset = 0x40;
            WoWUnit_HealthOffset = 0x58;
            WoWUnit_ManaOffset = 0x5C;
            WoWUnit_RageOffset = 0x60;
            WoWUnit_EnergyOffset = 0x68;
            WoWUnit_MaxHealthOffset = 0x70;
            WoWUnit_MaxManaOffset = 0x74;
            WoWUnit_LevelOffset = 0x88;
            WoWUnit_FactionIdOffset = 0x8C;
            WoWUnit_UnitFlagsOffset = 0xB8;
            WoWUnit_BuffsBaseOffset = 0xBC;
            WoWUnit_DebuffsBaseOffset = 0x13C;
            WoWUnit_DynamicFlagsOffset = 0x23C;
            WoWUnit_MovementFlagsOffset = 0x9E8;
            WoWUnit_CurrentSpellcastOffset = 0xC8C;
            LocalPlayerFirstExtraBag = 0x00BDD060;
            SignalEventFunPtr = 0x00703F76;
            SignalEventNoParamsFunPtr = 0x00703E72;
            WardenLoadHook = 0x006CA22E;
            WardenBase = 0x00CE8978;
            WardenPageScanOffset = 0x00002B21;
            WardenMemScanOffset = 0x00002A7F;
        }

        // Functions
        public static int EnumerateVisibleObjectsFunPtr;
        public static int GetObjectPtrFunPtr;
        public static int GetPlayerGuidFunPtr;
        public static int SetFacingFunPtr;
        public static int SendMovementUpdateFunPtr;
        public static int SetControlBitFunPtr;
        public static int SetControlBitDevicePtr;
        public static int GetCreatureTypeFunPtr;
        public static int GetCreatureRankFunPtr;
        public static int GetUnitReactionFunPtr;
        public static int LuaCallFunPtr;
        public static int GetTextFunPtr;
        public static int IntersectFunPtr;
        public static int SetTargetFunPtr;
        public static int RetrieveCorpseFunPtr;
        public static int ReleaseCorpseFunPtr;
        public static int GetItemCacheEntryFunPtr;
        public static int ItemCacheEntryBasePtr;
        public static int IsSpellOnCooldownFunPtr;
        public static int LootSlotFunPtr;
        public static int UseItemFunPtr;
        public static int SellItemByGuidFunPtr;
        public static int BuyVendorItemFunPtr;
        public static int CastAtPositionFunPtr;
        public static int GetAuraFunPtr;
        public static int GetAuraCountFunPtr;

        // Statics
        public static int ZoneTextPtr;
        public static int MinimapZoneTextPtr;
        public static int MapId;
        public static int ServerName;
        public static int LocalPlayerCorpsePositionX;
        public static int LocalPlayerCorpsePositionY;
        public static int LocalPlayerCorpsePositionZ;
        public static int LastHardwareAction;
        public static int LocalPlayerSpellsBase;
        public static int SignalEventFunPtr;
        public static int SignalEventNoParamsFunPtr;
        public static int WardenLoadHook;
        public static int WardenBase;
        public static int WardenPageScanOffset;
        public static int WardenMemScanOffset;
        public static int LocalPlayerFirstExtraBag;
        public static int LocalPlayerClass;

        // Frames
        public static int MerchantFrameItemsBasePtr;
        public static int MerchantFrameItemPtr;
        public static int LootFrameItemOffset;
        public static int MerchantFrameItemOffset;
        public static int DialogFrameBase;
        public static int LootFrameItemsBasePtr;
        public static int CoinCountPtr;

        // Descriptors
        public static int LocalPlayer_BackpackFirstItemOffset;
        public static int WoWItem_ItemIdOffset;
        public static int WoWItem_StackCountOffset;
        public static int WoWItem_DurabilityOffset;
        public static int WoWItem_ContainerFirstItemOffset;
        public static int WoWPet_SpellsBase;
        public static int WoWUnit_SummonedByGuidOffset;
        public static int WoWUnit_TargetGuidOffset;
        public static int WoWUnit_HealthOffset;
        public static int WoWUnit_ManaOffset;
        public static int WoWUnit_RageOffset;
        public static int WoWUnit_EnergyOffset;
        public static int WoWUnit_MaxHealthOffset;
        public static int WoWUnit_MaxManaOffset;
        public static int WoWUnit_LevelOffset;
        public static int WoWUnit_FactionIdOffset;
        public static int WoWUnit_UnitFlagsOffset;
        public static int WoWUnit_DynamicFlagsOffset;
        public static int WoWUnit_CurrentChannelingOffset;

        // Offsets
        public static int WoWObject_DescriptorOffset;
        public static int WoWObject_GetPositionFunOffset;
        public static int WoWObject_GetFacingFunOffset;
        public static int WoWObject_InteractFunOffset;
        public static int WoWObject_GetNameFunOffset;
        public static int LocalPlayer_EquipmentFirstItemOffset;
        public static int WoWItem_ContainerSlotsOffset;
        public static int LocalPlayer_SetFacingOffset;
        public static int WoWUnit_BuffsBaseOffset;
        public static int WoWUnit_DebuffsBaseOffset;
        public static int WoWUnit_MovementFlagsOffset;
        public static int WoWUnit_CurrentSpellcastOffset;
    }
}
