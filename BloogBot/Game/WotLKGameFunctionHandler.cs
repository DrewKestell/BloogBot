using BloogBot.Game.Enums;
using System;

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

        public void EnumerateVisibleObjects(IntPtr callback, int filter)
        {
            // throw new NotImplementedException();
        }

        public int GetCreatureRank(IntPtr unitPtr)
        {
            throw new NotImplementedException();
        }

        public CreatureType GetCreatureType(IntPtr unitPtr)
        {
            throw new NotImplementedException();
        }

        public IntPtr GetItemCacheEntry(int itemId)
        {
            throw new NotImplementedException();
        }

        public IntPtr GetObjectPtr(ulong guid)
        {
            throw new NotImplementedException();
        }

        public ulong GetPlayerGuid()
        {
            // throw new NotImplementedException();
            return 0;
        }

        public Spell GetSpellDBEntry(int index)
        {
            throw new NotImplementedException();
        }

        public IntPtr GetText(string varName)
        {
            throw new NotImplementedException();
        }

        public UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2)
        {
            throw new NotImplementedException();
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

        public void SendMovementUpdate(IntPtr playerPtr, int opcode)
        {
            throw new NotImplementedException();
        }

        public void SetControlBit(int bit, int state, int tickCount)
        {
            throw new NotImplementedException();
        }

        public void SetFacing(IntPtr playerSetFacingPtr, float facing)
        {
            throw new NotImplementedException();
        }

        public void UseItem(IntPtr itemPtr)
        {
            throw new NotImplementedException();
        }
    }
}
