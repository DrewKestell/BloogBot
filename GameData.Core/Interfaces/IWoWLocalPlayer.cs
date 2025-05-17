using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWLocalPlayer : IWoWPlayer
    {
        Position CorpsePosition { get; }
        bool InGhostForm { get; }
        bool IsCursed { get; }
        bool IsPoisoned { get; }
        int ComboPoints { get; }
        bool IsDiseased { get; }
        string CurrentStance { get; }
        bool HasMagicDebuff { get; }
        bool TastyCorpsesNearby { get; }
        bool CanRiposte { get; }
        bool MainhandIsEnchanted { get; }
        uint Copper { get; }
        bool IsAutoAttacking { get; }
        bool CanResurrect { get; }
        float Facing { get; }

        void DoEmote(Emote emote);
        void DoEmote(TextEmote emote);
        void Face(Position position);
        uint GetManaCost(string healingTouch);
        void MoveToward(Position position, float facing);
        void RefreshSkills();
        void RefreshSpells();
        void RetrieveCorpse();
        void SetTarget(ulong guid);
        void StopAttack();
        void SetFacing(float facing);
        void StartMovement(ControlBits bits);
        void StopMovement(ControlBits bits);
        bool IsSpellReady(string spellName);
        void StopAllMovement();
        void StopCasting();
        void CastSpell(string spellName, int rank = -1, bool castOnSelf = false);
        void CastSpell(int spellId, int rank = -1, bool castOnSelf = false);
        bool CanCastSpell(int spellId, ulong targetGuid);
        void UseItem(int bagId, int slotId, ulong targetGuid = 0);
        ulong GetBackpackItemGuid(int parSlot);
        ulong GetEquippedItemGuid(EquipSlot slot);
        IWoWItem GetEquippedItem(EquipSlot ranged);
        IWoWItem GetContainedItem(int bagSlot, int slotId);
        IEnumerable<IWoWItem> GetEquippedItems();
        IEnumerable<IWoWItem> GetContainedItems();
        uint GetBagGuid(EquipSlot equipSlot);
        void PickupContainedItem(int bagSlot, int slotId, int quantity);
        void PlaceItemInContainer(int bagSlot, int slotId);
        void DestroyItemInContainer(int bagSlot, int slotId, int quantity = -1);
        void Logout();
        void SplitStack(int bag, int slot, int quantity, int destinationBag, int destinationSlot);
        void EquipItem(int bagSlot, int slotId, EquipSlot? equipSlot = null);
        void UnequipItem(EquipSlot slot);
        void AcceptResurrect();
    }
}
