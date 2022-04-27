using BloogBot.Game.Enums;
using System;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    public class VanillaGameFunctionHandler : IGameFunctionHandler
    {
        [DllImport("FastCall.dll", EntryPoint = "BuyVendorItem")]
        static extern void BuyVendorItemFunction(int itemId, int quantity, ulong vendorGuid, IntPtr ptr);

        public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            BuyVendorItemFunction(itemId, quantity, vendorGuid, (IntPtr)MemoryAddresses.BuyVendorItemFunPtr);
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

        [DllImport("FastCall.dll", EntryPoint = "EnumerateVisibleObjects")]
        static extern void EnumerateVisibleObjectsFunction(IntPtr callback, int filter, IntPtr ptr);

        public void EnumerateVisibleObjects(IntPtr callback, int filter)
        {
            EnumerateVisibleObjectsFunction(callback, filter, (IntPtr)MemoryAddresses.EnumerateVisibleObjectsFunPtr);
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

        public IntPtr GetItemCacheEntry(int itemId)
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate ulong GetPlayerGuidDelegate();

        static GetPlayerGuidDelegate GetPlayerGuidFunction =
            Marshal.GetDelegateForFunctionPointer<GetPlayerGuidDelegate>((IntPtr)MemoryAddresses.GetPlayerGuidFunPtr);

        public ulong GetPlayerGuid()
        {
            return GetPlayerGuidFunction();
        }

        public Spell GetSpellDBEntry(int index)
        {
            throw new NotImplementedException();
        }

        [DllImport("FastCall.dll", EntryPoint = "GetText")]
        static extern IntPtr GetTextFunction(string varName, IntPtr ptr);

        public IntPtr GetText(string varName)
        {
            return GetTextFunction(varName, (IntPtr)MemoryAddresses.GetTextFunPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetUnitReactionDelegate(IntPtr unitPtr1, IntPtr unitPtr2);

        static readonly GetUnitReactionDelegate GetUnitReactionFunction =
            Marshal.GetDelegateForFunctionPointer<GetUnitReactionDelegate>((IntPtr)MemoryAddresses.GetUnitReactionFunPtr);

        public UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2)
        {
            return (UnitReaction)GetUnitReactionFunction(unitPtr1, unitPtr2);
        }

        [DllImport("FastCall.dll", EntryPoint = "Intersect")]
        static extern byte IntersectFunction(ref XYZXYZ points, ref float distance, ref XYZ intersection, uint flags, IntPtr Ptr);

        public XYZ Intersect(Position start, Position end)
        {
            var intersection = new XYZ();
            var distance = start.DistanceTo(end);
            var points = new XYZXYZ(start.X, start.Y, start.Z, end.X, end.Y, end.Z);
            points.Z1 += 5;
            points.Z2 += 5;

            IntersectFunction(ref points, ref distance, ref intersection, 0x00100010, (IntPtr)MemoryAddresses.IntersectFunPtr);

            return intersection;
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

        [DllImport("FastCall.dll", EntryPoint = "LootSlot")]
        static extern byte LootSlotFunction(int slot, IntPtr ptr);

        public void LootSlot(int slot)
        {
            LootSlotFunction(slot, (IntPtr)MemoryAddresses.LootSlotFunPtr);
        }

        public void LootUnit(IntPtr playerPtr, IntPtr targetPtr)
        {
            throw new NotImplementedException();
        }

        [DllImport("FastCall.dll", EntryPoint = "LuaCall")]
        static extern void LuaCallFunction(string code, int ptr);

        public void LuaCall(string code)
        {
            LuaCallFunction(code, MemoryAddresses.LuaCallFunPtr);
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

        public void SelectObject(ulong guid)
        {
            throw new NotImplementedException();
        }

        [DllImport("FastCall.dll", EntryPoint = "SellItemByGuid")]
        static extern void SellItemByGuidFunction(uint itemCount, ulong npcGuid, ulong itemGuid, IntPtr sellItemFunPtr);

        public void SellItemByGuid(ulong vendorGuid, ulong itemGuid)
        {
            SellItemByGuidFunction(1, vendorGuid, itemGuid, (IntPtr)MemoryAddresses.SellItemByGuidFunPtr);
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
    }
}
