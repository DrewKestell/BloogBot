using Communication;
using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IObjectManager
    {
        ILoginScreen LoginScreen { get; }
        IRealmSelectScreen RealmSelectScreen { get; }
        ICharacterSelectScreen CharacterSelectScreen { get; }
        HighGuid PlayerGuid { get; }
        uint MapId { get; }
        string ZoneText { get; }
        string MinimapZoneText { get; }
        string ServerName { get; }
        IGossipFrame GossipFrame { get; }
        ILootFrame LootFrame { get; }
        IMerchantFrame MerchantFrame { get; }
        ICraftFrame CraftFrame { get; }
        IQuestFrame QuestFrame { get; }
        IQuestGreetingFrame QuestGreetingFrame { get; }
        ITaxiFrame TaxiFrame { get; }
        ITradeFrame TradeFrame { get; }
        ITrainerFrame TrainerFrame { get; }
        ITalentFrame TalentFrame { get; }
        IWoWLocalPlayer Player { get; }
        IWoWLocalPet Pet { get; }
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
        void JoinBattleGroundQueue();
        void ResetInstances();
        void PickupMacro(uint v);
        void PlaceAction(uint v);
        void InviteToGroup(ulong guid);
        void KickPlayer(ulong guid);
        void AcceptGroupInvite();
        void DeclineGroupInvite();
        void LeaveGroup();
        void DisbandGroup();
        void ConvertToRaid();
        bool HasPendingGroupInvite();
        bool HasLootRollWindow(int itemId);
        void LootPass(int itemId);
        void LootRollGreed(int itemId);
        void LootRollNeed(int itemId);
        void AssignLoot(int itemId, ulong playerGuid);
        void SetGroupLoot(GroupLootSetting setting);
        void PromoteLootManager(ulong playerGuid);
        void PromoteAssistant(ulong playerGuid);
        void PromoteLeader(ulong playerGuid);

        void UpdateSnapshot(ActivitySnapshot activitySnapshot)
        {

        }
    }
}
