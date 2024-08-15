using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using Communication;

namespace BotRunner.Base
{
    public abstract class BaseObjectManager(IWoWEventHandler woWEventHandler, ActivityMemberState activityMemberState) : IObjectManager
    {
        private readonly IWoWEventHandler _woWEventHandler = woWEventHandler;
        private readonly ActivityMemberState _activityMemberState = activityMemberState;

        public HighGuid PlayerGuid { get; } = new HighGuid(new byte[4], new byte[4]);

        public uint MapId { get; }
        public string ZoneText { get; } = string.Empty;
        public string MinimapZoneText { get; } = string.Empty;
        public string ServerName { get; } = string.Empty;
        public ILocalPlayer Player => (ILocalPlayer)Players.First(x => x.Guid == PlayerGuid.FullGuid);
        public ILocalPet Pet { get; } = null;
        public IList<IWoWObject> Objects { get; } = [];
        public IEnumerable<IWoWGameObject> GameObjects => Objects.OfType<IWoWGameObject>();
        public IEnumerable<IWoWUnit> Units => Objects.OfType<IWoWUnit>();
        public IEnumerable<IWoWPlayer> Players => Objects.OfType<IWoWPlayer>();
        public IEnumerable<IWoWItem> Items => Objects.OfType<IWoWItem>();
        public IEnumerable<IWoWContainer> Containers => Objects.OfType<IWoWContainer>();
        public IEnumerable<IWoWUnit> CasterAggressors => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> MeleeAggressors => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> Aggressors => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWUnit> Hostiles => Units.OfType<IWoWUnit>();
        public IEnumerable<IWoWPlayer> PartyMembers => Players.Where(x =>
                                                                   x.Guid == Party1Guid
                                                                || x.Guid == Party2Guid
                                                                || x.Guid == Party3Guid
                                                                || x.Guid == Party4Guid);
        public IWoWPlayer PartyLeader => PartyMembers.First(x => x.Guid == PartyLeaderGuid);
        public ulong PartyLeaderGuid { get; }
        public ulong Party1Guid { get; }
        public ulong Party2Guid { get; }
        public ulong Party3Guid { get; }
        public ulong Party4Guid { get; }
        public ulong StarTargetGuid { get; }
        public ulong CircleTargetGuid { get; }
        public ulong DiamondTargetGuid { get; }
        public ulong TriangleTargetGuid { get; }
        public ulong MoonTargetGuid { get; }
        public ulong SquareTargetGuid { get; }
        public ulong CrossTargetGuid { get; }
        public ulong SkullTargetGuid { get; }
        public string GlueDialogText { get; } = string.Empty;

        public LoginStates LoginState { get; }
        public abstract void AcceptGroupInvite();
        public abstract void AntiAfk();
        public abstract void ConfirmItemEquip();
        public abstract void ConvertToRaid();
        public abstract int CountFreeSlots(bool v);
        public abstract void DefaultServerLogin(string accountName, string password);
        public abstract void DeleteCursorItem();
        public abstract void EnterWorld();
        public abstract void EquipCursorItem();
        public abstract uint GetBagId(ulong guid);

        public IWoWItem GetEquippedItem(EquipSlot slot) => Items.First(x => x.Guid == Player.GetEquippedItemGuid(slot));

        public IEnumerable<IWoWItem> GetEquippedItems()
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetItem(int v1, int v2)
        {
            throw new NotImplementedException();
        }

        public uint GetItemCount(uint itemId)
        {
            return 0;
        }

        public uint GetSlotId(ulong guid)
        {
            return 0;
        }

        public sbyte GetTalentRank(uint tabIndex, uint talentIndex)
        {
            throw new NotImplementedException();
        }

        public IWoWUnit GetTarget(IWoWUnit woWUnit)
        {
            if (woWUnit == null || woWUnit.TargetGuid == 0)
            {
                return null;
            }

            return Objects.First(x => x.Guid == woWUnit.TargetGuid) as IWoWUnit;
        }

        public void InviteToGroup(string characterName)
        {

        }

        public void JoinBattleGroundQueue()
        {

        }

        public void LeaveGroup()
        {

        }

        public void PickupContainerItem(uint v1, uint v2)
        {

        }

        public void PickupInventoryItem(uint inventorySlot)
        {

        }

        public void PickupMacro(uint v)
        {

        }

        public void PlaceAction(uint v)
        {

        }

        public void ResetInstances()
        {

        }

        public void ResetLogin()
        {

        }

        public void SendChatMessage(string chatMessage)
        {

        }

        public void SetRaidTarget(IWoWUnit target, TargetMarker v)
        {

        }

        public void UseContainerItem(int v1, int v2)
        {

        }
    }
}
