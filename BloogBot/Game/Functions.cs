using BloogBot.Game.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.Game
{
    static public class Functions
    {
        static readonly Random random = new Random();
        static readonly List<string> luaVarNames = new List<string>();

        static readonly IGameFunctionHandler gameFunctionHandler;

        static Functions()
        {
            if (ClientHelper.ClientVersion == ClientVersion.WotLK)
                gameFunctionHandler = new WotLKGameFunctionHandler();
            else if (ClientHelper.ClientVersion == ClientVersion.TBC)
                gameFunctionHandler = new TBCGameFunctionHandler();
            else if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                gameFunctionHandler = new VanillaGameFunctionHandler();
        }

        static public void BuyVendorItem(ulong vendorGuid, int itemId, int quantity)
        {
            gameFunctionHandler.BuyVendorItem(vendorGuid, itemId, quantity);
        }

        static public void CastAtPosition(string spellName, Position position)
        {
            gameFunctionHandler.CastAtPosition(spellName, position);
        }

        static public int GetAuraCount(IntPtr unitPtr)
        {
            return gameFunctionHandler.GetAuraCount(unitPtr);
        }

        static public IntPtr GetAuraPointer(IntPtr unitPtr, int index)
        {
            return gameFunctionHandler.GetAuraPointer(unitPtr, index);
        }

        static public int CastSpellById(int spellId, ulong targetGuid)
        {
            return gameFunctionHandler.CastSpellById(spellId, targetGuid);
        }

        static public int Dismount(IntPtr unitPtr)
        {
            return gameFunctionHandler.Dismount(unitPtr);
        }

        static public void EnumerateVisibleObjects(IntPtr callback, int filter)
        {
            gameFunctionHandler.EnumerateVisibleObjects(callback, filter);
        }

        static public int GetCreatureRank(IntPtr unitPtr)
        {
            return gameFunctionHandler.GetCreatureRank(unitPtr);
        }

        static public CreatureType GetCreatureType(IntPtr unitPtr)
        {
            return gameFunctionHandler.GetCreatureType(unitPtr);
        }

        static public IntPtr GetItemCacheEntry(int itemId, ulong guid = 0)
        {
            return gameFunctionHandler.GetItemCacheEntry(itemId, guid);
        }

        static public IntPtr GetObjectPtr(ulong guid)
        {
            return gameFunctionHandler.GetObjectPtr(guid);
        }

        static public ulong GetPlayerGuid()
        {
            return gameFunctionHandler.GetPlayerGuid();
        }

        static public UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2)
        {
            return gameFunctionHandler.GetUnitReaction(unitPtr1, unitPtr2);
        }

        static public XYZ Intersect(Position start, Position end)
        {
            return gameFunctionHandler.Intersect(start, end);
        }

        static public bool IsSpellOnCooldown(int spellId)
        {
            return gameFunctionHandler.IsSpellOnCooldown(spellId);
        }

        static public void Jump()
        {
            gameFunctionHandler.Jump();
        }

        static public void LuaCall(string code)
        {
            gameFunctionHandler.LuaCall(code);
        }

        static public string[] LuaCallWithResult(string code)
        {
            // Make some temporary varaible names for return values.
            var usedVarNames = new List<string>();
            for (var i = 0; ; i++)
            {
                var currentPlaceHolder = "{" + i + "}";
                if (!code.Contains(currentPlaceHolder))
                {
                    break;
                }
                var varName = GetLuaVarName(i);
                code = code.Replace(currentPlaceHolder, varName);
                usedVarNames.Add(varName);
            }

            // If there is no return value, just call lua.
            if (usedVarNames.Count == 0)
            {
                LuaCall(code);
                return Array.Empty<string>();
            }

            // Make sure the temporary variables don't contain any values, then run the code.
            var clearCode = string.Join(", ", usedVarNames) + " = nil";
            LuaCall(clearCode + "\n" + code);

            try
            {
                // Retrieve the return values.
                var results = new string[usedVarNames.Count];
                for (var i = 0; i < usedVarNames.Count; i++)
                {
                    var address = gameFunctionHandler.GetText(usedVarNames[i]);
                    results[i] = MemoryManager.ReadString(address);
                }

                return results;
            }
            finally
            {
                LuaCall(clearCode);
            }
        }

        static public Spell GetSpellDBEntry(int index)
        {
            return gameFunctionHandler.GetSpellDBEntry(index);
        }

        static public void ReleaseCorpse(IntPtr ptr)
        {
            gameFunctionHandler.ReleaseCorpse(ptr);
        }

        static public void RetrieveCorpse()
        {
            gameFunctionHandler.RetrieveCorpse();
        }

        static public void LootSlot(int slot)
        {
            gameFunctionHandler.LootSlot(slot);
        }

        static public void SetTarget(ulong guid)
        {
            gameFunctionHandler.SetTarget(guid);
        }

        static public void SellItemByGuid(uint itemCount, ulong vendorGuid, ulong itemGuid)
        {
            gameFunctionHandler.SellItemByGuid(itemCount, vendorGuid, itemGuid);
        }

        static public void SendMovementUpdate(IntPtr playerPtr, int opcode)
        {
            gameFunctionHandler.SendMovementUpdate(playerPtr, opcode);
        }

        static public void SetControlBit(int bit, int state, int tickCount)
        {
            gameFunctionHandler.SetControlBit(bit, state, tickCount);
        }

        static public void SetFacing(IntPtr playerSetFacingPtr, float facing)
        {
            gameFunctionHandler.SetFacing(playerSetFacingPtr, facing);
        }

        static public void UseItem(IntPtr itemPtr)
        {
            gameFunctionHandler.UseItem(itemPtr);
        }

        static public IntPtr GetRow(IntPtr tablePtr, int index)
        {
            return gameFunctionHandler.GetRow(tablePtr, index);
        }

        static public IntPtr GetLocalizedRow(IntPtr tablePtr, int index, IntPtr rowPtr)
        {
            return gameFunctionHandler.GetLocalizedRow(tablePtr, index, rowPtr);
        }

        static string GetLuaVarName(int index)
        {
            while (luaVarNames.Count <= index)
            {
                var candidate = GetRandomLuaVarName();
                if (candidate != "function" && !luaVarNames.Contains(candidate))
                {
                    luaVarNames.Add(candidate);
                }
            }

            return luaVarNames[index];
        }

        static string GetRandomLuaVarName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(8).ToArray());
        }
    }
}
