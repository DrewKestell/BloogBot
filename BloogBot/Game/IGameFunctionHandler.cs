using BloogBot.Game.Enums;
using System;

namespace BloogBot.Game
{
    public interface IGameFunctionHandler
    {
        void EnumerateVisibleObjects(IntPtr callback, int filter);

        IntPtr GetObjectPtr(ulong guid);

        ulong GetPlayerGuid();

        void SetFacing(IntPtr playerSetFacingPtr, float facing);

        void SendMovementUpdate(IntPtr playerPtr, int opcode);

        void SetControlBit(int bit, int state, int tickCount);

        void Jump();

        CreatureType GetCreatureType(IntPtr unitPtr);

        int GetCreatureRank(IntPtr unitPtr);

        UnitReaction GetUnitReaction(IntPtr unitPtr1, IntPtr unitPtr2);

        void LuaCall(string code);

        IntPtr GetText(string varName);

        int CastSpellById(int spellId, ulong targetGuid);

        Spell GetSpellDBEntry(int index);

        XYZ Intersect(Position start, Position end);

        void SelectObject(ulong guid);

        bool IsLooting(IntPtr unitPtr);

        bool CanLoot(IntPtr playerPtr, IntPtr targetPtr);

        void LootUnit(IntPtr playerPtr, IntPtr targetPtr);

        bool CanAttack(IntPtr playerPtr, IntPtr targetPtr);

        bool IsDead(IntPtr unitPtr);

        void RetrieveCorpse();

        void ReleaseCorpse(IntPtr ptr);

        IntPtr GetItemCacheEntry(int itemId);

        bool IsSpellOnCooldown(int spellId);

        void LootSlot(int slot);

        void UseItem(IntPtr itemPtr);

        void SellItemByGuid(ulong vendorGuid, ulong itemGuid);

        void BuyVendorItem(ulong vendorGuid, int itemId, int quantity);

        int Dismount(IntPtr unitPtr);

        void CastAtPosition(string spellName, Position position);

        IntPtr GetRow(IntPtr tablePtr, int index);

        IntPtr GetLocalizedRow(IntPtr tablePtr, int index, IntPtr rowPtr);
    }
}
