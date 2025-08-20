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
        IEnumerable<IWoWObject> Objects { get; }
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
        bool HasEnteredWorld { get; }
        public void Face(Position pos)
        {
            if (pos == null) return;

            // sometimes the client gets in a weird state and CurrentFacing is negative. correct that here.
            if (Player.Facing < 0)
            {
                SetFacing((float)(Math.PI * 2) + Player.Facing);
                return;
            }
            if (!Player.IsFacing(pos))
                SetFacing(Player.GetFacingForPosition(pos));
            return;
        }
        public void MoveToward(Position pos)
        {
            Face(pos);
            StartMovement(ControlBits.Front);
        }

        public void Turn180()
        {
            var newFacing = Player.Facing + Math.PI;
            if (newFacing > Math.PI * 2)
                newFacing -= Math.PI * 2;
            SetFacing((float)newFacing);
        }
        public void StopAllMovement()
        {
            var bits = ControlBits.Front | ControlBits.Back | ControlBits.Left | ControlBits.Right | ControlBits.StrafeLeft | ControlBits.StrafeRight;

            StopMovement(bits);
        }
        public IEnumerable<IWoWGameObject> GameObjects => Objects.OfType<IWoWGameObject>();
        public IEnumerable<IWoWUnit> Units => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWPlayer> Players => Objects.OfType<IWoWPlayer>();
        public IEnumerable<IWoWItem> Items => Objects.OfType<IWoWItem>();
        public IEnumerable<IWoWContainer> Containers => Objects.OfType<IWoWContainer>();
        public IEnumerable<IWoWUnit> CasterAggressors => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> MeleeAggressors => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> Aggressors => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> Hostiles => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWPlayer> PartyMembers
        {
            get
            {
                var partyMembers = new List<IWoWPlayer>() { Player };

                var partyMember1 = (IWoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party1Guid);
                if (partyMember1 != null)
                    partyMembers.Add(partyMember1);

                var partyMember2 = (IWoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party2Guid);
                if (partyMember2 != null)
                    partyMembers.Add(partyMember2);

                var partyMember3 = (IWoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party3Guid);
                if (partyMember3 != null)
                    partyMembers.Add(partyMember3);

                var partyMember4 = (IWoWPlayer)Objects.FirstOrDefault(p => p.Guid == Party4Guid);
                if (partyMember4 != null)
                    partyMembers.Add(partyMember4);

                return partyMembers;
            }
        }
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
        void DoEmote(Emote emote);
        void DoEmote(TextEmote emote);
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
        void UpdateSnapshot(ActivitySnapshot activitySnapshot)
        {

        }

        void EnterWorld(ulong characterGuid);
    }
}
