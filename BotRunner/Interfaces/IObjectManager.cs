using BotRunner.Constants;
using BotRunner.Models;

namespace BotRunner.Interfaces
{
    public interface IObjectManager
    {
        HighGuid PlayerGuid { get; }
        uint MapId { get; }
        string ZoneText { get; }
        string MinimapZoneText { get; }
        string ServerName { get; }
        ILocalPlayer Player { get; }
        ILocalPet Pet { get; }
        IList<IWoWObject> Objects { get; }
        IEnumerable<IWoWGameObject> GameObjects { get; }
        IEnumerable<IWoWUnit> Units { get; }
        IEnumerable<IWoWPlayer> Players { get; }
        IEnumerable<IWoWItem> Items { get; }
        IEnumerable<IWoWContainer> Containers { get; }
        IEnumerable<IWoWUnit> CasterAggressors { get; }
        IEnumerable<IWoWUnit> MeleeAggressors { get; }
        IEnumerable<IWoWUnit> Aggressors { get; }
        IEnumerable<IWoWUnit> Hostiles { get; }
        IEnumerable<IWoWPlayer> PartyMembers { get; }
        IWoWPlayer PartyLeader { get; }
        ulong PartyLeaderGuid { get; }
        ulong Party1Guid { get; }
        ulong Party2Guid { get; }
        ulong Party3Guid { get; }
        ulong Party4Guid { get; }
        ulong StarTargetGuid { get; }
        ulong CircleTargetGuid { get; }
        ulong DiamondTargetGuid { get; }
        ulong TriangleTargetGuid { get; }
        ulong MoonTargetGuid { get; }
        ulong SquareTargetGuid { get; }
        ulong CrossTargetGuid { get; }
        ulong SkullTargetGuid { get; }
        void AntiAfk();
        string GlueDialogText { get; }
        LoginStates LoginState { get; }
        IWoWUnit GetTarget(IWoWUnit woWUnit);
        sbyte GetTalentRank(uint tabIndex, uint talentIndex);
        void PickupInventoryItem(uint inventorySlot);
        void DeleteCursorItem();
        void EquipCursorItem();
        void ConfirmItemEquip();
        void SendChatMessage(string chatMessage);
        void SetRaidTarget(IWoWUnit target, TargetMarker v);
        void AcceptGroupInvite();
        void LeaveGroup();
        void EnterWorld();
        void DefaultServerLogin(string accountName, string password);
        void ResetLogin();
        void JoinBattleGroundQueue();
        void ResetInstances();
        void PickupMacro(uint v);
        void PlaceAction(uint v);
        void ConvertToRaid();
        void InviteToGroup(string characterName);
        uint GetItemCount(uint itemId);
        IWoWItem GetEquippedItem(EquipSlot ranged);
        IEnumerable<IWoWItem> GetEquippedItems();
        int CountFreeSlots(bool v);
        IWoWItem GetItem(int v1, int v2);
        void UseContainerItem(int v1, int v2);
        uint GetBagId(ulong guid);
        uint GetSlotId(ulong guid);
        void PickupContainerItem(uint v1, uint v2);
    }
}
