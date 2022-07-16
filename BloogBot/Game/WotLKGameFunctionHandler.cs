using BloogBot.Game.Enums;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public class WotLKGameFunctionHandler : IGameFunctionHandler
    {
        public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            throw new NotImplementedException();
        }

        public bool CanAttack(IntPtr playerPtr, IntPtr targetPtr)
        {
            throw new NotImplementedException();
        }

        public bool CanLoot(IntPtr playerPtr, IntPtr targetPtr)
        {
            throw new NotImplementedException();
        }

        public void CastAtPosition(string spellName, Position position)
        {
            throw new NotImplementedException();
        }

        public int CastSpellById(int spellId, ulong targetGuid)
        {
            throw new NotImplementedException();
        }

        public int Dismount(IntPtr unitPtr)
        {
            throw new NotImplementedException();
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

        public IntPtr GetItemCacheEntry(int itemId)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public IntPtr GetText(string varName)
        {
            throw new NotImplementedException();
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetUnitReactionDelegate(IntPtr unitPtr1, IntPtr unitPtr2);

        static readonly GetUnitReactionDelegate GetUnitReactionFunction =
            Marshal.GetDelegateForFunctionPointer<GetUnitReactionDelegate>((IntPtr)MemoryAddresses.GetUnitReactionFunPtr);

        public UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2)
        {
            return (UnitReaction)GetUnitReactionFunction(unitPtr1, unitPtr2);
        }

        public XYZ Intersect(Position start, Position end)
        {
            throw new NotImplementedException();
        }

        public bool IsDead(IntPtr unitPtr)
        {
            throw new NotImplementedException();
        }

        public bool IsLooting(IntPtr unitPtr)
        {
            throw new NotImplementedException();
        }

        public bool IsSpellOnCooldown(int spellId)
        {
            throw new NotImplementedException();
        }

        public void Jump()
        {
            throw new NotImplementedException();
        }

        public void LootSlot(int slot)
        {
            throw new NotImplementedException();
        }

        public void LootUnit(IntPtr playerPtr, IntPtr targetPtr)
        {
            throw new NotImplementedException();
        }

        public void LuaCall(string code)
        {
            throw new NotImplementedException();
        }

        public void ReleaseCorpse(IntPtr ptr)
        {
            throw new NotImplementedException();
        }

        public void RetrieveCorpse()
        {
            throw new NotImplementedException();
        }

        public void SelectObject(ulong guid)
        {
            throw new NotImplementedException();
        }

        public void SellItemByGuid(ulong vendorGuid, ulong itemGuid)
        {
            throw new NotImplementedException();
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
    }
}
