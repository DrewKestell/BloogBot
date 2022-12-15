using BloogBot.Game.Enums;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public class TBCGameFunctionHandler : IGameFunctionHandler
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void BuyVendorItemDelegate(ulong vendorGuid, int itemId, int quantity, int unused);

        static readonly BuyVendorItemDelegate BuyVendorItemFunction =
            Marshal.GetDelegateForFunctionPointer<BuyVendorItemDelegate>((IntPtr)MemoryAddresses.BuyVendorItemFunPtr);

        public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            BuyVendorItemFunction(vendorGuid, itemId, quantity, 1);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int CastAtPositionDelegate(ref XYZ parPos);

        static readonly CastAtPositionDelegate CastAtPositionFunction =
            Marshal.GetDelegateForFunctionPointer<CastAtPositionDelegate>((IntPtr)MemoryAddresses.CastAtPositionFunPtr);

        public void CastAtPosition(string spellName, Position position)
        {
            MemoryManager.WriteByte((IntPtr)0xCECAC0, 0);
            LuaCall($"CastSpellByName('{spellName}')");
            var pos = position.ToXYZ();
            CastAtPositionFunction(ref pos);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int CastSpellByIdDelegate(int spellId, int unknown, ulong targetGuid);

        static readonly CastSpellByIdDelegate CastSpellByIdFunction =
            Marshal.GetDelegateForFunctionPointer<CastSpellByIdDelegate>((IntPtr)MemoryAddresses.CastSpellByIdFunPtr);

        public int CastSpellById(int spellId, ulong targetGuid)
        {
            return CastSpellByIdFunction(spellId, 0, targetGuid);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int DismountDelegate(IntPtr unitPtr);

        static readonly DismountDelegate DismountFunction =
            Marshal.GetDelegateForFunctionPointer<DismountDelegate>((IntPtr)MemoryAddresses.DismountFunPtr);

        public int Dismount(IntPtr unitPtr)
        {
            return DismountFunction(unitPtr);
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
            IntPtr unknown,
            int unused1,
            int unused2,
            char unused3);

        static readonly ItemCacheGetRowDelegate GetItemCacheEntryFunction =
            Marshal.GetDelegateForFunctionPointer<ItemCacheGetRowDelegate>((IntPtr)MemoryAddresses.GetItemCacheEntryFunPtr);

        public IntPtr GetItemCacheEntry(int itemId, ulong guid)
        {
            return GetItemCacheEntryFunction((IntPtr)MemoryAddresses.ItemCacheEntryBasePtr, itemId, IntPtr.Zero, 0, 0, (char)0);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate IntPtr GetObjectPtrDelegate(ulong guid);

        static readonly GetObjectPtrDelegate GetObjectPtrFunction =
            Marshal.GetDelegateForFunctionPointer<GetObjectPtrDelegate>((IntPtr)MemoryAddresses.GetObjectPtrFunPtr);

        public IntPtr GetObjectPtr(ulong guid)
        {
            return GetObjectPtrFunction(guid);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate ulong GetPlayerGuidDelegate();

        static GetPlayerGuidDelegate GetPlayerGuidFunction =
            Marshal.GetDelegateForFunctionPointer<GetPlayerGuidDelegate>((IntPtr)MemoryAddresses.GetPlayerGuidFunPtr);

        public ulong GetPlayerGuid()
        {
            return GetPlayerGuidFunction();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetRow2Delegate(IntPtr ptr, int index, byte[] buffer);

        static readonly GetRow2Delegate GetRow2Function =
            Marshal.GetDelegateForFunctionPointer<GetRow2Delegate>((IntPtr)MemoryAddresses.GetRow2FunPtr);

        public Spell GetSpellDBEntry(int index)
        {
            var buffer = new byte[0x260];

            GetRow2Function((IntPtr)0x00BA0BE0, index, buffer);

            var spellId = BitConverter.ToInt32(buffer, 0);
            var cost = BitConverter.ToInt32(buffer, 0x90);
            var spellNamePtr = BitConverter.ToInt32(buffer, 0x1FC);
            var spellName = "";
            if (spellNamePtr != 0)
                spellName = MemoryManager.ReadString((IntPtr)spellNamePtr);
            var spellDescriptionPtr = BitConverter.ToInt32(buffer, 0x204);
            var spellDescription = "";
            if (spellDescriptionPtr != 0)
                spellDescription = MemoryManager.ReadString((IntPtr)spellDescriptionPtr);
            var spellTooltipPtr = BitConverter.ToInt32(buffer, 0x208);
            var spellTooltip = "";
            if (spellTooltipPtr != 0)
                spellTooltip = MemoryManager.ReadString((IntPtr)spellTooltipPtr);

            return new Spell(spellId, cost, spellName, spellDescription, spellTooltip);
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

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate bool IsSpellOnCooldownDelegate(IntPtr ptr, int spellId, int unknown1, int unknown2, int unknown3, int unknown4);

        static readonly IsSpellOnCooldownDelegate IsSpellOnCooldownFunction =
            Marshal.GetDelegateForFunctionPointer<IsSpellOnCooldownDelegate>((IntPtr)MemoryAddresses.IsSpellOnCooldownFunPtr);

        public bool IsSpellOnCooldown(int spellId)
        {
            return IsSpellOnCooldownFunction((IntPtr)0x00E1D7F4, spellId, 0, 0, 0, 0);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int JumpDelegate();

        static readonly JumpDelegate JumpFunction =
            Marshal.GetDelegateForFunctionPointer<JumpDelegate>((IntPtr)MemoryAddresses.JumpFunPtr);

        public void Jump()
        {
            JumpFunction();
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

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int ReleaseCorpseDelegate(IntPtr ptr);

        static readonly ReleaseCorpseDelegate ReleaseCorpseFunction =
            Marshal.GetDelegateForFunctionPointer<ReleaseCorpseDelegate>((IntPtr)MemoryAddresses.ReleaseCorpseFunPtr);

        public void ReleaseCorpse(IntPtr ptr)
        {
            ReleaseCorpseFunction(ptr);
        }

        delegate int RetrieveCorpseDelegate();

        static readonly RetrieveCorpseDelegate RetrieveCorpseFunction =
            Marshal.GetDelegateForFunctionPointer<RetrieveCorpseDelegate>((IntPtr)MemoryAddresses.RetrieveCorpseFunPtr);

        public void RetrieveCorpse()
        {
            RetrieveCorpseFunction();
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
        delegate void SetFacingDelegate(IntPtr playerSetFacingPtr, float facing);

        static readonly SetFacingDelegate SetFacingFunction =
            Marshal.GetDelegateForFunctionPointer<SetFacingDelegate>((IntPtr)MemoryAddresses.SetFacingFunPtr);

        public void SetFacing(IntPtr playerSetFacingPtr, float facing)
        {
            SetFacingFunction(playerSetFacingPtr, facing);
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

        public IntPtr GetRow(IntPtr tablePtr, int index)
        {
            throw new NotImplementedException();
        }

        public IntPtr GetLocalizedRow(IntPtr tablePtr, int index, IntPtr rowPtr)
        {
            throw new NotImplementedException();
        }

        public int GetAuraCount(IntPtr unitPtr)
        {
            throw new NotImplementedException();
        }

        public IntPtr GetAuraPointer(IntPtr unitPtr, int index)
        {
            throw new NotImplementedException();
        }
    }
}
