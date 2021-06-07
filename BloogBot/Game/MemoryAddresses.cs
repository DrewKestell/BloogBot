using BloogBot.Game.Enums;
using System;
using System.Diagnostics;

namespace BloogBot.Game
{
    public static class MemoryAddresses
    {
        public static readonly ClientVersion ClientVersion;

        static MemoryAddresses()
        {
            var clientVersion = Process.GetProcessesByName("WoW")[0].MainModule.FileVersionInfo.FileVersion;

            if (clientVersion == "2, 4, 3, 8606")
            {
                ClientVersion = ClientVersion.TBC;

                EnumerateVisibleObjectsFunPtr = 0x0046B3F0;
                GetObjectPtrFunPtr = 0x0046b520;
                GetPlayerGuidFunPtr = 0x00469DD0;
                SetFacingFunPtr = 0x007B9DE0;
                SendMovementUpdateFunPtr = 0x0060D200;
                SetControlBitFunPtr = 0x005343A0;
                JumpFunPtr = 0x00534400;
                GetCreatureTypeFunPtr = 0x0060D9A0;
                GetCreatureRankFunPtr = 0x006080E0;
                GetUnitReactionFunPtr = 0x00610C00;
                LuaCallFunPtr = 0x00706C80;
                GetTextFunPtr = 0x00707200;
                CastSpellByIdFunPtr = 0x006FC520;
                GetRow2FunPtr = 0x00466680;
                IntersectFunPtr = 0x006A37B0;
                SelectObjectFunPtr = 0x004A6690;
                IsLootingFunPtr = 0x0060B3E0;
                CanLootFunPtr = 0x005DE280;
                LootUnitFunPtr = 0x005E2460;
                CanAttackFunPtr = 0x00613BD0;
                IsDeadFunPtr = 0x005E22C0;
                RetrieveCorpseFunPtr = 0x0049F770;
                ReleaseCorpseFunPtr = 0x005DC310;
                GetItemCacheEntryFunPtr = 0x00591600;
                IsSpellOnCooldownFunPtr = 0x006F8100;
                LootSlotFunPtr = 0x004D2750;
                UseItemFunPtr = 0x005F8A50;
                SellItemByGuidFunPtr = 0x005DC6F0;
                BuyVendorItemFunPtr = 0x005DC790;
                DismountFunPtr = 0x00622490;
                CastAtPositionFunPtr = 0x006E60F0;
                ZoneTextPtrAddr = 0x00C6E81C;
                SubZoneTextPtrAddr = 0x00C6E818;
                MinimapZoneTextPtrAddr = 0x00C6E810;
                MapIdAddr = 0x00E18DB4;
                ServerNameAddr = 0x00BDC520;
                LootFrameItemsBasePtr = 0x00C894B4;
                CoinCountPtr = 0x00C896B0;
                MerchantFrameItemsBasePtr = 0x00C89210;
                MerchantFrameItemPtr = 0x00C87F4C;
                FrameItemOffset = 0x20;
                DialogFrameBaseAddr = 0x00C6E958;
                LocalPlayer_SetFacingOffset = 0xBE0;
                LocalPlayer_CorpsePositionX = 0x00C6EA80;
                LocalPlayer_CorpsePositionY = 0x00C6EA84;
                LocalPlayer_CorpsePositionZ = 0x00C6EA88;
                LocalPlayer_LastHardwareAction = 0x00BE10FC;
                LocalPlayer_PlayerSpellsBase = 0x00C6FB00;
                LocalPlayer_ClassOffset = 0xE2578C;
                LocalPlayer_BackpackFirstItemOffset = 0xAE0;
                LocalPlayer_EquipmentFirstItemOffset = 0x3068;
                LocalPlayer_CanOverpowerAddr = 0x00CF288C;
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
                FirstExtraBagAddr = 0x00CF19C8;
                SignalEventFunPtr = 0x00000000; // TODO
                SignalEventNoParamsFunPtr = 0x00000000; // TODO
                WardenLoadHookAddr = 0x006D0BFC;
                WardenPageScanOffset = 0x00002B21;
                WardenMemScanOffset = 0x00002A7F;
            }
            else if (clientVersion == "1, 12, 1, 5875")
            {
                ClientVersion = ClientVersion.Vanilla;

                EnumerateVisibleObjectsFunPtr = 0x00468380;
                GetObjectPtrFunPtr = 0x00464870;
                GetPlayerGuidFunPtr = 0x00468550;
                SetFacingFunPtr = 0x007C6F30;
                SendMovementUpdateFunPtr = 0x00600A30;
                SetControlBitFunPtr = 0x00515090;
                JumpFunPtr = 0x00000000; // TODO
                GetCreatureTypeFunPtr = 0x00605570;
                GetCreatureRankFunPtr = 0x00605620;
                GetUnitReactionFunPtr = 0x006061E0;
                LuaCallFunPtr = 0x00704CD0;
                GetTextFunPtr = 0x00703BF0;
                CastSpellByIdFunPtr = 0x00000000; // TODO
                GetRow2FunPtr = 0x00000000; // TODO
                IntersectFunPtr = 0x006AA160;
                SelectObjectFunPtr = 0x00000000; // TODO
                IsLootingFunPtr = 0x00000000; // TODO
                CanLootFunPtr = 0x00000000; // TODO
                LootUnitFunPtr = 0x00000000; // TODO
                CanAttackFunPtr = 0x00000000; // TODO
                IsDeadFunPtr = 0x00000000; // TODO
                RetrieveCorpseFunPtr = 0x0048D260;
                ReleaseCorpseFunPtr = 0x005E0AE0;
                GetItemCacheEntryFunPtr = 0x0055BA30;
                IsSpellOnCooldownFunPtr = 0x006E13E0;
                LootSlotFunPtr = 0x004C2790;
                UseItemFunPtr = 0x005D8D00;
                SellItemByGuidFunPtr = 0x005E1D50;
                BuyVendorItemFunPtr = 0x005E1E90;
                DismountFunPtr = 0x00000000; // TODO
                CastAtPositionFunPtr = 0x006E60F0;
                ZoneTextPtrAddr = 0x00B4B404;
                SubZoneTextPtrAddr = 0x00000000; // TODO
                MinimapZoneTextPtrAddr = 0x00B4DA28;
                MapIdAddr = 0x00000000; // TODO
                ServerNameAddr = 0x00000000; // TODO
                LootFrameItemsBasePtr = 0x00B7196C;
                CoinCountPtr = 0x00B71BA0;
                MerchantFrameItemsBasePtr = 0x00BDDFA0;
                MerchantFrameItemPtr = 0x00BDD11C;
                FrameItemOffset = 0x1C;
                DialogFrameBaseAddr = 0x00000000; // TODO
                LocalPlayer_SetFacingOffset = 0x9A8;
                LocalPlayer_CorpsePositionX = 0x00B4E284;
                LocalPlayer_CorpsePositionY = 0x00B4E288;
                LocalPlayer_CorpsePositionZ = 0x00B4E28C;
                LocalPlayer_LastHardwareAction = 0x00CF0BC8;
                LocalPlayer_PlayerSpellsBase = 0x00B700F0;
                LocalPlayer_ClassOffset = 0x00000000; // TODO
                LocalPlayer_BackpackFirstItemOffset = 0x00000000; // TODO
                LocalPlayer_EquipmentFirstItemOffset = 0x00000000; // TODO
                LocalPlayer_CanOverpowerAddr = 0x00000000; // TODO
                WoWItem_ItemIdOffset = 0xC;
                WoWItem_StackCountOffset = 0x38;
                WoWItem_DurabilityOffset = 0xB8;
                WoWItem_ContainerFirstItemOffset = 0x00000000; // TODO
                WoWItem_ContainerSlotsOffset = 0x6c8;
                WoWObject_DescriptorOffset = 0x00000000; // TODO
                WoWObject_GetPositionFunOffset = 0x00000000; // TODO
                WoWObject_GetFacingFunOffset = 0x00000000; // TODO
                WoWObject_InteractFunOffset = 0x00000000; // TODO
                WoWObject_GetNameFunOffset = 0x00000000; // TODO
                WoWPet_SpellsBase = 0x00000000; // TODO
                WoWUnit_SummonedByGuidOffset = 0x30; // TODO
                WoWUnit_TargetGuidOffset = 0x40; // TODO
                WoWUnit_HealthOffset = 0x58; // TODO
                WoWUnit_ManaOffset = 0x5C; // TODO
                WoWUnit_RageOffset = 0x60; // TODO
                WoWUnit_EnergyOffset = 0x68; // TODO
                WoWUnit_MaxHealthOffset = 0x70; // TODO
                WoWUnit_MaxManaOffset = 0x74; // TODO
                WoWUnit_LevelOffset = 0x88; // TODO
                WoWUnit_FactionIdOffset = 0x8C; // TODO
                WoWUnit_UnitFlagsOffset = 0xB8; // TODO
                WoWUnit_BuffsBaseOffset = 0xBC; // TODO
                WoWUnit_DebuffsBaseOffset = 0x13C; // TODO
                WoWUnit_DynamicFlagsOffset = 0x23C; // TODO
                WoWUnit_CurrentChannelingOffset = 0x240; // TODO
                WoWUnit_MovementFlagsOffset = 0x9E8; // TODO
                WoWUnit_CurrentSpellcastOffset = 0xC8C; // TODO
                FirstExtraBagAddr = 0x00000000; // TODO
                SignalEventFunPtr = 0x00703F76;
                SignalEventNoParamsFunPtr = 0x00703E72;
                WardenLoadHookAddr = 0x006CA22E;
                WardenPageScanOffset = 0x00002B21;
                WardenMemScanOffset = 0x00002A7F;
            }
            else
                throw new InvalidOperationException("Unknown client version.");
        }

        public static int EnumerateVisibleObjectsFunPtr;

        public static int GetObjectPtrFunPtr;

        public static int GetPlayerGuidFunPtr;

        public static int SetFacingFunPtr;

        public static int SendMovementUpdateFunPtr;

        public static int SetControlBitFunPtr;

        public static int JumpFunPtr;

        public static int GetCreatureTypeFunPtr;

        public static int GetCreatureRankFunPtr;

        public static int GetUnitReactionFunPtr;

        public static int LuaCallFunPtr;

        public static int GetTextFunPtr;

        public static int CastSpellByIdFunPtr;

        public static int GetRow2FunPtr;

        public static int IntersectFunPtr; // not tested in TBC

        public static int SelectObjectFunPtr;

        public static int IsLootingFunPtr;

        public static int CanLootFunPtr;

        public static int LootUnitFunPtr;

        public static int CanAttackFunPtr;

        public static int IsDeadFunPtr;

        public static int RetrieveCorpseFunPtr;

        public static int ReleaseCorpseFunPtr;

        public static int GetItemCacheEntryFunPtr;

        public static int IsSpellOnCooldownFunPtr;

        public static int LootSlotFunPtr;

        public static int UseItemFunPtr;

        public static int SellItemByGuidFunPtr;

        public static int BuyVendorItemFunPtr;

        public static int DismountFunPtr; // not tested in TBC

        public static int CastAtPositionFunPtr; // not tested in TBC

        public static int ZoneTextPtrAddr;

        public static int SubZoneTextPtrAddr;

        public static int MinimapZoneTextPtrAddr;

        public static int MapIdAddr;

        public static int ServerNameAddr;

        public static int LootFrameItemsBasePtr;

        public static int CoinCountPtr;

        public static int MerchantFrameItemsBasePtr;

        public static int MerchantFrameItemPtr;

        public static int FrameItemOffset;

        public static int DialogFrameBaseAddr;

        public static int LocalPlayer_SetFacingOffset;

        public static int LocalPlayer_CorpsePositionX;

        public static int LocalPlayer_CorpsePositionY;

        public static int LocalPlayer_CorpsePositionZ;

        public static int LocalPlayer_LastHardwareAction;

        public static int LocalPlayer_PlayerSpellsBase;

        public static int LocalPlayer_ClassOffset;

        public static int LocalPlayer_BackpackFirstItemOffset;

        public static int LocalPlayer_EquipmentFirstItemOffset;

        public static int LocalPlayer_CanOverpowerAddr;

        public static int WoWItem_ItemIdOffset;

        public static int WoWItem_StackCountOffset;

        public static int WoWItem_DurabilityOffset;

        public static int WoWItem_ContainerFirstItemOffset;

        public static int WoWItem_ContainerSlotsOffset;

        public static int WoWObject_DescriptorOffset;

        public static int WoWObject_GetPositionFunOffset;

        public static int WoWObject_GetFacingFunOffset;

        public static int WoWObject_InteractFunOffset;

        public static int WoWObject_GetNameFunOffset;

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

        public static int WoWUnit_BuffsBaseOffset;

        public static int WoWUnit_DebuffsBaseOffset;

        public static int WoWUnit_DynamicFlagsOffset;

        public static int WoWUnit_CurrentChannelingOffset;

        public static int WoWUnit_MovementFlagsOffset;

        public static int WoWUnit_CurrentSpellcastOffset;

        public static int FirstExtraBagAddr;

        public static int SignalEventFunPtr;

        public static int SignalEventNoParamsFunPtr;

        public static int WardenLoadHookAddr;

        public static int WardenPageScanOffset;

        public static int WardenMemScanOffset;
    }
}
