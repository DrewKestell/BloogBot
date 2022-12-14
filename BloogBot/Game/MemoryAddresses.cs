using BloogBot.Game.Enums;

namespace BloogBot.Game
{
    public static class MemoryAddresses
    {
        static MemoryAddresses()
        {
            if (ClientHelper.ClientVersion == ClientVersion.WotLK)
            {
                EnumerateVisibleObjectsFunPtr = 0x004D4B30;
                GetObjectPtrFunPtr = 0x004D4DB0;
                GetPlayerGuidFunPtr = 0x004D3790; // not used. we can get the PlayerGuid by static memory address
                SetFacingFunPtr = 0x0072EA50;
                SendMovementUpdateFunPtr = 0x007413F0;
                SetControlBitFunPtr = 0x005FBE10;
                SetControlBitDevicePtr = 0x00D3F78C;
                JumpFunPtr = 0x006F0DD0;
                GetCreatureTypeFunPtr = 0x0071F300;
                GetCreatureRankFunPtr = 0x00718A00;
                GetUnitReactionFunPtr = 0x007251C0;
                LuaCallFunPtr = 0x00819210;
                GetTextFunPtr = 0x00819D40;
                CastSpellByIdFunPtr = 0x0080DA40;
                GetRow2FunPtr = 0x0065C290;
                IntersectFunPtr = 0x0077F310;
                SetTargetFunPtr = 0x00524BF0;
                RetrieveCorpseFunPtr = 0x0051B800;
                ReleaseCorpseFunPtr = 0x0051AA90;
                GetItemCacheEntryFunPtr = 0x0067CA30;
                ItemCacheEntryBasePtr = 0x00C5D828;
                IsSpellOnCooldownFunPtr = 0x00809000;
                LootSlotFunPtr = 0x00589140;
                UseItemFunPtr = 0x00708C20;
                SellItemByGuidFunPtr = 0x006D2D40;
                BuyVendorItemFunPtr = 0x006D2DE0;
                DismountFunPtr = 0x0051D170;
                CastAtPositionFunPtr = 0x0; // TODO
                GetAuraFunPtr = 0x00556E10;
                GetAuraCountFunPtr = 0x004F8850;
                ZoneTextPtr = 0x00BD0788;
                SubZoneTextPtr = 0x00BD0784;
                MinimapZoneTextPtr = 0x00BD077C;
                MapId = 0x00AB63BC;
                ServerName = 0x00C79B9E;
                LootFrameItemsBasePtr = 0x00C9D340;
                CoinCountPtr = 0x00BFA8D0;
                MerchantFrameItemsBasePtr = 0x00BFA3F0;
                MerchantFrameItemPtr = 0x00BF912C;
                LootFrameItemOffset = 0x18;
                MerchantFrameItemOffset = 0x20;
                DialogFrameBase = 0x00BD07A8;
                LocalPlayer_SetFacingOffset = 0x7A8;
                LocalPlayerCorpsePositionX = 0x00BD0A58;
                LocalPlayerCorpsePositionY = LocalPlayerCorpsePositionX + 0x4;
                LocalPlayerCorpsePositionZ = LocalPlayerCorpsePositionX + 0x8;
                LastHardwareAction = 0x00B499A4;
                LocalPlayerSpellsBase = 0x00BE5D88;
                LocalPlayerClass = 0x00C79E89;
                LocalPlayer_BackpackFirstItemOffset = 0x5C8;
                LocalPlayer_EquipmentFirstItemOffset = 0x1E68;
                LocalPlayerCanOverpower = 0x0; // TODO
                WoWItem_ItemIdOffset = 0xC;
                WoWItem_StackCountOffset = 0x38;
                WoWItem_DurabilityOffset = 0xF0;
                WoWItem_ContainerFirstItemOffset = 0x108;
                WoWItem_ContainerSlotsOffset = 0x760;
                WoWObject_DescriptorOffset = 0x8;
                WoWObject_GetPositionFunOffset = 0x30;
                WoWObject_GetFacingFunOffset = 0x38;
                WoWObject_InteractFunOffset = 0xB0;
                WoWObject_GetNameFunOffset = 0xD8;
                WoWPet_SpellsBase = 0x0; // TODO
                WoWUnit_SummonedByGuidOffset = 0x38;
                WoWUnit_TargetGuidOffset = 0x48;
                WoWUnit_HealthOffset = 0x60;
                WoWUnit_ManaOffset = 0x64;
                WoWUnit_RageOffset = 0x68;
                WoWUnit_EnergyOffset = 0x70;
                WoWUnit_MaxHealthOffset = 0x80;
                WoWUnit_MaxManaOffset = 0x84;
                WoWUnit_LevelOffset = 0xD8;
                WoWUnit_FactionIdOffset = 0x24;
                WoWUnit_UnitFlagsOffset = 0xEC;
                WoWUnit_BuffsBaseOffset = 0xC70;
                WoWUnit_DebuffsBaseOffset = 0x0; // unused - we get debuffs a different way in WotLK
                WoWUnit_DynamicFlagsOffset = 0x13C;
                WoWUnit_CurrentChannelingOffset = 0xA80;
                WoWUnit_MovementFlagsOffset = 0x7CC;
                WoWUnit_CurrentSpellcastOffset = 0xA6C;
                LocalPlayerFirstExtraBag = 0x00C23540;
                SignalEventFunPtr = 0x0; // TODO
                SignalEventNoParamsFunPtr = 0x0; // TODO
                WardenLoadHook = 0x008724C0;
                WardenBase = 0x00A9C414;
                WardenPageScanOffset = 0x00002B21;
                WardenMemScanOffset = 0x00002A7F;
                WowDbTableBase = 0x006337D0;
                GetRowFunPtr = 0x004BB1C0;
                GetLocalizedRowFunPtr = 0x004CFD20;
            }
            else if (ClientHelper.ClientVersion == ClientVersion.TBC)
            {
                EnumerateVisibleObjectsFunPtr = 0x0046B3F0;
                GetObjectPtrFunPtr = 0x0046b520;
                GetPlayerGuidFunPtr = 0x00469DD0;
                SetFacingFunPtr = 0x007B9DE0;
                SendMovementUpdateFunPtr = 0x0060D200;
                SetControlBitFunPtr = 0x005343A0;
                SetControlBitDevicePtr = 0x00CF31E4;
                JumpFunPtr = 0x00534400;
                GetCreatureTypeFunPtr = 0x0060D9A0;
                GetCreatureRankFunPtr = 0x006080E0;
                GetUnitReactionFunPtr = 0x00610C00;
                LuaCallFunPtr = 0x00706C80;
                GetTextFunPtr = 0x00707200;
                CastSpellByIdFunPtr = 0x006FC520;
                GetRow2FunPtr = 0x00466680;
                IntersectFunPtr = 0x006A37B0;
                SetTargetFunPtr = 0x004A6690;
                RetrieveCorpseFunPtr = 0x0049F770;
                ReleaseCorpseFunPtr = 0x005DC310;
                GetItemCacheEntryFunPtr = 0x00591600;
                ItemCacheEntryBasePtr = 0x00D29A98;
                IsSpellOnCooldownFunPtr = 0x006F8100;
                LootSlotFunPtr = 0x004D2750;
                UseItemFunPtr = 0x005F8A50;
                SellItemByGuidFunPtr = 0x005DC6F0;
                BuyVendorItemFunPtr = 0x005DC790;
                DismountFunPtr = 0x00622490;
                CastAtPositionFunPtr = 0x006E60F0;
                GetAuraFunPtr = 0x0;
                GetAuraCountFunPtr = 0x0;
                ZoneTextPtr = 0x00C6E81C;
                SubZoneTextPtr = 0x00C6E818;
                MinimapZoneTextPtr = 0x00C6E810;
                MapId = 0x00E18DB4;
                ServerName = 0x00BDC520;
                LootFrameItemsBasePtr = 0x00C894B4;
                CoinCountPtr = 0x00C896B0;
                MerchantFrameItemsBasePtr = 0x00C89210;
                MerchantFrameItemPtr = 0x00C87F4C;
                LootFrameItemOffset = 0x20;
                MerchantFrameItemOffset = 0x20;
                DialogFrameBase = 0x00C6E958;
                LocalPlayer_SetFacingOffset = 0xBE0;
                LocalPlayerCorpsePositionX = 0x00C6EA80;
                LocalPlayerCorpsePositionY = 0x00C6EA84;
                LocalPlayerCorpsePositionZ = 0x00C6EA88;
                LastHardwareAction = 0x00BE10FC;
                LocalPlayerSpellsBase = 0x00C6FB00;
                LocalPlayerClass = 0xE2578C;
                LocalPlayer_BackpackFirstItemOffset = 0xAE0;
                LocalPlayer_EquipmentFirstItemOffset = 0x3068;
                LocalPlayerCanOverpower = 0x00CF288C;
                WoWItem_ItemIdOffset = 0xC;
                WoWItem_StackCountOffset = 0x38;
                WoWItem_DurabilityOffset = 0xE8;
                WoWItem_ContainerFirstItemOffset = 0xF8;
                WoWItem_ContainerSlotsOffset = 0x778;
                WoWObject_DescriptorOffset = 0x8;
                WoWObject_GetPositionFunOffset = 0x20;
                WoWObject_GetFacingFunOffset = 0x24;
                WoWObject_InteractFunOffset = 0x88;
                WoWObject_GetNameFunOffset = 0xA8;
                WoWPet_SpellsBase = 0x00C70B00;
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
                WoWUnit_BuffsBaseOffset = 0xC0;
                WoWUnit_DebuffsBaseOffset = 0xE20;
                WoWUnit_DynamicFlagsOffset = 0x290;
                WoWUnit_CurrentChannelingOffset = 0x240;
                WoWUnit_MovementFlagsOffset = 0xC20;
                WoWUnit_CurrentSpellcastOffset = 0xF3C;
                LocalPlayerFirstExtraBag = 0x00CF19C8;
                SignalEventFunPtr = 0x00000000; // TODO
                SignalEventNoParamsFunPtr = 0x00000000; // TODO
                WardenLoadHook = 0x006D0BFC;
                WardenBase = 0x00E118D4;
                WardenPageScanOffset = 0x00002B21;
                WardenMemScanOffset = 0x00002A7F;
                WowDbTableBase = 0x0; // only used in WotLK
                GetRowFunPtr = 0x0; // only used in WotLK
                GetLocalizedRowFunPtr = 0x0; // only used in WotLK
            }
            else if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
            {
                EnumerateVisibleObjectsFunPtr = 0x00468380;
                GetObjectPtrFunPtr = 0x00464870;
                GetPlayerGuidFunPtr = 0x00468550;
                SetFacingFunPtr = 0x007C6F30;
                SendMovementUpdateFunPtr = 0x00600A30;
                SetControlBitFunPtr = 0x00515090;
                SetControlBitDevicePtr = 0x00BE1148;
                JumpFunPtr = 0x00000000;// unused in Vanilla
                GetCreatureTypeFunPtr = 0x00605570;
                GetCreatureRankFunPtr = 0x00605620;
                GetUnitReactionFunPtr = 0x006061E0;
                LuaCallFunPtr = 0x00704CD0;
                GetTextFunPtr = 0x00703BF0;
                CastSpellByIdFunPtr = 0x00000000;  // unused in Vanilla
                GetRow2FunPtr = 0x00000000; // unused in Vanilla
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
                DismountFunPtr = 0x00000000; // TODO
                CastAtPositionFunPtr = 0x006E60F0;
                GetAuraFunPtr = 0x0;
                GetAuraCountFunPtr = 0x0;
                ZoneTextPtr = 0x00B4B404;
                SubZoneTextPtr = 0x00000000; // TODO
                MinimapZoneTextPtr = 0x00B4DA28;
                MapId = 0x00000000; // unused in vanilla
                ServerName = 0x00000000; // TODO
                LootFrameItemsBasePtr = 0x00B7196C;
                CoinCountPtr = 0x00B71BA0;
                MerchantFrameItemsBasePtr = 0x00BDDFA8;
                MerchantFrameItemPtr = 0x00BDD11C;
                LootFrameItemOffset = 0x1C;
                MerchantFrameItemOffset = 0x1C;
                DialogFrameBase = 0x00000000; // unused in vanilla
                LocalPlayer_SetFacingOffset = 0x9A8;
                LocalPlayerCorpsePositionX = 0x00B4E284;
                LocalPlayerCorpsePositionY = 0x00B4E288;
                LocalPlayerCorpsePositionZ = 0x00B4E28C;
                LastHardwareAction = 0x00CF0BC8;
                LocalPlayerSpellsBase = 0x00B700F0;
                LocalPlayerClass = 0x00C27E81;
                LocalPlayer_BackpackFirstItemOffset = 0x850;
                LocalPlayer_EquipmentFirstItemOffset = 0x2508;
                LocalPlayerCanOverpower = 0x00000000; // unused in vanilla
                WoWItem_ItemIdOffset = 0xC;
                WoWItem_StackCountOffset = 0x38;
                WoWItem_DurabilityOffset = 0xB8;
                WoWItem_ContainerFirstItemOffset = 0xC0;
                WoWItem_ContainerSlotsOffset = 0x6c8;
                WoWObject_DescriptorOffset = 0x8;
                WoWObject_GetPositionFunOffset = 0x20; // unused in vanilla
                WoWObject_GetFacingFunOffset = 0x24; // unused in vanilla
                WoWObject_InteractFunOffset = 0x88; // unused in vanilla
                WoWObject_GetNameFunOffset = 0xA8; // unused in vanilla
                WoWPet_SpellsBase = 0x0; // TODO
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
                WoWUnit_CurrentChannelingOffset = 0x0; // unused in vanilla
                WoWUnit_MovementFlagsOffset = 0x9E8;
                WoWUnit_CurrentSpellcastOffset = 0xC8C;
                LocalPlayerFirstExtraBag = 0x00BDD060;
                SignalEventFunPtr = 0x00703F76;
                SignalEventNoParamsFunPtr = 0x00703E72;
                WardenLoadHook = 0x006CA22E;
                WardenBase = 0x00CE8978;
                WardenPageScanOffset = 0x00002B21;
                WardenMemScanOffset = 0x00002A7F;
                WowDbTableBase = 0x0; // unused in vanilla
                GetRowFunPtr = 0x0; // unused in vanilla
                GetLocalizedRowFunPtr = 0x0; // unused in vanilla
            }
        }

        // Functions
        public static int EnumerateVisibleObjectsFunPtr;
        public static int GetObjectPtrFunPtr;
        public static int GetPlayerGuidFunPtr;
        public static int SetFacingFunPtr;
        public static int SendMovementUpdateFunPtr;
        public static int SetControlBitFunPtr;
        public static int SetControlBitDevicePtr;
        public static int JumpFunPtr;
        public static int GetCreatureTypeFunPtr;
        public static int GetCreatureRankFunPtr;
        public static int GetUnitReactionFunPtr;
        public static int LuaCallFunPtr;
        public static int GetTextFunPtr;
        public static int CastSpellByIdFunPtr;
        public static int GetRow2FunPtr;
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
        public static int DismountFunPtr;
        public static int CastAtPositionFunPtr;
        public static int GetAuraFunPtr;
        public static int GetAuraCountFunPtr;
        public static int GetRowFunPtr;
        public static int GetLocalizedRowFunPtr;

        // Statics
        public static int ZoneTextPtr;
        public static int SubZoneTextPtr;
        public static int MinimapZoneTextPtr;
        public static int MapId;
        public static int ServerName;
        public static int LocalPlayerCorpsePositionX;
        public static int LocalPlayerCorpsePositionY;
        public static int LocalPlayerCorpsePositionZ;
        public static int LastHardwareAction;
        public static int LocalPlayerSpellsBase;
        public static int LocalPlayerCanOverpower;
        public static int SignalEventFunPtr;
        public static int SignalEventNoParamsFunPtr;
        public static int WardenLoadHook;
        public static int WardenBase;
        public static int WardenPageScanOffset;
        public static int WardenMemScanOffset;
        public static int LocalPlayerFirstExtraBag;
        public static int LocalPlayerClass;
        public static int WowDbTableBase;

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
