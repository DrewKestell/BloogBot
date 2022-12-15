using BloogBot.Game.Enums;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public class WotLKGameFunctionHandler : IGameFunctionHandler
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void BuyVendorItemDelegate(ulong vendorGuid, int itemId, int quantity, int unused);

        static readonly BuyVendorItemDelegate BuyVendorItemFunction =
            Marshal.GetDelegateForFunctionPointer<BuyVendorItemDelegate>((IntPtr)MemoryAddresses.BuyVendorItemFunPtr);

        public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            BuyVendorItemFunction(vendorGuid, itemId, quantity, 1);
        }

        public void CastAtPosition(string spellName, Position position)
        {
            throw new NotImplementedException();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int CastSpellByIdDelegate(int spellId, int itemId, ulong guid, int isTrade, int a6, int a7, int a8);

        static readonly CastSpellByIdDelegate CastSpellByIdFunction =
            Marshal.GetDelegateForFunctionPointer<CastSpellByIdDelegate>((IntPtr)MemoryAddresses.CastSpellByIdFunPtr);

        public int CastSpellById(int spellId, ulong targetGuid)
        {
            return CastSpellByIdFunction(spellId, 0, targetGuid, 0, 0, 0, 0);
        }

        public int Dismount(IntPtr unitPtr)
        {
            LuaCall("Dismount()");

            return 0;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate char EnumerateVisibleObjectsDelegate(IntPtr callback, int filter);

        static readonly EnumerateVisibleObjectsDelegate EnumerateVisibleObjectsFunction =
            Marshal.GetDelegateForFunctionPointer<EnumerateVisibleObjectsDelegate>((IntPtr)MemoryAddresses.EnumerateVisibleObjectsFunPtr);

        [HandleProcessCorruptedStateExceptions]
        public void EnumerateVisibleObjects(IntPtr callback, int filter)
        {
            EnumerateVisibleObjectsFunction(callback, filter);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetCreatureRankDelegate
            (IntPtr unitPtr);

        static readonly GetCreatureRankDelegate GetCreatureRankFunction =
            Marshal.GetDelegateForFunctionPointer<GetCreatureRankDelegate>((IntPtr)MemoryAddresses.GetCreatureRankFunPtr);

        public int GetCreatureRank(IntPtr unitPtr)
        {
            return GetCreatureRankFunction(unitPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetCreatureTypeDelegate(IntPtr unitPtr);

        static readonly GetCreatureTypeDelegate GetCreatureTypeFunction =
            Marshal.GetDelegateForFunctionPointer<GetCreatureTypeDelegate>((IntPtr)MemoryAddresses.GetCreatureTypeFunPtr);

        public CreatureType GetCreatureType(IntPtr unitPtr)
        {
            return (CreatureType)GetCreatureTypeFunction(unitPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr ItemCacheGetRowDelegate(
            IntPtr ptr,
            int itemId,
            ref ulong guid,
            int unused1,
            int unused2,
            int unused3);

        static readonly ItemCacheGetRowDelegate GetItemCacheEntryFunction =
            Marshal.GetDelegateForFunctionPointer<ItemCacheGetRowDelegate>((IntPtr)MemoryAddresses.GetItemCacheEntryFunPtr);

        public IntPtr GetItemCacheEntry(int itemId, ulong guid)
        {
            return GetItemCacheEntryFunction((IntPtr)MemoryAddresses.ItemCacheEntryBasePtr, itemId, ref guid, 0, 0, 0);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr GetObjectPtrDelegate(ulong objectGuid, uint typeMask, string file, int line);

        static readonly GetObjectPtrDelegate GetObjectPtrFunction =
            Marshal.GetDelegateForFunctionPointer<GetObjectPtrDelegate>((IntPtr)MemoryAddresses.GetObjectPtrFunPtr);

        [HandleProcessCorruptedStateExceptions]
        public IntPtr GetObjectPtr(ulong guid)
        {
            return GetObjectPtrFunction(guid, 0xFFFFFFFF, string.Empty, 0);
        }

        public ulong GetPlayerGuid()
        {
            return MemoryManager.ReadUlong((IntPtr)0x00CA1238);
        }

        public Spell GetSpellDBEntry(int index)
        {
            var spellPtr = WowDb.Tables[ClientDb.Spell].GetLocalizedRow(index);

            var costAddr = IntPtr.Add(spellPtr, 0xA8);
            var cost = MemoryManager.ReadInt(costAddr);

            var spellNamePtrAddr = IntPtr.Add(spellPtr, 0x220);
            var spellNamePtr = MemoryManager.ReadIntPtr(spellNamePtrAddr);
            var name = MemoryManager.ReadString(spellNamePtr);

            var spellDescriptionPtrAddr = IntPtr.Add(spellPtr, 0x228);
            var spellDescriptionPtr = MemoryManager.ReadIntPtr(spellDescriptionPtrAddr);
            var description = MemoryManager.ReadString(spellDescriptionPtr);

            var spellTooltipPtrAddr = IntPtr.Add(spellPtr, 0x22C);
            var spellTooltipPtr = MemoryManager.ReadIntPtr(spellTooltipPtrAddr);
            var tooltip = MemoryManager.ReadString(spellTooltipPtr);

            Marshal.FreeHGlobal(spellPtr);

            return new Spell(index, cost, name, description, tooltip);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate IntPtr GetTextDelegate(string arg);

        static readonly GetTextDelegate GetTextFunction =
            Marshal.GetDelegateForFunctionPointer<GetTextDelegate>((IntPtr)MemoryAddresses.GetTextFunPtr);

        public IntPtr GetText(string varName)
        {
            return GetTextFunction(varName);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetUnitReactionDelegate(IntPtr unitPtr1, IntPtr unitPtr2);

        static readonly GetUnitReactionDelegate GetUnitReactionFunction =
            Marshal.GetDelegateForFunctionPointer<GetUnitReactionDelegate>((IntPtr)MemoryAddresses.GetUnitReactionFunPtr);

        public UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2)
        {
            return (UnitReaction)GetUnitReactionFunction(unitPtr1, unitPtr2);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int IntersectDelegate(ref XYZ start, ref XYZ end, ref XYZ intersection, ref float distance, uint flags, IntPtr Ptr);

        static readonly IntersectDelegate IntersectFunction =
            Marshal.GetDelegateForFunctionPointer<IntersectDelegate>((IntPtr)MemoryAddresses.IntersectFunPtr);

        public XYZ Intersect(Position start, Position end)
        {
            var intersection = new XYZ();
            var distance = start.DistanceTo(end);
            var startXYZ = new XYZ(start.X, start.Y, start.Z + 5.0f);
            var endXYZ = new XYZ(end.X, end.Y, end.Z + 5.0f);

            IntersectFunction(ref startXYZ, ref endXYZ, ref intersection, ref distance, 0x00100171, IntPtr.Zero);

            return intersection;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool GetSpellCooldownDelegate(int spellId, bool isPet, ref int duration, ref int start, ref bool isReady, ref int unk0);

        static readonly GetSpellCooldownDelegate GetSpellCooldownFunction =
            Marshal.GetDelegateForFunctionPointer<GetSpellCooldownDelegate>((IntPtr)MemoryAddresses.IsSpellOnCooldownFunPtr);

        public bool IsSpellOnCooldown(int spellId)
        {
            var duration = 0;
            var start = 0;
            var isReady = false;
            var unk0 = 0;
            GetSpellCooldownFunction(spellId, false, ref duration, ref start, ref isReady, ref unk0);

            var result = start + duration - (int)PerformanceCounter();
            var cooldown = isReady ? (result > 0 ? result / 1000f : 0f) : float.MaxValue;

            return cooldown > 0;
        }

        public void Jump()
        {
            LuaCall("JumpOrAscendStart()");
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void LootSlotDelegate(int slot);

        static readonly LootSlotDelegate LootSlotFunction =
            Marshal.GetDelegateForFunctionPointer<LootSlotDelegate>((IntPtr)MemoryAddresses.LootSlotFunPtr);

        public void LootSlot(int slot)
        {
            LootSlotFunction(slot);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int LuaCallDelegate(string code1, string code2, int unknown);

        static readonly LuaCallDelegate LuaCallFunction =
            Marshal.GetDelegateForFunctionPointer<LuaCallDelegate>((IntPtr)MemoryAddresses.LuaCallFunPtr);

        public void LuaCall(string code)
        {
            LuaCallFunction(code, code, 0);
        }

        public void ReleaseCorpse(IntPtr ptr)
        {
            LuaCall("RepopMe()");
        }

        public void RetrieveCorpse()
        {
            LuaCall("RetrieveCorpse()");
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SetTargetDelegate(ulong guid);

        static readonly SetTargetDelegate SetTargetFunction =
            Marshal.GetDelegateForFunctionPointer<SetTargetDelegate>((IntPtr)MemoryAddresses.SetTargetFunPtr);

        public void SetTarget(ulong guid)
        {
            SetTargetFunction(guid);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SellItemByGuidDelegate(ulong vendorGuid, ulong itemGuid, int unused);

        static readonly SellItemByGuidDelegate SellItemByGuidFunction =
            Marshal.GetDelegateForFunctionPointer<SellItemByGuidDelegate>((IntPtr)MemoryAddresses.SellItemByGuidFunPtr);

        public void SellItemByGuid(uint itemCount, ulong vendorGuid, ulong itemGuid)
        {
            SellItemByGuidFunction(vendorGuid, itemGuid, 0);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void SendMovementUpdateDelegate(
            IntPtr playerPtr,
            IntPtr unknown,
            int OpCode,
            int unknown2,
            int unknown3);

        static readonly SendMovementUpdateDelegate SendMovementUpdateFunction =
            Marshal.GetDelegateForFunctionPointer<SendMovementUpdateDelegate>((IntPtr)MemoryAddresses.SendMovementUpdateFunPtr);

        public void SendMovementUpdate(IntPtr playerPtr, int opcode)
        {
            SendMovementUpdateFunction(playerPtr, (IntPtr)0x00BE1E2C, opcode, 0, 0);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void SetControlBitDelegate(IntPtr device, int bit, int state, int tickCount);

        static readonly SetControlBitDelegate SetControlBitFunction =
            Marshal.GetDelegateForFunctionPointer<SetControlBitDelegate>((IntPtr)MemoryAddresses.SetControlBitFunPtr);

        public void SetControlBit(int bit, int state, int tickCount)
        {
            var ptr = MemoryManager.ReadIntPtr((IntPtr)MemoryAddresses.SetControlBitDevicePtr);
            SetControlBitFunction(ptr, bit, state, tickCount);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void SetFacingDelegate(IntPtr playerSetFacingPtr, uint time, float facing);

        static readonly SetFacingDelegate SetFacingFunction =
            Marshal.GetDelegateForFunctionPointer<SetFacingDelegate>((IntPtr)MemoryAddresses.SetFacingFunPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate uint PerformanceCounterDelegate();

        static readonly PerformanceCounterDelegate PerformanceCounter =
            Marshal.GetDelegateForFunctionPointer<PerformanceCounterDelegate>((IntPtr)0x0086AE20);

        public void SetFacing(IntPtr playerSetFacingPtr, float facing)
        {
            var performanceCounter = PerformanceCounter();
            SetFacingFunction(playerSetFacingPtr, performanceCounter, facing);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void UseItemDelegate(IntPtr itemPtr, ref ulong unused1, int unused2);

        static readonly UseItemDelegate UseItemFunction =
            Marshal.GetDelegateForFunctionPointer<UseItemDelegate>((IntPtr)MemoryAddresses.UseItemFunPtr);

        public void UseItem(IntPtr itemPtr)
        {
            ulong unused1 = 0;
            UseItemFunction(itemPtr, ref unused1, 0);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr GetRowDelegate(IntPtr tablePtr, int index);

        static readonly GetRowDelegate GetRowFunction =
            Marshal.GetDelegateForFunctionPointer<GetRowDelegate>((IntPtr)MemoryAddresses.GetRowFunPtr);

        public IntPtr GetRow(IntPtr tablePtr, int index)
        {
            return GetRowFunction(tablePtr, index);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr GetLocalizedRowDelegate(IntPtr tablePtr, int index, IntPtr rowPtr);

        static readonly GetLocalizedRowDelegate GetLocalizedRowFunction =
            Marshal.GetDelegateForFunctionPointer<GetLocalizedRowDelegate>((IntPtr)MemoryAddresses.GetLocalizedRowFunPtr);

        public IntPtr GetLocalizedRow(IntPtr tablePtr, int index, IntPtr rowPtr)
        {
            return GetLocalizedRowFunction(tablePtr, index, rowPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetAuraCountDelegate(IntPtr thisObj);

        static readonly GetAuraCountDelegate GetAuraCountFunction =
            Marshal.GetDelegateForFunctionPointer<GetAuraCountDelegate>((IntPtr)MemoryAddresses.GetAuraCountFunPtr);

        public int GetAuraCount(IntPtr unitPtr)
        {
            return GetAuraCountFunction(unitPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr GetAuraDelegate(IntPtr unitPtr, int index);

        static readonly GetAuraDelegate GetAuraFunction =
            Marshal.GetDelegateForFunctionPointer<GetAuraDelegate>((IntPtr)MemoryAddresses.GetAuraFunPtr);

        public IntPtr GetAuraPointer(IntPtr unitPtr, int index)
        {
            return GetAuraFunction(unitPtr, index);
        }
    }
}
