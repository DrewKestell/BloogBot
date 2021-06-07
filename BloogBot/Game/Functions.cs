using BloogBot.Game.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace BloogBot.Game
{
    static public class Functions
    {
        static readonly Random random = new Random();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate char EnumerateVisibleObjectsDelegate(IntPtr callback, int filter);

        static readonly EnumerateVisibleObjectsDelegate EnumerateVisibleObjectsTBC =
            Marshal.GetDelegateForFunctionPointer<EnumerateVisibleObjectsDelegate>((IntPtr)MemoryAddresses.EnumerateVisibleObjectsFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "EnumerateVisibleObjects")]
        static extern void EnumerateVisibleObjectsVanilla(IntPtr callback, int filter, IntPtr ptr);

        [HandleProcessCorruptedStateExceptions]
        static public void EnumerateVisibleObjects(IntPtr callback, int filter)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                EnumerateVisibleObjectsTBC(callback, filter);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                EnumerateVisibleObjectsVanilla(callback, filter, (IntPtr)MemoryAddresses.EnumerateVisibleObjectsFunPtr);
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate IntPtr GetObjectPtrDelegate(ulong guid);

        static readonly GetObjectPtrDelegate GetObjectPtrFunction =
            Marshal.GetDelegateForFunctionPointer<GetObjectPtrDelegate>((IntPtr)MemoryAddresses.GetObjectPtrFunPtr);

        static public IntPtr GetObjectPtr(ulong guid) => GetObjectPtrFunction(guid);


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate ulong GetPlayerGuidTBCDelegate();

        static readonly GetPlayerGuidTBCDelegate GetPlayerGuidTBCFunction =
            Marshal.GetDelegateForFunctionPointer<GetPlayerGuidTBCDelegate>((IntPtr)MemoryAddresses.GetPlayerGuidFunPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate ulong GetPlayerGuidVanillaDelegate();

        static readonly GetPlayerGuidVanillaDelegate GetPlayerGuidVanillaFunction =
            Marshal.GetDelegateForFunctionPointer<GetPlayerGuidVanillaDelegate>((IntPtr)MemoryAddresses.GetPlayerGuidFunPtr);

        static public ulong GetPlayerGuid()
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return GetPlayerGuidTBCFunction();
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                return GetPlayerGuidVanillaFunction();
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void SetFacingDelegate(IntPtr playerSetFacingPtr, float facing);

        static readonly SetFacingDelegate SetFacingFunction =
            Marshal.GetDelegateForFunctionPointer<SetFacingDelegate>((IntPtr)MemoryAddresses.SetFacingFunPtr);

        static public void SetFacing(IntPtr playerSetFacingPtr, float facing) =>
            SetFacingFunction(playerSetFacingPtr, facing);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void SendMovementUpdateDelegate(
            IntPtr playerPtr,
            IntPtr unknown,
            int OpCode,
            int unknown2,
            int unknown3);

        static readonly SendMovementUpdateDelegate SendMovementUpdateFunction =
            Marshal.GetDelegateForFunctionPointer<SendMovementUpdateDelegate>((IntPtr)MemoryAddresses.SendMovementUpdateFunPtr);

        static public void SendMovementUpdate(IntPtr playerPtr, int opcode) =>
            SendMovementUpdateFunction(playerPtr, (IntPtr)0x00BE1E2C, opcode, 0, 0);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void SetControlBitDelegate(IntPtr device, int bit, int state, int tickCount);

        static readonly SetControlBitDelegate SetControlBitFunction =
            Marshal.GetDelegateForFunctionPointer<SetControlBitDelegate>((IntPtr)MemoryAddresses.SetControlBitFunPtr);

        static public void SetControlBit(int bit, int state, int tickCount)
        {
            IntPtr ptr;
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                ptr = MemoryManager.ReadIntPtr((IntPtr)0x00CF31E4);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                ptr = MemoryManager.ReadIntPtr((IntPtr)0x00BE1148);
            else
                throw new NotImplementedException("Unknown client version");

            SetControlBitFunction(ptr, bit, state, tickCount);
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int JumpDelegate();

        static readonly JumpDelegate JumpFunction =
            Marshal.GetDelegateForFunctionPointer<JumpDelegate>((IntPtr)MemoryAddresses.JumpFunPtr);

        static public void Jump()
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                JumpFunction();
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out for Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetCreatureTypeDelegate(IntPtr unitPtr);

        static readonly GetCreatureTypeDelegate GetCreatureTypeFunction =
            Marshal.GetDelegateForFunctionPointer<GetCreatureTypeDelegate>((IntPtr)MemoryAddresses.GetCreatureTypeFunPtr);

        static public CreatureType GetCreatureType(IntPtr unitPtr) =>
            (CreatureType)GetCreatureTypeFunction(unitPtr);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetCreatureRankDelegate
            (IntPtr unitPtr);

        static readonly GetCreatureRankDelegate GetCreatureRankFunction =
            Marshal.GetDelegateForFunctionPointer<GetCreatureRankDelegate>((IntPtr)MemoryAddresses.GetCreatureRankFunPtr);

        static public int GetCreatureRank(IntPtr unitPtr) =>
            GetCreatureRankFunction(unitPtr);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetUnitReactionDelegate(IntPtr unitPtr1, IntPtr unitPtr2);

        static readonly GetUnitReactionDelegate GetUnitReactionFunction =
            Marshal.GetDelegateForFunctionPointer<GetUnitReactionDelegate>((IntPtr)MemoryAddresses.GetUnitReactionFunPtr);

        static public UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2) =>
            (UnitReaction)GetUnitReactionFunction(unitPtr1, unitPtr2);


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int LuaCallDelegate(string code1, string code2, int unknown);

        static readonly LuaCallDelegate LuaCallTBCFunction =
            Marshal.GetDelegateForFunctionPointer<LuaCallDelegate>((IntPtr)MemoryAddresses.LuaCallFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "LuaCall")]
        static extern void LuaCallVanillaFunction(string code, int ptr);

        static public void LuaCall(string code)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                LuaCallTBCFunction(code, code, 0);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                LuaCallVanillaFunction(code, MemoryAddresses.LuaCallFunPtr);
            else
                throw new NotImplementedException("Unknown client version");
        }

        static public string[] LuaCallWithResult(string code)
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

            var results = GetText(luaVarNames.ToArray());

            return results.ToArray();
        }

        static string GetRandomLuaVarName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(8).ToArray());
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate IntPtr GetTextDelegate(string arg);

        static readonly GetTextDelegate GetTextTBCFunction =
            Marshal.GetDelegateForFunctionPointer<GetTextDelegate>((IntPtr)MemoryAddresses.GetTextFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "GetText")]
        static extern IntPtr GetTextVanillaFunction(string varName, IntPtr ptr);

        static public string[] GetText(string[] varNames)
        {
            var results = new List<string>();

            foreach (var varName in varNames)
            {
                IntPtr address;

                if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                    address = GetTextTBCFunction(varName);
                else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                    address = GetTextVanillaFunction(varName, (IntPtr)MemoryAddresses.GetTextFunPtr);
                else
                    throw new NotImplementedException("Unknown client version");

                results.Add(MemoryManager.ReadString(address));
            }

            return results.ToArray();
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int CastSpellByIdDelegate(int spellId, int unknown, ulong targetGuid);

        static readonly CastSpellByIdDelegate CastSpellByIdFunction =
            Marshal.GetDelegateForFunctionPointer<CastSpellByIdDelegate>((IntPtr)MemoryAddresses.CastSpellByIdFunPtr);

        static public int CastSpellById(int spellId, ulong targetGuid)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return CastSpellByIdFunction(spellId, 0, targetGuid);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out in Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int GetRow2Delegate(IntPtr ptr, int index, byte[] buffer);

        static readonly GetRow2Delegate GetRow2Function =
            Marshal.GetDelegateForFunctionPointer<GetRow2Delegate>((IntPtr)MemoryAddresses.GetRow2FunPtr);

        static public Spell GetSpellDBEntry(int index)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
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
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out in Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate int IntersectDelegate(ref XYZ start, ref XYZ end, ref XYZ intersection, ref float distance, uint flags, IntPtr Ptr);

        static readonly IntersectDelegate IntersectTBCFunction =
            Marshal.GetDelegateForFunctionPointer<IntersectDelegate>((IntPtr)MemoryAddresses.IntersectFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "Intersect")]
        static extern byte IntersectVanillaFunction(ref XYZXYZ points, ref float distance, ref XYZ intersection, uint flags, IntPtr Ptr);

        static public XYZ Intersect(Position start, Position end)
        {
            var intersection = new XYZ();
            var distance = start.DistanceTo(end);

            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
            {
                var startXYZ = new XYZ(start.X, start.Y, start.Z + 5.0f);
                var endXYZ = new XYZ(end.X, end.Y, end.Z + 5.0f);

                IntersectTBCFunction(ref startXYZ, ref endXYZ, ref intersection, ref distance, 0x00100171, IntPtr.Zero);
            }
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
            {
                var points = new XYZXYZ(start.X, start.Y, start.Z, end.X, end.Y, end.Z);
                points.Z1 += 5;
                points.Z2 += 5;

                IntersectVanillaFunction(ref points, ref distance, ref intersection, 0x00100010, (IntPtr)MemoryAddresses.IntersectFunPtr);
            }
            else
                throw new NotImplementedException("Unknown client version");

            return intersection;
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SelectObjectDelegate(ulong guid);

        static readonly SelectObjectDelegate SelectObjectFunction =
            Marshal.GetDelegateForFunctionPointer<SelectObjectDelegate>((IntPtr)MemoryAddresses.SelectObjectFunPtr);

        static public void SelectObject(ulong guid)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                SelectObjectFunction(guid);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out for Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate bool IsLootingDelegate(IntPtr unitPtr);

        static readonly IsLootingDelegate IsLootingFunction =
            Marshal.GetDelegateForFunctionPointer<IsLootingDelegate>((IntPtr)MemoryAddresses.IsLootingFunPtr);

        static public bool IsLooting(IntPtr unitPtr)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return IsLootingFunction(unitPtr);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out for Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate bool CanLootDelegate(IntPtr unitPtr, IntPtr targetPtr);

        static readonly CanLootDelegate CanLootFunction =
            Marshal.GetDelegateForFunctionPointer<CanLootDelegate>((IntPtr)MemoryAddresses.CanLootFunPtr);

        static public bool CanLoot(IntPtr targetPtr)
        {
            var player = ObjectManager.Player;

            if (player != null)
            {
                if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                    return CanLootFunction(player.Pointer, targetPtr);
                else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                    throw new NotImplementedException("TODO: figure this out for Vanilla");
                else
                    throw new NotImplementedException("Unknown client version");
            }
            else
                return false;
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int LootUnitDelegate(IntPtr unitPtr, IntPtr targetPtr, IntPtr unknown);

        static readonly LootUnitDelegate LootUnitFunction =
            Marshal.GetDelegateForFunctionPointer<LootUnitDelegate>((IntPtr)MemoryAddresses.LootUnitFunPtr);

        static public void LootUnit(IntPtr targetPtr)
        {
            var player = ObjectManager.Player;

            if (player != null)
            {
                if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                    LootUnitFunction(player.Pointer, targetPtr, IntPtr.Zero);
                else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                    throw new NotImplementedException("TODO: figure this out for Vanilla");
                else
                    throw new NotImplementedException("Unknown client version");
            } 
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate bool CanAttackDelegate(IntPtr unitPtr, IntPtr targetPtr);

        static readonly CanAttackDelegate CanAttackFunction =
            Marshal.GetDelegateForFunctionPointer<CanAttackDelegate>((IntPtr)MemoryAddresses.CanAttackFunPtr);

        static public bool CanAttack(IntPtr targetPtr)
        {
            var player = ObjectManager.Player;

            if (player != null)
            {
                if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                    return CanAttackFunction(player.Pointer, targetPtr);
                else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                    throw new NotImplementedException("TODO: figure this out for Vanilla");
                else
                    throw new NotImplementedException("Unknown client version");
            }
            else
                return false;
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate bool IsDeadDelegate(IntPtr unitPtr);

        static readonly IsDeadDelegate IsDeadFunction =
            Marshal.GetDelegateForFunctionPointer<IsDeadDelegate>((IntPtr)MemoryAddresses.IsDeadFunPtr);

        static public bool IsDead(IntPtr unitPtr)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return IsDeadFunction(unitPtr);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out for Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        delegate int RetrieveCorpseDelegate();

        static readonly RetrieveCorpseDelegate RetrieveCorpseFunction =
            Marshal.GetDelegateForFunctionPointer<RetrieveCorpseDelegate>((IntPtr)MemoryAddresses.RetrieveCorpseFunPtr);

        static public void RetrieveCorpse() => RetrieveCorpseFunction();


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int ReleaseCorpseDelegate(IntPtr ptr);

        static readonly ReleaseCorpseDelegate ReleaseCorpseFunction =
            Marshal.GetDelegateForFunctionPointer<ReleaseCorpseDelegate>((IntPtr)MemoryAddresses.ReleaseCorpseFunPtr);

        static public void ReleaseCorpse(IntPtr ptr) => ReleaseCorpseFunction(ptr);


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

        static public IntPtr GetItemCacheEntry(int itemId)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return GetItemCacheEntryFunction((IntPtr)0x00D29A98, itemId, IntPtr.Zero, 0, 0, (char)0);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                return GetItemCacheEntryFunction((IntPtr)0x00C0E2A0, itemId, IntPtr.Zero, 0, 0, (char)0); // this is not tested
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate bool IsSpellOnCooldownDelegate(IntPtr ptr, int spellId, int unknown1, int unknown2, int unknown3, int unknown4);

        static readonly IsSpellOnCooldownDelegate IsSpellOnCooldownFunction =
            Marshal.GetDelegateForFunctionPointer<IsSpellOnCooldownDelegate>((IntPtr)MemoryAddresses.IsSpellOnCooldownFunPtr);

        static public bool IsSpellOnCooldown(int spellId)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return IsSpellOnCooldownFunction((IntPtr)0x00E1D7F4, spellId, 0, 0, 0, 0);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure this out for Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void LootSlotDelegate(int slot);

        static readonly LootSlotDelegate LootSlotTBCFunction =
            Marshal.GetDelegateForFunctionPointer<LootSlotDelegate>((IntPtr)MemoryAddresses.LootSlotFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "LootSlot")]
        static extern byte LootSlotVanillaFunction(int slot, IntPtr ptr);

        static public void LootSlot(int slot)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                LootSlotTBCFunction(slot);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                LootSlotVanillaFunction(slot, (IntPtr)MemoryAddresses.LootSlotFunPtr);
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void UseItemDelegate(IntPtr itemPtr, ref ulong unused1, int unused2);

        static readonly UseItemDelegate UseItemFunction =
            Marshal.GetDelegateForFunctionPointer<UseItemDelegate>((IntPtr)MemoryAddresses.UseItemFunPtr);

        static public void UseItem(IntPtr itemPtr)
        {
            ulong unused1 = 0;
            UseItemFunction(itemPtr, ref unused1, 0);
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void SellItemByGuidDelegate(ulong vendorGuid, ulong itemGuid, int unused);

        static readonly SellItemByGuidDelegate SellItemByGuidTBCFunction =
            Marshal.GetDelegateForFunctionPointer<SellItemByGuidDelegate>((IntPtr)MemoryAddresses.SellItemByGuidFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "SellItemByGuid")]
        static extern void SellItemByGuidVanillaFunction(uint itemCount, ulong npcGuid, ulong itemGuid, IntPtr sellItemFunPtr);

        static public void SellItemByGuid(ulong vendorGuid, ulong itemGuid)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                SellItemByGuidTBCFunction(vendorGuid, itemGuid, 0);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                SellItemByGuidVanillaFunction(1, vendorGuid, itemGuid, (IntPtr)MemoryAddresses.SellItemByGuidFunPtr);
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void BuyVendorItemDelegate(ulong vendorGuid, int itemId, int quantity, int unused);

        static readonly BuyVendorItemDelegate BuyVendorItemTBCFunction =
            Marshal.GetDelegateForFunctionPointer<BuyVendorItemDelegate>((IntPtr)MemoryAddresses.BuyVendorItemFunPtr);

        [DllImport("FastCall.dll", EntryPoint = "BuyVendorItem")]
        static extern void BuyVendorItemVanillaFunction(int itemId, int quantity, ulong vendorGuid, IntPtr ptr);

        static public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                BuyVendorItemTBCFunction(vendorGuid, itemId, quantity, 1);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                BuyVendorItemVanillaFunction(itemId, quantity, vendorGuid, (IntPtr)MemoryAddresses.BuyVendorItemFunPtr);
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int DismountDelegate(IntPtr unitPtr);

        static readonly DismountDelegate DismountFunction =
            Marshal.GetDelegateForFunctionPointer<DismountDelegate>((IntPtr)MemoryAddresses.DismountFunPtr);

        static public int Dismount(IntPtr unitPtr)
        {
            if (MemoryAddresses.ClientVersion == ClientVersion.TBC)
                return DismountFunction(unitPtr);
            else if (MemoryAddresses.ClientVersion == ClientVersion.Vanilla)
                throw new NotImplementedException("TODO: figure out how this works for Vanilla");
            else
                throw new NotImplementedException("Unknown client version");
        }


        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int CastAtPositionDelegate(ref XYZ parPos);

        static readonly CastAtPositionDelegate CastAtPositionFunction =
            Marshal.GetDelegateForFunctionPointer<CastAtPositionDelegate>((IntPtr)MemoryAddresses.CastAtPositionFunPtr);

        static public void CastAtPosition(string spellName, Position position)
        {
            MemoryManager.WriteByte((IntPtr)0xCECAC0, 0);
            LuaCall($"CastSpellByName('{spellName}')");
            var pos = position.ToXYZ();
            CastAtPositionFunction(ref pos);
        }
    }
}
