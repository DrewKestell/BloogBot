using BotRunner.Constants;
using BotRunner.Interfaces;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace ActivityForegroundMember.Mem
{
    static public class Functions
    {
        private static readonly object locker = new();
        private static readonly Random random = new();

        [DllImport("FastCall.dll", EntryPoint = "BuyVendorItem")]
        private static extern void BuyVendorItemFunction(int itemId, int quantity, ulong vendorGuid, nint ptr);

        static public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            BuyVendorItemFunction(itemId, quantity, vendorGuid, MemoryAddresses.BuyVendorItemFunPtr);
        }

        [DllImport("FastCall.dll", EntryPoint = "EnumerateVisibleObjects")]
        private static extern void EnumerateVisibleObjectsFunction(nint callback, int filter, nint ptr);

        // what does this do? [HandleProcessCorruptedStateExceptions]
        static public void EnumerateVisibleObjects(nint callback, int filter)
        {
            EnumerateVisibleObjectsFunction(callback, filter, MemoryAddresses.EnumerateVisibleObjectsFunPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetCreatureRankDelegate
            (nint unitPtr);

        private static readonly GetCreatureRankDelegate GetCreatureRankFunction =
            Marshal.GetDelegateForFunctionPointer<GetCreatureRankDelegate>(MemoryAddresses.GetCreatureRankFunPtr);

        static public int GetCreatureRank(nint unitPtr)
        {
            return GetCreatureRankFunction(unitPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetCreatureTypeDelegate(nint unitPtr);

        private static readonly GetCreatureTypeDelegate GetCreatureTypeFunction =
            Marshal.GetDelegateForFunctionPointer<GetCreatureTypeDelegate>(MemoryAddresses.GetCreatureTypeFunPtr);

        static public CreatureType GetCreatureType(nint unitPtr)
        {
            return (CreatureType)GetCreatureTypeFunction(unitPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate nint ItemCacheGetRowDelegate(
            nint ptr,
            int itemId,
            nint unknown,
            int unused1,
            int unused2,
            char unused3);

        private static readonly ItemCacheGetRowDelegate GetItemCacheEntryFunction =
            Marshal.GetDelegateForFunctionPointer<ItemCacheGetRowDelegate>(MemoryAddresses.GetItemCacheEntryFunPtr);

        static public nint GetItemCacheEntry(int itemId)
        {
            return GetItemCacheEntryFunction(MemoryAddresses.ItemCacheEntryBasePtr, itemId, nint.Zero, 0, 0, (char)0);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate nint GetObjectPtrDelegate(ulong guid);

        private static readonly GetObjectPtrDelegate GetObjectPtrFunction =
            Marshal.GetDelegateForFunctionPointer<GetObjectPtrDelegate>(MemoryAddresses.GetObjectPtrFunPtr);

        static public nint GetObjectPtr(ulong guid)
        {
            return GetObjectPtrFunction(guid);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ulong GetPlayerGuidDelegate();

        private static readonly GetPlayerGuidDelegate GetPlayerGuidFunction =
            Marshal.GetDelegateForFunctionPointer<GetPlayerGuidDelegate>(MemoryAddresses.GetPlayerGuidFunPtr);

        static public ulong GetPlayerGuid()
        {
            return GetPlayerGuidFunction();
        }

        [DllImport("FastCall.dll", EntryPoint = "GetText")]
        private static extern nint GetTextFunction(string varName, nint ptr);

        static public nint GetText(string varName)
        {
            return GetTextFunction(varName, MemoryAddresses.GetTextFunPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int GetUnitReactionDelegate(nint unitPtr1, nint unitPtr2);

        private static readonly GetUnitReactionDelegate GetUnitReactionFunction =
            Marshal.GetDelegateForFunctionPointer<GetUnitReactionDelegate>(MemoryAddresses.GetUnitReactionFunPtr);

        static public UnitReaction GetUnitReaction(nint unitPtr1, nint unitPtr2)
        {
            return (UnitReaction)GetUnitReactionFunction(unitPtr1, unitPtr2);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void IsSpellOnCooldownDelegate(
            nint spellCooldownPtr,
            int spellId,
            int unused1,
            ref int cooldownDuration,
            int unused2,
            bool unused3);

        private static readonly IsSpellOnCooldownDelegate IsSpellOnCooldownFunction =
            Marshal.GetDelegateForFunctionPointer<IsSpellOnCooldownDelegate>(MemoryAddresses.IsSpellOnCooldownFunPtr);

        static public bool IsSpellOnCooldown(int spellId)
        {
            var cooldownDuration = 0;
            IsSpellOnCooldownFunction(
                0x00CECAEC,
                spellId,
                0,
                ref cooldownDuration,
                0,
                false);

            return cooldownDuration != 0;
        }

        [DllImport("FastCall.dll", EntryPoint = "LootSlot")]
        private static extern byte LootSlotFunction(int slot, nint ptr);

        static public void LootSlot(int slot)
        {
            LootSlotFunction(slot, MemoryAddresses.LootSlotFunPtr);
        }

        [DllImport("FastCall.dll", EntryPoint = "LuaCall")]
        private static extern void LuaCallFunction(string code, int ptr);

        static public void LuaCall(string code)
        {
            lock (locker)
            {
                LuaCallFunction(code, MemoryAddresses.LuaCallFunPtr);
            }
        }
        static public string[] LuaCallWithResult(string code)
        {
            lock (locker)
            {
                var luaVarNames = new List<string>();
                for (var i = 0; i < 11; i++)
                {
                    var currentPlaceHolder = "{" + i + "}";
                    if (!code.Contains(currentPlaceHolder)) break;
                    var randomName = GetRandomLuaVarName();
                    code = code.Replace(currentPlaceHolder, randomName);
                    luaVarNames.Add(randomName);
                }

                LuaCall(code);

                var results = new List<string>();
                foreach (var varName in luaVarNames)
                {
                    var address = GetText(varName);
                    results.Add(MemoryManager.ReadString(address));
                }

                return [.. results];
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int ReleaseCorpseDelegate(nint ptr);

        private static readonly ReleaseCorpseDelegate ReleaseCorpseFunction =
            Marshal.GetDelegateForFunctionPointer<ReleaseCorpseDelegate>(MemoryAddresses.ReleaseCorpseFunPtr);

        [HandleProcessCorruptedStateExceptions]
        static public void ReleaseCorpse(nint ptr)
        {
            try
            {
                ReleaseCorpseFunction(ptr);
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("AccessViolationException occurred while trying to release corpse. Most likely, this is due to a transient error that caused the player pointer to temporarily equal IntPtr.Zero. The bot should keep trying to release and recover from this error.");
            }
        }

        private delegate int RetrieveCorpseDelegate();

        private static readonly RetrieveCorpseDelegate RetrieveCorpseFunction =
            Marshal.GetDelegateForFunctionPointer<RetrieveCorpseDelegate>(MemoryAddresses.RetrieveCorpseFunPtr);

        static public void RetrieveCorpse()
        {
            RetrieveCorpseFunction();
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SetTargetDelegate(ulong guid);

        private static readonly SetTargetDelegate SetTargetFunction =
            Marshal.GetDelegateForFunctionPointer<SetTargetDelegate>(MemoryAddresses.SetTargetFunPtr);

        static public void SetTarget(ulong guid)
        {
            SetTargetFunction(guid);
        }

        [DllImport("FastCall.dll", EntryPoint = "SellItemByGuid")]
        private static extern void SellItemByGuidFunction(uint itemCount, ulong npcGuid, ulong itemGuid, nint sellItemFunPtr);

        static public void SellItemByGuid(uint itemCount, ulong vendorGuid, ulong itemGuid)
        {
            SellItemByGuidFunction(itemCount, vendorGuid, itemGuid, MemoryAddresses.SellItemByGuidFunPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SendMovementUpdateDelegate(
            nint playerPtr,
            nint unknown,
            int OpCode,
            int unknown2,
            int unknown3);

        private static readonly SendMovementUpdateDelegate SendMovementUpdateFunction =
            Marshal.GetDelegateForFunctionPointer<SendMovementUpdateDelegate>(MemoryAddresses.SendMovementUpdateFunPtr);

        static public void SendMovementUpdate(nint playerPtr, int opcode)
        {
            SendMovementUpdateFunction(playerPtr, 0x00BE1E2C, opcode, 0, 0);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetControlBitDelegate(nint device, int bit, int state, int tickCount);

        private static readonly SetControlBitDelegate SetControlBitFunction =
            Marshal.GetDelegateForFunctionPointer<SetControlBitDelegate>(MemoryAddresses.SetControlBitFunPtr);

        static public void SetControlBit(int bit, int state, int tickCount)
        {
            var ptr = MemoryManager.ReadIntPtr(MemoryAddresses.SetControlBitDevicePtr);
            SetControlBitFunction(ptr, bit, state, tickCount);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void SetFacingDelegate(nint playerSetFacingPtr, float facing);

        private static readonly SetFacingDelegate SetFacingFunction =
            Marshal.GetDelegateForFunctionPointer<SetFacingDelegate>(MemoryAddresses.SetFacingFunPtr);

        static public void SetFacing(nint playerSetFacingPtr, float facing)
        {
            SetFacingFunction(playerSetFacingPtr, facing);
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void UseItemDelegate(nint itemPtr, ref ulong unused1, int unused2);

        private static readonly UseItemDelegate UseItemFunction =
            Marshal.GetDelegateForFunctionPointer<UseItemDelegate>(MemoryAddresses.UseItemFunPtr);

        static public void UseItem(nint itemPtr)
        {
            ulong unused1 = 0;
            UseItemFunction(itemPtr, ref unused1, 0);
        }

        private static string GetRandomLuaVarName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(8).ToArray());
        }
    }
}
