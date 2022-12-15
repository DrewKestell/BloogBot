using BloogBot.Game.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.Game
{
    static public class Functions
    {
        static readonly Random random = new Random();

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
                var address = gameFunctionHandler.GetText(varName);
                results.Add(MemoryManager.ReadString(address));
            }

            return results.ToArray();
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

        static string GetRandomLuaVarName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(8).ToArray());
        }
    }
}
